<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SelectFolderDialog
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
    Me.components = New System.ComponentModel.Container()
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SelectFolderDialog))
    Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
    Me.OK_Button = New System.Windows.Forms.Button()
    Me.Cancel_Button = New System.Windows.Forms.Button()
    Me.imgList = New System.Windows.Forms.ImageList(Me.components)
    Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
    Me.txtCurrentPath = New System.Windows.Forms.TextBox()
    Me.btnParentFolder = New System.Windows.Forms.Button()
    Me.lstFolders = New System.Windows.Forms.ListView()
    Me.TableLayoutPanel1.SuspendLayout()
    Me.TableLayoutPanel2.SuspendLayout()
    Me.SuspendLayout()
    '
    'TableLayoutPanel1
    '
    Me.TableLayoutPanel1.ColumnCount = 2
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
    Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom
    Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 352)
    Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(4)
    Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
    Me.TableLayoutPanel1.RowCount = 1
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.Size = New System.Drawing.Size(580, 36)
    Me.TableLayoutPanel1.TabIndex = 0
    '
    'OK_Button
    '
    Me.OK_Button.Dock = System.Windows.Forms.DockStyle.Fill
    Me.OK_Button.Location = New System.Drawing.Point(4, 4)
    Me.OK_Button.Margin = New System.Windows.Forms.Padding(4)
    Me.OK_Button.Name = "OK_Button"
    Me.OK_Button.Size = New System.Drawing.Size(282, 28)
    Me.OK_Button.TabIndex = 0
    Me.OK_Button.Text = "Select Folder"
    '
    'Cancel_Button
    '
    Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.Cancel_Button.Dock = System.Windows.Forms.DockStyle.Fill
    Me.Cancel_Button.Location = New System.Drawing.Point(294, 4)
    Me.Cancel_Button.Margin = New System.Windows.Forms.Padding(4)
    Me.Cancel_Button.Name = "Cancel_Button"
    Me.Cancel_Button.Size = New System.Drawing.Size(282, 28)
    Me.Cancel_Button.TabIndex = 1
    Me.Cancel_Button.Text = "Cancel"
    '
    'imgList
    '
    Me.imgList.ImageStream = CType(resources.GetObject("imgList.ImageStream"), System.Windows.Forms.ImageListStreamer)
    Me.imgList.TransparentColor = System.Drawing.Color.Transparent
    Me.imgList.Images.SetKeyName(0, "Folder_256x256.png")
    '
    'TableLayoutPanel2
    '
    Me.TableLayoutPanel2.ColumnCount = 2
    Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32.0!))
    Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.TableLayoutPanel2.Controls.Add(Me.txtCurrentPath, 1, 0)
    Me.TableLayoutPanel2.Controls.Add(Me.btnParentFolder, 0, 0)
    Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top
    Me.TableLayoutPanel2.Location = New System.Drawing.Point(0, 0)
    Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
    Me.TableLayoutPanel2.RowCount = 1
    Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
    Me.TableLayoutPanel2.Size = New System.Drawing.Size(580, 32)
    Me.TableLayoutPanel2.TabIndex = 3
    '
    'txtCurrentPath
    '
    Me.txtCurrentPath.Dock = System.Windows.Forms.DockStyle.Top
    Me.txtCurrentPath.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.txtCurrentPath.Location = New System.Drawing.Point(35, 3)
    Me.txtCurrentPath.Name = "txtCurrentPath"
    Me.txtCurrentPath.Size = New System.Drawing.Size(542, 30)
    Me.txtCurrentPath.TabIndex = 2
    '
    'btnParentFolder
    '
    Me.btnParentFolder.BackgroundImage = CType(resources.GetObject("btnParentFolder.BackgroundImage"), System.Drawing.Image)
    Me.btnParentFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
    Me.btnParentFolder.Dock = System.Windows.Forms.DockStyle.Fill
    Me.btnParentFolder.FlatStyle = System.Windows.Forms.FlatStyle.Popup
    Me.btnParentFolder.Location = New System.Drawing.Point(3, 3)
    Me.btnParentFolder.Name = "btnParentFolder"
    Me.btnParentFolder.Size = New System.Drawing.Size(26, 26)
    Me.btnParentFolder.TabIndex = 3
    Me.btnParentFolder.UseVisualStyleBackColor = True
    '
    'lstFolders
    '
    Me.lstFolders.Activation = System.Windows.Forms.ItemActivation.TwoClick
    Me.lstFolders.Dock = System.Windows.Forms.DockStyle.Fill
    Me.lstFolders.Location = New System.Drawing.Point(0, 32)
    Me.lstFolders.MultiSelect = False
    Me.lstFolders.Name = "lstFolders"
    Me.lstFolders.Size = New System.Drawing.Size(580, 320)
    Me.lstFolders.SmallImageList = Me.imgList
    Me.lstFolders.Sorting = System.Windows.Forms.SortOrder.Ascending
    Me.lstFolders.TabIndex = 4
    Me.lstFolders.UseCompatibleStateImageBehavior = False
    Me.lstFolders.View = System.Windows.Forms.View.List
    '
    'SelectFolderDialog
    '
    Me.AcceptButton = Me.OK_Button
    Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.CancelButton = Me.Cancel_Button
    Me.ClientSize = New System.Drawing.Size(580, 388)
    Me.Controls.Add(Me.lstFolders)
    Me.Controls.Add(Me.TableLayoutPanel2)
    Me.Controls.Add(Me.TableLayoutPanel1)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Margin = New System.Windows.Forms.Padding(4)
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.Name = "SelectFolderDialog"
    Me.ShowInTaskbar = False
    Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
    Me.Text = "Select a Folder..."
    Me.TableLayoutPanel1.ResumeLayout(False)
    Me.TableLayoutPanel2.ResumeLayout(False)
    Me.TableLayoutPanel2.PerformLayout()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents OK_Button As System.Windows.Forms.Button
  Friend WithEvents Cancel_Button As System.Windows.Forms.Button
  Friend WithEvents imgList As System.Windows.Forms.ImageList
  Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents txtCurrentPath As System.Windows.Forms.TextBox
  Friend WithEvents lstFolders As System.Windows.Forms.ListView
  Friend WithEvents btnParentFolder As System.Windows.Forms.Button

End Class
