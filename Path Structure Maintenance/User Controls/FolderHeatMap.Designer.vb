<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FolderHeatMap
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
    Me.components = New System.ComponentModel.Container()
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FolderHeatMap))
    Me.lstObjects = New System.Windows.Forms.ListView()
    Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
    Me.Label1 = New System.Windows.Forms.Label()
    Me.txtFolder = New System.Windows.Forms.TextBox()
    Me.btnBrowse = New System.Windows.Forms.Button()
    Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
    Me.statProgress = New System.Windows.Forms.ToolStripProgressBar()
    Me.statFolder = New System.Windows.Forms.ToolStripStatusLabel()
    Me.statSize = New System.Windows.Forms.ToolStripStatusLabel()
    Me.statObject = New System.Windows.Forms.ToolStripStatusLabel()
    Me.imkImages = New System.Windows.Forms.ImageList(Me.components)
    Me.TableLayoutPanel1.SuspendLayout()
    Me.StatusStrip1.SuspendLayout()
    Me.SuspendLayout()
    '
    'lstObjects
    '
    Me.lstObjects.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lstObjects.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
    Me.lstObjects.LargeImageList = Me.imkImages
    Me.lstObjects.Location = New System.Drawing.Point(0, 52)
    Me.lstObjects.Margin = New System.Windows.Forms.Padding(4)
    Me.lstObjects.Name = "lstObjects"
    Me.lstObjects.Size = New System.Drawing.Size(439, 185)
    Me.lstObjects.TabIndex = 5
    Me.lstObjects.UseCompatibleStateImageBehavior = False
    '
    'TableLayoutPanel1
    '
    Me.TableLayoutPanel1.ColumnCount = 3
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
    Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.txtFolder, 1, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.btnBrowse, 2, 0)
    Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top
    Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
    Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(5, 6, 5, 6)
    Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
    Me.TableLayoutPanel1.Padding = New System.Windows.Forms.Padding(3)
    Me.TableLayoutPanel1.RowCount = 1
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.TableLayoutPanel1.Size = New System.Drawing.Size(439, 52)
    Me.TableLayoutPanel1.TabIndex = 4
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.Label1.Location = New System.Drawing.Point(8, 3)
    Me.Label1.Margin = New System.Windows.Forms.Padding(5, 0, 5, 0)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(98, 46)
    Me.Label1.TabIndex = 0
    Me.Label1.Text = "Folder:"
    Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
    '
    'txtFolder
    '
    Me.txtFolder.Dock = System.Windows.Forms.DockStyle.Fill
    Me.txtFolder.Location = New System.Drawing.Point(116, 9)
    Me.txtFolder.Margin = New System.Windows.Forms.Padding(5, 6, 5, 6)
    Me.txtFolder.Name = "txtFolder"
    Me.txtFolder.Size = New System.Drawing.Size(206, 32)
    Me.txtFolder.TabIndex = 1
    '
    'btnBrowse
    '
    Me.btnBrowse.Dock = System.Windows.Forms.DockStyle.Fill
    Me.btnBrowse.Location = New System.Drawing.Point(332, 9)
    Me.btnBrowse.Margin = New System.Windows.Forms.Padding(5, 6, 5, 6)
    Me.btnBrowse.Name = "btnBrowse"
    Me.btnBrowse.Size = New System.Drawing.Size(99, 34)
    Me.btnBrowse.TabIndex = 2
    Me.btnBrowse.Text = "Go"
    Me.btnBrowse.UseVisualStyleBackColor = True
    '
    'StatusStrip1
    '
    Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
    Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statProgress, Me.statFolder, Me.statSize, Me.statObject})
    Me.StatusStrip1.Location = New System.Drawing.Point(0, 237)
    Me.StatusStrip1.Name = "StatusStrip1"
    Me.StatusStrip1.Padding = New System.Windows.Forms.Padding(1, 0, 22, 0)
    Me.StatusStrip1.Size = New System.Drawing.Size(439, 36)
    Me.StatusStrip1.TabIndex = 3
    Me.StatusStrip1.Text = "StatusStrip1"
    '
    'statProgress
    '
    Me.statProgress.Name = "statProgress"
    Me.statProgress.Size = New System.Drawing.Size(156, 30)
    '
    'statFolder
    '
    Me.statFolder.Name = "statFolder"
    Me.statFolder.Size = New System.Drawing.Size(51, 31)
    Me.statFolder.Text = "Folder"
    '
    'statSize
    '
    Me.statSize.Name = "statSize"
    Me.statSize.Size = New System.Drawing.Size(36, 31)
    Me.statSize.Text = "Size"
    '
    'statObject
    '
    Me.statObject.Name = "statObject"
    Me.statObject.Size = New System.Drawing.Size(105, 31)
    Me.statObject.Text = "Current Object"
    '
    'imkImages
    '
    Me.imkImages.ImageStream = CType(resources.GetObject("imkImages.ImageStream"), System.Windows.Forms.ImageListStreamer)
    Me.imkImages.TransparentColor = System.Drawing.Color.Transparent
    Me.imkImages.Images.SetKeyName(0, "Folder_48x48.png")
    Me.imkImages.Images.SetKeyName(1, "Image_File.png")
    '
    'FolderHeatMap
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 24.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.lstObjects)
    Me.Controls.Add(Me.TableLayoutPanel1)
    Me.Controls.Add(Me.StatusStrip1)
    Me.Font = New System.Drawing.Font("Calibri", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.Margin = New System.Windows.Forms.Padding(4)
    Me.Name = "FolderHeatMap"
    Me.Size = New System.Drawing.Size(439, 273)
    Me.TableLayoutPanel1.ResumeLayout(False)
    Me.TableLayoutPanel1.PerformLayout()
    Me.StatusStrip1.ResumeLayout(False)
    Me.StatusStrip1.PerformLayout()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents lstObjects As System.Windows.Forms.ListView
  Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents txtFolder As System.Windows.Forms.TextBox
  Friend WithEvents btnBrowse As System.Windows.Forms.Button
  Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
  Friend WithEvents statProgress As System.Windows.Forms.ToolStripProgressBar
  Friend WithEvents statFolder As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents statSize As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents statObject As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents imkImages As System.Windows.Forms.ImageList

End Class
