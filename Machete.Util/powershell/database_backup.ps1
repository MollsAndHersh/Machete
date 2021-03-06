[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoExtended") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoEnum") | Out-Null
Import-Module BitsTransfer
$backupSourceDir = 'C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\Backup\'
$serverInstance = ".\SQLEXPRESS"
$mainDB = "macheteStageProd"
$accountsDB = "machetelogStageProd"
$logDB = "aspnetdbStageProd"
$archiveDir = "C:\Archives\"
$bakExt = ".bak"
$zipExt = ".zip"
$zipexe = "c:\program files (x86)\7-zip\7z.exe "
$destserver = "casaserv"
$destFS = "\\"+$destserver+"\Common Documents"
$emailFrom = "machete@casa-latina.org"
$emailTo = "jciispam@gmail.com"
$emailSmtpClient = "casaserv"
$bitsTimeout = 30 #in seconds
$bitsStartError = "BitsTransfer failed to initiate the file transfer. This is probably a credential issue. Check ID & password."
$bitsTransError = "BitsTranfer either failed to transfer before timeout, or returned an error on transfer. BitsJob error condition & description: "
$password = ConvertTo-SecureString -String "" -AsPlainText -Force
$cred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList "", $password

function MAIN () {
    $a = Get-Date
    $predicate = "SQL" + $a.Year + "-" + $a.Month + "-" + $a.Day + "-machete-"
    # Call backupDB Function for each database
    backupDB $backupSourceDir $predicate $mainDB
    backupDB $backupSourceDir $predicate $accountsDB
    backupDB $backupSourceDir $predicate $logDB    
    $archiveFile = $archiveDir + $predicate + $mainDB + $zipExt
    [array]$arguments = "a", $archiveFile, $($backupSourceDir+$predicate+"*")
    # print arguments sent to 7zip
    #$arguments
    & $zipexe $arguments 
    # I hate doing this, but fighting with powershell seems like a waste
    # can't get invoke-expression to take params, can't get wait to work with &
    # so I'm telling the script to sleep while the asynchronous zip runs
    Start-sleep -milliseconds 80000
    rm $($backupSourceDir + $predicate + "*")
    #$archiveFile
    #$backupCopy
    $destination=new-Psdrive -name $destserver -PsProvider FileSystem -root $destFS -Credential $cred
    Copy-Item $archiveFile -Destination $backupCopy -Force
    if (!$?)
    {
        sendEmail $emailFrom  $emailTo "[MACHETE] BitsTransfer error" $bitsStartError
        Remove-PSDrive $destserver
        return
    }
    Remove-PSDrive $destserver
    sendEmail $emailFrom $emailTo "[MACHETE] Backup successful" "No errors found."
}

function backupDB ([System.String] $backupSourceDir, [System.String] $predicate, [System.String] $databaseName) {
    $backupFile = $predicate + $databaseName + $bakExt
    $server = New-Object ("Microsoft.SqlServer.Management.Smo.Server") ($serverInstance)

    $dbBackup = new-Object ("Microsoft.SqlServer.Management.Smo.Backup")
    $dbRestore = new-object ("Microsoft.SqlServer.Management.Smo.Restore")

    $dbBackup.Database = $databaseName
    $dbBackup.Devices.AddDevice($backupSourceDir + $backupFile, "File")
    $dbBackup.Action="Database"
    $dbBackup.Initialize = $TRUE
    $dbBackup.SqlBackup($server)

    $dbRestore.Devices.AddDevice($backupSourceDir + $backupFile, "File")
    if (!($dbRestore.SqlVerify($server))){
     
     sendEmail($emailFrom, $emailTo, "[MACHETE] SQL Server backup error", 
     "SQL Server Backup verify failed. Powershell received an error executing a Database backup. Action required immediately for Full Backup.")
     Exit
    }    
}

function sendEmail([System.String] $from, [System.String] $to, [System.String] $subject, [System.String] $msg) {
    $smtp = new-object Net.Mail.SmtpClient($emailSmtpClient)
    $smtp.Send($from, $to, $subject, $msg)
}


MAIN
