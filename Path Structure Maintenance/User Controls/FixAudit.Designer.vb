<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FixAudit
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
    Me.spltPanel = New System.Windows.Forms.SplitContainer()
    Me.lstFSO = New System.Windows.Forms.ListBox()
    Me.pnlFormat = New System.Windows.Forms.Panel()
    CType(Me.spltPanel, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.spltPanel.Panel1.SuspendLayout()
    Me.spltPanel.Panel2.SuspendLayout()
    Me.spltPanel.SuspendLayout()
    Me.SuspendLayout()
    '
    'spltPanel
    '
    Me.spltPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.spltPanel.Dock = System.Windows.Forms.DockStyle.Fill
    Me.spltPanel.Location = New System.Drawing.Point(0, 0)
    Me.spltPanel.Name = "spltPanel"
    '
    'spltPanel.Panel1
    '
    Me.spltPanel.Panel1.Controls.Add(Me.lstFSO)
    Me.spltPanel.Panel1.Padding = New System.Windows.Forms.Padding(3)
    '
    'spltPanel.Panel2
    '
    Me.spltPanel.Panel2.Controls.Add(Me.pnlFormat)
    Me.spltPanel.Panel2.Padding = New System.Windows.Forms.Padding(3)
    Me.spltPanel.Size = New System.Drawing.Size(520, 276)
    Me.spltPanel.SplitterDistance = 230
    Me.spltPanel.TabIndex = 0
    '
    'lstFSO
    '
    Me.lstFSO.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lstFSO.FormattingEnabled = True
    Me.lstFSO.ItemHeight = 25
    Me.lstFSO.Location = New System.Drawing.Point(3, 3)
    Me.lstFSO.Name = "lstFSO"
    Me.lstFSO.Size = New System.Drawing.Size(220, 266)
    Me.lstFSO.TabIndex = 0
    '
    'pnlFormat
    '
    Me.pnlFormat.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlFormat.Location = New System.Drawing.Point(3, 3)
    Me.pnlFormat.Name = "pnlFormat"
    Me.pnlFormat.Size = New System.Drawing.Size(276, 266)
    Me.pnlFormat.TabIndex = 0
    '
    'FixAudit
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.spltPanel)
    Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
    Me.Name = "FixAudit"
    Me.Size = New System.Drawing.Size(520, 276)
    Me.spltPanel.Panel1.ResumeLayout(False)
    Me.spltPanel.Panel2.ResumeLayout(False)
    CType(Me.spltPanel, System.ComponentModel.ISupportInitialize).EndInit()
    Me.spltPanel.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents spltPanel As System.Windows.Forms.SplitContainer
  Friend WithEvents lstFSO As System.Windows.Forms.ListBox
  Friend WithEvents pnlFormat As System.Windows.Forms.Panel

End Class
