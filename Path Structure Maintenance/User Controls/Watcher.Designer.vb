<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Watcher
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Watcher))
    Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
    Me.statPath = New System.Windows.Forms.ToolStripStatusLabel()
    Me.statWatchLabel = New System.Windows.Forms.ToolStripStatusLabel()
    Me.statDisableRename = New System.Windows.Forms.ToolStripStatusLabel()
    Me.mnuWatcher = New System.Windows.Forms.MenuStrip()
    Me.mnuOpen = New System.Windows.Forms.ToolStripMenuItem()
    Me.ProcessToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
    Me.AddTimeStampToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuTSPrefix = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuTSSuffix = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuPreviewWindow = New System.Windows.Forms.ToolStripMenuItem()
    Me.ViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuAlwaysOnTop = New System.Windows.Forms.ToolStripMenuItem()
    Me.mnuPlugins = New System.Windows.Forms.ToolStripMenuItem()
    Me.lblPath = New System.Windows.Forms.Label()
    Me.trvPathStructure = New System.Windows.Forms.TreeView()
    Me.imgFSO = New System.Windows.Forms.ImageList(Me.components)
    Me.lblReal = New System.Windows.Forms.Label()
    Me.trvFileSystem = New System.Windows.Forms.TreeView()
    Me.spltWorkSpace = New System.Windows.Forms.SplitContainer()
    Me.tlTip = New System.Windows.Forms.ToolTip(Me.components)
    Me.StatusStrip1.SuspendLayout()
    Me.mnuWatcher.SuspendLayout()
    CType(Me.spltWorkSpace, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.spltWorkSpace.Panel1.SuspendLayout()
    Me.spltWorkSpace.Panel2.SuspendLayout()
    Me.spltWorkSpace.SuspendLayout()
    Me.SuspendLayout()
    '
    'StatusStrip1
    '
    Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
    Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statPath, Me.statWatchLabel, Me.statDisableRename})
    Me.StatusStrip1.Location = New System.Drawing.Point(0, 342)
    Me.StatusStrip1.Name = "StatusStrip1"
    Me.StatusStrip1.Size = New System.Drawing.Size(674, 29)
    Me.StatusStrip1.TabIndex = 0
    Me.StatusStrip1.Text = "StatusStrip1"
    '
    'statPath
    '
    Me.statPath.AutoToolTip = True
    Me.statPath.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
    Me.statPath.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter
    Me.statPath.Name = "statPath"
    Me.statPath.Size = New System.Drawing.Size(468, 24)
    Me.statPath.Spring = True
    Me.statPath.Text = "[Path]"
    Me.statPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    '
    'statWatchLabel
    '
    Me.statWatchLabel.AutoToolTip = True
    Me.statWatchLabel.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
    Me.statWatchLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter
    Me.statWatchLabel.Name = "statWatchLabel"
    Me.statWatchLabel.Size = New System.Drawing.Size(63, 24)
    Me.statWatchLabel.Text = "[Status]"
    '
    'statDisableRename
    '
    Me.statDisableRename.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
    Me.statDisableRename.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter
    Me.statDisableRename.Name = "statDisableRename"
    Me.statDisableRename.Size = New System.Drawing.Size(128, 24)
    Me.statDisableRename.Tag = ""
    Me.statDisableRename.Text = "Disable Rename?"
    '
    'mnuWatcher
    '
    Me.mnuWatcher.ImageScalingSize = New System.Drawing.Size(20, 20)
    Me.mnuWatcher.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuOpen, Me.ProcessToolStripMenuItem, Me.ViewToolStripMenuItem, Me.mnuPlugins})
    Me.mnuWatcher.Location = New System.Drawing.Point(0, 0)
    Me.mnuWatcher.Name = "mnuWatcher"
    Me.mnuWatcher.Size = New System.Drawing.Size(674, 28)
    Me.mnuWatcher.TabIndex = 1
    Me.mnuWatcher.Text = "MenuStrip1"
    '
    'mnuOpen
    '
    Me.mnuOpen.Name = "mnuOpen"
    Me.mnuOpen.Size = New System.Drawing.Size(78, 24)
    Me.mnuOpen.Text = "Browse..."
    '
    'ProcessToolStripMenuItem
    '
    Me.ProcessToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AddTimeStampToolStripMenuItem, Me.mnuPreviewWindow})
    Me.ProcessToolStripMenuItem.Name = "ProcessToolStripMenuItem"
    Me.ProcessToolStripMenuItem.Size = New System.Drawing.Size(70, 24)
    Me.ProcessToolStripMenuItem.Text = "Process"
    '
    'AddTimeStampToolStripMenuItem
    '
    Me.AddTimeStampToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuTSPrefix, Me.mnuTSSuffix})
    Me.AddTimeStampToolStripMenuItem.Name = "AddTimeStampToolStripMenuItem"
    Me.AddTimeStampToolStripMenuItem.Size = New System.Drawing.Size(243, 26)
    Me.AddTimeStampToolStripMenuItem.Text = "Add Time Stamp"
    '
    'mnuTSPrefix
    '
    Me.mnuTSPrefix.Name = "mnuTSPrefix"
    Me.mnuTSPrefix.Size = New System.Drawing.Size(181, 26)
    Me.mnuTSPrefix.Text = "Prefix"
    '
    'mnuTSSuffix
    '
    Me.mnuTSSuffix.Name = "mnuTSSuffix"
    Me.mnuTSSuffix.Size = New System.Drawing.Size(181, 26)
    Me.mnuTSSuffix.Text = "Suffix"
    '
    'mnuPreviewWindow
    '
    Me.mnuPreviewWindow.Name = "mnuPreviewWindow"
    Me.mnuPreviewWindow.Size = New System.Drawing.Size(243, 26)
    Me.mnuPreviewWindow.Text = "Open Preview Window..."
    '
    'ViewToolStripMenuItem
    '
    Me.ViewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuAlwaysOnTop})
    Me.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem"
    Me.ViewToolStripMenuItem.Size = New System.Drawing.Size(53, 24)
    Me.ViewToolStripMenuItem.Text = "View"
    '
    'mnuAlwaysOnTop
    '
    Me.mnuAlwaysOnTop.CheckOnClick = True
    Me.mnuAlwaysOnTop.Name = "mnuAlwaysOnTop"
    Me.mnuAlwaysOnTop.Size = New System.Drawing.Size(183, 26)
    Me.mnuAlwaysOnTop.Text = "Always On Top"
    '
    'mnuPlugins
    '
    Me.mnuPlugins.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
    Me.mnuPlugins.Name = "mnuPlugins"
    Me.mnuPlugins.Size = New System.Drawing.Size(68, 24)
    Me.mnuPlugins.Text = "Plugins"
    '
    'lblPath
    '
    Me.lblPath.AutoSize = True
    Me.lblPath.Dock = System.Windows.Forms.DockStyle.Top
    Me.lblPath.Location = New System.Drawing.Point(3, 3)
    Me.lblPath.Name = "lblPath"
    Me.lblPath.Size = New System.Drawing.Size(175, 17)
    Me.lblPath.TabIndex = 2
    Me.lblPath.Text = "Path Structure File System"
    '
    'trvPathStructure
    '
    Me.trvPathStructure.AllowDrop = True
    Me.trvPathStructure.Dock = System.Windows.Forms.DockStyle.Fill
    Me.trvPathStructure.FullRowSelect = True
    Me.trvPathStructure.HideSelection = False
    Me.trvPathStructure.HotTracking = True
    Me.trvPathStructure.ImageIndex = 0
    Me.trvPathStructure.ImageList = Me.imgFSO
    Me.trvPathStructure.Location = New System.Drawing.Point(3, 20)
    Me.trvPathStructure.Name = "trvPathStructure"
    Me.trvPathStructure.SelectedImageIndex = 0
    Me.trvPathStructure.Size = New System.Drawing.Size(335, 287)
    Me.trvPathStructure.TabIndex = 3
    '
    'imgFSO
    '
    Me.imgFSO.ImageStream = CType(resources.GetObject("imgFSO.ImageStream"), System.Windows.Forms.ImageListStreamer)
    Me.imgFSO.TransparentColor = System.Drawing.Color.Transparent
    Me.imgFSO.Images.SetKeyName(0, "Folder_256x256.png")
    Me.imgFSO.Images.SetKeyName(1, "Generic_Document.png")
    '
    'lblReal
    '
    Me.lblReal.AutoSize = True
    Me.lblReal.Dock = System.Windows.Forms.DockStyle.Top
    Me.lblReal.Location = New System.Drawing.Point(3, 3)
    Me.lblReal.Name = "lblReal"
    Me.lblReal.Size = New System.Drawing.Size(113, 17)
    Me.lblReal.TabIndex = 0
    Me.lblReal.Text = "Real File System"
    '
    'trvFileSystem
    '
    Me.trvFileSystem.AllowDrop = True
    Me.trvFileSystem.Dock = System.Windows.Forms.DockStyle.Fill
    Me.trvFileSystem.HideSelection = False
    Me.trvFileSystem.HotTracking = True
    Me.trvFileSystem.ImageIndex = 0
    Me.trvFileSystem.ImageList = Me.imgFSO
    Me.trvFileSystem.Location = New System.Drawing.Point(3, 20)
    Me.trvFileSystem.Name = "trvFileSystem"
    Me.trvFileSystem.SelectedImageIndex = 0
    Me.trvFileSystem.Size = New System.Drawing.Size(315, 287)
    Me.trvFileSystem.TabIndex = 1
    '
    'spltWorkSpace
    '
    Me.spltWorkSpace.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
    Me.spltWorkSpace.Dock = System.Windows.Forms.DockStyle.Fill
    Me.spltWorkSpace.Location = New System.Drawing.Point(0, 28)
    Me.spltWorkSpace.Name = "spltWorkSpace"
    '
    'spltWorkSpace.Panel1
    '
    Me.spltWorkSpace.Panel1.Controls.Add(Me.trvFileSystem)
    Me.spltWorkSpace.Panel1.Controls.Add(Me.lblReal)
    Me.spltWorkSpace.Panel1.Padding = New System.Windows.Forms.Padding(3)
    '
    'spltWorkSpace.Panel2
    '
    Me.spltWorkSpace.Panel2.Controls.Add(Me.trvPathStructure)
    Me.spltWorkSpace.Panel2.Controls.Add(Me.lblPath)
    Me.spltWorkSpace.Panel2.Padding = New System.Windows.Forms.Padding(3)
    Me.spltWorkSpace.Size = New System.Drawing.Size(674, 314)
    Me.spltWorkSpace.SplitterDistance = 325
    Me.spltWorkSpace.TabIndex = 3
    '
    'Watcher
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.Controls.Add(Me.spltWorkSpace)
    Me.Controls.Add(Me.StatusStrip1)
    Me.Controls.Add(Me.mnuWatcher)
    Me.Name = "Watcher"
    Me.Size = New System.Drawing.Size(674, 371)
    Me.StatusStrip1.ResumeLayout(False)
    Me.StatusStrip1.PerformLayout()
    Me.mnuWatcher.ResumeLayout(False)
    Me.mnuWatcher.PerformLayout()
    Me.spltWorkSpace.Panel1.ResumeLayout(False)
    Me.spltWorkSpace.Panel1.PerformLayout()
    Me.spltWorkSpace.Panel2.ResumeLayout(False)
    Me.spltWorkSpace.Panel2.PerformLayout()
    CType(Me.spltWorkSpace, System.ComponentModel.ISupportInitialize).EndInit()
    Me.spltWorkSpace.ResumeLayout(False)
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
  Friend WithEvents statWatchLabel As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents mnuWatcher As System.Windows.Forms.MenuStrip
  Friend WithEvents statPath As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents lblPath As System.Windows.Forms.Label
  Friend WithEvents trvPathStructure As System.Windows.Forms.TreeView
  Friend WithEvents lblReal As System.Windows.Forms.Label
  Friend WithEvents trvFileSystem As System.Windows.Forms.TreeView
  Friend WithEvents spltWorkSpace As System.Windows.Forms.SplitContainer
  Friend WithEvents imgFSO As System.Windows.Forms.ImageList
  Friend WithEvents ProcessToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents statDisableRename As System.Windows.Forms.ToolStripStatusLabel
  Friend WithEvents mnuOpen As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents AddTimeStampToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuTSPrefix As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuTSSuffix As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents tlTip As System.Windows.Forms.ToolTip
  Friend WithEvents mnuPreviewWindow As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents ViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuAlwaysOnTop As System.Windows.Forms.ToolStripMenuItem
  Friend WithEvents mnuPlugins As System.Windows.Forms.ToolStripMenuItem

End Class
