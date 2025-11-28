<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ProgressForm
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ProgressForm))
        Me.lblPercentage = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblFinalReport = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.AnimationTimer = New System.Windows.Forms.Timer(Me.components)
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.SuspendLayout()
        '
        'lblPercentage
        '
        Me.lblPercentage.BackColor = System.Drawing.Color.Transparent
        Me.lblPercentage.Font = New System.Drawing.Font("Segoe UI Semibold", 11.0!, System.Drawing.FontStyle.Bold)
        Me.lblPercentage.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblPercentage.Location = New System.Drawing.Point(12, 15)
        Me.lblPercentage.Name = "lblPercentage"
        Me.lblPercentage.Size = New System.Drawing.Size(460, 25)
        Me.lblPercentage.TabIndex = 198
        Me.lblPercentage.Text = "0%"
        Me.lblPercentage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblStatus
        '
        Me.lblStatus.BackColor = System.Drawing.Color.Transparent
        Me.lblStatus.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.lblStatus.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblStatus.Location = New System.Drawing.Point(12, 80)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(460, 60)
        Me.lblStatus.TabIndex = 197
        Me.lblStatus.Text = "Processing..."
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.lblStatus.AutoEllipsis = True
        '
        'lblFinalReport
        '
        Me.lblFinalReport.BackColor = System.Drawing.Color.White
        Me.lblFinalReport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblFinalReport.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!, System.Drawing.FontStyle.Bold)
        Me.lblFinalReport.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(120, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.lblFinalReport.Location = New System.Drawing.Point(0, 0)
        Me.lblFinalReport.Name = "lblFinalReport"
        Me.lblFinalReport.Padding = New System.Windows.Forms.Padding(10, 10, 10, 50)
        Me.lblFinalReport.Size = New System.Drawing.Size(484, 211)
        Me.lblFinalReport.TabIndex = 199
        Me.lblFinalReport.Text = "Report Text Here"
        Me.lblFinalReport.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblFinalReport.Visible = False
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.SystemColors.Control
        Me.btnClose.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnClose.Location = New System.Drawing.Point(182, 155)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(120, 35)
        Me.btnClose.TabIndex = 200
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = False
        Me.btnClose.Visible = False
        '
        'AnimationTimer
        '
        Me.AnimationTimer.Interval = 50
        '
        'btnCancel
        '
        Me.btnCancel.BackColor = System.Drawing.Color.MistyRose
        Me.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray
        Me.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCancel.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnCancel.Location = New System.Drawing.Point(182, 155)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(120, 35)
        Me.btnCancel.TabIndex = 201
        Me.btnCancel.TabStop = False
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 45)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(460, 25)
        Me.ProgressBar1.TabIndex = 196
        '
        'ProgressForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(484, 211)
        Me.ControlBox = False
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.lblFinalReport)
        Me.Controls.Add(Me.lblPercentage)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblStatus)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "ProgressForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Processing..."
        Me.TopMost = False
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblPercentage As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblFinalReport As Label
    Friend WithEvents btnClose As Button
    Friend WithEvents AnimationTimer As Timer
    Friend WithEvents btnCancel As Button
    Friend WithEvents ProgressBar1 As ProgressBar
End Class