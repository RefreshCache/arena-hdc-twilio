using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Portal;

namespace Arena.Custom.HDC.Twilio
{
    public partial class SmsCommunicationInjection : PortalControl
    {
        protected int IsMediumEmail = 0;
        protected string FromEmail = "";
        protected string ReplyToEmail = "";

        #region Module Settings

        [LookupMultiSelectSetting("Allowed Phone Numbers", "List of topic areas to include.", true, "11B4ADEC-CB8C-4D01-B99E-7A0FFE2007B5", "")]
        public String AllowedPhoneNumbersSetting { get { return Setting("AllowedPhoneNumbers", "", true); } }

        #endregion

        
        protected void Page_Load(object sender, EventArgs e)
        {
            LookupCollection validNumbers = new LookupCollection(new Guid("11B4ADEC-CB8C-4D01-B99E-7A0FFE2007B5"));
            StringBuilder sb = new StringBuilder();


            foreach (String s in AllowedPhoneNumbersSetting.Split(','))
            {
                Lookup lk = new Lookup(Convert.ToInt32(s));
                sb.AppendLine(String.Format("ddl.append('<option value=\"{0}\">{1}</option>');",
                    Arena.Custom.HDC.Twilio.TwilioSMS.CleanPhone(lk.Qualifier), Server.HtmlEncode(lk.Value)));
            }

            ltSelect.Text = sb.ToString();

            if (!String.IsNullOrEmpty(Request.Params["MEDIUM"]) && Request.Params["MEDIUM"] == "email")
            {
                IsMediumEmail = 1;
                FromEmail = CurrentPerson.Emails.FirstActive;
                ReplyToEmail = CurrentPerson.Emails.FirstActive;
            }
        }
    }
}