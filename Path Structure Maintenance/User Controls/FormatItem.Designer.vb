<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Format_Item
  Inherits System.Windows.Forms.UserControl

  'UserControl overrides dispose to clean up the component list.
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
    Me.pnlFileType = New System.Windows.Forms.Panel()
    Me.cmbFiles = New System.Windows.Forms.ComboBox()
    Me.Label1 = New System.Windows.Forms.Label()
    Me.pnlOptions = New System.Windows.Forms.Panel()
    Me.cmbOptions = New System.Windows.Forms.ComboBox()
    Me.Label2 = New System.Windows.Forms.Label()
    Me.pnlPreview = New System.Windows.Forms.Panel()
    Me.txtPreview = New System.Windows.Forms.TextBox()
    Me.btnAccept = New System.Windows.Forms.Button()
    Me.pnlVariables = New System.Windows.Forms.Panel()
    Me.pnlFileType.SuspendLayout()
    Me.pnlOptions.SuspendLayout()
    Me.pnlPreview.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlFileType
    '
    Me.pnlFileType.AutoSize = True
    Me.pnlFileType.Controls.Add(Me.cmbFiles)
    Me.pnlFileType.Controls.Add(Me.Label1)
    Me.pnlFileType.Dock = System.Windows.Forms.DockStyle.Top
    Me.pnlFileType.Location = New System.Drawing.Point(0, 0)
    Me.pnlFileType.MinimumSize = New System.Drawing.Size(520, 44)
    Me.pnlFileType.Name = "pnlFileType"
    Me.pnlFileType.Padding = New System.Windows.Forms.Padding(5)
    Me.pnlFileType.Size = New System.Drawing.Size(520, 44)
    Me.pnlFileType.TabIndex = 0
    '
    'cmbFiles
    '
    Me.cmbFiles.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbFiles.FormattingEnabled = True
    Me.cmbFiles.Location = New System.Drawing.Point(179, 5)
    Me.cmbFiles.Name = "cmbFiles"
    Me.cmbFiles.Size = New System.Drawing.Size(336, 33)
    Me.cmbFiles.TabIndex = 1
    '
    'Label1
    '
    Me.Label1.Dock = System.Windows.Forms.DockStyle.Left
    Me.Label1.Location = New System.Drawing.Point(5, 5)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(174, 34)
    Me.Label1.TabIndex = 0
    Me.Label1.Text = "Select a file type:"
    Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    '
    'pnlOptions
    '
    Me.pnlOptions.AutoSize = True
    Me.pnlOptions.Controls.Add(Me.cmbOptions)
    Me.pnlOptions.Controls.Add(Me.Label2)
    Me.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top
    Me.pnlOptions.Enabled = False
    Me.pnlOptions.Location = New System.Drawing.Point(0, 44)
    Me.pnlOptions.MinimumSize = New System.Drawing.Size(520, 44)
    Me.pnlOptions.Name = "pnlOptions"
    Me.pnlOptions.Padding = New System.Windows.Forms.Padding(5)
    Me.pnlOptions.Size = New System.Drawing.Size(520, 44)
    Me.pnlOptions.TabIndex = 1
    '
    'cmbOptions
    '
    Me.cmbOptions.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbOptions.FormattingEnabled = True
    Me.cmbOptions.Location = New System.Drawing.Point(179, 5)
    Me.cmbOptions.Name = "cmbOptions"
    Me.cmbOptions.Size = New System.Drawing.Size(336, 33)
    Me.cmbOptions.TabIndex = 1
    '
    'Label2
    '
    Me.Label2.Dock = System.Windows.Forms.DockStyle.Left
    Me.Label2.Location = New System.Drawing.Point(5, 5)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(174, 34)
    Me.Label2.TabIndex = 0
    Me.Label2.Text = "File Options:"
    Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
    '
    'pnlPreview
    '
    Me.pnlPreview.Controls.Add(Me.txtPreview)
    Me.pnlPreview.Controls.Add(Me.btnAccept)
    Me.pnlPreview.Dock = System.Windows.Forms.DockStyle.Bottom
    Me.pnlPreview.Location = New System.Drawing.Point(0, 244)
    Me.pnlPreview.Name = "pnlPreview"
    Me.pnlPreview.Size = New System.Drawing.Size(520, 32)
    Me.pnlPreview.TabIndex = 3
    '
    'txtPreview
    '
    Me.txtPreview.Dock = System.Windows.Forms.DockStyle.Fill
    Me.txtPreview.Location = New System.Drawing.Point(0, 0)
    Me.txtPreview.Name = "txtPreview"
    Me.txtPreview.ReadOnly = True
    Me.txtPreview.Size = New System.Drawing.Size(422, 30)
    Me.txtPreview.TabIndex = 1
    '
    'btnAccept
    '
    Me.btnAccept.Dock = System.Windows.Forms.DockStyle.Right
    Me.btnAccept.Location = New System.Drawing.Point(422, 0)
    Me.btnAccept.Name = "btnAccept"
    Me.btnAccept.Size = New System.Drawing.Size(98, 32)
    Me.btnAccept.TabIndex = 0
    Me.btnAccept.Text = "Accept"
    Me.btnAccept.UseVisualStyleBackColor = True
    '
    'pnlVariables
    '
    Me.pnlVariables.AutoScroll = True
    Me.pnlVariables.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.pnlVariables.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlVariables.Location = New System.Drawing.Point(0, 88)
    Me.pnlVariables.Name = "pnlVariables"
    Me.pnlVariables.Size = New System.Drawing.Size(520, 156)
    Me.pnlVariables.TabIndex = 4
    '
    'Format_Item
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.pnlVariables)
    Me.Controls.Add(Me.pnlPreview)
    Me.Controls.Add(Me.pnlOptions)
    Me.Controls.Add(Me.pnlFileType)
    Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
    Me.Name = "Format_Item"
    Me.Size = New System.Drawing.Size(520, 276)
    Me.pnlFileType.ResumeLayout(False)
    Me.pnlOptions.ResumeLayout(False)
    Me.pnlPreview.ResumeLayout(False)
    Me.pnlPreview.PerformLayout()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents pnlFileType As System.Windows.Forms.Panel
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents cmbFiles As System.Windows.Forms.ComboBox
  Friend WithEvents pnlOptions As System.Windows.Forms.Panel
  Friend WithEvents cmbOptions As System.Windows.Forms.ComboBox
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents pnlPreview As System.Windows.Forms.Panel
  Friend WithEvents pnlVariables As System.Windows.Forms.Panel
  Friend WithEvents txtPreview As System.Windows.Forms.TextBox
  Friend WithEvents btnAccept As System.Windows.Forms.Button

End Class
