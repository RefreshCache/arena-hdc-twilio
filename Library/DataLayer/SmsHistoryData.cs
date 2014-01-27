using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Arena.DataLib;

namespace Arena.Custom.HDC.Twilio.DataLayer
{
    public class SmsHistoryData : SqlData
    {
        public SmsHistoryData()
        {
        }


        public SqlDataReader GetSmsHistoryByID(int smsHistoryId)
        {
            ArrayList parms = new ArrayList();


            parms.Add(new SqlParameter("@SmsHistoryId", smsHistoryId));

            try
            {
                return this.ExecuteSqlDataReader("cust_hdc_twilio_sp_get_smsHistoryByID", parms);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                parms = null;
            }
        }


        public SqlDataReader GetSmsHistoryBySID(string smsSid)
        {
            ArrayList parms = new ArrayList();


            parms.Add(new SqlParameter("@SmsSid", smsSid));

            try
            {
                return this.ExecuteSqlDataReader("cust_hdc_twilio_sp_get_smsHistoryBySID", parms);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                parms = null;
            }
        }


        public void DeleteSmsHistoryByID(int smsHistoryId)
        {
            ArrayList parms = new ArrayList();


            parms.Add(new SqlParameter("@SmsHistoryId", smsHistoryId));

            try
            {
                this.ExecuteNonQuery("cust_hdc_twilio_sp_delete_smsHistory", parms);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                parms = null;
            }
        }


        public int SaveSmsHistory(string userId, int smsHistoryId, string smsSid, int communicationId, int personId)
        {
            ArrayList parms = new ArrayList();


            parms.Add(new SqlParameter("@UserId", userId));
            parms.Add(new SqlParameter("@SmsHistoryId", smsHistoryId));
            parms.Add(new SqlParameter("@SmsSid", smsSid));
            parms.Add(new SqlParameter("@CommunicationId", communicationId));
            parms.Add(new SqlParameter("@PersonId", personId));

            SqlParameter paramOut = new SqlParameter();
            paramOut.ParameterName = "@ID";
            paramOut.Direction = ParameterDirection.Output;
            paramOut.SqlDbType = SqlDbType.Int;
            parms.Add(paramOut);

            try
            {
                this.ExecuteNonQuery("cust_hdc_twilio_sp_save_smsHistory", parms);
                return (int)((SqlParameter)(parms[parms.Count - 1])).Value;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Unique Key Violation
                    return -1;
                else
                    throw ex;
            }
            finally
            {
                parms = null;
            }
        }
    }
}
