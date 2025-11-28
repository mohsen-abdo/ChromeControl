Imports System.IO
Imports System.Diagnostics
Imports Newtonsoft.Json.Linq
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms

Public Class Form1
    Private pauseOperation As Boolean = False
    Private cancelOperation As Boolean = False
    Private currentProfileIndex As Integer = 0
    Private profileOrder As New List(Of String)
    Private workerThread As Thread = Nothing
    Private isPaused As Boolean = False
    Private isArabic As Boolean = False

    ' دوال النظام
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    End Function
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function FindWindowEx(ByVal hwndParent As IntPtr, ByVal hwndChildAfter As IntPtr, ByVal lpszClass As String, ByVal lpszWindow As String) As IntPtr
    End Function
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As UInteger, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function
    <DllImport("user32.dll")>
    Private Shared Function BlockInput(ByVal fBlockIt As Boolean) As Boolean
    End Function

    Private Const WM_CLOSE As UInteger = &H10

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadChromeProfiles()
        SetLanguage(False)
    End Sub

    Private Sub SetLanguage(arabic As Boolean)
        isArabic = arabic
        SimpleLanguageManager.IsArabic = arabic
        Me.Text = SimpleLanguageManager.GetForm1Text("title")
        Label1.Text = SimpleLanguageManager.GetForm1Text("select_profiles")
        Button1.Text = SimpleLanguageManager.GetForm1Text("open_browsers")
        btnPause.Text = SimpleLanguageManager.GetForm1Text("pause")
        btnStop.Text = SimpleLanguageManager.GetForm1Text("stop")
        Button2.Text = SimpleLanguageManager.GetForm1Text("close_chrome")
        Button3.Text = SimpleLanguageManager.GetForm1Text("uncheck_all")
        Button4.Text = SimpleLanguageManager.GetForm1Text("select_list")
        Button5.Text = SimpleLanguageManager.GetForm1Text("clear_data")
        btnBackup.Text = SimpleLanguageManager.GetForm1Text("backup")
        Label2.Text = SimpleLanguageManager.GetForm1Text("enter_names")
        Label4.Text = SimpleLanguageManager.GetForm1Text("delay")
        Label5.Text = SimpleLanguageManager.GetForm1Text("url")
        Label6.Text = SimpleLanguageManager.GetForm1Text("footer")
        btnLanguage.Text = SimpleLanguageManager.GetForm1Text("lang_btn")
    End Sub

    Private Sub btnLanguage_Click(sender As Object, e As EventArgs) Handles btnLanguage.Click
        SetLanguage(Not isArabic)
    End Sub

    Private Sub LoadChromeProfiles()
        Try
            Dim localStatePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data\Local State")
            If File.Exists(localStatePath) Then
                Dim jsonContent As String = File.ReadAllText(localStatePath)
                Dim jsonObject As JObject = JObject.Parse(jsonContent)
                Dim profiles As JObject = jsonObject.SelectToken("profile.info_cache")
                If profiles IsNot Nothing Then
                    Dim profileNames As New Dictionary(Of String, String)
                    For Each profile As JProperty In profiles.Properties()
                        Dim profileData As JObject = CType(profile.Value, JObject)
                        Dim profileName As String = profileData("name").ToString()
                        profileNames.Add(profileName, profile.Name)
                    Next
                    Dim sortedNames = profileNames.Keys.ToList()
                    sortedNames.Sort()
                    CheckedListBox1.Items.AddRange(sortedNames.ToArray())
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If CheckedListBox1.CheckedItems.Count = 0 Then
            MessageBox.Show(If(isArabic, "الرجاء تحديد ملف تعريف واحد على الأقل لفتحه.", "Please select at least one browser profile."),
                          If(isArabic, "تنبيه", "Notice"), MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        If isPaused Then
            pauseOperation = False
            isPaused = False
            btnPause.Text = SimpleLanguageManager.GetForm1Text("pause")
            btnPause.BackColor = Color.FromArgb(255, 165, 0)
            Button1.Enabled = False
            lblProgress.Visible = True
            Exit Sub
        End If

        btnPause.Enabled = True
        btnStop.Enabled = True
        Button1.Enabled = False
        cancelOperation = False
        pauseOperation = False
        isPaused = False
        currentProfileIndex = 0
        profileOrder.Clear()

        Dim inputOrder As String = TextBox1.Text.Trim()
        If Not String.IsNullOrEmpty(inputOrder) Then
            Dim separators() As String = {Environment.NewLine, ",", ";", ":"}
            Dim names() As String = inputOrder.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            For Each n In names
                profileOrder.Add(n.Trim())
            Next
        Else
            For i As Integer = 0 To CheckedListBox1.Items.Count - 1
                If CheckedListBox1.GetItemChecked(i) Then
                    profileOrder.Add(CheckedListBox1.Items(i).ToString())
                End If
            Next
        End If

        workerThread = New Thread(AddressOf OpenBrowsersThread)
        workerThread.IsBackground = True
        workerThread.Start()
    End Sub

    Private Sub OpenBrowsersThread()
        Try
            Dim localStatePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\Chrome\User Data\Local State")
            Dim jsonContent As String = File.ReadAllText(localStatePath)
            Dim jsonObject As JObject = JObject.Parse(jsonContent)
            Dim profiles As JObject = jsonObject.SelectToken("profile.info_cache")

            Dim delaySeconds As Integer = 7
            Me.Invoke(Sub()
                          If Not String.IsNullOrEmpty(TextBox2.Text.Trim()) Then
                              Integer.TryParse(TextBox2.Text.Trim(), delaySeconds)
                          End If
                      End Sub)
            Dim delayMs As Integer = delaySeconds * 1000
            Dim targetUrl As String = ""
            Me.Invoke(Sub() targetUrl = TextBox3.Text.Trim())
            Dim chromePath As String = GetChromePath()

            For i As Integer = currentProfileIndex To profileOrder.Count - 1
                Dim currentIndex As Integer = i
                Me.Invoke(Sub()
                              lblProgress.Text = $"Opening {currentIndex + 1} of {profileOrder.Count}"
                              lblProgress.Visible = True
                              ProgressBar1.Maximum = profileOrder.Count
                              ProgressBar1.Value = currentIndex + 1
                              Application.DoEvents()
                          End Sub)

                While pauseOperation AndAlso Not cancelOperation
                    Thread.Sleep(100)
                End While

                If cancelOperation Then Exit For

                Dim selectedProfileName As String = profileOrder(i)
                Dim currentProfile As String = selectedProfileName
                Dim isChecked As Boolean = False

                Me.Invoke(Sub()
                              For j As Integer = 0 To CheckedListBox1.Items.Count - 1
                                  If CheckedListBox1.Items(j).ToString().Equals(currentProfile, StringComparison.OrdinalIgnoreCase) Then
                                      isChecked = CheckedListBox1.GetItemChecked(j)
                                      Exit For
                                  End If
                              Next
                          End Sub)

                If isChecked Then
                    Dim profileDirectory As String = ""
                    For Each profile As JProperty In profiles.Properties()
                        Dim profileData As JObject = CType(profile.Value, JObject)
                        If profileData("name").ToString() = selectedProfileName Then
                            profileDirectory = profile.Name
                            Exit For
                        End If
                    Next

                    If Not String.IsNullOrEmpty(profileDirectory) Then
                        If cancelOperation Then Exit For

                        ' تشغيل المتصفح
                        Process.Start(chromePath, $"--profile-directory=""{profileDirectory}""")

                        If Not String.IsNullOrEmpty(targetUrl) Then
                            ' انتظار ظهور النافذة وتحميلها (3 ثواني)
                            Thread.Sleep(3000)

                            ' *** التعديل هنا: إزالة الشرط المعتمد على Process Handle ***
                            ' سنفترض أن النافذة النشطة الآن هي الكروم الذي فتحناه للتو
                            Try
                                BlockInput(True) ' تجميد الماوس والكيبورد
                                SendKeys.SendWait("^l") ' Ctrl+L
                                Thread.Sleep(100)
                                SendKeys.SendWait(targetUrl) ' كتابة الرابط
                                Thread.Sleep(100)
                                SendKeys.SendWait("{ENTER}") ' انتر
                            Catch ex As Exception
                                ' تجاهل الأخطاء
                            Finally
                                BlockInput(False) ' فك التجميد ضرورى جداً
                            End Try
                        End If
                    End If
                    currentProfileIndex = i + 1
                    If Not cancelOperation Then Thread.Sleep(delayMs)
                End If
            Next

            Me.Invoke(Sub()
                          Button1.Enabled = True
                          btnPause.Enabled = False
                          btnStop.Enabled = False
                          btnPause.Text = SimpleLanguageManager.GetForm1Text("pause")
                          btnPause.BackColor = Color.FromArgb(255, 165, 0)
                          lblProgress.Visible = False
                          ProgressBar1.Value = 0
                          currentProfileIndex = 0
                          isPaused = False
                          BlockInput(False)
                          If Not cancelOperation Then
                              MessageBox.Show(SimpleLanguageManager.GetForm1Text("done_msg"),
                                            SimpleLanguageManager.GetForm1Text("done"), MessageBoxButtons.OK, MessageBoxIcon.Information)
                          End If
                      End Sub)
        Catch ex As Exception
            BlockInput(False)
        End Try
    End Sub

    ' دالة الإغلاق القديمة (WM_CLOSE)
    Public Shared Sub CloseAllChromeBrowsersOldWay()
        Try
            Dim chromeHandle As IntPtr = FindWindow("Chrome_WidgetWin_1", Nothing)
            Dim maxLoop As Integer = 0
            While chromeHandle <> IntPtr.Zero AndAlso maxLoop < 50
                SendMessage(chromeHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero)
                Thread.Sleep(150)
                chromeHandle = FindWindowEx(IntPtr.Zero, chromeHandle, "Chrome_WidgetWin_1", Nothing)
                If chromeHandle = IntPtr.Zero Then
                    chromeHandle = FindWindow("Chrome_WidgetWin_1", Nothing)
                End If
                maxLoop += 1
            End While
            Thread.Sleep(2000)
        Catch ex As Exception
        End Try
    End Sub

    Public Shared Function GetChromePath() As String
        Dim path1 As String = "C:\Program Files\Google\Chrome\Application\chrome.exe"
        Dim path2 As String = "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
        If File.Exists(path1) Then Return path1
        If File.Exists(path2) Then Return path2
        Return path1
    End Function

    Private Sub btnPause_Click(sender As Object, e As EventArgs) Handles btnPause.Click
        If Not isPaused Then
            pauseOperation = True
            isPaused = True
            btnPause.Text = SimpleLanguageManager.GetForm1Text("resume")
            btnPause.BackColor = Color.FromArgb(144, 238, 144)
            Button1.Enabled = False
        Else
            pauseOperation = False
            isPaused = False
            btnPause.Text = SimpleLanguageManager.GetForm1Text("pause")
            btnPause.BackColor = Color.FromArgb(255, 165, 0)
        End If
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        cancelOperation = True
        pauseOperation = False
        isPaused = False
        btnStop.Enabled = False
        btnPause.Enabled = False
        currentProfileIndex = 0
        BlockInput(False)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        CloseAllChromeBrowsersOldWay()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        For i As Integer = 0 To CheckedListBox1.Items.Count - 1
            CheckedListBox1.SetItemChecked(i, False)
        Next
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim inputText As String = TextBox1.Text.Trim()
        If String.IsNullOrEmpty(inputText) Then Exit Sub
        Dim separators() As String = {Environment.NewLine, ",", ";", ":"}
        Dim profileNames() As String = inputText.Split(separators, StringSplitOptions.RemoveEmptyEntries)
        For Each name As String In profileNames
            Dim trimmedName As String = name.Trim()
            For i As Integer = 0 To CheckedListBox1.Items.Count - 1
                If CheckedListBox1.Items(i).ToString().Equals(trimmedName, StringComparison.OrdinalIgnoreCase) Then
                    CheckedListBox1.SetItemChecked(i, True)
                    Exit For
                End If
            Next
        Next
    End Sub

    Private Sub CheckedListBox1_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox1.ItemCheck
        Dim newCount As Integer = CheckedListBox1.CheckedItems.Count
        If e.NewValue = CheckState.Checked Then newCount += 1 Else newCount -= 1
        Label3.Text = newCount.ToString()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        SimpleLanguageManager.IsArabic = isArabic
        Dim cleaningForm As New Form2()
        cleaningForm.ShowDialog()
    End Sub

    Private Sub btnBackup_Click(sender As Object, e As EventArgs) Handles btnBackup.Click
        SimpleLanguageManager.IsArabic = isArabic
        Dim backupForm As New Form3()
        backupForm.ShowDialog()
    End Sub
End Class