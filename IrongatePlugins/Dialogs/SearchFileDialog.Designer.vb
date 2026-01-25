<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SearchFileDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SearchFileDialog))
    Me.statStrip = New System.Windows.Forms.StatusStrip()
    Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
    Me.OK_Button = New System.Windows.Forms.Button()
    Me.Cancel_Button = New System.Windows.Forms.Button()
    Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
    Me.btnSpeech = New System.Windows.Forms.Button()
    Me.Label1 = New System.Windows.Forms.Label()
    Me.cmbFiles = New System.Windows.Forms.ComboBox()
    Me.pnlVariables = New System.Windows.Forms.Panel()
    Me.txtURI = New System.Windows.Forms.TextBox()
    Me.pnlFound = New System.Windows.Forms.Panel()
    Me.txtClosestMatch = New System.Windows.Forms.TextBox()
    Me.statStatus = New System.Windows.Forms.ToolStripStatusLabel()
    Me.statSpeech = New System.Windows.Forms.ToolStripStatusLabel()
    Me.statStrip.SuspendLayout()
    Me.TableLayoutPanel1.SuspendLayout()
    Me.TableLayoutPanel2.SuspendLayout()
    Me.pnlFound.SuspendLayout()
    Me.SuspendLayout()
    '
    'statStrip
    '
    Me.statStrip.ImageScalingSize = New System.Drawing.Size(20, 20)
    Me.statStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statStatus, Me.statSpeech})
    Me.statStrip.Location = New System.Drawing.Point(0, 580)
    Me.statStrip.Name = "statStrip"
    Me.statStrip.Size = New System.Drawing.Size(794, 25)
    Me.statStrip.TabIndex = 1
    Me.statStrip.Text = "StatusStrip1"
    '
    'TableLayoutPanel1
    '
    Me.TableLayoutPanel1.ColumnCount = 2
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
    Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom
    Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 530)
    Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(6, 7, 6, 7)
    Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
    Me.TableLayoutPanel1.RowCount = 1
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.Size = New System.Drawing.Size(794, 50)
    Me.TableLayoutPanel1.TabIndex = 2
    '
    'OK_Button
    '
    Me.OK_Button.Dock = System.Windows.Forms.DockStyle.Fill
    Me.OK_Button.Location = New System.Drawing.Point(6, 7)
    Me.OK_Button.Margin = New System.Windows.Forms.Padding(6, 7, 6, 7)
    Me.OK_Button.Name = "OK_Button"
    Me.OK_Button.Size = New System.Drawing.Size(385, 36)
    Me.OK_Button.TabIndex = 0
    Me.OK_Button.Text = "OK"
    '
    'Cancel_Button
    '
    Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.Cancel_Button.Dock = System.Windows.Forms.DockStyle.Fill
    Me.Cancel_Button.Location = New System.Drawing.Point(403, 7)
    Me.Cancel_Button.Margin = New System.Windows.Forms.Padding(6, 7, 6, 7)
    Me.Cancel_Button.Name = "Cancel_Button"
    Me.Cancel_Button.Size = New System.Drawing.Size(385, 36)
    Me.Cancel_Button.TabIndex = 1
    Me.Cancel_Button.Text = "Cancel"
    '
    'TableLayoutPanel2
    '
    Me.TableLayoutPanel2.ColumnCount = 3
    Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
    Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel2.Controls.Add(Me.btnSpeech, 0, 0)
    Me.TableLayoutPanel2.Controls.Add(Me.Label1, 1, 0)
    Me.TableLayoutPanel2.Controls.Add(Me.cmbFiles, 2, 0)
    Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top
    Me.TableLayoutPanel2.Location = New System.Drawing.Point(0, 0)
    Me.TableLayoutPanel2.MinimumSize = New System.Drawing.Size(0, 48)
    Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
    Me.TableLayoutPanel2.RowCount = 1
    Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
    Me.TableLayoutPanel2.Size = New System.Drawing.Size(794, 48)
    Me.TableLayoutPanel2.TabIndex = 3
    '
    'btnSpeech
    '
    Me.btnSpeech.BackgroundImage = CType(resources.GetObject("btnSpeech.BackgroundImage"), System.Drawing.Image)
    Me.btnSpeech.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
    Me.btnSpeech.Cursor = System.Windows.Forms.Cursors.Hand
    Me.btnSpeech.Dock = System.Windows.Forms.DockStyle.Fill
    Me.btnSpeech.FlatStyle = System.Windows.Forms.FlatStyle.Popup
    Me.btnSpeech.Location = New System.Drawing.Point(3, 3)
    Me.btnSpeech.Name = "btnSpeech"
    Me.btnSpeech.Size = New System.Drawing.Size(42, 42)
    Me.btnSpeech.TabIndex = 0
    Me.btnSpeech.UseVisualStyleBackColor = True
    '
    'Label1
    '
    Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.Label1.Location = New System.Drawing.Point(51, 0)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(367, 48)
    Me.Label1.TabIndex = 1
    Me.Label1.Text = "What file are you looking for?"
    Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
    '
    'cmbFiles
    '
    Me.cmbFiles.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbFiles.FormattingEnabled = True
    Me.cmbFiles.Location = New System.Drawing.Point(424, 3)
    Me.cmbFiles.Name = "cmbFiles"
    Me.cmbFiles.Size = New System.Drawing.Size(367, 36)
    Me.cmbFiles.TabIndex = 2
    '
    'pnlVariables
    '
    Me.pnlVariables.AutoScroll = True
    Me.pnlVariables.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.pnlVariables.Dock = System.Windows.Forms.DockStyle.Top
    Me.pnlVariables.Location = New System.Drawing.Point(0, 48)
    Me.pnlVariables.Name = "pnlVariables"
    Me.pnlVariables.Size = New System.Drawing.Size(794, 289)
    Me.pnlVariables.TabIndex = 4
    '
    'txtURI
    '
    Me.txtURI.Dock = System.Windows.Forms.DockStyle.Top
    Me.txtURI.Location = New System.Drawing.Point(0, 34)
    Me.txtURI.Name = "txtURI"
    Me.txtURI.ReadOnly = True
    Me.txtURI.Size = New System.Drawing.Size(790, 34)
    Me.txtURI.TabIndex = 0
    '
    'pnlFound
    '
    Me.pnlFound.AutoScroll = True
    Me.pnlFound.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.pnlFound.Controls.Add(Me.txtURI)
    Me.pnlFound.Controls.Add(Me.txtClosestMatch)
    Me.pnlFound.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlFound.Location = New System.Drawing.Point(0, 337)
    Me.pnlFound.Name = "pnlFound"
    Me.pnlFound.Size = New System.Drawing.Size(794, 193)
    Me.pnlFound.TabIndex = 6
    '
    'txtClosestMatch
    '
    Me.txtClosestMatch.Dock = System.Windows.Forms.DockStyle.Top
    Me.txtClosestMatch.Location = New System.Drawing.Point(0, 0)
    Me.txtClosestMatch.Name = "txtClosestMatch"
    Me.txtClosestMatch.ReadOnly = True
    Me.txtClosestMatch.Size = New System.Drawing.Size(790, 34)
    Me.txtClosestMatch.TabIndex = 1
    '
    'statStatus
    '
    Me.statStatus.Name = "statStatus"
    Me.statStatus.Size = New System.Drawing.Size(49, 20)
    Me.statStatus.Text = "Status"
    '
    'statSpeech
    '
    Me.statSpeech.Name = "statSpeech"
    Me.statSpeech.Size = New System.Drawing.Size(97, 20)
    Me.statSpeech.Text = "Not Listening"
    '
    'SearchFileDialog
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(11.0!, 28.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(794, 605)
    Me.Controls.Add(Me.pnlFound)
    Me.Controls.Add(Me.pnlVariables)
    Me.Controls.Add(Me.TableLayoutPanel2)
    Me.Controls.Add(Me.TableLayoutPanel1)
    Me.Controls.Add(Me.statStrip)
    Me.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
    Me.Margin = New System.Windows.Forms.Padding(6, 7, 6, 7)
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.Name = "SearchFileDialog"
    Me.ShowInTaskbar = False
    Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
    Me.Text = "Search File"
    Me.statStrip.ResumeLayout(False)
    Me.statStrip.PerformLayout()
    Me.TableLayoutPanel1.ResumeLayout(False)
    Me.TableLayoutPanel2.ResumeLayout(False)
    Me.pnlFound.ResumeLayout(False)
    Me.pnlFound.PerformLayout()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents statStrip As System.Windows.Forms.StatusStrip
  Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents OK_Button As System.Windows.Forms.Button
  Friend WithEvents Cancel_Button As System.Windows.Forms.Button
  Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents btnSpeech As System.Windows.Forms.Button
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents cmbFiles As System.Windows.Forms.ComboBox
  Friend WithEvents pnlVariables As System.Windows.Forms.Panel
  Friend WithEvents pnlFound As System.Windows.Forms.Panel
  Friend WithEvents txtURI As System.Windows.Forms.TextBox
  Friend WithEvents txtClosestMatch As System.Windows.Forms.TextBox
  Friend WithEvents statStatus As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents statSpeech As System.Windows.Forms.ToolStripStatusLabel

End Class
