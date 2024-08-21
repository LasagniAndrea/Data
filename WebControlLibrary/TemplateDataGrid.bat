@echo off
Rem ------------------------------------------------------------------------------------------------
Rem Generation de l'assembly EFS.Controls.TemplateDataGrid (EFS.Controls.TemplateDataGrid.dll)
Rem à partir de la classe CSharp TemplateDataGrid.cs
Rem ------------------------------------------------------------------------------------------------
if not exist EFS.Controls.TemplateDataGrid.dll Goto Compil
	del  EFS.Controls.TemplateDataGrid.dll
:Compil
csc  /t:library /r:system.dll,..\Referentiel\obj\Debug\EFS.Referentiel.dll,..\ACommon\obj\Debug\EFS.ACommon.dll /out:EFS.Controls.TemplateDataGrid.dll TemplateDataGrid.cs

Rem ------------------------------------------------------------------------------------------------
Rem Serveur Web: Suppression des anciennes assemblies et copy des nouvelles
Rem ------------------------------------------------------------------------------------------------
if not exist c:\inetpub\wwwroot\OTC\Bin\EFS.Controls.TemplateDataGrid.dll Goto Suite
	del  c:\inetpub\wwwroot\OTC\Bin\EFS.Controls.TemplateDataGrid.dll
:Suite
if exist EFS.Controls.TemplateDataGrid.dll Goto Suite
if exist EFS.Controls.TemplateDataGrid.dll Goto Suite
	echo ERROR assembly not found: EFS.Controls.TemplateDataGrid.dll
	goto End
:Suite
copy EFS.Controls.TemplateDataGrid.dll  c:\inetpub\wwwroot\OTC\Bin\*.*

if not exist EFS.Controls.TemplateDataGrid.dll Goto Suite
	del  c:\inetpub\wwwroot\OTC\Bin\EFS.Referentiel.dll
:Suite
if exist ..\Referentiel\obj\Debug\EFS.Referentiel.dll Goto Suite
	echo ERROR assembly not found: EFS.Controls.TemplateDataGrid.dll
	goto End
:Suite
copy ..\Referentiel\obj\Debug\EFS.Referentiel.dll  c:\inetpub\wwwroot\OTC\Bin\*.*

:End 