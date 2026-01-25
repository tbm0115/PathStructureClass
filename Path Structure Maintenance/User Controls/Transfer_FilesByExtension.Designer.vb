<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Transfer_FilesByExtension
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
    Me.txtFileExtension = New System.Windows.Forms.TextBox()
    Me.Label2 = New System.Windows.Forms.Label()
    Me.Panel1 = New System.Windows.Forms.Panel()
    Me.cmbFolder = New System.Windows.Forms.ComboBox()
    Me.Label1 = New System.Windows.Forms.Label()
    Me.btnTransfer = New System.Windows.Forms.Button()
    Me.pnlFileType.SuspendLayout()
    Me.Panel1.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlFileType
    '
    Me.pnlFileType.Controls.Add(Me.txtFileExtension)
    Me.pnlFileType.Controls.Add(Me.Label2)
    Me.pnlFileType.Dock = System.Windows.Forms.DockStyle.Top
    Me.pnlFileType.Location = New System.Drawing.Point(0, 0)
    Me.pnlFileType.Name = "pnlFileType"
    Me.pnlFileType.Padding = New System.Windows.Forms.Padding(5)
    Me.pnlFileType.Size = New System.Drawing.Size(520, 44)
    Me.pnlFileType.TabIndex = 7
    '
    'txtFileExtension
    '
    Me.txtFileExtension.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
    Me.txtFileExtension.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource
    Me.txtFileExtension.Dock = System.Windows.Forms.DockStyle.Fill
    Me.txtFileExtension.Location = New System.Drawing.Point(238, 5)
    Me.txtFileExtension.Name = "txtFileExtension"
    Me.txtFileExtension.Size = New System.Drawing.Size(277, 30)
    Me.txtFileExtension.TabIndex = 1
    '
    'Label2
    '
    Me.Label2.Dock = System.Windows.Forms.DockStyle.Left
    Me.Label2.Location = New System.Drawing.Point(5, 5)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(233, 34)
    Me.Label2.TabIndex = 0
    Me.Label2.Text = "Enter a File Extension:"
    Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    '
    'Panel1
    '
    Me.Panel1.Controls.Add(Me.cmbFolder)
    Me.Panel1.Controls.Add(Me.Label1)
    Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
    Me.Panel1.Location = New System.Drawing.Point(0, 44)
    Me.Panel1.Name = "Panel1"
    Me.Panel1.Padding = New System.Windows.Forms.Padding(5)
    Me.Panel1.Size = New System.Drawing.Size(520, 44)
    Me.Panel1.TabIndex = 8
    '
    'cmbFolder
    '
    Me.cmbFolder.Dock = System.Windows.Forms.DockStyle.Fill
    Me.cmbFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
    Me.cmbFolder.FormattingEnabled = True
    Me.cmbFolder.Location = New System.Drawing.Point(238, 5)
    Me.cmbFolder.Name = "cmbFolder"
    Me.cmbFolder.Size = New System.Drawing.Size(277, 33)
    Me.cmbFolder.TabIndex = 1
    '
    'Label1
    '
    Me.Label1.Dock = System.Windows.Forms.DockStyle.Left
    Me.Label1.Location = New System.Drawing.Point(5, 5)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(233, 34)
    Me.Label1.TabIndex = 0
    Me.Label1.Text = "Select a folder to send to:"
    Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    '
    'btnTransfer
    '
    Me.btnTransfer.Dock = System.Windows.Forms.DockStyle.Bottom
    Me.btnTransfer.Location = New System.Drawing.Point(0, 242)
    Me.btnTransfer.Name = "btnTransfer"
    Me.btnTransfer.Size = New System.Drawing.Size(520, 34)
    Me.btnTransfer.TabIndex = 9
    Me.btnTransfer.Text = "Transfer"
    Me.btnTransfer.UseVisualStyleBackColor = True
    '
    'Transfer_FilesByExtension
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.btnTransfer)
    Me.Controls.Add(Me.Panel1)
    Me.Controls.Add(Me.pnlFileType)
    Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
    Me.Name = "Transfer_FilesByExtension"
    Me.Size = New System.Drawing.Size(520, 276)
    Me.pnlFileType.ResumeLayout(False)
    Me.pnlFileType.PerformLayout()
    Me.Panel1.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents pnlFileType As System.Windows.Forms.Panel
  Friend WithEvents txtFileExtension As System.Windows.Forms.TextBox
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents Panel1 As System.Windows.Forms.Panel
  Friend WithEvents cmbFolder As System.Windows.Forms.ComboBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents btnTransfer As System.Windows.Forms.Button

End Class
