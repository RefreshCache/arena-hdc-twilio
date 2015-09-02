--
-- Create the core lookup type for Twilio SQL Handlers
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
	VALUES('FC1BA7E8-C22A-48CF-8A30-5DF640049373'
		,'Twilio Handlers'
		,'Provides a list of stored procedures that will handle certain words and phrases when received via text message.'
		,''
		,'Number Lookup ID'
		,'RegEx'
		,'Procedure'
		,'Forward E-mail'
		,''
		,''
		,''
		,''
		,1
		,0)
