<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Preview
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
    Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
    Me.lstFolders = New System.Windows.Forms.ListBox()
    Me.lstFiles = New System.Windows.Forms.ListBox()
    Me.pnlPreview = New System.Windows.Forms.Panel()
    Me.btnOpenFile = New System.Windows.Forms.Button()
    Me.btnOpenFolder = New System.Windows.Forms.Button()
    Me.TableLayoutPanel1.SuspendLayout()
    Me.SuspendLayout()
    '
    'TableLayoutPanel1
    '
    Me.TableLayoutPanel1.ColumnCount = 3
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.Controls.Add(Me.lstFolders, 0, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.lstFiles, 1, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.pnlPreview, 2, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.btnOpenFile, 2, 1)
    Me.TableLayoutPanel1.Controls.Add(Me.btnOpenFolder, 0, 1)
    Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
    Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
    Me.TableLayoutPanel1.RowCount = 2
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.0!))
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
    Me.TableLayoutPanel1.Size = New System.Drawing.Size(600, 400)
    Me.TableLayoutPanel1.TabIndex = 0
    '
    'lstFolders
    '
    Me.lstFolders.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lstFolders.FormattingEnabled = True
    Me.lstFolders.ItemHeight = 16
    Me.lstFolders.Location = New System.Drawing.Point(3, 3)
    Me.lstFolders.Name = "lstFolders"
    Me.lstFolders.Size = New System.Drawing.Size(144, 354)
    Me.lstFolders.TabIndex = 0
    '
    'lstFiles
    '
    Me.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lstFiles.FormattingEnabled = True
    Me.lstFiles.ItemHeight = 16
    Me.lstFiles.Location = New System.Drawing.Point(153, 3)
    Me.lstFiles.Name = "lstFiles"
    Me.lstFiles.Size = New System.Drawing.Size(144, 354)
    Me.lstFiles.TabIndex = 1
    '
    'pnlPreview
    '
    Me.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill
    Me.pnlPreview.Location = New System.Drawing.Point(303, 3)
    Me.pnlPreview.Name = "pnlPreview"
    Me.pnlPreview.Padding = New System.Windows.Forms.Padding(5)
    Me.pnlPreview.Size = New System.Drawing.Size(294, 354)
    Me.pnlPreview.TabIndex = 2
    '
    'btnOpenFile
    '
    Me.btnOpenFile.Dock = System.Windows.Forms.DockStyle.Fill
    Me.btnOpenFile.Location = New System.Drawing.Point(303, 363)
    Me.btnOpenFile.Name = "btnOpenFile"
    Me.btnOpenFile.Size = New System.Drawing.Size(294, 34)
    Me.btnOpenFile.TabIndex = 3
    Me.btnOpenFile.Text = "Open File"
    Me.btnOpenFile.UseVisualStyleBackColor = True
    '
    'btnOpenFolder
    '
    Me.btnOpenFolder.Dock = System.Windows.Forms.DockStyle.Fill
    Me.btnOpenFolder.Location = New System.Drawing.Point(3, 363)
    Me.btnOpenFolder.Name = "btnOpenFolder"
    Me.btnOpenFolder.Size = New System.Drawing.Size(144, 34)
    Me.btnOpenFolder.TabIndex = 4
    Me.btnOpenFolder.Text = "Open Folder"
    Me.btnOpenFolder.UseVisualStyleBackColor = True
    '
    'Preview
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.TableLayoutPanel1)
    Me.Name = "Preview"
    Me.Size = New System.Drawing.Size(600, 400)
    Me.TableLayoutPanel1.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents lstFolders As System.Windows.Forms.ListBox
  Friend WithEvents lstFiles As System.Windows.Forms.ListBox
  Friend WithEvents pnlPreview As System.Windows.Forms.Panel
  Friend WithEvents btnOpenFile As System.Windows.Forms.Button
  Friend WithEvents btnOpenFolder As System.Windows.Forms.Button

End Class
