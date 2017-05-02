Param
(
	[Parameter(Mandatory=$true)][string] $checkoutDir, 
	[Parameter(Mandatory=$true)][string] $nunitVersion, 
	[Parameter(Mandatory=$true)][string] $browsersToRun, 
	[Parameter(Mandatory=$true)][string] $testedApp, 
	[Parameter(Mandatory=$true)][string] $appsToRun, 
	[Parameter(Mandatory=$false)][string] $helpersToRun, 
	[Parameter(Mandatory=$false)][string] $testsPath
)

$StarcounterDir = "$checkoutDir\sc"
$StarcounterWorkDirPath = "$StarcounterDir\starcounter-workdir"
$StarcounterRepoPath = "$StarcounterWorkDirPath\personal"
$StarcounterConfigPath = "$StarcounterDir\Configuration"
$StarExePath = "$StarcounterDir\star.exe"
$StarAdminExePath = "$StarcounterDir\staradmin.exe"

Function createXML()
{
	$fileContent = "<?xml version=`"1.0`" encoding=`"UTF-8`"?><service><server-dir>$StarcounterRepoPath</server-dir></service>"
	New-Item -Path $StarcounterConfigPath -Name personal.xml -ItemType "file" -force -Value $fileContent | Out-Null
}

Function createRepo()
{
	Start-Process -FilePath $StarExePath -ArgumentList "`@`@createrepo $StarcounterWorkDirPath" -NoNewWindow -Wait
}

Function runApps($apps, $source)
{
	foreach ($app in $apps -split ",")
	{
		$AppWWWPath = "$checkoutDir\$testedApp\$source\$app\wwwroot"
		$AppExePath = "$checkoutDir\$testedApp\$source\$app\bin\Debug\$app.exe"
		$AppArg = "--resourcedir=$AppWWWPath $AppExePath"
		
		$process = Start-Process -FilePath $StarExePath -ArgumentList $AppArg -PassThru -NoNewWindow		
		wait-process -id $process.Id
	}
}

Function runTests()
{
	$NunitConsoleRunnerExePath = "$checkoutDir\$testedApp\packages\NUnit.ConsoleRunner.$nunitVersion\tools\nunit3-console.exe"
	$NunitArg = "$testsPath --noheader --teamcity --params Browsers=$browsersToRun"
	
	Start-Process -FilePath $NunitConsoleRunnerExePath -ArgumentList $NunitArg -NoNewWindow -Wait
}

Function killStarcounter()
{
	Start-Process -FilePath $StarAdminExePath -ArgumentList "kill all" -NoNewWindow -Wait
}

Function runAppsAndTests()
{
	try
	{
		createRepo
		createXML
		runApps -apps $appsToRun -source "src"
		if($helpersToRun)
		{
			runApps -apps $helpersToRun -source "test"
		}
		runTests
		killStarcounter
	}
	Catch
	{
		$ErrorMessage = $_.Exception.Message
		Write-Output $ErrorMessage
		exit(1)
	}
}

Function Main()
{
	if($testsPath)
	{
		runAppsAndTests
	}
	else 
	{ 
		Write-Output "No tests to run"
		exit(0)				
	}
}

Main