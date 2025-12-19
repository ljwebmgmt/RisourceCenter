<cftry>
<cfset tr_user = '#cookie.RISOURCEUSRID#'>
<!--- <cfset tr_ip = '#cgi.REMOTE_ADDR#'> --->
<cfset tr_module = '1'>

<cfquery name="insertResults" datasource="RC_NET">
	INSERT INTO stats_training (tr_usr, tr_module, tr_date, tr_NumQuestions, tr_PassGrade, tr_score)
	VALUES (#tr_user#, '#FORM.TestName#', CURRENT_TIMESTAMP, '#FORM.NumQuestions#', '#FORM.PassingGrade#', '#FORM.Score#')
</cfquery>

<cfcatch>
		<cfmail to="webmaster@rittal.us" from="webmaster@rittal.us" subject="error" type="html">
		<strong>CFDUMP</strong><br />
		<cfdump var="#cfcatch#"><br /><br />
		<strong>CGI Variables</strong><br />
		<cfdump var="#cgi#">
	</cfmail>
</cfcatch>
</cftry>