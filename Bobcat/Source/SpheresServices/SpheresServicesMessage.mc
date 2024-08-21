; // This is the header section. 

SeverityNames=(Success=0x0
               Informational=0x1
               Warning=0x2
               Error=0x3)

FacilityNames=(Application=0x0
               System=0x1)

LanguageNames=(English=0x409:MSG00409)

; // The following are message definitions.

; // PL Add next line
MessageIdTypedef=WORD

MessageId=0x1
Severity=Success
Facility=Application
SymbolicName=CAT_Success
Language=English
Success
.

MessageId=0x2
Severity=Success
Facility=Application
SymbolicName=CAT_Informational
Language=English
Informational
.

MessageId=0x3
Severity=Success
Facility=Application
SymbolicName=CAT_Warning
Language=English
Warning
.

MessageId=0x4
Severity=Success
Facility=Application
SymbolicName=CAT_Error
Language=English
Error
.

; // PL Add next line
MessageIdTypedef=DWORD

MessageId=1000
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_BusinessInformation
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1500
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_SystemInformation
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1501
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_ServiceStart
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1502
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_ServiceContinue
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1503
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_ServiceStop
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1504
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_ServicePause
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1505
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_ServiceInitialize
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=1506
Severity=Success
Facility=Application
SymbolicName=MSG_Spheres_ServiceConnected
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2000
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_BusinessError
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2500
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_SystemError
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2501
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceMOMPath
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2502
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceMOMInitialize
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2509
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceMOMMisc
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2530
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceObjRefNotSet
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2550
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceSQLError
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2551
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceSQLErrorTimeout
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect: TIMEOUT%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2552
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceSQLErrorDeadlock
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect: DEADLOCK%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2570
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_ServiceCMECore
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=2999
Severity=Error
Facility=Application
SymbolicName=MSG_Spheres_Undefined
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Defect:%r
%b%b%b%bCode: %3%r
%b%b%b%bMethod: %4%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.

MessageId=3000
Severity=Warning
Facility=Application
SymbolicName=MSG_Spheres_BusinessWarning
Language=English
%b%1 - (c)2024 EFS%r
%r
%2%r
%r
Environment:%r
%b%b%b%bMOM: %5 - %6%r
%b%b%b%bCS: %7%r
%r
Source:%r
%b%b%b%bMessageId: %8%r
%r
System:%r
%b%b%b%bHostName: %9%r
%b%b%b%bUserName: %10
.
