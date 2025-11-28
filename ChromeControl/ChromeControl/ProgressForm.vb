Imports System.Windows.Forms
Imports System.Globalization

Public Class ProgressForm

    ' ... (Load and Design code same as before) ...
    Public Worker As System.ComponentModel.BackgroundWorker
    Private _currentTarget As Integer = 0
    Private _currentValue As Integer = 0
    Private currentOperationType As String = ""

    Private Sub ProgressForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AcceptButton = Nothing
        Me.CancelButton = Nothing
        btnCancel.DialogResult = DialogResult.None
        Me.ActiveControl = lblPercentage
        Me.Location = New Point(20, 250)
        ApplyLanguage()
    End Sub

    Private Sub ApplyLanguage()
        Me.Text = SimpleLanguageManager.GetForm3Text("title")
        btnCancel.Text = SimpleLanguageManager.GetForm3Text("cancel")
        btnClose.Text = SimpleLanguageManager.GetForm3Text("close")
    End Sub

    Public Sub SetProgressTarget(targetPercentage As Integer, message As String)
        If targetPercentage > 100 Then targetPercentage = 100
        _currentTarget = targetPercentage
        lblStatus.Text = message
        If Not AnimationTimer.Enabled Then AnimationTimer.Start()
    End Sub

    Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs) Handles AnimationTimer.Tick
        If _currentValue < _currentTarget Then
            _currentValue += 1
        Else
            AnimationTimer.Stop()
        End If
        ProgressBar1.Maximum = 100
        ProgressBar1.Value = _currentValue
        lblPercentage.Text = _currentValue.ToString(CultureInfo.InvariantCulture) & "%"
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Dim confirmMsg = SimpleLanguageManager.GetForm3Text("confirm_cancel")
        If MessageBox.Show(New Form() With {.TopMost = True}, confirmMsg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            If Worker IsNot Nothing AndAlso Worker.IsBusy Then
                Worker.CancelAsync()
                btnCancel.Enabled = False
                btnCancel.Text = "..."
            End If
        End If
    End Sub

    Public Sub ShowFinalReport(ByVal cleanedCount As Integer, ByVal totalSizeFormatted As String)
        currentOperationType = "Clean"
        HideProgressControls()

        Dim countString As String = ChrW(&H200E) & cleanedCount.ToString(CultureInfo.InvariantCulture)

        If SimpleLanguageManager.IsArabic Then
            lblFinalReport.Text = "تمت العملية بنجاح!" & vbCrLf & vbCrLf &
                                 "تم تنظيف: " & countString & " متصفح" & vbCrLf &
                                 "المساحة: " & totalSizeFormatted
        Else
            lblFinalReport.Text = "Success!" & vbCrLf & vbCrLf &
                                 "Cleaned: " & countString & " Profiles" & vbCrLf &
                                 "Space: " & totalSizeFormatted
        End If
    End Sub

    Public Sub ShowFinalReportBackupDetailed(ByVal profilesCount As Integer, ByVal passwordsCount As Integer, ByVal registryCount As Integer)
        currentOperationType = "Backup"

        ProgressBar1.Value = 100
        lblPercentage.Text = "100%"
        System.Threading.Thread.Sleep(500)
        Application.DoEvents()

        HideProgressControls()

        ' فرض تنسيق الأرقام الإنجليزية
        Dim pCount As String = ChrW(&H200E) & profilesCount.ToString(CultureInfo.InvariantCulture)
        Dim pwCount As String = ChrW(&H200E) & passwordsCount.ToString(CultureInfo.InvariantCulture)
        Dim rCount As String = ChrW(&H200E) & registryCount.ToString(CultureInfo.InvariantCulture)

        If SimpleLanguageManager.IsArabic Then
            lblFinalReport.Text = "تم النسخ الاحتياطي بنجاح!" & vbCrLf &
                                 "----------------" & vbCrLf &
                                 "البروفايلات: " & pCount & vbCrLf &
                                 "كلمات المرور: " & pwCount & vbCrLf &
                                 "الريجستري: " & rCount
        Else
            lblFinalReport.Text = "Backup Completed!" & vbCrLf &
                                 "----------------" & vbCrLf &
                                 "Profiles: " & pCount & vbCrLf &
                                 "Passwords: " & pwCount & vbCrLf &
                                 "Registry: " & rCount
        End If
    End Sub

    Public Sub ShowCancellationReport()
        HideProgressControls()
        lblFinalReport.ForeColor = Color.Red
        lblFinalReport.Text = SimpleLanguageManager.GetForm3Text("cancelled_user")
    End Sub

    Private Sub HideProgressControls()
        btnCancel.Visible = False
        lblStatus.Visible = False
        ProgressBar1.Visible = False
        lblPercentage.Visible = False

        lblFinalReport.Visible = True
        lblFinalReport.BringToFront()

        btnClose.Visible = True
        btnClose.BringToFront()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class