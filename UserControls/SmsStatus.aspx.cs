using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.DataLayer.Core;
using Arena.DataLayer.Organization;
using Arena.Utility;
using Arena.Custom.HDC.Twilio.Entity;

namespace Arena.Custom.HDC.Twilio
{
    public partial class SmsStatus : System.Web.UI.Page
    {
        private const String personDetailPageId = "7";


        protected void Page_Load(object sender, EventArgs e)
        {
            String smsSid = Request.Form["SmsSid"].ToString();
            String status = Request.Form["SmsStatus"].ToString().ToLower();
            SmsHistory history;


            //
            // Check if this is a received message.
            //
            if (status == "received")
            {
                OrganizationData orgData = new OrganizationData();
                LookupCollection validNumbers = new LookupCollection(new Guid("11B4ADEC-CB8C-4D01-B99E-7A0FFE2007B5"));
                LookupCollection twilioHandlers = new LookupCollection(new Guid("FC1BA7E8-C22A-48CF-8A30-5DF640049373"));
                String body = Request.Form["Body"].ToString();
                String from = Request.Form["From"].ToString();
                String to = Request.Form["To"].ToString();
                String baseUrl;
                StringBuilder sb = new StringBuilder();
                List<String> possiblyFrom = new List<String>();
                Boolean handled = false;
                PersonCollection col = new PersonCollection();
                Lookup lkTo = null;

                //
                // Look for the twilio phone number, if not found ignore the incoming message.
                //
                to = TwilioSMS.CleanPhone(to);
                foreach (Lookup lk in validNumbers)
                {
                    if (lk.Active && to == TwilioSMS.CleanPhone(lk.Qualifier))
                    {
                        lkTo = lk;
                        break;
                    }
                }
                if (lkTo == null)
                    return;

                //
                // Load all person records that match the phone number.
                //
                from = TwilioSMS.CleanPhone(from);
                if (from.StartsWith("1"))
                    col.LoadByPhone(from.Substring(1));
                else
                    col.LoadByPhone(from);

                //
                // Build a list of ID numbers for the person IDs that this could be from.
                //
                foreach (Person p in col)
                {
                    possiblyFrom.Add(p.PersonID.ToString());
                }

                //
                // See if we can find a stored procedure that wants to handle this message.
                //
                try
                {
                    foreach (Lookup handler in twilioHandlers)
                    {
                        ArrayList parms = new ArrayList();
                        SqlParameter outStatus = new SqlParameter("@OutStatus", SqlDbType.Int);
                        SqlParameter outMessage = new SqlParameter("@OutMessage", SqlDbType.VarChar, 2000);

                        if (handler.Active == false)
                            continue;

                        //
                        // Check if this handler is for us.
                        //
                        if (Convert.ToInt32(handler.Qualifier) != lkTo.LookupID && lkTo.LookupID != -1)
                            continue;

                        //
                        // Check if there is a match on the regular expression.
                        //
                        if (!Regex.IsMatch(body, handler.Qualifier2, RegexOptions.IgnoreCase))
                            continue;

                        outStatus.Direction = ParameterDirection.Output;
                        outMessage.Direction = ParameterDirection.Output;
                        parms.Add(new SqlParameter("@FromNumber", from));
                        parms.Add(new SqlParameter("@PossiblyFrom", String.Join(",", possiblyFrom.ToArray())));
                        parms.Add(new SqlParameter("@ToNumber", to));
                        parms.Add(new SqlParameter("@ToNumberID", lkTo.LookupID));
                        parms.Add(new SqlParameter("@Message", body));
                        parms.Add(outStatus);
                        parms.Add(outMessage);

                        orgData.ExecuteNonQuery(handler.Qualifier3, parms);

                        //
                        // See if a response should be sent.
                        //
                        if ((int)outStatus.Value == 1)
                        {
                            if (!String.IsNullOrEmpty((String)outMessage.Value))
                            {
                                int len = outMessage.Value.ToString().Length;

                                if (len > 160)
                                    len = 160;
                                Response.Clear();
                                Response.ContentType = "text/plain";
                                Response.Write(outMessage.Value.ToString().Substring(0, len));

                                //
                                // Set the base url.
                                //
                                baseUrl = ArenaContext.Current.AppSettings["ApplicationURLPath"];
                                if (!baseUrl.EndsWith("/"))
                                    baseUrl = baseUrl + "/";
                                baseUrl = String.Format("{0}default.aspx?page={1}", baseUrl, personDetailPageId);

                                //
                                // If there is a valid e-mail address to forward the text to then build up an e-mail
                                // message and send an e-mail.
                                //
                                if (!String.IsNullOrEmpty(handler.Qualifier4))
                                {
                                    try
                                    {
                                        sb.AppendFormat("<p>Received a text message from {0} to {1} ({2})</p>", from, to, lkTo.Value);
                                        sb.AppendFormat("<p>Message: {0}</p>", body);

                                        if (col.Count > 0)
                                        {
                                            sb.Append("<p>Possibly received from:<br /><ul>");
                                            foreach (Person p in col)
                                            {
                                                sb.AppendFormat("<li><a href=\"{0}&guid={1}\">{2}</a></li>", baseUrl, p.PersonGUID.ToString(), Server.HtmlEncode(p.FullName));
                                            }
                                            sb.Append("</ul></p>");
                                        }

                                        ArenaSendMail.SendMail(String.Empty, String.Empty, handler.Qualifier4, "Received SMS message", sb.ToString());
                                    }
                                    catch (System.Exception ex)
                                    {
                                        try
                                        {
                                            new Arena.DataLayer.Core.ExceptionHistoryData().AddUpdate_Exception(ex,
                                                ArenaContext.Current.Organization.OrganizationID,
                                                "HDC.Twilio", ArenaContext.Current.ServerUrl);
                                        }
                                        catch { }
                                    }
                                }
                                
                                Response.End();
                            }

                            handled = true;
                            break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    if (ex.Message != "Thread was being aborted.")
                    {
                        new Arena.DataLayer.Core.ExceptionHistoryData().AddUpdate_Exception(ex,
                            ArenaContext.Current.Organization.OrganizationID,
                            "HDC.Twilio", ArenaContext.Current.ServerUrl);
                    }
                }

                //
                // If the message has already been handled, we don't need to send any replies or
                // e-mail anybody.
                //
                if (!handled)
                {
                    //
                    // Set the base url.
                    //
                    baseUrl = ArenaContext.Current.AppSettings["ApplicationURLPath"];
                    if (!baseUrl.EndsWith("/"))
                        baseUrl = baseUrl + "/";
                    baseUrl = String.Format("{0}default.aspx?page={1}", baseUrl, personDetailPageId);

                    //
                    // If there is a valid e-mail address to forward the text to then build up an e-mail
                    // message and send an e-mail.
                    //
                    if (!String.IsNullOrEmpty(lkTo.Qualifier2))
                    {
                        try
                        {
                            sb.AppendFormat("<p>Received a text message from {0} to {1} ({2})</p>", from, to, lkTo.Value);
                            sb.AppendFormat("<p>Message: {0}</p>", body);

                            if (col.Count > 0)
                            {
                                sb.Append("<p>Possibly received from:<br /><ul>");
                                foreach (Person p in col)
                                {
                                    sb.AppendFormat("<li><a href=\"{0}&guid={1}\">{2}</a></li>", baseUrl, p.PersonGUID.ToString(), Server.HtmlEncode(p.FullName));
                                }
                                sb.Append("</ul></p>");
                            }

                            new ArenaSendMail().SendMail(String.Empty, String.Empty, lkTo.Qualifier2, "Received SMS message", sb.ToString());
                        }
                        catch (System.Exception ex)
                        {
                            try
                            {
                                new Arena.DataLayer.Core.ExceptionHistoryData().AddUpdate_Exception(ex,
                                    ArenaContext.Current.Organization.OrganizationID,
                                    "HDC.Twilio", ArenaContext.Current.ServerUrl);
                            }
                            catch { }
                        }
                    }

                    //
                    // If there is an auto-reply message in the lookup then send back that message.
                    //
                    if (!String.IsNullOrEmpty(lkTo.Qualifier8))
                    {
                        try
                        {
                            int len = lkTo.Qualifier8.Length;

                            if (len > 160)
                                len = 160;
                            Response.Clear();
                            Response.ContentType = "text/plain";
                            Response.Write(lkTo.Qualifier8.Substring(0, len));
                            Response.End();
                        }
                        catch (System.Exception ex)
                        {
                            try
                            {
                                if (ex.Message != "Thread was being aborted.")
                                {
                                    new Arena.DataLayer.Core.ExceptionHistoryData().AddUpdate_Exception(ex,
                                        ArenaContext.Current.Organization.OrganizationID,
                                        "HDC.Twilio", ArenaContext.Current.ServerUrl);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            else
            {
                //
                // This (should be) a status response to an outgoing message.
                //
                history = new SmsHistory(smsSid);
                if (history.SmsHistoryId != -1)
                {
                    try
                    {
                        if (status == "sent")
                        {
                            new PersonCommunicationData().SavePersonCommunication(history.CommunicationId, history.PersonId, DateTime.Now, "Success");
                        }
                        else
                        {
                            new PersonCommunicationData().SavePersonCommunication(history.CommunicationId, history.PersonId, DateTime.Now, "Failed");
                        }
                    }
                    catch (System.Exception)
                    {
                    }

                    history.Delete();
                }
            }
        }
    }
}