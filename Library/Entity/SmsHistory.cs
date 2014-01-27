using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Arena.Core;
using Arena.Custom.HDC.Twilio.DataLayer;

namespace Arena.Custom.HDC.Twilio.Entity
{
    public class SmsHistory : ArenaObjectBase
    {
        #region Private Members

        private int _smsHistoryId = -1;
        private int _communicationId = -1;
        private int _personId = -1;
        private string _smsSid = string.Empty;

        private DateTime _dateCreated = DateTime.MinValue;
        private DateTime _dateModified = DateTime.MinValue;
        private string _createdBy = string.Empty;
        private string _modifiedBy = string.Empty;

        #endregion


        #region Public Properties

        public int SmsHistoryId { get { return _smsHistoryId; } set { _smsHistoryId = value; } }
        public int CommunicationId { get { return _communicationId; } set { _communicationId = value; } }
        public int PersonId { get { return _personId; } set { _personId = value; } }
        public string SmsSid { get { return _smsSid; } set { _smsSid = value; } }

        public DateTime DateCreated { get { return _dateCreated; } set { _dateCreated = value; } }
        public DateTime DateModified { get { return _dateModified; } set { _dateModified = value; } }
        public string CreatedBy { get { return _createdBy; } set { _createdBy = value; } }
        public string ModifiedBy { get { return _modifiedBy; } set { _modifiedBy = value; } }

        #endregion


        #region Constructors

        public SmsHistory()
        {
        }


        public SmsHistory(int smsHistoryId)
        {
            SqlDataReader reader = new SmsHistoryData().GetSmsHistoryByID(smsHistoryId);


            if (reader.Read())
                LoadSmsHistory(reader);
            reader.Close();
        }


        public SmsHistory(string smsSid)
        {
            SqlDataReader reader = new SmsHistoryData().GetSmsHistoryBySID(smsSid);


            if (reader.Read())
                LoadSmsHistory(reader);
            reader.Close();
        }

        #endregion


        #region Public Methods

        public void Save(string userId)
        {
            SaveSmsHistory(userId);
        }


        public static void Delete(int smsHistoryId)
        {
            new SmsHistoryData().DeleteSmsHistoryByID(smsHistoryId);
        }


        public void Delete()
        {
            new SmsHistoryData().DeleteSmsHistoryByID(_smsHistoryId);
            _smsHistoryId = -1;
        }

        #endregion


        #region Private Methods

        private void SaveSmsHistory(string userId)
        {
            _smsHistoryId = new SmsHistoryData().SaveSmsHistory(userId, _smsHistoryId,
                _smsSid, _communicationId, _personId);
        }


        private void LoadSmsHistory(SqlDataReader reader)
        {
            _smsHistoryId = (int)reader["sms_history_id"];
            _communicationId = (int)reader["communication_id"];
            _personId = (int)reader["person_id"];
            _smsSid = reader["sms_sid"].ToString();

            _dateCreated = (DateTime)reader["date_created"];
            _dateModified = (DateTime)reader["date_modified"];
            _createdBy = reader["created_by"].ToString();
            _modifiedBy = reader["modified_by"].ToString();
        }

        #endregion
    }
}
