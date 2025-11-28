Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.ComponentModel
Imports Microsoft.VisualBasic

Public Class Form3

    ' ==========================================
    ' تعريف المتغيرات والأدوات
    ' ==========================================
    Private WithEvents bw As New BackgroundWorker()
    Private progressForm As ProgressForm
    Private backupPaths As New Dictionary(Of String, String)
    Private selectedProfiles As New List(Of String)
    Private uiActionComplete As New ManualResetEvent(False)
    Private profileNameToProcess As String = ""
    Private profileDirToProcess As String = ""
    Private passwordFilePathResult As String = ""

    ' عدادات التقرير النهائي
    Private countProfilesBackedUp As Integer = 0
    Private countPasswordsExported As Integer = 0
    Private countRegistryBackedUp As Integer = 0

    ' ==========================================
    ' دوال النظام (API)
    ' ==========================================
    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function BlockInput(ByVal fBlockIt As Boolean) As Boolean
    End Function

    Private Const SW_RESTORE As Integer = 9

    ' ==========================================
    ' أحداث بدء التشغيل والواجهة
    ' ==========================================
    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ApplyLanguage()
        LoadChromeProfiles()
        Label3.Text = "0"

        ' إعداد الـ Worker
        bw.WorkerReportsProgress = True
        bw.WorkerSupportsCancellation = True
    End Sub

    Private Sub ApplyLanguage()
        Me.Text = SimpleLanguageManager.GetForm4Text("title")
        Label11.Text = SimpleLanguageManager.GetForm4Text("select_backup")
        Button1.Text = SimpleLanguageManager.GetForm4Text("check_all")
        Button2.Text = SimpleLanguageManager.GetForm4Text("uncheck_all")
        Button3.Text = SimpleLanguageManager.GetForm4Text("start_backup")
        Label14.Text = SimpleLanguageManager.GetForm4Text("selected_count")
        GroupBox1.Text = SimpleLanguageManager.GetForm4Text("options_group")
        CheckBox1.Text = SimpleLanguageManager.GetForm4Text("backup_profiles")
        CheckBox2.Text = SimpleLanguageManager.GetForm4Text("backup_passwords")
        CheckBox3.Text = SimpleLanguageManager.GetForm4Text("backup_registry")
    End Sub

    Private Sub LoadChromeProfiles()
        Try
            Dim localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data\Local State")
            If Not File.Exists(localStatePath) Then Return
            Dim jsonContent = File.ReadAllText(localStatePath)
            Dim jsonObject = JObject.Parse(jsonContent)
            Dim profiles = TryCast(jsonObject.SelectToken("profile.info_cache"), JObject)
            If profiles Is Nothing Then Return
            Dim profileNames As New List(Of String)
            For Each profile As JProperty In profiles.Properties()
                Dim name = profile.Value("name")?.ToString()
                If Not String.IsNullOrEmpty(name) Then profileNames.Add(name)
            Next
            profileNames.Sort()
            CheckedListBox1.Items.Clear()
            CheckedListBox1.Items.AddRange(profileNames.ToArray())
        Catch
        End Try
    End Sub

    Private Sub CheckedListBox1_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox1.ItemCheck
        BeginInvoke(New MethodInvoker(Sub() Label3.Text = CheckedListBox1.CheckedItems.Count.ToString()))
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        For i = 0 To CheckedListBox1.Items.Count - 1
            CheckedListBox1.SetItemChecked(i, True)
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        For i = 0 To CheckedListBox1.Items.Count - 1
            CheckedListBox1.SetItemChecked(i, False)
        Next
    End Sub

    ' ==========================================
    ' زر بدء النسخ الاحتياطي
    ' ==========================================
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If CheckedListBox1.CheckedItems.Count = 0 Then
            MessageBox.Show(SimpleLanguageManager.GetForm4Text("msg_no_profile"), "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If Not CheckBox1.Checked AndAlso Not CheckBox2.Checked AndAlso Not CheckBox3.Checked Then
            MessageBox.Show(SimpleLanguageManager.GetForm4Text("msg_no_option"), "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If bw.IsBusy Then
            MessageBox.Show(SimpleLanguageManager.GetForm4Text("msg_busy"), "Wait", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If MessageBox.Show(SimpleLanguageManager.GetForm4Text("msg_confirm"), "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
            Return
        End If

        ' تحضير المسارات والقوائم
        PrepareBackupPaths()
        selectedProfiles.Clear()
        For Each item In CheckedListBox1.CheckedItems
            selectedProfiles.Add(item.ToString())
        Next

        Me.Enabled = False

        ' إغلاق المتصفحات (استدعاء الدالة من Form1)
        Form1.CloseAllChromeBrowsersOldWay()
        Thread.Sleep(2500) ' انتظار للتأكد

        ' تصفير العدادات
        countProfilesBackedUp = 0
        countPasswordsExported = 0
        countRegistryBackedUp = 0

        ' إظهار فورم التقدم وبدء العمل
        progressForm = New ProgressForm()
        progressForm.Worker = bw
        progressForm.Show(Me)
        bw.RunWorkerAsync()
    End Sub

    ' ==========================================
    ' منطق العملية في الخلفية (Background Worker)
    ' ==========================================
    Private Sub bw_DoWork(sender As Object, e As DoWorkEventArgs) Handles bw.DoWork
        Dim worker = CType(sender, BackgroundWorker)
        Try
            Dim doProfiles = False, doPasswords = False, doRegistry = False
            Me.Invoke(New MethodInvoker(Sub()
                                            doProfiles = CheckBox1.Checked
                                            doPasswords = CheckBox2.Checked
                                            doRegistry = CheckBox3.Checked
                                        End Sub))

            ' 1. تهيئة المجلدات (0%)
            worker.ReportProgress(0, SimpleLanguageManager.GetForm4Text("creating_folders"))
            CreateBackupFolders(worker, e)
            If e.Cancel Then Return

            ' 2. نسخ الريجستري (5% -> 10%)
            If doRegistry Then
                worker.ReportProgress(5, SimpleLanguageManager.GetForm4Text("copying_reg"))
                BackupRegistry(backupPaths("Registry"))
                countRegistryBackedUp = 1
                Thread.Sleep(500)
            End If
            If e.Cancel Then Return

            ' 3. نسخ البروفايلات (10% -> 60%)
            If doProfiles Then
                BackupProfilesFilesOnly(worker, e, backupPaths("UserData"), selectedProfiles)
            End If
            If e.Cancel Then Return

            ' 4. تصدير كلمات المرور (60% -> 95%)
            If doPasswords Then
                ExportPasswordsSequence(worker, e, backupPaths("Passwords"), selectedProfiles)
            End If

        Catch ex As Exception
            e.Cancel = True
        Finally
            ' فك تجميد الماوس في حال حدوث خطأ غير متوقع
            BlockInput(False)
        End Try
    End Sub

    Private Sub bw_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles bw.ProgressChanged
        If progressForm IsNot Nothing Then
            progressForm.SetProgressTarget(e.ProgressPercentage, e.UserState?.ToString())
        End If
    End Sub

    Private Sub bw_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles bw.RunWorkerCompleted
        BlockInput(False) ' تأمين نهائي

        If e.Cancelled Then
            If progressForm IsNot Nothing Then progressForm.ShowCancellationReport()
        ElseIf e.Error IsNot Nothing Then
            MessageBox.Show("Error: " & e.Error.Message)
            If progressForm IsNot Nothing Then progressForm.Close()
        Else
            If progressForm IsNot Nothing Then
                progressForm.ShowFinalReportBackupDetailed(countProfilesBackedUp, countPasswordsExported, countRegistryBackedUp)
            End If
        End If
        Me.Enabled = True
    End Sub

    ' ==========================================
    ' دوال العمليات المساعدة
    ' ==========================================

    Private Sub PrepareBackupPaths()
        backupPaths.Clear()
        Dim desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        Dim dateStr = DateTime.Now.ToString("yyyy-MM-dd")
        Dim mainFolder = Path.Combine(desktopPath, "Chrome Backup " & dateStr)

        backupPaths.Add("Main", mainFolder)
        backupPaths.Add("UserData", Path.Combine(mainFolder, "Profiles"))
        backupPaths.Add("Registry", Path.Combine(mainFolder, "Registry"))
        backupPaths.Add("Passwords", Path.Combine(mainFolder, "Passwords"))
    End Sub

    Private Sub CreateBackupFolders(worker As BackgroundWorker, e As DoWorkEventArgs)
        For Each path In backupPaths.Values
            If worker.CancellationPending Then e.Cancel = True : Return
            Try
                If Not Directory.Exists(path) Then Directory.CreateDirectory(path)
            Catch
            End Try
        Next
    End Sub

    Private Sub BackupRegistry(destinationFolder As String)
        Try
            Dim regKeyPath = "HKEY_CURRENT_USER\Software\Google\Chrome\PreferenceMACs"
            Dim exportPath = Path.Combine(destinationFolder, "Chrome_PreferenceMACs.reg")
            Dim psi As New ProcessStartInfo("reg.exe") With {
                .Arguments = String.Format("export ""{0}"" ""{1}"" /y", regKeyPath, exportPath),
                .CreateNoWindow = True, .UseShellExecute = False
            }
            Process.Start(psi).WaitForExit()
        Catch
        End Try
    End Sub

    ' *** دالة نسخ البروفايلات (المرحلة الثانية: 10% إلى 60%) ***
    Private Sub BackupProfilesFilesOnly(worker As BackgroundWorker, e As DoWorkEventArgs,
                                       userDataDestFolder As String, profilesList As List(Of String))

        Dim userDataBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data")
        Dim localStatePath = Path.Combine(userDataBasePath, "Local State")
        If Not File.Exists(localStatePath) Then Return
        Dim jsonObject = JObject.Parse(File.ReadAllText(localStatePath))
        Dim profiles = TryCast(jsonObject.SelectToken("profile.info_cache"), JObject)
        If profiles Is Nothing Then Return

        ' نسخ الملفات الجذرية والمجلدات العامة
        CopyUserDataFiles(userDataBasePath, userDataDestFolder)

        Dim total = profilesList.Count
        For i = 0 To total - 1
            If worker.CancellationPending Then e.Cancel = True : Return
            Dim selectedProfileName = profilesList(i)
            Dim profileDirectoryName = GetProfileDirectoryName(profiles, selectedProfileName)
            If String.IsNullOrEmpty(profileDirectoryName) Then Continue For

            ' حساب النسبة لتتحرك ببطء ومنطقية
            Dim p = CInt(10 + ((i + 1) / total * 50)) ' Max 60%

            worker.ReportProgress(p, SimpleLanguageManager.GetForm4Text("copying_profile") & selectedProfileName)

            Dim source = Path.Combine(userDataBasePath, profileDirectoryName)
            Dim dest = Path.Combine(userDataDestFolder, profileDirectoryName)

            If Directory.Exists(source) Then
                CopyDirectory(source, dest)
                countProfilesBackedUp += 1
            End If
        Next
    End Sub

    ' *** دالة تصدير كلمات المرور (المرحلة الثالثة: 60% إلى 95%) ***
    Private Sub ExportPasswordsSequence(worker As BackgroundWorker, e As DoWorkEventArgs,
                                       passwordsDestFolder As String, profilesList As List(Of String))

        Dim userDataBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data")
        Dim localStatePath = Path.Combine(userDataBasePath, "Local State")
        If Not File.Exists(localStatePath) Then Return
        Dim jsonObject = JObject.Parse(File.ReadAllText(localStatePath))
        Dim profiles = TryCast(jsonObject.SelectToken("profile.info_cache"), JObject)

        Dim total = profilesList.Count
        For i = 0 To total - 1
            If worker.CancellationPending Then e.Cancel = True : Return
            Dim selectedProfileName = profilesList(i)
            Dim profileDirectoryName = GetProfileDirectoryName(profiles, selectedProfileName)
            If String.IsNullOrEmpty(profileDirectoryName) Then Continue For

            ' حساب النسبة
            Dim p = CInt(60 + ((i + 1) / total * 35)) ' Max 95%

            worker.ReportProgress(p, SimpleLanguageManager.GetForm4Text("exporting_pass") & selectedProfileName)

            ExportPasswordsForProfile(selectedProfileName, profileDirectoryName, passwordsDestFolder)
        Next
    End Sub

    ' ==========================================
    ' الدوال المساعدة (نسخ، تسمية، أتمتة)
    ' ==========================================

    Private Sub CopyUserDataFiles(sourceBasePath As String, destBasePath As String)
        Try
            ' نسخ الملفات
            For Each filePath In Directory.GetFiles(sourceBasePath)
                Try
                    IO.File.Copy(filePath, Path.Combine(destBasePath, Path.GetFileName(filePath)), True)
                Catch
                End Try
            Next
            ' نسخ المجلدات العامة
            For Each dirPath In Directory.GetDirectories(sourceBasePath)
                Dim dirName = New DirectoryInfo(dirPath).Name
                If Not dirName.StartsWith("Profile ", StringComparison.OrdinalIgnoreCase) AndAlso
                   Not dirName.Equals("Default", StringComparison.OrdinalIgnoreCase) Then
                    Try
                        CopyDirectory(dirPath, Path.Combine(destBasePath, dirName))
                    Catch
                    End Try
                End If
            Next
        Catch
        End Try
    End Sub

    Private Function GetProfileDirectoryName(profiles As JObject, profileName As String) As String
        For Each profile As JProperty In profiles.Properties()
            If profile.Value("name")?.ToString() = profileName Then Return profile.Name
        Next
        Return Nothing
    End Function

    Private Sub ExportPasswordsForProfile(profileName As String, profileDir As String, passwordsFolder As String)
        profileNameToProcess = profileName
        profileDirToProcess = profileDir
        passwordFilePathResult = ""

        Dim retry As Boolean = True
        While retry
            retry = False
            uiActionComplete.Reset()
            Me.Invoke(New MethodInvoker(AddressOf PerformPasswordExportUI))
            uiActionComplete.WaitOne()

            If passwordFilePathResult = "CANCELLED" Then Exit While

            Dim success As Boolean = False
            If Not String.IsNullOrEmpty(passwordFilePathResult) AndAlso File.Exists(passwordFilePathResult) Then
                Try
                    Dim fi As New FileInfo(passwordFilePathResult)
                    If fi.Length > 0 Then
                        ' الصيغة: الاسم (Dir-اسم المجلد).csv
                        Dim safeProfileName = MakeSafeFileName(profileName)
                        Dim safeDirName = MakeSafeFileName(profileDir)
                        Dim finalFileName = String.Format("{0} (Dir-{1}).csv", safeProfileName, safeDirName)
                        Dim destPath = Path.Combine(passwordsFolder, finalFileName)

                        If File.Exists(destPath) Then File.Delete(destPath)
                        File.Move(passwordFilePathResult, destPath)

                        countPasswordsExported += 1
                        success = True
                    End If
                Catch
                End Try
            End If

            If Not success AndAlso passwordFilePathResult <> "CANCELLED" Then
                Dim res As DialogResult = DialogResult.No
                Me.Invoke(Sub()
                              res = MessageBox.Show(SimpleLanguageManager.GetForm4Text("retry_msg"),
                                                  SimpleLanguageManager.GetForm4Text("retry_title"),
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                          End Sub)
                If res = DialogResult.Yes Then retry = True
            End If
        End While
    End Sub

    Private Sub PerformPasswordExportUI()
        Dim proc As Process = Nothing
        Try
            Dim chromePath = Form1.GetChromePath()
            Dim args = String.Format("--profile-directory=""{0}""", profileDirToProcess)
            proc = Process.Start(chromePath, args)

            proc.WaitForInputIdle(5000)
            Thread.Sleep(3000)

            If proc.MainWindowHandle <> IntPtr.Zero Then
                ShowWindow(proc.MainWindowHandle, SW_RESTORE)
                SetForegroundWindow(proc.MainWindowHandle)
            End If

            Try
                BlockInput(True)
                SendKeys.SendWait("^l")
                Thread.Sleep(300)
                SendKeys.SendWait("chrome://password-manager/settings")
                Thread.Sleep(300)
                SendKeys.SendWait("{ENTER}")
            Catch
            Finally
                BlockInput(False)
            End Try

            Thread.Sleep(2500)

            ' بناء نص الرسالة المترجم
            Dim msgBuilder As New System.Text.StringBuilder()
            If SimpleLanguageManager.IsArabic Then
                msgBuilder.AppendLine("المتصفح: " & profileNameToProcess)
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_1"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_2"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_3"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_4"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_5"))
            Else
                msgBuilder.AppendLine("Profile: " & profileNameToProcess)
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_1"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_2"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_3"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_4"))
                msgBuilder.AppendLine(SimpleLanguageManager.GetForm4Text("pass_msg_5"))
            End If

            Dim title As String = SimpleLanguageManager.GetForm4Text("pass_title")
            ' تحديد المكان (0, 0)
            Dim userInput As String = Interaction.InputBox(msgBuilder.ToString(), title, "chrome://password-manager/settings", 0, 0)

            If String.IsNullOrEmpty(userInput) Then
                passwordFilePathResult = "CANCELLED"
                Try
                    If proc IsNot Nothing AndAlso Not proc.HasExited Then proc.Kill()
                Catch
                End Try
                uiActionComplete.Set()
                Return
            End If

            Try
                If proc IsNot Nothing AndAlso Not proc.HasExited Then
                    Form1.CloseAllChromeBrowsersOldWay()
                End If
            Catch
            End Try

            Using ofd As New OpenFileDialog()
                ofd.Title = If(SimpleLanguageManager.IsArabic, "حدد ملف كلمات المرور الذي حفظته", "Select the saved passwords file")
                ofd.Filter = "CSV Files|*.csv"
                ofd.FileName = "Chrome Passwords.csv"
                If ofd.ShowDialog(New Form() With {.TopMost = True}) = DialogResult.OK Then
                    passwordFilePathResult = ofd.FileName
                End If
            End Using

        Catch ex As Exception
        Finally
            uiActionComplete.Set()
        End Try
    End Sub

    Private Function MakeSafeFileName(fileName As String) As String
        Dim safe = fileName
        For Each c In Path.GetInvalidFileNameChars()
            safe = safe.Replace(c, "-"c)
        Next
        Return safe
    End Function

    Private Sub CopyDirectory(sourceDir As String, destDir As String)
        If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)
        For Each filePath As String In Directory.GetFiles(sourceDir)
            Try
                IO.File.Copy(filePath, Path.Combine(destDir, Path.GetFileName(filePath)), True)
            Catch
            End Try
        Next
        For Each dirPath As String In Directory.GetDirectories(sourceDir)
            Try
                CopyDirectory(dirPath, Path.Combine(destDir, Path.GetFileName(dirPath)))
            Catch
            End Try
        Next
    End Sub

End Class