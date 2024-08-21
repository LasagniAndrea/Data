/*==============================================================*/
/* Copyright © 2023 Euro Finance Systems                   */
/*==============================================================*/
declare @Edition as varchar(255),@ProductVersion as varchar(255),@ProductLevel as varchar(255),@MachineName as varchar(255),@ServerName as varchar(255)
select @Edition=convert(varchar,SERVERPROPERTY('Edition')),@ProductVersion=convert(varchar,SERVERPROPERTY('ProductVersion')),@ProductLevel=convert(varchar,SERVERPROPERTY('ProductLevel')),
@MachineName=convert(varchar,SERVERPROPERTY('MachineName')),@ServerName=convert(varchar,SERVERPROPERTY('ServerName'));
print '========================================================================';
print 'Euro Finance Systems - Support                  ' + convert(varchar,getdate(),113);;
print '========================================================================';
print 'Software: Spheres F&Oml ©';
print 'RDBMS   : ' + 'MS SQL Server '+ @Edition + ' v' + @ProductVersion + ' ' + @ProductLevel;
print 'Machine : ' + @MachineName + '    ' + replicate(' ',30-case when len(@MachineName)<=30 then len(@MachineName) else 0 end) + 'Server: ' + @ServerName;
print 'Database: ' + DB_NAME()    + '    ' + replicate(' ',30-case when len(DB_NAME())<=30 then len(DB_NAME()) else 0 end)       + 'User  : ' + SUSER_SNAME() + ' / ' + USER_NAME();

print '========================================================================';
Print 'Item    : %%TITLE_CA%%';
print 'Summary : %%SUMMARY_CA%%';
print '========================================================================';


declare @ErrorGuard int, @ErrorMAXGuard int, @errMsg varchar(4000), @GetDate datetime
declare @error bit, @count int, @totalcount int

set @error=0;
set @count=null;
set @totalcount=null;
set @ErrorGuard=0;
set @ErrorMAXGuard=10000;

declare @IdPROCESS_L UT_ID;
declare @ProcessName UT_UNC;
declare @ScriptName UT_UNC;
declare @HostName UT_UNC;
declare @AppName UT_UNC;
declare @AppVersion UT_UNC;

set @IdPROCESS_L=%%IDPROCESS_L%%;
set @ProcessName='%%PROCESS%%';
set @ScriptName='%%SCRIPTNAME%%';
set @HostName='%%HOSTNAME%%';
set @AppName='%%APPNAME%%';
set @AppVersion='%%APPVERSION%%';


/* Chargement des logs détails dans cette variable TABLE pour alimentation de PROCESSDET_L post transaction */
declare @tblLog table 
(
	STATUSLOG   UT_STATUS      not null, 
	DTSTPROCESS UT_DTINS       not null,
	SYSCODE     UT_EVENT       null,
	SYSNUMBER   UT_NUMBER5     null,
	LEVELORDER  UT_NUMBER5     null,
	MESSAGE     varchar(4000)  null,
	DATA1       varchar(128)   null,
	DATA2       varchar(128)   null,
	DATA3       varchar(128)   null,
	DATA4       varchar(128)   null,
	DATA5       varchar(128)   null,
	DATA6       varchar(128)   null,
	DATA7       varchar(128)   null,
	DATA8       varchar(128)   null,
	DATA9       varchar(128)   null,
	DATA10      varchar(128)   null
);

begin try
	begin
		set @ErrorGuard=@ErrorGuard+1;
		
		
		
		if (@IdPROCESS_L is not null)
		begin
			insert into @tblLog (STATUSLOG, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER,  MESSAGE) 
			values ('NONE', getutcdate(), 'SYS', 0, 2, '- START SCRIPT : ' + @ScriptName);
		end
		
		
		begin transaction;
		print 'Begin transaction ------------------------------------------------------';
		print ' ';
		set @error=0
		
		/* %%SCRIPT_CA%% */

		if (@ErrorGuard>=@ErrorMAXGuard)
		begin
			set @ERRMSG='Warning: guard exceeded! ' + '[' + convert(varchar,@ErrorGuard) + ']';
			raiserror (@ERRMSG, 16, 1);
		end

		if (@error=1)
		begin
			rollback transaction;
		end
		else
		begin
			commit transaction;
		end
		
	end

	if (@IdPROCESS_L is not null)
	begin
		insert into @tblLog (STATUSLOG, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER,  MESSAGE) 
		values ('NONE', getutcdate(), 'SYS', 0, 2, '- END SCRIPT : ' + @ScriptName);
	end
	
	insert into PROCESSDET_L (IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, 
	IDDATA, HOSTNAME, APPNAME, APPVERSION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10)	
	select @IdPROCESS_L, @ProcessName, STATUSLOG, DTSTPROCESS, SYSCODE, SYSNUMBER , 2, MESSAGE, 
	0, @HostName, @AppName, @AppVersion,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10
	from @tblLog;
	
end try

begin catch
	declare @errNum int, @errLine int, @errSeverity int, @errState int, @errProcedure nvarchar(126); 	
	select @errline = ERROR_LINE(), @errMsg = ERROR_MESSAGE(), @errNum = ERROR_NUMBER(), @errSeverity = ERROR_SEVERITY(), @errState = ERROR_STATE(), @errProcedure= ERROR_PROCEDURE();
	
	rollback transaction;

	insert into @tblLog (STATUSLOG, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER,  MESSAGE, DATA1, DATA2, DATA3, DATA4, DATA5, DATA6) 
	values ('WARNING', getdate(), 'SYS', 0, 2, @errMsg, 
	convert(varchar,@errLine), convert(varchar,@errNum), convert(varchar,@errSeverity),	convert(varchar,@errState), @errProcedure, convert(varchar,@errSeverity));
	
	insert into PROCESSDET_L (IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, 
	IDDATA, HOSTNAME, APPNAME, APPVERSION,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10)	
	select @IdPROCESS_L, @ProcessName, STATUSLOG, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, 
	0, @HostName, @AppName, @AppVersion,
	DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10
	from @tblLog;
	
end catch;


