<!--- This is an include file used by Lectora when it is published --->

<!--- Find out if user is logged in --->
<!--- set level needed to access page --->
<cfparam name="URL.lvl" default="15"> 
<cfcookie name = "Acclvl" value = "15">

<!--- access checking code --->
<!--- <cfinclude template="#request.accountDir#/checkaccess.cfm"> --->