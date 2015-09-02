--
-- Create the cust_hdc_twilio_sms_history table.
--
CREATE TABLE [dbo].[cust_hdc_twilio_sms_history](
	[sms_history_id] [int] IDENTITY(1,1) NOT NULL,
	[sms_sid] [char](34) NOT NULL,
	[communication_id] [int] NOT NULL,
	[person_id] [int] NOT NULL,
	[date_created] [datetime] NOT NULL,
	[date_modified] [datetime] NOT NULL,
	[created_by] [varchar](25) NOT NULL,
	[modified_by] [varchar](25) NOT NULL,
 CONSTRAINT [PK_cust_hdc_twilio_sms_history_1] PRIMARY KEY CLUSTERED 
(
	[sms_history_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_twilio_sms_history_core_communication] FOREIGN KEY([communication_id])
REFERENCES [dbo].[core_communication] ([communication_id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history] CHECK CONSTRAINT [FK_cust_hdc_twilio_sms_history_core_communication]
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_twilio_sms_history_core_person] FOREIGN KEY([person_id])
REFERENCES [dbo].[core_person] ([person_id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history] CHECK CONSTRAINT [FK_cust_hdc_twilio_sms_history_core_person]
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history] ADD  CONSTRAINT [DF_cust_hdc_twilio_sms_history_date_created]  DEFAULT (((1)/(1))/(1900)) FOR [date_created]
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history] ADD  CONSTRAINT [DF_cust_hdc_twilio_sms_history_date_modified]  DEFAULT (((1)/(1))/(1900)) FOR [date_modified]
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history] ADD  CONSTRAINT [DF_cust_hdc_twilio_sms_history_created_by]  DEFAULT ('') FOR [created_by]
GO

ALTER TABLE [dbo].[cust_hdc_twilio_sms_history] ADD  CONSTRAINT [DF_cust_hdc_twilio_sms_history_modified_by]  DEFAULT ('') FOR [modified_by]
GO


--
-- Create the cust_hdc_twilio_sp_delete_smsHistory sproc.
--
CREATE PROC[dbo].[cust_hdc_twilio_sp_delete_smsHistory]
@SmsHistoryId int
AS

        DELETE cust_hdc_twilio_sms_history
        WHERE [sms_history_id] = @SmsHistoryId

GO


--
-- Create the cust_hdc_twilio_sp_get_smsHistoryByID sproc.
--
CREATE PROC [dbo].[cust_hdc_twilio_sp_get_smsHistoryByID]
@SmsHistoryId int
AS

        SELECT * 
        FROM cust_hdc_twilio_sms_history
        WHERE [sms_history_id] = @SmsHistoryId

GO


--
-- Create the cust_hdc_twilio_sp_get_smsHistoryBySID sproc.
--
CREATE PROC [dbo].[cust_hdc_twilio_sp_get_smsHistoryBySID]
@SmsSid char(34)
AS

        SELECT * 
        FROM cust_hdc_twilio_sms_history
        WHERE [sms_sid] = @SmsSid

GO


--
-- Create the cust_hdc_twilio_sp_save_smsHistory sproc.
--
CREATE PROC [dbo].[cust_hdc_twilio_sp_save_smsHistory]
@SmsHistoryId int,
@SmsSid char(34),
@CommunicationId int,
@PersonId int,
@UserId varchar(50),
@ID int OUTPUT
AS

	DECLARE @UpdateDateTime DateTime SET @UpdateDateTime = GETDATE()

	IF NOT EXISTS(
		SELECT * FROM cust_hdc_twilio_sms_history
		WHERE [sms_history_id] = @SmsHistoryId
		)
		
	BEGIN
	
		INSERT INTO cust_hdc_twilio_sms_history
		(	
			 [date_created]
			,[date_modified]
			,[created_by]
			,[modified_by]
			,[sms_sid]
			,[communication_id]
			,[person_id]
		)
		values
		(	
			 @UpdateDateTime
			,@UpdateDateTime
			,@UserID
			,@UserID
			,@SmsSid
			,@CommunicationId
			,@PersonId
		)
		SET @ID = @@IDENTITY
	END
	ELSE
	BEGIN

		UPDATE cust_hdc_twilio_sms_history Set
			 [date_modified] = @UpdateDateTime 
			,[modified_by] = @UserID
			,[sms_sid] = @SmsSid
			,[communication_id] = @CommunicationId
			,[person_id] = @PersonId
		WHERE [sms_history_id] = @SmsHistoryId

		SET @ID = @SmsHistoryId
	END

GO


--
-- Create the core lookup type.
--
INSERT INTO core_lookup_type
	([guid]
	,lookup_type_name
	,lookup_type_desc
	,lookup_category
	,qualifier_1_title
	,qualifier_2_title
	,qualifier_3_title
	,qualifier_4_title
	,qualifier_5_title
	,qualifier_6_title
	,qualifier_7_title
	,qualifier_8_title
	,organization_id
	,system_flag)
	VALUES('11B4ADEC-CB8C-4D01-B99E-7A0FFE2007B5'
		,'Twilio Numbers'
		,'Phone numbers available for use with Twilio.'
		,''
		,'Phone Number'
		,'Forward E-mail'
		,''
		,''
		,''
		,''
		,''
		,'Autoresponse'
		,1
		,0)
		

--
-- Create the lookup for the Twilio SMS provider.
--
INSERT INTO core_lookup
	([guid]
	,lookup_type_id
	,lookup_value
	,lookup_qualifier
	,lookup_qualifier2
	,lookup_qualifier3
	,lookup_qualifier4
	,lookup_qualifier5
	,lookup_qualifier6
	,lookup_qualifier7
	,lookup_qualifier8
	,lookup_order
	,active
	,system_flag
	,organization_id)
	VALUES
	(NEWID()
	,(SELECT TOP 1 lookup_type_id FROM core_lookup_type WHERE [guid] = '82DD2E6A-4D66-447C-B29A-4E2656B5E557')
	,'Twilio SMS'
	,''
	,'Arena.Custom.HDC.Twilio.TwilioSMS'
	,'Arena.Custom.HDC.Twilio'
	,''
	,''
	,''
	,''
	,''
	,-1
	,0
	,0
	,1)
