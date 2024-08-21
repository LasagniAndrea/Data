@echo off
: ----------------------------------------------------------------------
: WARNING: mc.exe not available on VS2008 (VS 9.0)
: ----------------------------------------------------------------------

set EXISTERROR=
set SUITE=Main

rem [24512] FI 20211231 Mise en commentaire
rem C:
rem cd \
 
echo ---------------------------------------------------------
echo Compilation in progress...
echo ---------------------------------------------------------
echo.

:Main
echo ---------------------------------------------------------
echo Main
echo ---------------------------------------------------------
rem PL 20120319 set path=%path%;C:\Program Files\Microsoft Visual Studio 8\Common7\IDE;
set path=%path%;C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE;
rem pause

rem [24512] FI 20211231 Mise en commentaire
rem d:
rem cd \TFS\SpheresProject\Development\Bengal\Source\SpheresServices
rem cd \TFS\Source\Workspaces\SpheresProject\Development\Panther\Source\SpheresServices
	rem cd \SpheresProject\Development\Panther\Source\SpheresServices
rem pause 

rem PL 20120319 "C:\Program Files\Microsoft Visual Studio 8\Common7\Tools\Bin\mc.exe" SpheresServicesMessage.mc
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\mc.exe" SpheresServicesMessage.mc
if not %errorlevel%==0 goto ERROR
pause

rem PL 20120319 "C:\Program Files\Microsoft Visual Studio 8\Common7\Tools\Bin\rc.exe" -r -fo SpheresServicesMessage.res SpheresServicesMessage.rc
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\rc.exe" -r -fo SpheresServicesMessage.res SpheresServicesMessage.rc
if not %errorlevel%==0 goto ERROR
pause

rem PL 20120319 "C:\Program Files\Microsoft Visual Studio 8\VC\bin\link.exe" /DLL /SUBSYSTEM:WINDOWS /NOENTRY /MACHINE:x86 /OUT:SpheresServicesMessage.dll SpheresServicesMessage.res
"C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\bin\link.exe" /DLL /SUBSYSTEM:WINDOWS /NOENTRY /MACHINE:x86 /OUT:SpheresServicesMessage.dll SpheresServicesMessage.res
if not %errorlevel%==0 goto ERROR
pause
goto END1

:ERROR
set EXISTERROR=TRUE
echo *********************************************************
echo Warning, error !
echo *********************************************************
goto END

:END1
if "%EXISTERROR%"=="TRUE" goto END2
echo.
echo ---------------------------------------------------------
echo Terminated with succes !
echo ---------------------------------------------------------
goto END

:END2
echo.
echo *********************************************************
echo Warning, generated with error !
echo *********************************************************
goto END

:END
pause