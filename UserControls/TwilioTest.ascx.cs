using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using Arena.Portal;
using Arena.Custom.HDC.Twilio.Entity;
using Twilio;

namespace Arena.Custom.HDC.Twilio
{
    public partial class TwilioTest : PortalControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected void SendButton_Click(object sender, EventArgs e)
        {
            TwilioRestClient twilio = new TwilioRestClient("account", "secret");
            SMSMessage msg = null;


            try
            {
                msg = twilio.SendSmsMessage("+5555555555", tbToNumber.Text, tbMessage.Text, "https://arena.yourdomain.com/UserControls/Custom/HDC/Twilio/SmsStatus.aspx");
//                SmsHistory history = new SmsHistory();
//                history.CommunicationId = -1;
//                history.PersonId = -1;
//                history.SmsSid = msg.Sid;
//                history.Save(CurrentUser.Identity.Name);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Could not send SMS", ex);
            }

            StringBuilder sb = new StringBuilder();

            try
            {
                sb.AppendLine(String.Format("AccountSid: {0}", EnsureString(msg.AccountSid)));
                sb.AppendLine(String.Format("ApiVersion: {0}", EnsureString(msg.ApiVersion)));
                sb.AppendLine(String.Format("Body: {0}", EnsureString(msg.Body)));
                sb.AppendLine(String.Format("Direction: {0}", EnsureString(msg.Direction)));
                sb.AppendLine(String.Format("From: {0}", EnsureString(msg.From)));
                sb.AppendLine(String.Format("Price: {0}", EnsureString(msg.Price.ToString())));
                sb.AppendLine(String.Format("Sid: {0}", EnsureString(msg.Sid)));
                sb.AppendLine(String.Format("Status: {0}", EnsureString(msg.Status)));
                sb.AppendLine(String.Format("To: {0}", EnsureString(msg.To)));
//                sb.AppendLine(String.Format("Uri: {0}", EnsureString(msg.Uri.ToString())));
                sb.AppendLine(String.Format("Message: {0}", EnsureString(msg.ToString())));
            }
            catch (System.Exception ex)
            {
            }

            tbStatus.Text = sb.ToString();
        }


        protected string EnsureString(string text)
        {
            if (text == null)
                return "";

            return text;
        }
    }
}
