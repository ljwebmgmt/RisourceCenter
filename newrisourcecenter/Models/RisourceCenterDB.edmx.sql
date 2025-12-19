
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/04/2018 09:33:59
-- Generated from EDMX file: C:\Users\US84004970\source\RiSourceCenter\rittal.net_master\newrisourcecenter\Models\RisourceCenterDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [RisourceCenterDotNetDev];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_dbo_AspNetUserRoles_dbo_AspNetRoles_RoleId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_dbo_AspNetUserRoles_dbo_AspNetRoles_RoleId];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[__MigrationHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[__MigrationHistory];
GO
IF OBJECT_ID(N'[dbo].[adminPerms]', 'U') IS NOT NULL
    DROP TABLE [dbo].[adminPerms];
GO
IF OBJECT_ID(N'[dbo].[Announcement_logs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Announcement_logs];
GO
IF OBJECT_ID(N'[dbo].[Announcements]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Announcements];
GO
IF OBJECT_ID(N'[dbo].[AspNetRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AspNetRoles];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AspNetUserClaims];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AspNetUserLogins];
GO
IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AspNetUserRoles];
GO
IF OBJECT_ID(N'[dbo].[AspNetUsers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AspNetUsers];
GO
IF OBJECT_ID(N'[dbo].[CountLoggedin]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CountLoggedin];
GO
IF OBJECT_ID(N'[dbo].[countries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[countries];
GO
IF OBJECT_ID(N'[dbo].[data_state]', 'U') IS NOT NULL
    DROP TABLE [dbo].[data_state];
GO
IF OBJECT_ID(N'[dbo].[email_tracker]', 'U') IS NOT NULL
    DROP TABLE [dbo].[email_tracker];
GO
IF OBJECT_ID(N'[dbo].[Labels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Labels];
GO
IF OBJECT_ID(N'[dbo].[Large_enclosure_fmd]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Large_enclosure_fmd];
GO
IF OBJECT_ID(N'[dbo].[Large_enclosure_myaccessories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Large_enclosure_myaccessories];
GO
IF OBJECT_ID(N'[dbo].[Large_enclosure_myconfig]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Large_enclosure_myconfig];
GO
IF OBJECT_ID(N'[dbo].[Large_enclosure_pricing]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Large_enclosure_pricing];
GO
IF OBJECT_ID(N'[dbo].[Large_enclosure_ts8]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Large_enclosure_ts8];
GO
IF OBJECT_ID(N'[dbo].[literature]', 'U') IS NOT NULL
    DROP TABLE [dbo].[literature];
GO
IF OBJECT_ID(N'[dbo].[literature_requested]', 'U') IS NOT NULL
    DROP TABLE [dbo].[literature_requested];
GO
IF OBJECT_ID(N'[dbo].[Localization]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Localization];
GO
IF OBJECT_ID(N'[dbo].[mdf_file]', 'U') IS NOT NULL
    DROP TABLE [dbo].[mdf_file];
GO
IF OBJECT_ID(N'[dbo].[mdf_main]', 'U') IS NOT NULL
    DROP TABLE [dbo].[mdf_main];
GO
IF OBJECT_ID(N'[dbo].[mdf_subType]', 'U') IS NOT NULL
    DROP TABLE [dbo].[mdf_subType];
GO
IF OBJECT_ID(N'[dbo].[mdf_type]', 'U') IS NOT NULL
    DROP TABLE [dbo].[mdf_type];
GO
IF OBJECT_ID(N'[dbo].[nav1]', 'U') IS NOT NULL
    DROP TABLE [dbo].[nav1];
GO
IF OBJECT_ID(N'[dbo].[nav2]', 'U') IS NOT NULL
    DROP TABLE [dbo].[nav2];
GO
IF OBJECT_ID(N'[dbo].[nav3]', 'U') IS NOT NULL
    DROP TABLE [dbo].[nav3];
GO
IF OBJECT_ID(N'[dbo].[PartnerApplication]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PartnerApplication];
GO
IF OBJECT_ID(N'[dbo].[partnerCompany]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerCompany];
GO
IF OBJECT_ID(N'[dbo].[partnerCompany_Archive]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerCompany_Archive];
GO
IF OBJECT_ID(N'[dbo].[partnerIndustry]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerIndustry];
GO
IF OBJECT_ID(N'[dbo].[partnerLocation]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerLocation];
GO
IF OBJECT_ID(N'[dbo].[partnerProducts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerProducts];
GO
IF OBJECT_ID(N'[dbo].[partnerStockCheck]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerStockCheck];
GO
IF OBJECT_ID(N'[dbo].[partnerType]', 'U') IS NOT NULL
    DROP TABLE [dbo].[partnerType];
GO
IF OBJECT_ID(N'[dbo].[ReturnToolActionLogs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReturnToolActionLogs];
GO
IF OBJECT_ID(N'[dbo].[ReturnToolExtentions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReturnToolExtentions];
GO
IF OBJECT_ID(N'[dbo].[ReturnToolFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReturnToolFiles];
GO
IF OBJECT_ID(N'[dbo].[ReturnTools]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReturnTools];
GO
IF OBJECT_ID(N'[dbo].[RFQ_Action_Log]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RFQ_Action_Log];
GO
IF OBJECT_ID(N'[dbo].[RFQ_Data]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RFQ_Data];
GO
IF OBJECT_ID(N'[dbo].[RFQ_Data_Extend]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RFQ_Data_Extend];
GO
IF OBJECT_ID(N'[dbo].[RFQ_Files]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RFQ_Files];
GO
IF OBJECT_ID(N'[dbo].[RFQ_Parts_Installed]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RFQ_Parts_Installed];
GO
IF OBJECT_ID(N'[dbo].[RFQ_Parts_Shipped]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RFQ_Parts_Shipped];
GO
IF OBJECT_ID(N'[dbo].[RiSourceCart]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RiSourceCart];
GO
IF OBJECT_ID(N'[dbo].[RiSources]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RiSources];
GO
IF OBJECT_ID(N'[dbo].[RiSources_Action_Log]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RiSources_Action_Log];
GO
IF OBJECT_ID(N'[dbo].[RiSourcesCart]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RiSourcesCart];
GO
IF OBJECT_ID(N'[dbo].[risourcesCategories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[risourcesCategories];
GO
IF OBJECT_ID(N'[dbo].[risourcesType_image]', 'U') IS NOT NULL
    DROP TABLE [dbo].[risourcesType_image];
GO
IF OBJECT_ID(N'[dbo].[salesComm]', 'U') IS NOT NULL
    DROP TABLE [dbo].[salesComm];
GO
IF OBJECT_ID(N'[dbo].[salesRequest]', 'U') IS NOT NULL
    DROP TABLE [dbo].[salesRequest];
GO
IF OBJECT_ID(N'[dbo].[salesRequest_Action_Log]', 'U') IS NOT NULL
    DROP TABLE [dbo].[salesRequest_Action_Log];
GO
IF OBJECT_ID(N'[dbo].[salesRequest_Additional_Info]', 'U') IS NOT NULL
    DROP TABLE [dbo].[salesRequest_Additional_Info];
GO
IF OBJECT_ID(N'[dbo].[salesRequest_Approvers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[salesRequest_Approvers];
GO
IF OBJECT_ID(N'[dbo].[salesRequest_File]', 'U') IS NOT NULL
    DROP TABLE [dbo].[salesRequest_File];
GO
IF OBJECT_ID(N'[dbo].[SiteActionLogModels]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SiteActionLogModels];
GO
IF OBJECT_ID(N'[dbo].[SiteApprovers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SiteApprovers];
GO
IF OBJECT_ID(N'[dbo].[SPA_Account_Manager]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_Account_Manager];
GO
IF OBJECT_ID(N'[dbo].[SPA_FIles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_FIles];
GO
IF OBJECT_ID(N'[dbo].[SPA_Intostock_Multiplier]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_Intostock_Multiplier];
GO
IF OBJECT_ID(N'[dbo].[SPA_Material_Master]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_Material_Master];
GO
IF OBJECT_ID(N'[dbo].[SPA_Notes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_Notes];
GO
IF OBJECT_ID(N'[dbo].[SPA_Rebates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_Rebates];
GO
IF OBJECT_ID(N'[dbo].[SPA_RebatesItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_RebatesItems];
GO
IF OBJECT_ID(N'[dbo].[SPA_SalesReps]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_SalesReps];
GO
IF OBJECT_ID(N'[dbo].[SPA_Territory_Codes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPA_Territory_Codes];
GO
IF OBJECT_ID(N'[dbo].[SPAItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPAItems];
GO
IF OBJECT_ID(N'[dbo].[SPAs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPAs];
GO
IF OBJECT_ID(N'[dbo].[SPASkus]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SPASkus];
GO
IF OBJECT_ID(N'[dbo].[stats_training]', 'U') IS NOT NULL
    DROP TABLE [dbo].[stats_training];
GO
IF OBJECT_ID(N'[dbo].[themes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[themes];
GO
IF OBJECT_ID(N'[dbo].[usr_user]', 'U') IS NOT NULL
    DROP TABLE [dbo].[usr_user];
GO
IF OBJECT_ID(N'[dbo].[usr_user_temp]', 'U') IS NOT NULL
    DROP TABLE [dbo].[usr_user_temp];
GO
IF OBJECT_ID(N'[dbo].[usr_user_test]', 'U') IS NOT NULL
    DROP TABLE [dbo].[usr_user_test];
GO
IF OBJECT_ID(N'[dbo].[Webshop_connect]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Webshop_connect];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'C__MigrationHistory'
CREATE TABLE [dbo].[C__MigrationHistory] (
    [MigrationId] nvarchar(150)  NOT NULL,
    [ContextKey] nvarchar(300)  NOT NULL,
    [Model] varbinary(max)  NOT NULL,
    [ProductVersion] nvarchar(32)  NOT NULL
);
GO

-- Creating table 'adminPerms'
CREATE TABLE [dbo].[adminPerms] (
    [ap_ID] int IDENTITY(1,1) NOT NULL,
    [usr_ID] bigint  NULL,
    [ap_user] tinyint  NULL,
    [ap_partner] tinyint  NULL,
    [ap_salesComms] tinyint  NULL,
    [ap_ru] tinyint  NULL,
    [ap_tools] tinyint  NULL,
    [ap_reports] tinyint  NULL,
    [ap_MDF] tinyint  NULL,
    [ap_QQ] tinyint  NULL,
    [ap_PR] tinyint  NULL,
    [ap_FX] tinyint  NULL,
    [ap_MDFA] tinyint  NULL,
    [ap_product] tinyint  NULL
);
GO

-- Creating table 'AspNetRoles'
CREATE TABLE [dbo].[AspNetRoles] (
    [Id] nvarchar(128)  NOT NULL,
    [Name] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'AspNetUserClaims'
CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(128)  NOT NULL,
    [ClaimType] nvarchar(max)  NULL,
    [ClaimValue] nvarchar(max)  NULL
);
GO

-- Creating table 'AspNetUserLogins'
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] nvarchar(128)  NOT NULL,
    [ProviderKey] nvarchar(128)  NOT NULL,
    [UserId] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'AspNetUsers'
CREATE TABLE [dbo].[AspNetUsers] (
    [Id] nvarchar(128)  NOT NULL,
    [Email] nvarchar(256)  NULL,
    [EmailConfirmed] bit  NOT NULL,
    [PasswordHash] nvarchar(max)  NULL,
    [SecurityStamp] nvarchar(max)  NULL,
    [PhoneNumber] nvarchar(max)  NULL,
    [PhoneNumberConfirmed] bit  NOT NULL,
    [TwoFactorEnabled] bit  NOT NULL,
    [LockoutEndDateUtc] datetime  NULL,
    [LockoutEnabled] bit  NOT NULL,
    [AccessFailedCount] int  NOT NULL,
    [UserName] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'data_state'
CREATE TABLE [dbo].[data_state] (
    [stateid] int IDENTITY(1,1) NOT NULL,
    [state_abbr] nvarchar(255)  NULL,
    [state_long] nvarchar(255)  NULL,
    [state_country] nvarchar(255)  NULL
);
GO

-- Creating table 'literatures'
CREATE TABLE [dbo].[literatures] (
    [lit_ID] int IDENTITY(1,1) NOT NULL,
    [lit_name] varchar(250)  NULL,
    [created_by] bigint  NULL,
    [date_created] datetime  NULL,
    [date_updated] datetime  NULL,
    [updated_by] bigint  NULL,
    [risource] varchar(max)  NULL
);
GO

-- Creating table 'literature_requested'
CREATE TABLE [dbo].[literature_requested] (
    [rlit_ID] int IDENTITY(1,1) NOT NULL,
    [usr_ID] int  NULL,
    [date_created] datetime  NULL,
    [status] int  NULL,
    [rlit_info] varchar(max)  NULL
);
GO

-- Creating table 'nav1'
CREATE TABLE [dbo].[nav1] (
    [n1ID] int IDENTITY(1,1) NOT NULL,
    [n1order] int  NULL,
    [n1_nameShort] nvarchar(50)  NULL,
    [n1_nameLong] nvarchar(250)  NULL,
    [linkId] varchar(50)  NULL,
    [pageName] varchar(50)  NULL,
    [controller] varchar(50)  NULL,
    [n1_descShort] varchar(5000)  NULL,
    [n1_descLong] varchar(max)  NULL,
    [n1_editBy] bigint  NULL,
    [n1_editDate] datetime  NULL,
    [n1_headerImg] nvarchar(250)  NULL,
    [usr_group] varchar(max)  NULL,
    [n1_active] int  NULL,
    [default_language] int  NULL,
    [locations] varchar(max)  NULL
);
GO

-- Creating table 'nav2'
CREATE TABLE [dbo].[nav2] (
    [n2ID] bigint IDENTITY(1,1) NOT NULL,
    [n1ID] int  NULL,
    [n2order] int  NULL,
    [n2_nameShort] nvarchar(50)  NULL,
    [n2_nameLong] nvarchar(250)  NULL,
    [PageName] nvarchar(50)  NULL,
    [Controller] nvarchar(50)  NULL,
    [n2_descShort] varchar(5000)  NULL,
    [n2_descLong] varchar(max)  NULL,
    [n2_descLongAlt] varchar(max)  NULL,
    [n2_active] int  NULL,
    [n2_products] nvarchar(50)  NULL,
    [n2_usrTypes] nvarchar(50)  NULL,
    [n2_editBy] bigint  NULL,
    [n2_editDate] datetime  NULL,
    [n2_headerImg] nvarchar(250)  NULL,
    [n2_redirect] nvarchar(250)  NULL,
    [n2_redirectJS] varchar(max)  NULL,
    [n2_keywords] nvarchar(1000)  NULL,
    [old_n3id] bigint  NULL,
    [old_n2id] bigint  NULL,
    [n2_industry] nvarchar(10)  NULL,
    [n2_IT_approver] bigint  NULL,
    [n2_IE_approver] bigint  NULL,
    [usr_group] varchar(max)  NULL,
    [default_language] int  NULL
);
GO

-- Creating table 'nav3'
CREATE TABLE [dbo].[nav3] (
    [n3ID] bigint IDENTITY(1,1) NOT NULL,
    [n2ID] bigint  NULL,
    [n3order] int  NULL,
    [n3_nameShort] nvarchar(50)  NULL,
    [n3_nameLong] nvarchar(250)  NULL,
    [n3_descShort] varchar(5000)  NULL,
    [n3_descLong] varchar(max)  NULL,
    [n3_active] int  NULL,
    [n3_products] nvarchar(50)  NULL,
    [n3_usrTypes] nvarchar(50)  NULL,
    [n3_editBy] bigint  NULL,
    [n3_editDate] datetime  NULL,
    [n3_redirect] nvarchar(250)  NULL,
    [n3_keywords] nvarchar(1000)  NULL,
    [n3_industry] nvarchar(10)  NULL,
    [old_n3id] bigint  NULL,
    [old_n2id] bigint  NULL,
    [default_language] int  NULL,
    [file_name] varchar(150)  NULL
);
GO

-- Creating table 'partnerCompanies'
CREATE TABLE [dbo].[partnerCompanies] (
    [comp_ID] bigint IDENTITY(1,1) NOT NULL,
    [comp_name] nvarchar(100)  NULL,
    [comp_industry] tinyint  NULL,
    [comp_type] tinyint  NULL,
    [comp_level] tinyint  NULL,
    [comp_products] nvarchar(50)  NULL,
    [comp_SAP] tinyint  NULL,
    [comp_POS] tinyint  NULL,
    [comp_SPA] tinyint  NULL,
    [comp_project_reg] tinyint  NULL,
    [comp_MDF] tinyint  NULL,
    [comp_MDF_amount] float  NULL,
    [comp_MDF_tLimit] float  NULL,
    [comp_MDF_aLimit] float  NULL,
    [comp_MDF_mLimit] float  NULL,
    [comp_FX] tinyint  NULL,
    [comp_active] tinyint  NULL,
    [comp_dateCreated] datetime  NULL,
    [comp_dateUpdated] datetime  NULL,
    [comp_createdBy] bigint  NULL,
    [comp_updatedBy] bigint  NULL,
    [old_ID] bigint  NULL,
    [comp_RiCRM] int  NULL,
    [comp_region] nvarchar(50)  NULL,
    [comp_MDF_eLimit] float  NULL,
    [comp_MDF_dLimit] float  NULL,
    [comp_MDF_oLimit] float  NULL
);
GO

-- Creating table 'partnerIndustries'
CREATE TABLE [dbo].[partnerIndustries] (
    [pi_ID] int IDENTITY(1,1) NOT NULL,
    [pi_industry] nvarchar(20)  NULL
);
GO

-- Creating table 'partnerProducts'
CREATE TABLE [dbo].[partnerProducts] (
    [pp_ID] int IDENTITY(1,1) NOT NULL,
    [pp_product] nvarchar(20)  NULL
);
GO

-- Creating table 'partnerStockChecks'
CREATE TABLE [dbo].[partnerStockChecks] (
    [ps_ID] bigint IDENTITY(1,1) NOT NULL,
    [ps_account] bigint  NULL,
    [usr_user] int  NULL,
    [loc_id] bigint  NULL
);
GO

-- Creating table 'partnerTypes'
CREATE TABLE [dbo].[partnerTypes] (
    [pt_ID] int IDENTITY(1,1) NOT NULL,
    [pt_type] nvarchar(20)  NULL
);
GO

-- Creating table 'RiSources'
CREATE TABLE [dbo].[RiSources] (
    [ris_ID] int IDENTITY(1,1) NOT NULL,
    [n2ID] bigint  NULL,
    [n3ID] bigint  NULL,
    [ris_headline] nvarchar(250)  NULL,
    [ris_teaser] varchar(max)  NULL,
    [ris_body] varchar(max)  NULL,
    [ris_status] varchar(5)  NULL,
    [ris_keywords] nvarchar(50)  NULL,
    [ris_products] nvarchar(50)  NULL,
    [ris_industry] nvarchar(50)  NULL,
    [ris_usrTypes] nvarchar(50)  NULL,
    [ris_categories] nvarchar(50)  NULL,
    [ris_editedBy] nvarchar(50)  NULL,
    [ris_owner] varchar(50)  NULL,
    [dateCreated] datetime  NULL,
    [ris_link] varchar(max)  NULL,
    [file_size] varchar(50)  NULL,
    [file_type] varchar(10)  NULL,
    [ris_order] int  NULL,
    [ris_startDate] datetime  NULL,
    [ris_endDate] datetime  NULL,
    [displayimage] varchar(200)  NULL,
    [ris_partnerApp] nvarchar(50)  NULL
);
GO

-- Creating table 'RiSources_Action_Log'
CREATE TABLE [dbo].[RiSources_Action_Log] (
    [section_ID] int IDENTITY(1,1) NOT NULL,
    [Form_ID] int  NULL,
    [Action] nvarchar(max)  NULL,
    [Action_Time] datetime  NULL,
    [Notes] nvarchar(max)  NULL,
    [Usr_ID] nvarchar(max)  NULL
);
GO

-- Creating table 'risourcesCategories'
CREATE TABLE [dbo].[risourcesCategories] (
    [cat_id] int IDENTITY(1,1) NOT NULL,
    [ris_categories] varchar(50)  NULL
);
GO

-- Creating table 'risourcesType_image'
CREATE TABLE [dbo].[risourcesType_image] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [type_link] varchar(250)  NULL,
    [type_name] varchar(250)  NULL,
    [type_order] int  NULL
);
GO

-- Creating table 'salesComms'
CREATE TABLE [dbo].[salesComms] (
    [scID] bigint IDENTITY(1,1) NOT NULL,
    [n2ID] bigint  NULL,
    [n3ID] bigint  NULL,
    [sc_status] int  NULL,
    [sc_headline] nvarchar(250)  NULL,
    [sc_teaser] varchar(max)  NULL,
    [sc_body] varchar(max)  NULL,
    [sc_keywords] nvarchar(1000)  NULL,
    [sc_products] nvarchar(50)  NULL,
    [sc_usrTypes] nvarchar(50)  NULL,
    [sc_startDate] datetime  NULL,
    [sc_endDate] datetime  NULL,
    [sc_owner] bigint  NULL,
    [sc_industry] nvarchar(10)  NULL,
    [old_scid] bigint  NULL,
    [attach_risource] nvarchar(max)  NULL,
    [countries] varchar(250)  NULL,
    [default_lang] int  NULL,
    [languages] varchar(250)  NULL,
    [submission_date] datetime  NULL
);
GO

-- Creating table 'themes'
CREATE TABLE [dbo].[themes] (
    [theme_id] int IDENTITY(1,1) NOT NULL,
    [theme_name] varchar(50)  NULL
);
GO

-- Creating table 'usr_user'
CREATE TABLE [dbo].[usr_user] (
    [usr_ID] int IDENTITY(1,1) NOT NULL,
    [system_ID] varchar(max)  NULL,
    [usr_fName] nvarchar(50)  NULL,
    [usr_lName] nvarchar(50)  NULL,
    [usr_email] nvarchar(50)  NULL,
    [usr_password] nvarchar(20)  NULL,
    [usr_add1] nvarchar(300)  NULL,
    [usr_add2] nvarchar(300)  NULL,
    [usr_city] nvarchar(50)  NULL,
    [usr_state] int  NULL,
    [usr_zip] nvarchar(10)  NULL,
    [usr_phone] nvarchar(20)  NULL,
    [usr_fax] nvarchar(20)  NULL,
    [usr_web] nvarchar(300)  NULL,
    [usr_role] tinyint  NULL,
    [usr_SAP] tinyint  NULL,
    [usr_POS] tinyint  NULL,
    [usr_SPA] tinyint  NULL,
    [usr_project_reg] tinyint  NULL,
    [usr_MDF] tinyint  NULL,
    [usr_FX] tinyint  NULL,
    [usr_siteRole] tinyint  NULL,
    [usr_dateCreated] datetime  NULL,
    [usr_dateUpdated] datetime  NULL,
    [comp_ID] bigint  NULL,
    [comp_loc_ID] bigint  NULL,
    [wN] tinyint  NULL,
    [old_usr_id] bigint  NULL,
    [old_UserID] bigint  NULL,
    [usr_lastLogin] datetime  NULL,
    [usr_countLogin] bigint  NULL,
    [usr_title] nvarchar(50)  NULL,
    [usr_jigsaw_password] nvarchar(25)  NULL,
    [usr_status] int  NULL,
    [usr_jigsaw_login] nvarchar(50)  NULL,
    [usr_MDF_login] nvarchar(50)  NULL,
    [usr_MDF_password] nvarchar(20)  NULL,
    [usr_rfq] int  NULL,
    [usr_RiCRM] int  NULL,
    [admin_theme] int  NULL,
    [show_message] int  NULL,
    [usr_pages] nvarchar(max)  NULL,
    [usr_country] int  NULL,
    [usr_language] int  NULL,
    [usr_sales] int  NULL,
    [pp_username] varchar(200)  NULL
);
GO

-- Creating table 'countries'
CREATE TABLE [dbo].[countries] (
    [country_id] int IDENTITY(1,1) NOT NULL,
    [country_abbr] varchar(10)  NULL,
    [country_long] varchar(100)  NULL,
    [Language] varchar(max)  NULL
);
GO

-- Creating table 'Labels'
CREATE TABLE [dbo].[Labels] (
    [label_id] int IDENTITY(1,1) NOT NULL,
    [label_name] varchar(50)  NULL,
    [controller_name] varchar(50)  NULL,
    [page_name] varchar(50)  NULL,
    [language] int  NULL,
    [status] varchar(50)  NULL,
    [date] datetime  NULL,
    [translated_label] varchar(50)  NULL
);
GO

-- Creating table 'Localizations'
CREATE TABLE [dbo].[Localizations] (
    [localization_id] int IDENTITY(1,1) NOT NULL,
    [table_name] varchar(250)  NULL,
    [parent_id] int  NULL,
    [column_name] varchar(50)  NULL,
    [message_original] varchar(max)  NULL,
    [message_translated] varchar(max)  NULL,
    [language] int  NULL,
    [status] varchar(50)  NULL,
    [date] datetime  NULL
);
GO

-- Creating table 'stats_training'
CREATE TABLE [dbo].[stats_training] (
    [trid] int  NOT NULL,
    [tr_usr] bigint  NULL,
    [tr_date] datetime  NULL,
    [tr_module] varchar(254)  NULL,
    [tr_NumQuestions] nvarchar(50)  NULL,
    [tr_PassGrade] nvarchar(50)  NULL,
    [tr_score] nvarchar(50)  NULL
);
GO

-- Creating table 'ReturnToolActionLogs'
CREATE TABLE [dbo].[ReturnToolActionLogs] (
    [log_id] int IDENTITY(1,1) NOT NULL,
    [form_id] int  NULL,
    [user_id] int  NOT NULL,
    [action] nvarchar(max)  NULL,
    [action_time] datetime  NULL,
    [notes] nvarchar(max)  NULL,
    [authNumber] nvarchar(max)  NULL
);
GO

-- Creating table 'ReturnToolExtentions'
CREATE TABLE [dbo].[ReturnToolExtentions] (
    [ext_id] int IDENTITY(1,1) NOT NULL,
    [form_id] int  NULL,
    [return_type] nvarchar(max)  NULL,
    [part_num] nvarchar(max)  NULL,
    [quantity] int  NULL,
    [quote_num] nvarchar(max)  NULL,
    [return_reason] nvarchar(max)  NULL,
    [reasoncheckbox] nvarchar(max)  NULL,
    [partpo_num] nvarchar(max)  NULL,
    [po_num] nvarchar(max)  NULL
);
GO

-- Creating table 'ReturnToolFiles'
CREATE TABLE [dbo].[ReturnToolFiles] (
    [file_id] int IDENTITY(1,1) NOT NULL,
    [form_id] int  NULL,
    [return_type] nvarchar(max)  NULL,
    [file_name] nvarchar(max)  NULL,
    [identifier] nvarchar(max)  NULL,
    [offset_po] nvarchar(max)  NULL
);
GO

-- Creating table 'PartnerApplication1'
CREATE TABLE [dbo].[PartnerApplication1] (
    [appli_id] int IDENTITY(1,1) NOT NULL,
    [appli_name] nvarchar(max)  NULL,
    [order] nvarchar(max)  NULL
);
GO

-- Creating table 'Announcement_logs'
CREATE TABLE [dbo].[Announcement_logs] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [announcementID] int  NULL,
    [userID] nvarchar(max)  NULL,
    [Time_Seen] datetime  NULL
);
GO

-- Creating table 'mdf_file'
CREATE TABLE [dbo].[mdf_file] (
    [mdf_file_ID] bigint IDENTITY(1,1) NOT NULL,
    [mdf_ID] bigint  NULL,
    [mdf_file_name] nvarchar(500)  NULL,
    [mdf_file_type] tinyint  NULL
);
GO

-- Creating table 'mdf_main'
CREATE TABLE [dbo].[mdf_main] (
    [mdf_ID] bigint IDENTITY(1,1) NOT NULL,
    [mdf_user] bigint  NULL,
    [mdf_SAP] bigint  NULL,
    [mdf_comp] bigint  NULL,
    [mdf_title] nvarchar(200)  NULL,
    [mdf_desc] varchar(max)  NULL,
    [mdf_loc] bigint  NULL,
    [mdf_totalCost] float  NULL,
    [mdf_mdfCost] float  NULL,
    [mdf_date] datetime  NULL,
    [mdf_type] int  NULL,
    [mdf_status] int  NULL,
    [mdf_comments] varchar(max)  NULL,
    [mdf_comments2] varchar(max)  NULL,
    [mdf_comments3] varchar(max)  NULL,
    [mdf_requestDate] datetime  NULL,
    [mdf_reviewDate] datetime  NULL,
    [mdf_validationDate] datetime  NULL,
    [mdf_creditIssueDate] datetime  NULL,
    [mdf_approvedAmt] float  NULL,
    [mdf_validatedAmt] float  NULL,
    [mdf_creditMemoNum] nvarchar(25)  NULL,
    [mdf_accountingInstruct] varchar(max)  NULL,
    [archive_year] int  NULL
);
GO

-- Creating table 'mdf_subType'
CREATE TABLE [dbo].[mdf_subType] (
    [mdf_subType_ID] int IDENTITY(1,1) NOT NULL,
    [mdf_type_ID] int  NULL,
    [mdf_subType_name] nvarchar(200)  NULL,
    [mdf_type_desc] varchar(max)  NULL
);
GO

-- Creating table 'mdf_type'
CREATE TABLE [dbo].[mdf_type] (
    [mdf_type_ID] int IDENTITY(1,1) NOT NULL,
    [mdf_type_name] nvarchar(200)  NULL,
    [mdf_type_desc] varchar(max)  NULL
);
GO

-- Creating table 'RFQ_Data'
CREATE TABLE [dbo].[RFQ_Data] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [sales_engineer] nvarchar(max)  NULL,
    [regional_director] nvarchar(max)  NULL,
    [cell_phone] nvarchar(max)  NULL,
    [email] nvarchar(max)  NULL,
    [submission_date] datetime  NULL,
    [updated_quote] nvarchar(max)  NULL,
    [sold_to_party] nvarchar(max)  NULL,
    [qte_num] nvarchar(max)  NULL,
    [sap_account_num] nvarchar(max)  NULL,
    [location] nvarchar(max)  NULL,
    [end_contact] nvarchar(max)  NULL,
    [opportunity_num] nvarchar(max)  NULL,
    [qte_ref] nvarchar(max)  NULL,
    [qte_description] nvarchar(max)  NULL,
    [draw_num] nvarchar(max)  NULL,
    [total_qty] nvarchar(max)  NULL,
    [release_qty] nvarchar(max)  NULL,
    [competition] nvarchar(max)  NULL,
    [target_price] nvarchar(max)  NULL,
    [scale_volume] nvarchar(max)  NULL,
    [spa_contract_num] nvarchar(max)  NULL,
    [spa_mult] nvarchar(max)  NULL,
    [drawing_approval] smallint  NULL,
    [product_category] nvarchar(max)  NULL,
    [xpress_mod_data] nvarchar(max)  NULL,
    [xpress_mod_non_data] nvarchar(max)  NULL,
    [enclosure_type_it] nvarchar(max)  NULL,
    [part_num_it] nvarchar(max)  NULL,
    [size_hxwxd_it] nvarchar(max)  NULL,
    [color_it] nvarchar(max)  NULL,
    [sidewall_style_it] nvarchar(max)  NULL,
    [sidewall_location_it] nvarchar(max)  NULL,
    [castors_it] nvarchar(max)  NULL,
    [Leveling_feet_it] nvarchar(max)  NULL,
    [front_it] nvarchar(max)  NULL,
    [rear_it] nvarchar(max)  NULL,
    [cable_it] nvarchar(max)  NULL,
    [handles_it] nvarchar(max)  NULL,
    [inserts_it] nvarchar(max)  NULL,
    [partition_wall_it] nvarchar(max)  NULL,
    [baffles_it] nvarchar(max)  NULL,
    [bsaying_brackets_it] nvarchar(max)  NULL,
    [additional_info_datacenter] nvarchar(max)  NULL,
    [intell_data] nvarchar(max)  NULL,
    [voltage_data] nvarchar(max)  NULL,
    [amp_data] nvarchar(max)  NULL,
    [outlet_it] nvarchar(max)  NULL,
    [quantity_type_data] nvarchar(max)  NULL,
    [input_cord_it] nvarchar(max)  NULL,
    [expansion_it] nvarchar(max)  NULL,
    [part_num_ie] nvarchar(max)  NULL,
    [size_hxwxd_ie] nvarchar(max)  NULL,
    [material_ie] nvarchar(max)  NULL,
    [mpl_ie] nvarchar(max)  NULL,
    [sidewall_ie] nvarchar(max)  NULL,
    [front_ie] nvarchar(max)  NULL,
    [rear_ie] nvarchar(max)  NULL,
    [plinths_ie] nvarchar(max)  NULL,
    [cable_ie] nvarchar(max)  NULL,
    [handles_ie] nvarchar(max)  NULL,
    [inserts_ie] nvarchar(max)  NULL,
    [Rails] nvarchar(max)  NULL,
    [Suited] nvarchar(max)  NULL,
    [suited_bay_ie] nvarchar(max)  NULL,
    [door_ie] nvarchar(max)  NULL,
    [roof_ie] nvarchar(max)  NULL,
    [rear_wall_ie] nvarchar(max)  NULL,
    [sidewall_mod_ie] nvarchar(max)  NULL,
    [mpl_mod_ie] nvarchar(max)  NULL,
    [special_paint_ie] nvarchar(max)  NULL,
    [color_mod_ie] nvarchar(max)  NULL,
    [ul_nema_other_ie] nvarchar(max)  NULL,
    [rating_ie] nvarchar(max)  NULL,
    [part_num_WM_AE_JB] nvarchar(max)  NULL,
    [size_hxwxd_WM_AE_JB] nvarchar(max)  NULL,
    [material_WM_AE_JB] nvarchar(max)  NULL,
    [mpl_WM_AE_JB] nvarchar(max)  NULL,
    [latching_wm] nvarchar(max)  NULL,
    [body_modified_wm] nvarchar(max)  NULL,
    [door_modified_wm] nvarchar(max)  NULL,
    [mpl_modified_wm] nvarchar(max)  NULL,
    [special_paint_wm] nvarchar(max)  NULL,
    [color_WM_AE_JB] nvarchar(max)  NULL,
    [ul_nema_WM_AE_JB] nvarchar(max)  NULL,
    [rating_WM_AE_JB] nvarchar(max)  NULL,
    [part_num_other_1] nvarchar(max)  NULL,
    [size_hxwxd_other_1] nvarchar(max)  NULL,
    [producttype_other_1] nvarchar(max)  NULL,
    [material_other_1] nvarchar(max)  NULL,
    [body_modified_other_1] nvarchar(max)  NULL,
    [door_modified_other_1] nvarchar(max)  NULL,
    [mpl_modified_other_1] nvarchar(max)  NULL,
    [specialpaint_other_1] nvarchar(max)  NULL,
    [ul_nema_other_1] nvarchar(max)  NULL,
    [rating_other_1] nvarchar(max)  NULL,
    [additional_info_footer] nvarchar(max)  NULL,
    [qty_installed_1] nvarchar(max)  NULL,
    [part_number_installed_1] nvarchar(max)  NULL,
    [description_installeed_1] nvarchar(max)  NULL,
    [qty_installed_2] nvarchar(max)  NULL,
    [part_number_installed_2] nvarchar(max)  NULL,
    [description_installeed_2] nvarchar(max)  NULL,
    [qty_installed_3] nvarchar(max)  NULL,
    [part_number_installed_3] nvarchar(max)  NULL,
    [description_installeed_3] nvarchar(max)  NULL,
    [qty_shipped_1] nvarchar(max)  NULL,
    [part_number_shipped_1] nvarchar(max)  NULL,
    [description_shipped_1] nvarchar(max)  NULL,
    [qty_shipped_2] nvarchar(max)  NULL,
    [part_number_shipped_2] nvarchar(max)  NULL,
    [description_shipped_2] nvarchar(max)  NULL,
    [qty_shipped_3] nvarchar(max)  NULL,
    [part_number_shipped_3] nvarchar(max)  NULL,
    [description_shipped_3] nvarchar(max)  NULL,
    [send] nvarchar(max)  NULL,
    [fileupload] nvarchar(max)  NULL,
    [user_id] int  NULL,
    [save] nvarchar(max)  NULL,
    [distro_name] nvarchar(max)  NULL,
    [distro_company] nvarchar(max)  NULL,
    [specialpaint_ie_1] nvarchar(50)  NULL,
    [specialpaint_wm_1] nvarchar(50)  NULL,
    [specialpaint_other] nvarchar(50)  NULL,
    [color_mod_other] nvarchar(50)  NULL,
    [Quote_Num] nvarchar(max)  NULL,
    [completion_date] datetime  NULL,
    [admin_status] nvarchar(max)  NULL,
    [plinths_type_ie] nvarchar(max)  NULL
);
GO

-- Creating table 'RFQ_Data_Extend'
CREATE TABLE [dbo].[RFQ_Data_Extend] (
    [id] int IDENTITY(1,1) NOT NULL,
    [form_id] nvarchar(max)  NULL,
    [prod_id] int  NULL,
    [total_qty] nvarchar(max)  NULL,
    [release_qty] nvarchar(max)  NULL,
    [target_price] nvarchar(max)  NULL,
    [product_category] nvarchar(max)  NULL,
    [xpress_mod_data] nvarchar(max)  NULL,
    [xpress_mod_non_data] nvarchar(max)  NULL,
    [enclosure_type_it] nvarchar(max)  NULL,
    [part_num_it] nvarchar(max)  NULL,
    [size_hxwxd_it] nvarchar(max)  NULL,
    [color_it] nvarchar(max)  NULL,
    [sidewall_style_it] nvarchar(max)  NULL,
    [sidewall_location_it] nvarchar(max)  NULL,
    [castors_it] nvarchar(max)  NULL,
    [Leveling_feet_it] nvarchar(max)  NULL,
    [front_it] nvarchar(max)  NULL,
    [rear_it] nvarchar(max)  NULL,
    [cable_it] nvarchar(max)  NULL,
    [handles_it] nvarchar(max)  NULL,
    [inserts_it] nvarchar(max)  NULL,
    [partition_wall_it] nvarchar(max)  NULL,
    [baffles_it] nvarchar(max)  NULL,
    [bsaying_brackets_it] nvarchar(max)  NULL,
    [additional_info_datacenter] nvarchar(max)  NULL,
    [intell_data] nvarchar(max)  NULL,
    [voltage_data] nvarchar(max)  NULL,
    [amp_data] nvarchar(max)  NULL,
    [outlet_it] nvarchar(max)  NULL,
    [quantity_type_data] nvarchar(max)  NULL,
    [input_cord_it] nvarchar(max)  NULL,
    [expansion_it] nvarchar(max)  NULL,
    [part_num_ie] nvarchar(max)  NULL,
    [size_hxwxd_ie] nvarchar(max)  NULL,
    [material_ie] nvarchar(max)  NULL,
    [mpl_ie] nvarchar(max)  NULL,
    [sidewall_ie] nvarchar(max)  NULL,
    [front_ie] nvarchar(max)  NULL,
    [rear_ie] nvarchar(max)  NULL,
    [plinths_ie] nvarchar(max)  NULL,
    [cable_ie] nvarchar(max)  NULL,
    [handles_ie] nvarchar(max)  NULL,
    [inserts_ie] nvarchar(max)  NULL,
    [Rails] nvarchar(max)  NULL,
    [Suited] nvarchar(max)  NULL,
    [suited_bay_ie] nvarchar(max)  NULL,
    [door_ie] nvarchar(max)  NULL,
    [roof_ie] nvarchar(max)  NULL,
    [rear_wall_ie] nvarchar(max)  NULL,
    [sidewall_mod_ie] nvarchar(max)  NULL,
    [mpl_mod_ie] nvarchar(max)  NULL,
    [special_paint_ie] nvarchar(max)  NULL,
    [color_mod_ie] nvarchar(max)  NULL,
    [ul_nema_other_ie] nvarchar(max)  NULL,
    [rating_ie] nvarchar(max)  NULL,
    [part_num_WM_AE_JB] nvarchar(max)  NULL,
    [size_hxwxd_WM_AE_JB] nvarchar(max)  NULL,
    [material_WM_AE_JB] nvarchar(max)  NULL,
    [mpl_WM_AE_JB] nvarchar(max)  NULL,
    [latching_wm] nvarchar(max)  NULL,
    [body_modified_wm] nvarchar(max)  NOT NULL,
    [door_modified_wm] nvarchar(max)  NULL,
    [mpl_modified_wm] nvarchar(max)  NULL,
    [special_paint_wm] nvarchar(max)  NULL,
    [color_WM_AE_JB] nvarchar(max)  NULL,
    [ul_nema_WM_AE_JB] nvarchar(max)  NULL,
    [rating_WM_AE_JB] nvarchar(max)  NULL,
    [part_num_other_1] nvarchar(max)  NULL,
    [size_hxwxd_other_1] nvarchar(max)  NULL,
    [producttype_other_1] nvarchar(max)  NULL,
    [material_other_1] nvarchar(max)  NULL,
    [body_modified_other_1] nvarchar(max)  NULL,
    [door_modified_other_1] nvarchar(max)  NULL,
    [mpl_modified_other_1] nvarchar(max)  NULL,
    [specialpaint_other_1] nvarchar(max)  NULL,
    [ul_nema_other_1] nvarchar(max)  NULL,
    [rating_other_1] nvarchar(max)  NULL,
    [additional_info_footer] nvarchar(max)  NULL,
    [qty_installed_1] nvarchar(max)  NULL,
    [part_number_installed_1] nvarchar(max)  NULL,
    [description_installeed_1] nvarchar(max)  NULL,
    [qty_installed_2] nvarchar(max)  NULL,
    [part_number_installed_2] nvarchar(max)  NULL,
    [description_installeed_2] nvarchar(max)  NULL,
    [qty_installed_3] nvarchar(max)  NULL,
    [part_number_installed_3] nvarchar(max)  NULL,
    [description_installeed_3] nvarchar(max)  NULL,
    [qty_shipped_1] nvarchar(max)  NULL,
    [part_number_shipped_1] nvarchar(max)  NULL,
    [description_shipped_1] nvarchar(max)  NULL,
    [qty_shipped_2] nvarchar(max)  NULL,
    [part_number_shipped_2] nvarchar(max)  NULL,
    [description_shipped_2] nvarchar(max)  NULL,
    [qty_shipped_3] nvarchar(max)  NULL,
    [part_number_shipped_3] nvarchar(max)  NULL,
    [description_shipped_3] nvarchar(max)  NULL,
    [send] nvarchar(max)  NULL,
    [Image_Name] nvarchar(max)  NULL,
    [specialpaint_ie_1] nvarchar(50)  NULL,
    [specialpaint_wm_1] nvarchar(50)  NULL,
    [specialpaint_other] nvarchar(50)  NULL,
    [color_mod_other] nvarchar(50)  NULL,
    [plinths_type_ie] nvarchar(max)  NULL
);
GO

-- Creating table 'usr_user_temp'
CREATE TABLE [dbo].[usr_user_temp] (
    [usr_ID] int IDENTITY(1,1) NOT NULL,
    [system_ID] varchar(max)  NULL,
    [usr_fName] nvarchar(50)  NULL,
    [usr_lName] nvarchar(50)  NULL,
    [usr_email] nvarchar(50)  NULL,
    [usr_password] nvarchar(20)  NULL,
    [usr_add1] nvarchar(300)  NULL,
    [usr_add2] nvarchar(300)  NULL,
    [usr_city] nvarchar(50)  NULL,
    [usr_state] int  NULL,
    [usr_country] int  NULL,
    [usr_language] int  NULL,
    [usr_zip] nvarchar(10)  NULL,
    [usr_phone] nvarchar(20)  NULL,
    [usr_fax] nvarchar(20)  NULL,
    [usr_web] nvarchar(300)  NULL,
    [usr_role] tinyint  NULL,
    [usr_SAP] tinyint  NULL,
    [usr_POS] tinyint  NULL,
    [usr_SPA] tinyint  NULL,
    [usr_project_reg] tinyint  NULL,
    [usr_MDF] tinyint  NULL,
    [usr_FX] tinyint  NULL,
    [usr_siteRole] tinyint  NULL,
    [usr_dateCreated] datetime  NULL,
    [usr_dateUpdated] datetime  NULL,
    [comp_ID] bigint  NULL,
    [comp_loc_ID] bigint  NULL,
    [wN] tinyint  NULL,
    [old_usr_id] bigint  NULL,
    [old_UserID] bigint  NULL,
    [usr_lastLogin] datetime  NULL,
    [usr_countLogin] bigint  NULL,
    [usr_title] nvarchar(50)  NULL,
    [usr_jigsaw_password] nvarchar(25)  NULL,
    [usr_status] int  NULL,
    [usr_jigsaw_login] nvarchar(50)  NULL,
    [usr_MDF_login] nvarchar(50)  NULL,
    [usr_MDF_password] nvarchar(20)  NULL,
    [usr_rfq] int  NULL,
    [usr_RiCRM] int  NULL,
    [admin_theme] int  NULL,
    [show_message] int  NULL,
    [usr_pages] varchar(max)  NULL,
    [sap_numb] varchar(50)  NULL,
    [pp_username] varchar(200)  NULL
);
GO

-- Creating table 'RFQ_Files'
CREATE TABLE [dbo].[RFQ_Files] (
    [file_id] int IDENTITY(1,1) NOT NULL,
    [form_id] int  NULL,
    [ext_form_id] int  NULL,
    [user_id] int  NULL,
    [file_name] nvarchar(max)  NULL,
    [file_type] nvarchar(max)  NULL
);
GO

-- Creating table 'SiteActionLogModels'
CREATE TABLE [dbo].[SiteActionLogModels] (
    [log_id] int IDENTITY(1,1) NOT NULL,
    [feature_page] varchar(250)  NULL,
    [form_id] bigint  NULL,
    [action] varchar(50)  NULL,
    [action_Time] datetime  NULL,
    [notes] varchar(max)  NULL,
    [usr_id] bigint  NULL
);
GO

-- Creating table 'SiteApprovers'
CREATE TABLE [dbo].[SiteApprovers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(50)  NULL,
    [Email] nvarchar(50)  NULL,
    [CountryId] int  NOT NULL,
    [CompType] int  NOT NULL
);
GO

-- Creating table 'ReturnTools'
CREATE TABLE [dbo].[ReturnTools] (
    [form_id] int IDENTITY(1,1) NOT NULL,
    [user_id] int  NULL,
    [admin_id] int  NULL,
    [po_num] nvarchar(max)  NULL,
    [sap_num] nvarchar(max)  NULL,
    [request_date] datetime  NULL,
    [submission_date] datetime  NULL,
    [completion_date] datetime  NULL,
    [status] nvarchar(max)  NULL,
    [location] nvarchar(max)  NULL,
    [admin_notes] nvarchar(max)  NULL,
    [return_type] nvarchar(max)  NULL,
    [warranty] nvarchar(50)  NULL
);
GO

-- Creating table 'RFQ_Parts_Installed'
CREATE TABLE [dbo].[RFQ_Parts_Installed] (
    [id] int IDENTITY(1,1) NOT NULL,
    [form_id] int  NULL,
    [ext_form_id] int  NULL,
    [user_id] int  NULL,
    [qty_installed] nvarchar(max)  NULL,
    [part_number_installed] nvarchar(max)  NULL,
    [description_installed] varchar(max)  NULL
);
GO

-- Creating table 'RFQ_Parts_Shipped'
CREATE TABLE [dbo].[RFQ_Parts_Shipped] (
    [id] int IDENTITY(1,1) NOT NULL,
    [form_id] int  NULL,
    [ext_form_id] int  NULL,
    [user_id] int  NULL,
    [qty_shipped] nvarchar(max)  NULL,
    [part_number_shipped] nvarchar(max)  NULL,
    [description_shipped] varchar(max)  NULL
);
GO

-- Creating table 'RFQ_Action_Log'
CREATE TABLE [dbo].[RFQ_Action_Log] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Form_ID] int  NULL,
    [Action] nvarchar(max)  NULL,
    [Action_Time] datetime  NULL,
    [Notes] nvarchar(max)  NULL,
    [Usr_ID] nvarchar(max)  NULL,
    [Admin_ID] nvarchar(max)  NULL
);
GO

-- Creating table 'usr_user_test'
CREATE TABLE [dbo].[usr_user_test] (
    [usr_ID] int IDENTITY(1,1) NOT NULL,
    [system_ID] varchar(max)  NULL,
    [usr_fName] nvarchar(50)  NULL,
    [usr_lName] nvarchar(50)  NULL,
    [usr_email] nvarchar(50)  NULL,
    [usr_password] nvarchar(20)  NULL,
    [usr_add1] nvarchar(300)  NULL,
    [usr_add2] nvarchar(300)  NULL,
    [usr_city] nvarchar(50)  NULL,
    [usr_state] int  NULL,
    [usr_country] int  NULL,
    [usr_language] int  NULL,
    [usr_zip] nvarchar(10)  NULL,
    [usr_phone] nvarchar(20)  NULL,
    [usr_fax] nvarchar(20)  NULL,
    [usr_web] nvarchar(300)  NULL,
    [usr_role] tinyint  NULL,
    [usr_SAP] tinyint  NULL,
    [usr_POS] tinyint  NULL,
    [usr_SPA] tinyint  NULL,
    [usr_project_reg] tinyint  NULL,
    [usr_MDF] tinyint  NULL,
    [usr_FX] tinyint  NULL,
    [usr_siteRole] tinyint  NULL,
    [usr_dateCreated] datetime  NULL,
    [usr_dateUpdated] datetime  NULL,
    [comp_ID] bigint  NULL,
    [comp_loc_ID] bigint  NULL,
    [wN] tinyint  NULL,
    [old_usr_id] bigint  NULL,
    [old_UserID] bigint  NULL,
    [usr_lastLogin] datetime  NULL,
    [usr_countLogin] bigint  NULL,
    [usr_title] nvarchar(50)  NULL,
    [usr_jigsaw_password] nvarchar(25)  NULL,
    [usr_status] int  NULL,
    [usr_jigsaw_login] nvarchar(50)  NULL,
    [usr_MDF_login] nvarchar(50)  NULL,
    [usr_MDF_password] nvarchar(20)  NULL,
    [usr_rfq] int  NULL,
    [usr_RiCRM] int  NULL,
    [admin_theme] int  NULL,
    [show_message] int  NULL,
    [usr_pages] varchar(max)  NULL
);
GO

-- Creating table 'partnerCompany_Archive'
CREATE TABLE [dbo].[partnerCompany_Archive] (
    [table_ID] bigint IDENTITY(1,1) NOT NULL,
    [comp_ID] bigint  NULL,
    [comp_name] nvarchar(100)  NULL,
    [comp_industry] tinyint  NULL,
    [comp_type] tinyint  NULL,
    [comp_level] tinyint  NULL,
    [comp_products] nvarchar(50)  NULL,
    [comp_SAP] tinyint  NULL,
    [comp_POS] tinyint  NULL,
    [comp_SPA] tinyint  NULL,
    [comp_project_reg] tinyint  NULL,
    [comp_MDF] tinyint  NULL,
    [comp_MDF_amount] float  NULL,
    [comp_MDF_tLimit] float  NULL,
    [comp_MDF_aLimit] float  NULL,
    [comp_MDF_eLimit] float  NULL,
    [comp_MDF_dLimit] float  NULL,
    [comp_MDF_mLimit] float  NULL,
    [comp_MDF_oLimit] float  NULL,
    [comp_FX] tinyint  NULL,
    [comp_active] tinyint  NULL,
    [comp_dateCreated] datetime  NULL,
    [comp_dateUpdated] datetime  NULL,
    [comp_createdBy] bigint  NULL,
    [comp_updatedBy] bigint  NULL,
    [old_ID] bigint  NULL,
    [comp_RiCRM] int  NULL,
    [comp_region] nvarchar(50)  NULL,
    [archive_year] nvarchar(50)  NULL
);
GO

-- Creating table 'salesRequests'
CREATE TABLE [dbo].[salesRequests] (
    [FormID] int IDENTITY(1,1) NOT NULL,
    [SalesRepID] bigint  NULL,
    [paymentMethod] varchar(50)  NULL,
    [department] varchar(50)  NULL,
    [requestType] varchar(100)  NULL,
    [supplier] varchar(50)  NULL,
    [shipTo] varchar(50)  NULL,
    [supplierNumber] varchar(50)  NULL,
    [shiptoAttn] varchar(50)  NULL,
    [partNumberDescription] varchar(max)  NULL,
    [estimatedTotalCost] varchar(50)  NULL,
    [status] varchar(50)  NULL,
    [dateCreated] datetime  NULL,
    [dateUpdated] datetime  NULL,
    [region] nvarchar(max)  NULL,
    [ponumber] nvarchar(max)  NULL,
    [compID] int  NULL,
    [dateCompleted] datetime  NULL,
    [title] nvarchar(max)  NULL,
    [description] nvarchar(max)  NULL,
    [estimatedCost] nvarchar(max)  NULL,
    [activitydate] datetime  NULL,
    [InvoiceAmountIsEqual] int  NOT NULL
);
GO

-- Creating table 'salesRequest_Action_Log'
CREATE TABLE [dbo].[salesRequest_Action_Log] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Form_ID] int  NOT NULL,
    [action] nvarchar(max)  NULL,
    [action_Time] datetime  NULL,
    [notes] nvarchar(max)  NULL,
    [usr_ID] nvarchar(max)  NULL
);
GO

-- Creating table 'salesRequest_Additional_Info'
CREATE TABLE [dbo].[salesRequest_Additional_Info] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Form_ID] int  NOT NULL,
    [quantity] varchar(50)  NULL,
    [achType] varchar(50)  NULL,
    [partNumberOrdescription] varchar(50)  NULL,
    [unitPrice] varchar(50)  NULL,
    [totalPrice] varchar(50)  NULL,
    [cccType] varchar(50)  NULL,
    [deliveryDate] varchar(50)  NULL
);
GO

-- Creating table 'salesRequest_Approvers'
CREATE TABLE [dbo].[salesRequest_Approvers] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [UserID] int  NULL,
    [regionName] varchar(50)  NULL,
    [status] int  NULL,
    [Department] nvarchar(max)  NULL
);
GO

-- Creating table 'salesRequest_File'
CREATE TABLE [dbo].[salesRequest_File] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [FormID] int  NOT NULL,
    [fileName] nvarchar(max)  NULL,
    [AttachmentType] int  NOT NULL
);
GO

-- Creating table 'CountLoggedins'
CREATE TABLE [dbo].[CountLoggedins] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [SystemID] varchar(50)  NULL
);
GO

-- Creating table 'email_tracker'
CREATE TABLE [dbo].[email_tracker] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [usr_ID] int  NULL,
    [email_type] varchar(50)  NULL,
    [msg_action] varchar(50)  NULL,
    [url_tracked] varchar(max)  NULL,
    [date_sent] datetime  NULL,
    [date_opened] datetime  NULL
);
GO

-- Creating table 'SPAs'
CREATE TABLE [dbo].[SPAs] (
    [Spa_id] int IDENTITY(1,1) NOT NULL,
    [Usr_id] int  NULL,
    [Comp_id] int  NULL,
    [Customer_name] nvarchar(max)  NULL,
    [Customer_address1] nvarchar(max)  NULL,
    [Customer_address2] nvarchar(max)  NULL,
    [Customer_city] nvarchar(max)  NULL,
    [Customer_state] nvarchar(max)  NULL,
    [Customer_zip] nvarchar(max)  NULL,
    [Customer_phone] nvarchar(max)  NULL,
    [Customer_email] nvarchar(max)  NULL,
    [Customer_title] nvarchar(max)  NULL,
    [Start_date] datetime  NULL,
    [End_date] datetime  NULL,
    [Updated_date] datetime  NULL,
    [Approved_date] datetime  NULL,
    [Updated_by] int  NULL,
    [Projected_sales] nvarchar(max)  NULL,
    [Competition] nvarchar(max)  NULL,
    [Territory_code] nvarchar(max)  NULL,
    [Activity_status] nvarchar(max)  NULL,
    [Status] nvarchar(max)  NULL,
    [Contract_type] nvarchar(max)  NULL,
    [Market_segement] nvarchar(max)  NULL,
    [Account_type] nvarchar(max)  NULL,
    [Contract_id] nvarchar(50)  NULL,
    [Customer_company_name] nvarchar(max)  NULL,
    [Additional_information] nvarchar(max)  NULL,
    [Distributor_location] nvarchar(max)  NULL,
    [Sales_rep_user] nvarchar(max)  NULL
);
GO

-- Creating table 'SPAItems'
CREATE TABLE [dbo].[SPAItems] (
    [Item_id] int IDENTITY(1,1) NOT NULL,
    [Product_name] nvarchar(max)  NULL,
    [Form_id] int  NOT NULL,
    [Quantity] nvarchar(max)  NULL,
    [Sku] nvarchar(max)  NULL,
    [Item_Status] int  NOT NULL,
    [Updated_by] int  NOT NULL,
    [Date_Created] datetime  NULL,
    [Date_Updated] datetime  NULL,
    [Requested_price] nvarchar(max)  NULL,
    [Requested_multiplier] nvarchar(max)  NULL,
    [Target_price] nvarchar(max)  NULL
);
GO

-- Creating table 'SPASkus'
CREATE TABLE [dbo].[SPASkus] (
    [Sku_id] int IDENTITY(1,1) NOT NULL,
    [Sku_code] nvarchar(max)  NULL,
    [Description] nvarchar(max)  NULL,
    [ListPrice] real  NULL,
    [IntoStockPrice] real  NULL,
    [Cost] real  NULL,
    [IntoStockMultiplier] real  NULL
);
GO

-- Creating table 'SPA_FIles'
CREATE TABLE [dbo].[SPA_FIles] (
    [File_id] int IDENTITY(1,1) NOT NULL,
    [Form_id] int  NULL,
    [File_name] nvarchar(max)  NULL,
    [File_ext] nvarchar(max)  NULL
);
GO

-- Creating table 'Large_enclosure_pricing'
CREATE TABLE [dbo].[Large_enclosure_pricing] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Doors] varchar(50)  NULL,
    [Part_Number] varchar(50)  NULL,
    [Description] varchar(50)  NULL,
    [Price] varchar(50)  NULL,
    [Part_Of_Selector] varchar(50)  NULL,
    [Type] varchar(50)  NULL,
    [Width] varchar(50)  NULL,
    [Height] varchar(50)  NULL,
    [Depth] varchar(50)  NULL
);
GO

-- Creating table 'Large_enclosure_myaccessories'
CREATE TABLE [dbo].[Large_enclosure_myaccessories] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(max)  NULL,
    [Code] nvarchar(max)  NULL,
    [Config_name] nvarchar(max)  NULL,
    [Part_Number] nvarchar(max)  NULL,
    [Description] nvarchar(max)  NULL,
    [Quantity_Per_Pack] nvarchar(max)  NULL,
    [Number_of_Packs] nvarchar(max)  NULL,
    [Unit_Cost] nvarchar(max)  NULL,
    [Total_Cost] nvarchar(max)  NULL,
    [Accessory_Number] nvarchar(max)  NULL,
    [Baying_NotBaying] nvarchar(max)  NULL,
    [Type] nvarchar(max)  NULL,
    [Doors] nvarchar(max)  NULL,
    [Height] nvarchar(max)  NULL
);
GO

-- Creating table 'Large_enclosure_myconfig'
CREATE TABLE [dbo].[Large_enclosure_myconfig] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Config_name] nvarchar(max)  NULL,
    [Unique_id] nvarchar(max)  NULL,
    [Date_created] datetime  NOT NULL,
    [materials] nvarchar(max)  NULL,
    [Username] nvarchar(max)  NULL
);
GO

-- Creating table 'Large_enclosure_ts8'
CREATE TABLE [dbo].[Large_enclosure_ts8] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TS8_partnumber] varchar(50)  NULL,
    [Accessory_partnumber] varchar(50)  NULL,
    [Height] varchar(50)  NULL,
    [Width] varchar(50)  NULL,
    [Depth] varchar(50)  NULL,
    [Doors] varchar(50)  NULL,
    [Allocation_1] varchar(max)  NULL,
    [Allocation_1_1] varchar(50)  NULL,
    [Quantity_Per_Pack] varchar(50)  NULL,
    [Number_of_Packs] varchar(50)  NULL,
    [Description] varchar(max)  NULL
);
GO

-- Creating table 'Large_enclosure_fmd'
CREATE TABLE [dbo].[Large_enclosure_fmd] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [FMD_partnumber] varchar(50)  NULL,
    [Accessory_partnumber] varchar(50)  NULL,
    [Width] varchar(50)  NULL,
    [Height] varchar(50)  NULL,
    [Depth] varchar(50)  NULL,
    [Doors] varchar(50)  NULL,
    [Allocation_1] varchar(max)  NULL,
    [Allocation_1_1] varchar(50)  NULL,
    [Quantity_Per_Pack] varchar(50)  NULL,
    [Number_of_Packs] varchar(50)  NULL,
    [Description] varchar(max)  NULL
);
GO

-- Creating table 'SPA_Account_Manager'
CREATE TABLE [dbo].[SPA_Account_Manager] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [contact_name] varchar(50)  NULL,
    [contact_type] varchar(50)  NULL,
    [title] varchar(50)  NULL,
    [zip] varchar(50)  NULL,
    [email] varchar(50)  NULL,
    [territory_code] varchar(50)  NULL
);
GO

-- Creating table 'SPA_Territory_Codes'
CREATE TABLE [dbo].[SPA_Territory_Codes] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [zip_region] varchar(50)  NULL,
    [zip] varchar(50)  NULL,
    [city] varchar(50)  NULL,
    [state] varchar(50)  NULL,
    [county] varchar(50)  NULL,
    [zip_type] varchar(50)  NULL,
    [ie_sales_office] varchar(50)  NULL,
    [ie_manager] varchar(50)  NULL,
    [territory_code] varchar(50)  NULL
);
GO

-- Creating table 'SPA_SalesReps'
CREATE TABLE [dbo].[SPA_SalesReps] (
    [Rep_id] int IDENTITY(1,1) NOT NULL,
    [Usr_id] varchar(50)  NULL,
    [Form_id] int  NULL
);
GO

-- Creating table 'SPA_Material_Master'
CREATE TABLE [dbo].[SPA_Material_Master] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [material] varchar(50)  NOT NULL,
    [material_description] varchar(50)  NULL,
    [mpg] varchar(50)  NULL,
    [mpg_description] varchar(50)  NULL,
    [cost] nvarchar(max)  NULL,
    [list_price] nvarchar(max)  NULL
);
GO

-- Creating table 'SPA_Intostock_Multiplier'
CREATE TABLE [dbo].[SPA_Intostock_Multiplier] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [GG] varchar(50)  NULL,
    [PG] varchar(50)  NULL,
    [CC] varchar(50)  NULL,
    [AA] varchar(50)  NULL
);
GO

-- Creating table 'SPA_Notes'
CREATE TABLE [dbo].[SPA_Notes] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Note_Type] bit  NOT NULL,
    [Form_ID] int  NOT NULL,
    [Action] nvarchar(max)  NULL,
    [Action_Time] datetime  NOT NULL,
    [Note] nvarchar(max)  NULL,
    [User_ID] int  NOT NULL
);
GO

-- Creating table 'Announcements'
CREATE TABLE [dbo].[Announcements] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [message] nvarchar(max)  NULL,
    [pages] nvarchar(max)  NULL,
    [adminID] nvarchar(max)  NULL,
    [status] nvarchar(max)  NULL,
    [startDate] datetime  NULL,
    [endDate] datetime  NULL
);
GO

-- Creating table 'Webshop_connect'
CREATE TABLE [dbo].[Webshop_connect] (
    [ws_ID] int IDENTITY(1,1) NOT NULL,
    [ws_account] nvarchar(max)  NOT NULL,
    [usr_user] int  NOT NULL,
    [loc_id] bigint  NOT NULL
);
GO

-- Creating table 'partnerLocations'
CREATE TABLE [dbo].[partnerLocations] (
    [loc_ID] bigint IDENTITY(1,1) NOT NULL,
    [comp_ID] bigint  NULL,
    [loc_name] varchar(100)  NULL,
    [loc_add1] varchar(300)  NULL,
    [loc_add2] varchar(300)  NULL,
    [loc_city] varchar(50)  NULL,
    [loc_state] varchar(10)  NULL,
    [loc_zip] varchar(10)  NULL,
    [loc_phone] varchar(200)  NULL,
    [loc_fax] varchar(20)  NULL,
    [loc_web] varchar(200)  NULL,
    [loc_email] varchar(100)  NULL,
    [loc_logo] varchar(200)  NULL,
    [loc_lat] float  NULL,
    [loc_lon] float  NULL,
    [loc_dealor_status] tinyint  NULL,
    [loc_show_address] tinyint  NULL,
    [loc_SAP_account] bigint  NULL,
    [loc_SAP_password] nvarchar(20)  NULL,
    [loc_dateCreated] datetime  NULL,
    [loc_dateUpdated] datetime  NULL,
    [loc_createdBy] bigint  NULL,
    [loc_updatedBy] bigint  NULL,
    [old_locID] bigint  NULL,
    [price_group] nvarchar(max)  NULL,
    [loc_Webshop_account] nvarchar(max)  NULL,
    [loc_Webshop_password] nvarchar(max)  NULL
);
GO

-- Creating table 'SPA_Rebates'
CREATE TABLE [dbo].[SPA_Rebates] (
    [rebate_ID] int IDENTITY(1,1) NOT NULL,
    [contract_ID] int  NULL,
    [rebate_total_amount] nvarchar(max)  NULL,
    [status] nvarchar(max)  NULL,
    [submit_date] datetime  NULL,
    [memo_date] datetime  NULL,
    [credit_memo] nvarchar(max)  NULL
);
GO

-- Creating table 'RiSourcesCarts'
CREATE TABLE [dbo].[RiSourcesCarts] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ris_ID] int  NULL,
    [user_ID] int  NULL
);
GO

-- Creating table 'SPA_RebatesItems'
CREATE TABLE [dbo].[SPA_RebatesItems] (
    [rebateItem_ID] int IDENTITY(1,1) NOT NULL,
    [rebate_ID] int  NULL,
    [contract_ID] int  NULL,
    [sku] nvarchar(max)  NULL,
    [last_price] float  NULL,
    [spa_price] float  NULL,
    [quantity_rebated] int  NULL,
    [quantity_requested] int  NULL,
    [capping_status] nvarchar(max)  NULL,
    [current_rebate] nvarchar(max)  NULL,
    [distributor_requests] nvarchar(max)  NULL,
    [original_difference] nvarchar(max)  NULL,
    [invoice_date] datetime  NULL,
    [rebate_amount] nvarchar(max)  NULL,
    [reason] nvarchar(max)  NULL,
    [status] nvarchar(max)  NULL,
    [customer_invoice_number] nvarchar(max)  NULL,
    [rittal_invoice_number] nvarchar(max)  NULL
);
GO

-- Creating table 'RiSourceCarts'
CREATE TABLE [dbo].[RiSourceCarts] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ris_ID] int  NOT NULL,
    [user_id] int  NOT NULL
);
GO

-- Creating table 'AspNetUserRoles'
CREATE TABLE [dbo].[AspNetUserRoles] (
    [AspNetRoles_Id] nvarchar(128)  NOT NULL,
    [AspNetUsers_Id] nvarchar(128)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [MigrationId], [ContextKey] in table 'C__MigrationHistory'
ALTER TABLE [dbo].[C__MigrationHistory]
ADD CONSTRAINT [PK_C__MigrationHistory]
    PRIMARY KEY CLUSTERED ([MigrationId], [ContextKey] ASC);
GO

-- Creating primary key on [ap_ID] in table 'adminPerms'
ALTER TABLE [dbo].[adminPerms]
ADD CONSTRAINT [PK_adminPerms]
    PRIMARY KEY CLUSTERED ([ap_ID] ASC);
GO

-- Creating primary key on [Id] in table 'AspNetRoles'
ALTER TABLE [dbo].[AspNetRoles]
ADD CONSTRAINT [PK_AspNetRoles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'AspNetUserClaims'
ALTER TABLE [dbo].[AspNetUserClaims]
ADD CONSTRAINT [PK_AspNetUserClaims]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [LoginProvider], [ProviderKey], [UserId] in table 'AspNetUserLogins'
ALTER TABLE [dbo].[AspNetUserLogins]
ADD CONSTRAINT [PK_AspNetUserLogins]
    PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey], [UserId] ASC);
GO

-- Creating primary key on [Id] in table 'AspNetUsers'
ALTER TABLE [dbo].[AspNetUsers]
ADD CONSTRAINT [PK_AspNetUsers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [stateid] in table 'data_state'
ALTER TABLE [dbo].[data_state]
ADD CONSTRAINT [PK_data_state]
    PRIMARY KEY CLUSTERED ([stateid] ASC);
GO

-- Creating primary key on [lit_ID] in table 'literatures'
ALTER TABLE [dbo].[literatures]
ADD CONSTRAINT [PK_literatures]
    PRIMARY KEY CLUSTERED ([lit_ID] ASC);
GO

-- Creating primary key on [rlit_ID] in table 'literature_requested'
ALTER TABLE [dbo].[literature_requested]
ADD CONSTRAINT [PK_literature_requested]
    PRIMARY KEY CLUSTERED ([rlit_ID] ASC);
GO

-- Creating primary key on [n1ID] in table 'nav1'
ALTER TABLE [dbo].[nav1]
ADD CONSTRAINT [PK_nav1]
    PRIMARY KEY CLUSTERED ([n1ID] ASC);
GO

-- Creating primary key on [n2ID] in table 'nav2'
ALTER TABLE [dbo].[nav2]
ADD CONSTRAINT [PK_nav2]
    PRIMARY KEY CLUSTERED ([n2ID] ASC);
GO

-- Creating primary key on [n3ID] in table 'nav3'
ALTER TABLE [dbo].[nav3]
ADD CONSTRAINT [PK_nav3]
    PRIMARY KEY CLUSTERED ([n3ID] ASC);
GO

-- Creating primary key on [comp_ID] in table 'partnerCompanies'
ALTER TABLE [dbo].[partnerCompanies]
ADD CONSTRAINT [PK_partnerCompanies]
    PRIMARY KEY CLUSTERED ([comp_ID] ASC);
GO

-- Creating primary key on [pi_ID] in table 'partnerIndustries'
ALTER TABLE [dbo].[partnerIndustries]
ADD CONSTRAINT [PK_partnerIndustries]
    PRIMARY KEY CLUSTERED ([pi_ID] ASC);
GO

-- Creating primary key on [pp_ID] in table 'partnerProducts'
ALTER TABLE [dbo].[partnerProducts]
ADD CONSTRAINT [PK_partnerProducts]
    PRIMARY KEY CLUSTERED ([pp_ID] ASC);
GO

-- Creating primary key on [ps_ID] in table 'partnerStockChecks'
ALTER TABLE [dbo].[partnerStockChecks]
ADD CONSTRAINT [PK_partnerStockChecks]
    PRIMARY KEY CLUSTERED ([ps_ID] ASC);
GO

-- Creating primary key on [pt_ID] in table 'partnerTypes'
ALTER TABLE [dbo].[partnerTypes]
ADD CONSTRAINT [PK_partnerTypes]
    PRIMARY KEY CLUSTERED ([pt_ID] ASC);
GO

-- Creating primary key on [ris_ID] in table 'RiSources'
ALTER TABLE [dbo].[RiSources]
ADD CONSTRAINT [PK_RiSources]
    PRIMARY KEY CLUSTERED ([ris_ID] ASC);
GO

-- Creating primary key on [section_ID] in table 'RiSources_Action_Log'
ALTER TABLE [dbo].[RiSources_Action_Log]
ADD CONSTRAINT [PK_RiSources_Action_Log]
    PRIMARY KEY CLUSTERED ([section_ID] ASC);
GO

-- Creating primary key on [cat_id] in table 'risourcesCategories'
ALTER TABLE [dbo].[risourcesCategories]
ADD CONSTRAINT [PK_risourcesCategories]
    PRIMARY KEY CLUSTERED ([cat_id] ASC);
GO

-- Creating primary key on [ID] in table 'risourcesType_image'
ALTER TABLE [dbo].[risourcesType_image]
ADD CONSTRAINT [PK_risourcesType_image]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [scID] in table 'salesComms'
ALTER TABLE [dbo].[salesComms]
ADD CONSTRAINT [PK_salesComms]
    PRIMARY KEY CLUSTERED ([scID] ASC);
GO

-- Creating primary key on [theme_id] in table 'themes'
ALTER TABLE [dbo].[themes]
ADD CONSTRAINT [PK_themes]
    PRIMARY KEY CLUSTERED ([theme_id] ASC);
GO

-- Creating primary key on [usr_ID] in table 'usr_user'
ALTER TABLE [dbo].[usr_user]
ADD CONSTRAINT [PK_usr_user]
    PRIMARY KEY CLUSTERED ([usr_ID] ASC);
GO

-- Creating primary key on [country_id] in table 'countries'
ALTER TABLE [dbo].[countries]
ADD CONSTRAINT [PK_countries]
    PRIMARY KEY CLUSTERED ([country_id] ASC);
GO

-- Creating primary key on [label_id] in table 'Labels'
ALTER TABLE [dbo].[Labels]
ADD CONSTRAINT [PK_Labels]
    PRIMARY KEY CLUSTERED ([label_id] ASC);
GO

-- Creating primary key on [localization_id] in table 'Localizations'
ALTER TABLE [dbo].[Localizations]
ADD CONSTRAINT [PK_Localizations]
    PRIMARY KEY CLUSTERED ([localization_id] ASC);
GO

-- Creating primary key on [trid] in table 'stats_training'
ALTER TABLE [dbo].[stats_training]
ADD CONSTRAINT [PK_stats_training]
    PRIMARY KEY CLUSTERED ([trid] ASC);
GO

-- Creating primary key on [log_id] in table 'ReturnToolActionLogs'
ALTER TABLE [dbo].[ReturnToolActionLogs]
ADD CONSTRAINT [PK_ReturnToolActionLogs]
    PRIMARY KEY CLUSTERED ([log_id] ASC);
GO

-- Creating primary key on [ext_id] in table 'ReturnToolExtentions'
ALTER TABLE [dbo].[ReturnToolExtentions]
ADD CONSTRAINT [PK_ReturnToolExtentions]
    PRIMARY KEY CLUSTERED ([ext_id] ASC);
GO

-- Creating primary key on [file_id] in table 'ReturnToolFiles'
ALTER TABLE [dbo].[ReturnToolFiles]
ADD CONSTRAINT [PK_ReturnToolFiles]
    PRIMARY KEY CLUSTERED ([file_id] ASC);
GO

-- Creating primary key on [appli_id] in table 'PartnerApplication1'
ALTER TABLE [dbo].[PartnerApplication1]
ADD CONSTRAINT [PK_PartnerApplication1]
    PRIMARY KEY CLUSTERED ([appli_id] ASC);
GO

-- Creating primary key on [ID] in table 'Announcement_logs'
ALTER TABLE [dbo].[Announcement_logs]
ADD CONSTRAINT [PK_Announcement_logs]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [mdf_file_ID] in table 'mdf_file'
ALTER TABLE [dbo].[mdf_file]
ADD CONSTRAINT [PK_mdf_file]
    PRIMARY KEY CLUSTERED ([mdf_file_ID] ASC);
GO

-- Creating primary key on [mdf_ID] in table 'mdf_main'
ALTER TABLE [dbo].[mdf_main]
ADD CONSTRAINT [PK_mdf_main]
    PRIMARY KEY CLUSTERED ([mdf_ID] ASC);
GO

-- Creating primary key on [mdf_subType_ID] in table 'mdf_subType'
ALTER TABLE [dbo].[mdf_subType]
ADD CONSTRAINT [PK_mdf_subType]
    PRIMARY KEY CLUSTERED ([mdf_subType_ID] ASC);
GO

-- Creating primary key on [mdf_type_ID] in table 'mdf_type'
ALTER TABLE [dbo].[mdf_type]
ADD CONSTRAINT [PK_mdf_type]
    PRIMARY KEY CLUSTERED ([mdf_type_ID] ASC);
GO

-- Creating primary key on [ID] in table 'RFQ_Data'
ALTER TABLE [dbo].[RFQ_Data]
ADD CONSTRAINT [PK_RFQ_Data]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [id] in table 'RFQ_Data_Extend'
ALTER TABLE [dbo].[RFQ_Data_Extend]
ADD CONSTRAINT [PK_RFQ_Data_Extend]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [usr_ID] in table 'usr_user_temp'
ALTER TABLE [dbo].[usr_user_temp]
ADD CONSTRAINT [PK_usr_user_temp]
    PRIMARY KEY CLUSTERED ([usr_ID] ASC);
GO

-- Creating primary key on [file_id] in table 'RFQ_Files'
ALTER TABLE [dbo].[RFQ_Files]
ADD CONSTRAINT [PK_RFQ_Files]
    PRIMARY KEY CLUSTERED ([file_id] ASC);
GO

-- Creating primary key on [log_id] in table 'SiteActionLogModels'
ALTER TABLE [dbo].[SiteActionLogModels]
ADD CONSTRAINT [PK_SiteActionLogModels]
    PRIMARY KEY CLUSTERED ([log_id] ASC);
GO

-- Creating primary key on [Id] in table 'SiteApprovers'
ALTER TABLE [dbo].[SiteApprovers]
ADD CONSTRAINT [PK_SiteApprovers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [form_id] in table 'ReturnTools'
ALTER TABLE [dbo].[ReturnTools]
ADD CONSTRAINT [PK_ReturnTools]
    PRIMARY KEY CLUSTERED ([form_id] ASC);
GO

-- Creating primary key on [id] in table 'RFQ_Parts_Installed'
ALTER TABLE [dbo].[RFQ_Parts_Installed]
ADD CONSTRAINT [PK_RFQ_Parts_Installed]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'RFQ_Parts_Shipped'
ALTER TABLE [dbo].[RFQ_Parts_Shipped]
ADD CONSTRAINT [PK_RFQ_Parts_Shipped]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [ID] in table 'RFQ_Action_Log'
ALTER TABLE [dbo].[RFQ_Action_Log]
ADD CONSTRAINT [PK_RFQ_Action_Log]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [usr_ID] in table 'usr_user_test'
ALTER TABLE [dbo].[usr_user_test]
ADD CONSTRAINT [PK_usr_user_test]
    PRIMARY KEY CLUSTERED ([usr_ID] ASC);
GO

-- Creating primary key on [table_ID] in table 'partnerCompany_Archive'
ALTER TABLE [dbo].[partnerCompany_Archive]
ADD CONSTRAINT [PK_partnerCompany_Archive]
    PRIMARY KEY CLUSTERED ([table_ID] ASC);
GO

-- Creating primary key on [FormID] in table 'salesRequests'
ALTER TABLE [dbo].[salesRequests]
ADD CONSTRAINT [PK_salesRequests]
    PRIMARY KEY CLUSTERED ([FormID] ASC);
GO

-- Creating primary key on [ID] in table 'salesRequest_Action_Log'
ALTER TABLE [dbo].[salesRequest_Action_Log]
ADD CONSTRAINT [PK_salesRequest_Action_Log]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'salesRequest_Additional_Info'
ALTER TABLE [dbo].[salesRequest_Additional_Info]
ADD CONSTRAINT [PK_salesRequest_Additional_Info]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'salesRequest_Approvers'
ALTER TABLE [dbo].[salesRequest_Approvers]
ADD CONSTRAINT [PK_salesRequest_Approvers]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'salesRequest_File'
ALTER TABLE [dbo].[salesRequest_File]
ADD CONSTRAINT [PK_salesRequest_File]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'CountLoggedins'
ALTER TABLE [dbo].[CountLoggedins]
ADD CONSTRAINT [PK_CountLoggedins]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'email_tracker'
ALTER TABLE [dbo].[email_tracker]
ADD CONSTRAINT [PK_email_tracker]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Spa_id] in table 'SPAs'
ALTER TABLE [dbo].[SPAs]
ADD CONSTRAINT [PK_SPAs]
    PRIMARY KEY CLUSTERED ([Spa_id] ASC);
GO

-- Creating primary key on [Item_id] in table 'SPAItems'
ALTER TABLE [dbo].[SPAItems]
ADD CONSTRAINT [PK_SPAItems]
    PRIMARY KEY CLUSTERED ([Item_id] ASC);
GO

-- Creating primary key on [Sku_id] in table 'SPASkus'
ALTER TABLE [dbo].[SPASkus]
ADD CONSTRAINT [PK_SPASkus]
    PRIMARY KEY CLUSTERED ([Sku_id] ASC);
GO

-- Creating primary key on [File_id] in table 'SPA_FIles'
ALTER TABLE [dbo].[SPA_FIles]
ADD CONSTRAINT [PK_SPA_FIles]
    PRIMARY KEY CLUSTERED ([File_id] ASC);
GO

-- Creating primary key on [ID] in table 'Large_enclosure_pricing'
ALTER TABLE [dbo].[Large_enclosure_pricing]
ADD CONSTRAINT [PK_Large_enclosure_pricing]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Large_enclosure_myaccessories'
ALTER TABLE [dbo].[Large_enclosure_myaccessories]
ADD CONSTRAINT [PK_Large_enclosure_myaccessories]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Large_enclosure_myconfig'
ALTER TABLE [dbo].[Large_enclosure_myconfig]
ADD CONSTRAINT [PK_Large_enclosure_myconfig]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Large_enclosure_ts8'
ALTER TABLE [dbo].[Large_enclosure_ts8]
ADD CONSTRAINT [PK_Large_enclosure_ts8]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Large_enclosure_fmd'
ALTER TABLE [dbo].[Large_enclosure_fmd]
ADD CONSTRAINT [PK_Large_enclosure_fmd]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'SPA_Account_Manager'
ALTER TABLE [dbo].[SPA_Account_Manager]
ADD CONSTRAINT [PK_SPA_Account_Manager]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'SPA_Territory_Codes'
ALTER TABLE [dbo].[SPA_Territory_Codes]
ADD CONSTRAINT [PK_SPA_Territory_Codes]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Rep_id] in table 'SPA_SalesReps'
ALTER TABLE [dbo].[SPA_SalesReps]
ADD CONSTRAINT [PK_SPA_SalesReps]
    PRIMARY KEY CLUSTERED ([Rep_id] ASC);
GO

-- Creating primary key on [ID] in table 'SPA_Material_Master'
ALTER TABLE [dbo].[SPA_Material_Master]
ADD CONSTRAINT [PK_SPA_Material_Master]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'SPA_Intostock_Multiplier'
ALTER TABLE [dbo].[SPA_Intostock_Multiplier]
ADD CONSTRAINT [PK_SPA_Intostock_Multiplier]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'SPA_Notes'
ALTER TABLE [dbo].[SPA_Notes]
ADD CONSTRAINT [PK_SPA_Notes]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Announcements'
ALTER TABLE [dbo].[Announcements]
ADD CONSTRAINT [PK_Announcements]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ws_ID] in table 'Webshop_connect'
ALTER TABLE [dbo].[Webshop_connect]
ADD CONSTRAINT [PK_Webshop_connect]
    PRIMARY KEY CLUSTERED ([ws_ID] ASC);
GO

-- Creating primary key on [loc_ID] in table 'partnerLocations'
ALTER TABLE [dbo].[partnerLocations]
ADD CONSTRAINT [PK_partnerLocations]
    PRIMARY KEY CLUSTERED ([loc_ID] ASC);
GO

-- Creating primary key on [rebate_ID] in table 'SPA_Rebates'
ALTER TABLE [dbo].[SPA_Rebates]
ADD CONSTRAINT [PK_SPA_Rebates]
    PRIMARY KEY CLUSTERED ([rebate_ID] ASC);
GO

-- Creating primary key on [ID] in table 'RiSourcesCarts'
ALTER TABLE [dbo].[RiSourcesCarts]
ADD CONSTRAINT [PK_RiSourcesCarts]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [rebateItem_ID] in table 'SPA_RebatesItems'
ALTER TABLE [dbo].[SPA_RebatesItems]
ADD CONSTRAINT [PK_SPA_RebatesItems]
    PRIMARY KEY CLUSTERED ([rebateItem_ID] ASC);
GO

-- Creating primary key on [ID] in table 'RiSourceCarts'
ALTER TABLE [dbo].[RiSourceCarts]
ADD CONSTRAINT [PK_RiSourceCarts]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [AspNetRoles_Id], [AspNetUsers_Id] in table 'AspNetUserRoles'
ALTER TABLE [dbo].[AspNetUserRoles]
ADD CONSTRAINT [PK_AspNetUserRoles]
    PRIMARY KEY CLUSTERED ([AspNetRoles_Id], [AspNetUsers_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [UserId] in table 'AspNetUserClaims'
ALTER TABLE [dbo].[AspNetUserClaims]
ADD CONSTRAINT [FK_dbo_AspNetUserClaims_dbo_AspNetUsers_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[AspNetUsers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_AspNetUserClaims_dbo_AspNetUsers_UserId'
CREATE INDEX [IX_FK_dbo_AspNetUserClaims_dbo_AspNetUsers_UserId]
ON [dbo].[AspNetUserClaims]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'AspNetUserLogins'
ALTER TABLE [dbo].[AspNetUserLogins]
ADD CONSTRAINT [FK_dbo_AspNetUserLogins_dbo_AspNetUsers_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[AspNetUsers]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_AspNetUserLogins_dbo_AspNetUsers_UserId'
CREATE INDEX [IX_FK_dbo_AspNetUserLogins_dbo_AspNetUsers_UserId]
ON [dbo].[AspNetUserLogins]
    ([UserId]);
GO

-- Creating foreign key on [usr_ID] in table 'usr_user'
ALTER TABLE [dbo].[usr_user]
ADD CONSTRAINT [FK_usr_user_usr_user]
    FOREIGN KEY ([usr_ID])
    REFERENCES [dbo].[usr_user]
        ([usr_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [AspNetRoles_Id] in table 'AspNetUserRoles'
ALTER TABLE [dbo].[AspNetUserRoles]
ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles]
    FOREIGN KEY ([AspNetRoles_Id])
    REFERENCES [dbo].[AspNetRoles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [AspNetUsers_Id] in table 'AspNetUserRoles'
ALTER TABLE [dbo].[AspNetUserRoles]
ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers]
    FOREIGN KEY ([AspNetUsers_Id])
    REFERENCES [dbo].[AspNetUsers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AspNetUserRoles_AspNetUsers'
CREATE INDEX [IX_FK_AspNetUserRoles_AspNetUsers]
ON [dbo].[AspNetUserRoles]
    ([AspNetUsers_Id]);
GO

-- Creating foreign key on [FormID] in table 'salesRequest_File'
ALTER TABLE [dbo].[salesRequest_File]
ADD CONSTRAINT [FK_dbo_salesRequest_Files_dbo_salesRequest_FormID]
    FOREIGN KEY ([FormID])
    REFERENCES [dbo].[salesRequests]
        ([FormID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_salesRequest_Files_dbo_salesRequest_FormID'
CREATE INDEX [IX_FK_dbo_salesRequest_Files_dbo_salesRequest_FormID]
ON [dbo].[salesRequest_File]
    ([FormID]);
GO

-- Creating foreign key on [Form_ID] in table 'salesRequest_Additional_Info'
ALTER TABLE [dbo].[salesRequest_Additional_Info]
ADD CONSTRAINT [FK_salesRequest_Additional_Info_salesRequest_Additional_Info]
    FOREIGN KEY ([Form_ID])
    REFERENCES [dbo].[salesRequests]
        ([FormID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_salesRequest_Additional_Info_salesRequest_Additional_Info'
CREATE INDEX [IX_FK_salesRequest_Additional_Info_salesRequest_Additional_Info]
ON [dbo].[salesRequest_Additional_Info]
    ([Form_ID]);
GO

-- Creating foreign key on [Form_ID] in table 'salesRequest_Action_Log'
ALTER TABLE [dbo].[salesRequest_Action_Log]
ADD CONSTRAINT [FK_salesRequest_Action_Log_salesRequest]
    FOREIGN KEY ([Form_ID])
    REFERENCES [dbo].[salesRequests]
        ([FormID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_salesRequest_Action_Log_salesRequest'
CREATE INDEX [IX_FK_salesRequest_Action_Log_salesRequest]
ON [dbo].[salesRequest_Action_Log]
    ([Form_ID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------