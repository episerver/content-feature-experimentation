:: Setup EPiServer CMS and Commerce databases
@echo off

set cms_db=mt.netcore.qs.cms
set commerce_db=mt.netcore.qs.commerce
set user=netcoreUser
set password=epi#Server7Local

:: Determine package folders
for /F " tokens=*" %%i in ('dir ".\Packages\EPiServer.CMS.Core\*" /b /o:d') do (set cms_core=%%i) 
for /F " tokens=*" %%i in ('dir ".\Packages\EPiServer.Commerce.Core\*" /b /o:d') do (set commerce_core=%%i)
for /F " tokens=*" %%i in ('dir ".\Packages\EPiServer.Personalization.Commerce\*" /b /o:d') do (set personalization_commerce=%%i) 
echo %cms_core%
if "%cms_core%"=="" (
	echo CMS Core package is missing. Please build the project before running the setup.
	pause /b
)
if "%commerce_core%"=="" (
	echo Commerce Core package is missing. Please build the project before running the setup.
	pause /b
)

if "%personalization_commerce%"=="" (
	echo Personalization Commerce package is missing. Please build the project before running the setup.
	pause /b
)

set sql=sqlcmd -S (LocalDB)\MSSQLLocalDB -E

echo Dropping databases...
%sql% -Q "EXEC msdb.dbo.sp_delete_database_backuphistory N'%cms_db%'"
%sql% -Q "if db_id('%cms_db%') is not null ALTER DATABASE [%cms_db%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
%sql% -Q "if db_id('%cms_db%') is not null DROP DATABASE [%cms_db%]"
%sql% -Q "EXEC msdb.dbo.sp_delete_database_backuphistory N'%commerce_db%'"
%sql% -Q "if db_id('%commerce_db%') is not null ALTER DATABASE [%commerce_db%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
%sql% -Q "if db_id('%commerce_db%') is not null DROP DATABASE [%commerce_db%]"

echo Dropping user...
%sql% -Q "if exists (select loginname from master.dbo.syslogins where name = '%user%') EXEC sp_droplogin @loginame='%user%'"

echo Creating databases...
%sql% -Q "CREATE DATABASE [%cms_db%] COLLATE SQL_Latin1_General_CP1_CI_AS"
%sql% -Q "CREATE DATABASE [%commerce_db%] COLLATE SQL_Latin1_General_CP1_CI_AS"

echo Creating user...
%sql% -Q "EXEC sp_addlogin @loginame='%user%', @passwd='%password%', @defdb='%cms_db%'"
%sql% -d %cms_db% -Q "EXEC sp_adduser @loginame='%user%'"
%sql% -d %cms_db% -Q "EXEC sp_addrolemember N'db_owner', N'%user%'"
%sql% -d %commerce_db% -Q "EXEC sp_adduser @loginame='%user%'"
%sql% -d %commerce_db% -Q "EXEC sp_addrolemember N'db_owner', N'%user%'"

echo Installing CMS database...
%sql% -d %cms_db% -b -i ".\packages\EPiServer.CMS.Core\%cms_core%\tools\EPiServer.Cms.Core.sql" > SetupCmsDb.log

echo Installing Commerce database...
%sql% -d %commerce_db% -b -i ".\packages\EPiServer.Commerce.Core\%commerce_core%\tools\EPiServer.Commerce.Core.sql" > SetupCommerceDb.log

echo Installing Commerce database...
%sql% -d %commerce_db% -b -i ".\packages\EPiServer.Personalization.Commerce\%personalization_commerce%\tools\epiupdates_commerce\sql\1.0.0.sql" > SetupPersonalizationCommerce.log

Pause
