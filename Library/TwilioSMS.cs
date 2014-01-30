using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Arena.Core;
using Arena.Core.SMS;
using Arena.Custom.HDC.Twilio.Entity;
using Arena.DataLayer.Core;
using Twilio;


namespace Arena.Custom.HDC.Twilio
{
    public class TwilioSMS : ISMS
    {
        private string _username, _password;
        private static Regex digitsOnly = new Regex(@"[^\d]");

        public string UserName { get { return _username; } set { _username = value; } }
        public string Password { get { return _password; } set { _password = value; } }
        public int RemainingCredits { get { return 0; } }


        public void SendMessage(int personID, int communicationID, string toNumber, string message, string userID)
        {
            this.SendMessage(personID, communicationID, toNumber, String.Empty, String.Empty, message, userID);
        }


        public void SendMessage(int personID, int communicationID, string toNumber, string fromNumber, string fromName, string message, string userID)
        {
            PersonCommunication pc;
            TwilioRestClient twilio = new TwilioRestClient(_username, _password);
            LookupCollection validNumbers = new LookupCollection(new Guid("11B4ADEC-CB8C-4D01-B99E-7A0FFE2007B5"));
            SMSMessage msg;
            SmsHistory history;
            String twilioNumber = null;
            String callback;


            //
            // Check if this message has already been sent. The agent seems to be broken and
            // will send each SMS message twice in rapid succession. If the communication is
            // no longer pending or queued, assume it has already been sent and silently ignore.
            //
            if (personID != -1 && communicationID != -1)
            {
                pc = new PersonCommunication(personID, communicationID);
                if (pc.CommunicationID != -1)
                {
                    if (pc.Status != "Pending" && pc.Status != "Queued")
                        return;
                }
            }

            //
            // Get the first enabled twilio number, use that as the default.
            //
            foreach (Lookup lk in validNumbers)
            {
                if (lk.Active)
                {
                    twilioNumber = CleanPhone(lk.Qualifier);
                    break;
                }
            }

            //
            // Check if the fromNumber is a valid Twilio number, if it isn't then use the default Twilio Number.
            //
            fromNumber = CleanPhone(fromNumber);
            foreach (Lookup lk in validNumbers)
            {
                string cleaned = CleanPhone(lk.Qualifier);

                if (lk.Active && fromNumber == cleaned)
                {
                    twilioNumber = cleaned;
                    break;
                }
            }

            //
            // Verify that we have our default twilio number defined.
            //
            if (String.IsNullOrEmpty(twilioNumber))
            {
                if (communicationID != -1 && personID != -1)
                    new PersonCommunicationData().SavePersonCommunication(communicationID, personID, DateTime.Now, "Failed -- Invalid from number specified");

                return;
            }

            //
            // If we don't have a valid outgoing number, fail.
            //
            toNumber = CleanPhone(toNumber);
            if (String.IsNullOrEmpty(toNumber))
            {
                if (communicationID != -1 && personID != -1)
                    new PersonCommunicationData().SavePersonCommunication(communicationID, personID, DateTime.Now, "Failed -- No SMS number available");

                return;
            }

            //
            // Construct the callback URL.
            //
            callback = ArenaContext.Current.AppSettings["ApplicationURLPath"];
            if (!callback.EndsWith("/"))
                callback = callback + "/";
            callback = callback + "UserControls/Custom/HDC/Twilio/SmsStatus.aspx";

            //
            // Update the person communication history to show that we have attempted to send, since this is an
            // asynchronous operation we don't want to chance the callback not working and the message gets
            // sent over and over again.
            //
            if (communicationID != -1 && personID != -1)
            {
                new PersonCommunicationData().SavePersonCommunication(communicationID, personID, DateTime.Now, "Pushed");
            }

            //
            // Send the message.
            //
            msg = twilio.SendSmsMessage(twilioNumber, toNumber, message, callback);
            if (personID != -1 && communicationID != -1)
            {
                if (String.IsNullOrEmpty(msg.Sid))
                {
                    if (communicationID != -1 && personID != -1)
                    {
                        String reason;

                        if (msg.RestException != null && !String.IsNullOrEmpty(msg.RestException.Message))
                            reason = String.Format("Failed -- SMS provider did not accept message ({0}).", msg.RestException.Message);
                        else if (!String.IsNullOrEmpty(msg.Status))
                            reason = String.Format("Failed -- SMS provider did not accept message ({0}).", msg.Status);
                        else
                            reason = String.Format("Failed -- SMS provider did not accept message (Unknown Error).");
                        new PersonCommunicationData().SavePersonCommunication(communicationID, personID, DateTime.Now, reason);
                    }
                }
                else
                {
                    history = new SmsHistory();
                    history.CommunicationId = communicationID;
                    history.PersonId = personID;
                    history.SmsSid = msg.Sid;
                    history.Save(userID);
                }
            }
        }


        /// <summary>
        /// Strip out non-numeric characters from a phone number.
        /// </summary>
        /// <param name="phone">The phone number to sanitize.</param>
        /// <returns>A sanitized version of phone.</returns>
        public static string CleanPhone(string phone)
        {
            return digitsOnly.Replace(phone, "");
        }
    }
}