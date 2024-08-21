# FI 20170505 Utilisation de StreamWriter 
$path = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition);
[System.IO.Directory]::SetCurrentDirectory($path);

#$pathOutput="RDBMS\TMDB\App\Sql\Scripts\Spheres\v8.1.0";
$pathOutput='RDBMS\TMDB\App\Sql\Scripts\Spheres\v14.0.0';

cls;

# ##############################################################################
# WARNING
# ##############################################################################
# Main   : il faut 5 « ..\”  (ex. Include="..\..\..\..\..\RDBMS\TMDB\App\Sql\Scripts\Spheres\v10.0.0\)
# Panther: il faut 6 « ..\”  (ex. Include="..\..\..\..\..\..\RDBMS\TMDB\App\Sql\Scripts\Spheres\v10.0.0\)
# ##############################################################################
if ($path.IndexOf('Main') -eq -1)
{
    $pathOutput = '..\..\..\..\..\..\' + $pathOutput;
}
else
{
    $pathOutput = '..\..\..\..\..\' + $pathOutput;
}

echo ('=======================================================================');
echo('Current path: ' + $path);  
echo('Output path : ' + $pathOutput);  
echo ('=======================================================================');

if(-not(test-path $pathOutput))
{
    echo ('');
    echo ('-----------------------------------------------------------------------');
	echo('Create folder: ' + $pathOutput);  
    echo ('-----------------------------------------------------------------------');
    New-Item -ItemType directory -Name $pathOutput;
}

function BuildSQLScript($pDbSvrType)
{
    $xml= ".\SystemMsgResource.xml";

    $settings = New-Object System.Xml.XmlReaderSettings;
	#IMPORTANT => INDISPENSABLE, car permet d'ignorer le formatage généré par Visula Studio lors d'une mise en forme du flux de resource. 
    $settings.IgnoreWhitespace= $true; 

    $xmlReader = [System.Xml.XmlReader]::create("$xml",$settings);

    $xsl= ".\ToScriptSystemMsg.xslt";

    if ($pDbSvrType -eq 'dbORA')
    {
		$output = "$pathOutput\ScriptSystemMsg_Ora9i2.sql";
    }
    elseif ($pDbSvrType -eq 'dbSQL')
    {
		$output = "$pathOutput\ScriptSystemMsg_Sql2k.sql";
    }
    echo('Output file: ' + $output);  

    $xslt = $null;
    if( [System.Diagnostics.Debugger]::IsAttached )
    {
		$xslt = New-Object System.Xml.Xsl.XslCompiledTransform( $true );
    }
    else
    {
		$xslt = New-Object System.Xml.Xsl.XslCompiledTransform( $false );
    }

    $arglist = $null; 
    $arglist = new-object System.Xml.Xsl.XsltArgumentList;
    $arglist.AddParam('pDbSvrType', '', $pDbSvrType);

    $xsltSettings = $null;
    $xsltSettings = New-Object System.Xml.Xsl.XsltSettings($false,$true);
    $xslt.Load($xsl, $xsltSettings, (New-Object System.Xml.XmlUrlResolver));
  
    $outFile = New-Object System.IO.FileStream ($output, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write);
	
	# FI 20170505 IMPORTANT: Utilisation de StreamWriter pour effectuer le codage Default (=> Le fichier sera encodé en CodePage 1252 Windows Latin 1 (ANSI))
	# INDISPENSABLE, car sous Oracle® TMDB applique set NLS_LANG=.WE8MSWIN1252 (voir StartUpdateScript.bat)
	$streamWriter = New-Object System.IO.StreamWriter ($outFile,[System.Text.Encoding]::Default);

    $xslt.Transform($xmlReader, $arglist, $streamWriter);
    $xmlReader.Close();
    $outFile.Close();
}

echo ('-----------------------------------------------------------------------');
echo('Script for MSS...');  
echo ('-----------------------------------------------------------------------');
BuildSQLScript('dbSQL');

echo ('-----------------------------------------------------------------------');
echo('Script for ORA...');  
echo ('-----------------------------------------------------------------------');
BuildSQLScript('dbORA');
echo ('=======================================================================');
