Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Threading
Imports System.ComponentModel
Imports System.Diagnostics

Public Class Form2

    Private WithEvents bw As New BackgroundWorker()
    Private progressForm As ProgressForm
    Private totalDeletedBytes As Long = 0
    Private cleanedProfilesCount As Integer = 0
    Private selectedDataTypes As New Dictionary(Of String, Boolean)
    Private selectedProfileNamesForWorker As New List(Of String)

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ApplyLanguage()
        LoadChromeProfiles()
        Label3.Text = "0"
        bw.WorkerReportsProgress = True
        bw.WorkerSupportsCancellation = True
    End Sub

    Private Sub ApplyLanguage()
        Me.Text = SimpleLanguageManager.GetForm2Text("title")
        CheckBox1.Text = SimpleLanguageManager.GetForm2Text("browsing_history")
        CheckBox2.Text = SimpleLanguageManager.GetForm2Text("download_history")
        CheckBox3.Text = SimpleLanguageManager.GetForm2Text("cookies")
        CheckBox4.Text = SimpleLanguageManager.GetForm2Text("cached")
        CheckBox8.Text = SimpleLanguageManager.GetForm2Text("passwords")
        CheckBox7.Text = SimpleLanguageManager.GetForm2Text("autofill")
        CheckBox6.Text = SimpleLanguageManager.GetForm2Text("site_settings")
        CheckBox5.Text = SimpleLanguageManager.GetForm2Text("hosted_data")
        Label11.Text = SimpleLanguageManager.GetForm2Text("select_clean")
        Button1.Text = SimpleLanguageManager.GetForm2Text("check_all")
        Button2.Text = SimpleLanguageManager.GetForm2Text("uncheck_all")
        Button3.Text = SimpleLanguageManager.GetForm2Text("clean_btn")
        Label14.Text = SimpleLanguageManager.GetForm2Text("selected_count")
    End Sub

    Private Sub LoadChromeProfiles()
        Dim localStatePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data\Local State")
        If File.Exists(localStatePath) Then
            Dim jsonContent As String = File.ReadAllText(localStatePath)
            Dim jsonObject As JObject = JObject.Parse(jsonContent)
            Dim profiles As JObject = jsonObject.SelectToken("profile.info_cache")
            If profiles IsNot Nothing Then
                Dim profileNames As New List(Of String)
                For Each profile As JProperty In profiles.Properties()
                    profileNames.Add(profile.Value("name").ToString())
                Next
                profileNames.Sort()
                CheckedListBox1.Items.Clear()
                CheckedListBox1.Items.AddRange(profileNames.ToArray())
            End If
        End If
    End Sub

    Private Sub CheckedListBox1_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox1.ItemCheck
        Dim newCount As Integer = CheckedListBox1.CheckedItems.Count
        If e.NewValue = CheckState.Checked Then newCount += 1 Else newCount -= 1
        Label3.Text = newCount.ToString()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        For i As Integer = 0 To CheckedListBox1.Items.Count - 1
            CheckedListBox1.SetItemChecked(i, True)
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        For i As Integer = 0 To CheckedListBox1.Items.Count - 1
            CheckedListBox1.SetItemChecked(i, False)
        Next
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If CheckedListBox1.CheckedItems.Count = 0 Then
            MessageBox.Show(If(SimpleLanguageManager.IsArabic, "الرجاء تحديد ملف تعريف واحد على الأقل.", "Please Select At Least One Profile."),
                          If(SimpleLanguageManager.IsArabic, "تنبيه", "Notice"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If Not (CheckBox1.Checked Or CheckBox2.Checked Or CheckBox3.Checked Or CheckBox4.Checked Or CheckBox5.Checked Or CheckBox6.Checked Or CheckBox7.Checked Or CheckBox8.Checked) Then
            MessageBox.Show(If(SimpleLanguageManager.IsArabic, "الرجاء تحديد نوع بيانات للحذف.", "Please Select Data Type To Delete."),
                          If(SimpleLanguageManager.IsArabic, "تنبيه", "Notice"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If bw.IsBusy Then
            MessageBox.Show(If(SimpleLanguageManager.IsArabic, "عملية تنظيف أخرى قيد التشغيل.", "Another Cleaning Operation Is Running."),
                          If(SimpleLanguageManager.IsArabic, "يرجى الانتظار", "Please Wait"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If
        If MessageBox.Show(If(SimpleLanguageManager.IsArabic, "سيتم إغلاق جميع نوافذ كروم وحذف البيانات المحددة. هل أنت متأكد؟", "All Chrome Windows Will Be Closed And Selected Data Will Be Deleted. Continue?"),
                         If(SimpleLanguageManager.IsArabic, "تأكيد", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return

        totalDeletedBytes = 0
        cleanedProfilesCount = 0
        selectedDataTypes.Clear()
        selectedDataTypes.Add("History", CheckBox1.Checked Or CheckBox2.Checked)
        selectedDataTypes.Add("Cookies", CheckBox3.Checked)
        selectedDataTypes.Add("Cache", CheckBox4.Checked)
        selectedDataTypes.Add("Passwords", CheckBox8.Checked)
        selectedDataTypes.Add("WebData", CheckBox7.Checked)
        selectedDataTypes.Add("SiteData", CheckBox6.Checked)

        selectedProfileNamesForWorker.Clear()
        For Each item In CheckedListBox1.CheckedItems
            selectedProfileNamesForWorker.Add(item.ToString())
        Next

        Me.Enabled = False
        progressForm = New ProgressForm()
        progressForm.Worker = bw
        progressForm.Show(Me)
        bw.RunWorkerAsync()
    End Sub

    Private Sub bw_DoWork(sender As Object, e As DoWorkEventArgs) Handles bw.DoWork
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)

        ' Step 1: Close Chrome (in background thread)
        worker.ReportProgress(0, If(SimpleLanguageManager.IsArabic, "جاري إغلاق متصفح كروم...", "Closing Chrome Processes..."))
        ShutdownChromeProcesses()

        If worker.CancellationPending Then e.Cancel = True : Return

        Dim localStatePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data\Local State")
        If Not File.Exists(localStatePath) Then Return

        Dim jsonObject As JObject
        Try
            jsonObject = JObject.Parse(File.ReadAllText(localStatePath))
        Catch
            Return
        End Try

        Dim profiles As JObject = jsonObject.SelectToken("profile.info_cache")
        If profiles Is Nothing Then Return

        Dim totalProfiles As Integer = selectedProfileNamesForWorker.Count
        Dim profileIndex As Integer = 0

        For Each selectedProfileName As String In selectedProfileNamesForWorker
            If worker.CancellationPending Then e.Cancel = True : Return
            profileIndex += 1
            Dim percentage = CInt((CDbl(profileIndex) / CDbl(totalProfiles)) * 100)

            worker.ReportProgress(percentage, If(SimpleLanguageManager.IsArabic, $"جاري التنظيف: {selectedProfileName}", $"Cleaning: {selectedProfileName}"))

            Dim profileDirectoryName As String = ""
            For Each profile As JProperty In profiles.Properties()
                If profile.Value("name").ToString() = selectedProfileName Then
                    profileDirectoryName = profile.Name
                    Exit For
                End If
            Next

            If Not String.IsNullOrEmpty(profileDirectoryName) Then
                Dim userDataPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data")
                Dim profilePath As String = Path.Combine(userDataPath, profileDirectoryName)

                If selectedDataTypes("History") Then totalDeletedBytes += DeleteFileAndGetSize(Path.Combine(profilePath, "History"))
                If selectedDataTypes("Cookies") Then
                    Dim networkPath = Path.Combine(profilePath, "Network")
                    totalDeletedBytes += DeleteFileAndGetSize(Path.Combine(networkPath, "Cookies"))
                End If
                If selectedDataTypes("Cache") Then
                    totalDeletedBytes += DeleteDirectoryAndGetSize(Path.Combine(profilePath, "Cache\Cache_Data"))
                    totalDeletedBytes += DeleteDirectoryAndGetSize(Path.Combine(profilePath, "Code Cache"))
                    totalDeletedBytes += DeleteDirectoryAndGetSize(Path.Combine(profilePath, "Media Cache"))
                End If
                If selectedDataTypes("Passwords") Then totalDeletedBytes += DeleteFileAndGetSize(Path.Combine(profilePath, "Login Data"))
                If selectedDataTypes("WebData") Then totalDeletedBytes += DeleteFileAndGetSize(Path.Combine(profilePath, "Web Data"))
                If selectedDataTypes("SiteData") Then
                    totalDeletedBytes += DeleteDirectoryAndGetSize(Path.Combine(profilePath, "Local Storage"))
                    totalDeletedBytes += DeleteDirectoryAndGetSize(Path.Combine(profilePath, "IndexedDB"))
                End If
                cleanedProfilesCount += 1
            End If

            Thread.Sleep(100) ' Small delay to prevent UI freeze
        Next
    End Sub

    Private Sub bw_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles bw.ProgressChanged
        progressForm.SetProgressTarget(e.ProgressPercentage, e.UserState.ToString())
        Application.DoEvents() ' Prevent Not Responding
    End Sub

    Private Sub bw_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles bw.RunWorkerCompleted
        If e.Error IsNot Nothing Then
            MessageBox.Show(If(SimpleLanguageManager.IsArabic, "خطأ غير متوقع: ", "Unexpected Error: ") & e.Error.Message,
                          If(SimpleLanguageManager.IsArabic, "خطأ", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            If progressForm IsNot Nothing Then progressForm.Close()
        ElseIf e.Cancelled Then
            progressForm.ShowCancellationReport()
            MessageBox.Show(If(SimpleLanguageManager.IsArabic, "تم إلغاء عملية التنظيف.", "Cleaning Operation Cancelled."),
                          If(SimpleLanguageManager.IsArabic, "تم الإلغاء", "Cancelled"), MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            progressForm.ShowFinalReport(cleanedProfilesCount, FormatBytes(totalDeletedBytes))
        End If
        Me.Enabled = True
    End Sub

    Private Function DeleteFileAndGetSize(ByVal filePath As String) As Long
        Dim fileSize As Long = 0
        Try
            If File.Exists(filePath) Then
                fileSize = New FileInfo(filePath).Length
                File.Delete(filePath)
            End If
        Catch
        End Try
        Return fileSize
    End Function

    Private Function GetDirectorySizeRecursive(ByVal d As DirectoryInfo) As Long
        Dim size As Long = 0
        Try
            For Each fi As FileInfo In d.GetFiles()
                size += fi.Length
            Next
            For Each di As DirectoryInfo In d.GetDirectories()
                size += GetDirectorySizeRecursive(di)
            Next
        Catch
        End Try
        Return size
    End Function

    Private Function DeleteDirectoryAndGetSize(ByVal dirPath As String) As Long
        Dim dirSize As Long = 0
        Try
            If Directory.Exists(dirPath) Then
                dirSize = GetDirectorySizeRecursive(New DirectoryInfo(dirPath))
                Directory.Delete(dirPath, True)
            End If
        Catch
        End Try
        Return dirSize
    End Function

    Private Function FormatBytes(ByVal bytes As Long) As String
        Dim sizes() As String = {"Bytes", "KB", "MB", "GB", "TB"}
        If bytes = 0 Then Return "0 Bytes"
        Dim i As Integer = CInt(Math.Floor(Math.Log(bytes) / Math.Log(1024)))
        Dim size As Double = Math.Round(bytes / Math.Pow(1024, i), 2)
        Return ChrW(&H200E) & size.ToString(System.Globalization.CultureInfo.InvariantCulture) & " " & sizes(i)
    End Function

    Private Sub ShutdownChromeProcesses()
        Try
            ' Method 1: Close windows gracefully
            Dim maxAttempts As Integer = 5
            Dim attempts As Integer = 0

            While attempts < maxAttempts
                Dim chromeProcesses = Process.GetProcessesByName("chrome")
                If chromeProcesses.Length = 0 Then Exit While

                For Each p In chromeProcesses
                    Try
                        If Not p.HasExited AndAlso p.MainWindowHandle <> IntPtr.Zero Then
                            p.CloseMainWindow()
                        End If
                    Catch
                    End Try
                Next

                Thread.Sleep(500)
                attempts += 1
            End While

            ' Method 2: Force kill remaining processes
            Thread.Sleep(1500)
            For Each p As Process In Process.GetProcessesByName("chrome")
                Try
                    If Not p.HasExited Then
                        p.Kill()
                        p.WaitForExit(2000) ' Wait up to 2 seconds
                    End If
                Catch
                End Try
            Next

        Catch ex As Exception
            Debug.WriteLine("ShutdownChromeProcesses error: " & ex.Message)
        End Try
    End Sub

End Class