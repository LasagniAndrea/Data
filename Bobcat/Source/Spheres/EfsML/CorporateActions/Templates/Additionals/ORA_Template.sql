/*==============================================================*/
/* Copyright © 2023 EFS                                         */
/*==============================================================*/
/*
EG 202111030 Script template pour CA 
Exécution d'un script avant INTEGRATION, AVANT  et/ou APRES TRAITEMENT de CA
*/
declare
    isError boolean:=false;
    nbCount number:=null;
    errGuard number:=0;
    errMaxGuard number:=10000;
    errMsg varchar2(4000);
    
    totalCount number;
    
    idProcess_L number(10):=%%IDPROCESS_L%%;
    processName varchar2(256):='%%PROCESS%%';
    scriptName varchar2(256):='%%SCRIPTNAME%%';

    hostName varchar2(256):='%%HOSTNAME%%';
    appName varchar2(256):='%%APPNAME%%';
    appVersion varchar2(256):='%%APPVERSION%%';
    
    TYPE recLog IS RECORD
    (
        STATUSLOG   varchar2(8), 
        DTSTPROCESS TIMESTAMP(7),
        SYSCODE     char(3),
        SYSNUMBER   number(5),
        LEVELORDER  number(5),
        MESSAGE     varchar2(4000),
        DATA1       varchar2(128) default null,
        DATA2       varchar2(128) default null,
        DATA3       varchar2(128) default null,
        DATA4       varchar2(128) default null,
        DATA5       varchar2(128) default null,
        DATA6       varchar2(128) default null,
        DATA7       varchar2(128) default null,
        DATA8       varchar2(128) default null,
        DATA9       varchar2(128) default null,
        DATA10      varchar2(128) default null   
    );    
    TYPE tblLog is TABLE of recLog  index by binary_integer;
    processLog tblLog;
    
    ownerException EXCEPTION;
    PRAGMA exception_init(ownerException, -20001);
    
    
begin

    errGuard := errGuard + 1;
    
    if (idProcess_L is not null) then
    begin
        processLog(1).STATUSLOG := 'NONE';
        processLog(1).DTSTPROCESS := SYS_EXTRACT_UTC(CURRENT_TIMESTAMP);
        processLog(1).SYSCODE := 'SYS';
        processLog(1).SYSNUMBER := 0;
        processLog(1).LEVELORDER := 2;
        processLog(1).MESSAGE := '- START SCRIPT : ' || scriptname;
    end;
    end if;
    
    
    /* %%SCRIPT_CA%% */
    
    if (errGuard > errMaxGuard) then
        raise VALUE_ERROR;
    end if;
    
    if (isError) then
        rollback;
    else
        commit;
    end if;

    if (idProcess_L is not null) then
    begin
        for i in processLog.FIRST .. processLog.LAST LOOP
            insert into PROCESSDET_L (IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, IDDATA, 
            HOSTNAME, APPNAME, APPVERSION,
            DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10)    
            values (idProcess_L, processName, processLog(i).STATUSLOG, processLog(i).DTSTPROCESS, processLog(i).SYSCODE, processLog(i).SYSNUMBER , 2, processLog(i).MESSAGE, 0, 
            hostName, appName, appVersion,
            processLog(i).DATA1, processLog(i).DATA2, processLog(i).DATA3, processLog(i).DATA4, processLog(i).DATA5, 
            processLog(i).DATA6, processLog(i).DATA7, processLog(i).DATA8, processLog(i).DATA9, processLog(i).DATA10);
        end loop;
        insert into PROCESSDET_L (IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, IDDATA, HOSTNAME, APPNAME, APPVERSION)    
        values (idProcess_L, processName, 'NONE', SYS_EXTRACT_UTC(CURRENT_TIMESTAMP), 'SYS', 0 , 2, '- END SCRIPT : ' || ScriptName, 0, hostName, appName, appVersion);
        commit;
    end;
    end if;
        
    
    exception
    when VALUE_ERROR then
    errMsg := 'Warning: guarde exceeded! ' || '[' + to_char(errGuard) + ']';
    raise_application_error( -20001, errMsg);
  
    when OTHERS then
    errMsg := 'Error: on script! [' || to_char( SQLCODE ) || ']' || '[' + SQLERRM + ']';
    rollback;

    if (idProcess_L is not null) then
    begin
        for i in processLog.FIRST .. processLog.LAST LOOP
            insert into PROCESSDET_L (IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, IDDATA, 
            HOSTNAME, APPNAME, APPVERSION,
            DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10)    
            values (idProcess_L, processName, processLog(i).STATUSLOG, processLog(i).DTSTPROCESS, processLog(i).SYSCODE, processLog(i).SYSNUMBER , 2, processLog(i).MESSAGE, 0, 
            hostName, appName, appVersion,
            processLog(i).DATA1, processLog(i).DATA2, processLog(i).DATA3, processLog(i).DATA4, processLog(i).DATA5, 
            processLog(i).DATA6, processLog(i).DATA7, processLog(i).DATA8, processLog(i).DATA9, processLog(i).DATA10);
        end loop;
        insert into PROCESSDET_L (IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER , LEVELORDER, MESSAGE, IDDATA, HOSTNAME, APPNAME, APPVERSION)    
        values (idProcess_L, processName, 'WARNING', SYS_EXTRACT_UTC(CURRENT_TIMESTAMP), 'SYS', 0 , 2, errMsg, 0, hostName, appName, appVersion);

        commit;
    end;
    end if;
    
    raise_application_error( -20001, errMsg);
    
end;

