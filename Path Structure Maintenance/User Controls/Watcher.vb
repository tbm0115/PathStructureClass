Imports PathStructureClass
Imports Pather_Structure_Interface

Public Class Watcher
  Private _pstruct As PathStructure
  Private _exploreWatcher As ExplorerWatcher
  Private _curPath As Path
  Public ReadOnly Property ActiveWatcher As ExplorerWatcher
    Get
      Return _exploreWatcher
    End Get
  End Property

  Public Property CurrentPath As Path
    Get
      Return _curPath
    End Get
    Set(value As Path)
      If Not value.IsNull() Then ' value.IsNull() = False Then
        If value.Type = Path.PathType.File Then value = value.Parent
        If value.UNCPath IsNot Nothing Then
          _curPath = value
          statPath.Text = _curPath.UNCPath
          FillRealTree(_curPath)
          FillPathTree(_curPath)

          '' Update plugins
          Plugins_ChangedCurrentPath(_curPath)
        Else
          trvFileSystem.Nodes.Clear()
          trvPathStructure.Nodes.Clear()
          statPath.Text = "No path selected!"
        End If
      Else
        trvFileSystem.Nodes.Clear()
        trvPathStructure.Nodes.Clear()
        statPath.Text = "No path selected!"
      End If
    End Set
  End Property

  Public Sub New(ByVal PStruct As PathStructure)
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    _pstruct = PStruct

    _exploreWatcher = New ExplorerWatcher(_pstruct) ', 250)
    AddHandler _exploreWatcher.ExplorerWatcherFound, AddressOf ExplorerFound
    AddHandler _exploreWatcher.ExplorerWatcherAborted, AddressOf ExplorerAbort
    _exploreWatcher.StartWatcher()

    statDisableRename.Tag = "False"

    statWatchLabel.Text = "Watching..."

    AddHandler Main.FormClosing, Sub()
                                   _exploreWatcher.StopWatcher()
                                 End Sub

    AddHandler Main.ResizeEnd, Sub()
                                 If Main.Width <= (Main.Height / 2) Then
                                   spltWorkSpace.Orientation = Orientation.Horizontal
                                 Else
                                   spltWorkSpace.Orientation = Orientation.Vertical
                                 End If
                               End Sub

    PopulatePluginList()
  End Sub

  Private Sub trvFileSystem_KeyUp(sender As Object, e As KeyEventArgs) Handles trvFileSystem.KeyUp
    If trvFileSystem.SelectedNode IsNot Nothing Then
      If e.KeyCode = Keys.Enter Then
        trvFileSystem_NodeMouseDoubleClick(trvFileSystem, Nothing)
      End If
    End If
  End Sub

  Private Sub trvFileSystem_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles trvFileSystem.NodeMouseDoubleClick
    If trvFileSystem.SelectedNode IsNot Nothing Then
      If trvFileSystem.SelectedNode.Tag IsNot Nothing Then
        If trvFileSystem.SelectedNode.Tag.Type = Path.PathType.Folder Then
          Me.CurrentPath = trvFileSystem.SelectedNode.Tag
        Else
          Try
            Process.Start(trvFileSystem.SelectedNode.Tag.UNCPath)
          Catch ex As Exception
            statWatchLabel.Text = "Couldn't open file!"
            Log("{Watcher}(FileSystem_Clicked) DoubleClick Open File failed: " & ex.Message)
          End Try
        End If
      Else
        Me.CurrentPath = _curPath.Parent
      End If
    End If
  End Sub

  Public Sub FillRealTree(ByVal Path As Path)
    trvFileSystem.Nodes.Clear()

    If Not _pstruct.defaultPaths.Contains(Path.UNCPath) Then
      trvFileSystem.Nodes.Add(".. [" & Path.PathName & "]")
    End If
    If Path.Children IsNot Nothing Then
      For i = 0 To Path.Children.Count - 1 Step 1
        Dim nd As TreeNode
        If Path.Children(i).Type = PathStructureClass.Path.PathType.Folder Then
          nd = New TreeNode(Path.Children(i).PathName, 0, 0)
        ElseIf Path.Children(i).Type = PathStructureClass.Path.PathType.File Then
          nd = New TreeNode(Path.Children(i).PathName, 1, 1)
        Else
          Continue For
        End If
        If Path.Children(i).IsNameStructured() Then
          nd.BackColor = Color.LightGreen
          nd.ToolTipText = Path.Children(i).StructureCandidates.GetHighestMatch().StructureDescription
        Else
          nd.BackColor = Color.PaleVioletRed
          If Path.Children(i).StructureCandidates.Count > 0 Then
            Debug.WriteLine(Path.Children(i).UNCPath)
            For j = 0 To Path.Children(i).StructureCandidates.Count - 1 Step 1
              Debug.WriteLine(vbTab & "Candidate Score: " & Path.Children(i).StructureCandidates(j).MatchPercentage.ToString & "%" & vbTab & Path.Children(i).StructureCandidates(j).PathName)
            Next
            If Path.Children(i).StructureCandidates.GetHighestMatch().MatchPercentage = 99 Then
              nd.BackColor = Color.Yellow '' Set to yellow because extension was not matched
            End If
          End If
        End If
        nd.Tag = Path.Children(i)
        trvFileSystem.Nodes.Add(nd)
      Next
    End If
  End Sub
  Public Sub FillPathTree(ByVal Path As Path)
    trvPathStructure.Nodes.Clear()
    Path.IsNameStructured()
    Dim structCand As StructureCandidate = Path.StructureCandidates.GetHighestMatch()
    Dim structEl As Xml.XmlElement

    If structCand IsNot Nothing Then
      structEl = structCand.XElement
    End If

    If Not _pstruct.defaultPaths.Contains(Path.UNCPath) And structEl IsNot Nothing Then
      trvPathStructure.Nodes.Add(".. [" & structEl.Attributes("name").Value & "]")
    End If
    If Path IsNot Nothing Then
      If Path.IsNameStructured() And structEl IsNot Nothing Then
        Dim nds As Xml.XmlNodeList = structEl.ChildNodes
        If nds IsNot Nothing Then
          If nds.Count > 0 Then
            For i = 0 To nds.Count - 1 Step 1
              Dim nd As TreeNode
              '' Check for the right icon
              If nds(i).Name = "Folder" Then
                nd = New TreeNode(nds(i).Attributes("name").Value, 0, 0)
              ElseIf nds(i).Name = "File" Then
                nd = New TreeNode(nds(i).Attributes("name").Value, 1, 1)
              Else
                Continue For
              End If

              If nds(i).Attributes("description") IsNot Nothing Then
                nd.ToolTipText = nds(i).Attributes("description").Value
              End If

              '' Try to find an existing child in the current path that matches this node's XPath
              Dim strXPath As String = nds(i).FindXPath()
              If _curPath.Children IsNot Nothing Then
                For j = 0 To _curPath.Children.Length - 1 Step 1
                  If _curPath.Children(j).IsNameStructured() Then
                    If String.Equals(_curPath.Children(j).StructureCandidates.GetHighestMatch().XElement.FindXPath, strXPath, System.StringComparison.OrdinalIgnoreCase) Then
                      nd.BackColor = Color.LightGreen
                      nd.Tag = _curPath.Children(j)
                      Exit For
                    End If
                  End If
                Next
              End If
              '' Add to treeview
              trvPathStructure.Nodes.Add(nd)
            Next
          End If
        End If
      Else
        For i = 0 To Path.StructureCandidates.Count - 1 Step 1
          Debug.WriteLine(vbTab & Path.StructureCandidates(i).MatchPercentage.ToString & "% " & Path.StructureCandidates(i).StructurePath)
        Next
      End If
    End If
  End Sub

  Private Sub trvPathStructure_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trvPathStructure.AfterSelect
    If trvPathStructure.SelectedNode IsNot Nothing Then
      If trvPathStructure.SelectedNode.Tag IsNot Nothing Then
        For i = 0 To trvFileSystem.Nodes.Count - 1 Step 1
          If trvFileSystem.Nodes(i).Tag IsNot Nothing Then
            If trvFileSystem.Nodes(i).Tag.Equals(trvPathStructure.SelectedNode.Tag) Then
              trvFileSystem.SelectedNode = trvFileSystem.Nodes(i)
              Exit For
            End If
          End If
        Next
      End If
    End If
  End Sub

  Private Sub trvFileSystem_MouseDown(sender As Object, e As MouseEventArgs) Handles trvFileSystem.MouseDown
    Dim trvPoint As Point = trvFileSystem.PointToClient(New Point(e.X, e.Y))
    If trvFileSystem.Bounds.Contains(e.X, e.Y) Then
      Dim nd As TreeNode = trvFileSystem.GetNodeAt(e.X, e.Y)
      If nd IsNot Nothing Then
        trvFileSystem.SelectedNode = nd
      End If
    ElseIf trvFileSystem.Bounds.Contains(trvPoint) Then
      Dim nd As TreeNode = trvFileSystem.GetNodeAt(trvPoint)
      If nd IsNot Nothing Then
        trvFileSystem.SelectedNode = nd
      End If

    End If
  End Sub
  Private Sub trvFileSystem_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles trvFileSystem.AfterSelect
    If trvFileSystem.SelectedNode IsNot Nothing Then
      If trvFileSystem.SelectedNode.Tag IsNot Nothing Then
        For i = 0 To trvPathStructure.Nodes.Count - 1 Step 1
          If trvPathStructure.Nodes(i).Tag IsNot Nothing Then
            If trvPathStructure.Nodes(i).Tag.Equals(trvFileSystem.SelectedNode.Tag) Then
              trvPathStructure.SelectedNode = trvPathStructure.Nodes(i)
              Debug.WriteLine("Selected '" & trvPathStructure.Nodes(i).Text & "'")
              Exit For
            End If
          End If
        Next
      Else
        Debug.WriteLine("Selected node's tag is nothing")
      End If
    Else
      Debug.WriteLine("No node selected")
    End If
  End Sub

  Delegate Sub ToolExplorerSearchCallback(ByVal URL As String)
  Public Sub ExplorerFound(ByVal URL As String)
    Try
      If Me IsNot Nothing Then
        If Me.InvokeRequired Then
          Dim d As New ToolExplorerSearchCallback(AddressOf ExplorerFound)
          Me.Invoke(d, New Object() {URL})
        Else
          Me.CurrentPath = New Path(_pstruct, URL)
          statWatchLabel.Text = "Watching..."
          GC.Collect()
        End If
      End If
    Catch ex As Exception
      Log("{Watcher}(ExplorerFound) Failed: " & ex.Message)
    End Try
  End Sub
  Public Sub ExplorerAbort(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)
    Static prompted As Boolean = False
    Try
      Log("{Watcher}(ExplorerAbort) An error occurred while watching Windows Explorer: " & e.ExceptionObject.Message)
      statWatchLabel.Text = "Error, attempting to restart!"
      _exploreWatcher.StopWatcher()
      Application.DoEvents()
      System.Threading.Thread.Sleep(1000)
      '_exploreWatcher = New ExplorerWatcher(_pstruct, 250)
      _exploreWatcher.StartWatcher()
      statWatchLabel.Text = "Attempted to restart watcher!"
    Catch ex As Exception
      If Not prompted Then
        MessageBox.Show("An error occurred while attempting to fix a crash in the Explorer Watcher:" & vbLf & ex.Message,
                        "Failed to Restart Watcher", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Main.mnuExplorerWatcher.Enabled = True
        prompted = True
        _exploreWatcher.StopWatcher()
        statWatchLabel.Text = "Aborted!"
      End If
    End Try
  End Sub

  Private _dragging As Boolean = False

  Private Sub trvPathStructure_DoubleClick(sender As Object, e As EventArgs) Handles trvPathStructure.DoubleClick
    If trvPathStructure.SelectedNode IsNot Nothing Then
      MessageBox.Show("Name: " & trvPathStructure.SelectedNode.Text & vbCrLf & "Description: " & trvPathStructure.SelectedNode.ToolTipText, "Path Structure Description", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End If
  End Sub
  Private Sub trvPathStructure_DragDrop(sender As Object, e As DragEventArgs) Handles trvPathStructure.DragDrop
    '' Process drop data

    '' Get the item that is hovered
    Dim trvPoint As Point = trvPathStructure.PointToClient(New Point(e.X, e.Y))
    If trvPathStructure.Bounds.Contains(e.X, e.Y) Then
      Dim nd As TreeNode = trvPathStructure.GetNodeAt(e.X, e.Y)
      If nd IsNot Nothing Then
        trvPathStructure.SelectedNode = nd
      End If
    ElseIf trvPathStructure.Bounds.Contains(trvPoint) Then
      Dim nd As TreeNode = trvPathStructure.GetNodeAt(trvPoint)
      If nd IsNot Nothing Then
        trvPathStructure.SelectedNode = nd
      End If
    End If

    If e.Data.GetDataPresent(DataFormats.FileDrop) Then
      Dim frmt As FormatDialog
      If trvPathStructure.SelectedNode IsNot Nothing Then
        Dim fsos() As String = e.Data.GetData(DataFormats.FileDrop)
        For i = 0 To fsos.Length - 1 Step 1
          If IO.File.Exists(fsos(i)) Then
            frmt = New FormatDialog(fsos(i), _pstruct, trvPathStructure.SelectedNode.Text, trvPathStructure.SelectedNode.Tag)
            frmt.ShowDialog()
          ElseIf IO.Directory.Exists(fsos(i)) Then
            Dim files() As String = IO.Directory.GetFiles(fsos(i))
            For j = 0 To files.Length - 1 Step 1
              frmt = New FormatDialog(files(j), _pstruct, trvPathStructure.SelectedNode.Text, trvPathStructure.SelectedNode.Tag)
              frmt.ShowDialog()
            Next
          End If
        Next
        statWatchLabel.Text = "FSO(s) added"
        Me.CurrentPath = New Path(_curPath.PStructure, _curPath.UNCPath) '' "Refresh" view
      End If
    ElseIf e.Data.GetDataPresent("System.Windows.Forms.TreeNode") Then
      If trvPathStructure.SelectedNode IsNot Nothing Then
        Dim nd As TreeNode = e.Data.GetData("System.Windows.Forms.TreeNode")
        Dim pt As Path = nd.Tag

        If Convert.ToBoolean(statDisableRename.Tag) = False Then
          Try
            Dim archive As String = pt.FindNearestArchive()
            If Not String.IsNullOrEmpty(archive) Then
              If IO.Directory.Exists(archive) Then
                IO.File.Copy(pt.UNCPath, archive & "\" & Now.ToString("yyyy-MM-dd") & "_" & pt.PathName)
              End If
            End If
          Catch ex As Exception
            Log("{Watcher}(PathStructure_Drop) Failed to copy to archive")
          End Try

          Dim frmt As New FormatDialog(nd.Tag.UNCPath, _pstruct, trvPathStructure.SelectedNode.Text, trvPathStructure.SelectedNode.Tag)
          frmt.ShowDialog()

          statWatchLabel.Text = "File formatted"
        Else '' perform move
          Dim structPath As Path
          If trvPathStructure.SelectedNode.Tag Is Nothing Then '' Folder doesn't exist, so create it
            '' Try to create the folder
            statWatchLabel.Text = "The folder needs to be created first!"
          Else
            structPath = trvPathStructure.SelectedNode.Tag
          End If
          If structPath IsNot Nothing Then
            If structPath.Type = Path.PathType.Folder Then
              Try
                If pt.Type = Path.PathType.File Then
                  IO.File.Move(pt.UNCPath, structPath.UNCPath & "\" & pt.PathName)
                ElseIf pt.Type = Path.PathType.Folder Then
                  IO.Directory.Move(pt.UNCPath, structPath.UNCPath & "\" & pt.PathName)
                End If
                statWatchLabel.Text = "Moved!"
              Catch ex As Exception
                statWatchLabel.Text = "Failed!"
                Log("{Watcher}(PathStructure_Drop) Move Failed: " & ex.Message)
              End Try
            Else
              statWatchLabel.Text = "Can only move into a folder!"
            End If
          End If
        End If
        Me.CurrentPath = New Path(_curPath.PStructure, _curPath.UNCPath) '' "Refresh" view
      End If
    End If

    _dragging = False
  End Sub
  Private Sub trvPathStructure_DragEnter(sender As Object, e As DragEventArgs) Handles trvPathStructure.DragEnter
    '' Adjust cursor
    _dragging = True
    If e.Data.GetDataPresent(DataFormats.FileDrop) Then
      e.Effect = DragDropEffects.Move
    ElseIf e.Data.GetDataPresent("System.Windows.Forms.TreeNode") Then
      e.Effect = DragDropEffects.Move
    End If
  End Sub
  Private Sub trvPathStructure_DragLeave(sender As Object, e As EventArgs) Handles trvPathStructure.DragLeave
    '' Disable flag to set "SelectedNode"
    _dragging = False
  End Sub

  Private Sub trvPathStructure_DragOver(sender As Object, e As DragEventArgs) Handles trvPathStructure.DragOver
    trvPathStructure.Focus()
    Dim pnt As New Point(e.X, e.Y)
    Static snd As TreeNode
    Dim trvPoint As Point = trvPathStructure.PointToClient(pnt)
    If trvPathStructure.Bounds.Contains(trvPoint) Then
      Dim nd As TreeNode = trvPathStructure.GetNodeAt(trvPoint)
      If nd IsNot Nothing Then
        If snd IsNot Nothing Then
          If Not snd.Text = nd.Text Then
            trvPathStructure.SelectedNode = trvPathStructure.GetNodeAt(trvPoint)
            snd = nd
          End If
        Else
          trvPathStructure.SelectedNode = trvPathStructure.GetNodeAt(trvPoint)
          snd = nd
        End If
      End If
    End If
    If trvPathStructure.SelectedNode IsNot Nothing Then
      statWatchLabel.Text = "Drop in " & trvPathStructure.SelectedNode.Text
    End If
  End Sub

  Private Sub trvFileSystem_ItemDrag(sender As Object, e As ItemDragEventArgs) Handles trvFileSystem.ItemDrag
    '' Allow the passing of Real objects to the path structure for Formatting/Moving
    If trvFileSystem.SelectedNode IsNot Nothing Then
      If trvFileSystem.SelectedNode.Tag IsNot Nothing Then
        If trvFileSystem.SelectedNode.Tag.Type = Path.PathType.File Or Convert.ToBoolean(statDisableRename.Tag) = True Then
          statWatchLabel.Text = "Drop into Path Structure to change format"
          trvFileSystem.DoDragDrop(New DataObject("System.Windows.Forms.TreeNode", trvFileSystem.SelectedNode), DragDropEffects.Move)
        Else
          statWatchLabel.Text = "Only files can be changed!"
        End If
      Else
        Debug.WriteLine("Real Node tag doesn't exist")
      End If
    Else
      statWatchLabel.Text = "Select a file!"
      Debug.WriteLine("Real Node not selected")
    End If
  End Sub

  Private Sub statDisableRename_Click(sender As Object, e As EventArgs) Handles statDisableRename.Click
    If statDisableRename.Tag Is Nothing Or String.IsNullOrEmpty(statDisableRename.Tag.ToString) Then statDisableRename.Tag = "False"
    If Convert.ToBoolean(statDisableRename.Tag) = False Then
      statDisableRename.BackColor = Color.Cyan
      statDisableRename.BorderStyle = Border3DStyle.SunkenOuter
      statDisableRename.Tag = "True"
    Else
      statDisableRename.BackColor = DefaultBackColor
      statDisableRename.BorderStyle = Border3DStyle.RaisedOuter
      statDisableRename.Tag = "False"
    End If
  End Sub

  Private Sub mnuOpen_Click(sender As Object, e As EventArgs) Handles mnuOpen.Click
    Dim selPath As New Select_Default_Path()
    selPath.ShowDialog()

    If selPath.DialogResult = DialogResult.OK Then
      Dim selFolder As New SelectFolderDialog()
      selFolder.InitialDirectory = selPath.DefaultPath
      selFolder.ShowDialog()

      If selFolder.DialogResult = DialogResult.OK Then
        Me.CurrentPath = New Path(_pstruct, selFolder.CurrentDirectory)
      End If
    End If
  End Sub

  Private Sub mnuTSPrefix_Click(sender As Object, e As EventArgs) Handles mnuTSPrefix.Click
    If trvFileSystem.SelectedNode IsNot Nothing Then
      If trvFileSystem.SelectedNode.Tag IsNot Nothing Then
        Dim tmp As Path = trvFileSystem.SelectedNode.Tag
        If tmp.Type = Path.PathType.File Then
          IO.File.Move(tmp.UNCPath, tmp.ParentPath & "\" & Now.ToString(My.Settings.strTimeStampFormat) & "_" & tmp.PathName)
        ElseIf tmp.Type = Path.PathType.Folder Then
          IO.Directory.Move(tmp.UNCPath, tmp.ParentPath & "\" & Now.ToString(My.Settings.strTimeStampFormat) & "_" & tmp.PathName)
        End If
      End If
    End If
    Me.CurrentPath = New Path(_pstruct, _curPath.UNCPath)
  End Sub
  Private Sub mnuTSSuffix_Click(sender As Object, e As EventArgs) Handles mnuTSSuffix.Click
    If trvFileSystem.SelectedNode IsNot Nothing Then
      If trvFileSystem.SelectedNode.Tag IsNot Nothing Then
        Dim tmp As Path = trvFileSystem.SelectedNode.Tag
        If tmp.Type = Path.PathType.File Then
          IO.File.Move(tmp.UNCPath, tmp.ParentPath & "\" & tmp.PathName.Replace(tmp.Extension, "") & "_" & Now.ToString(My.Settings.strTimeStampFormat) & tmp.Extension)
        ElseIf tmp.Type = Path.PathType.Folder Then
          IO.Directory.Move(tmp.UNCPath, tmp.ParentPath & "\" & tmp.PathName & "_" & Now.ToString(My.Settings.strTimeStampFormat))
        End If
      End If
    End If
    Me.CurrentPath = New Path(_pstruct, _curPath.UNCPath)
  End Sub
  Private _prevWindow As ExplorerPreview
  Private Sub mnuPreviewWindow_Click(sender As Object, e As EventArgs) Handles mnuPreviewWindow.Click
    If _prevWindow Is Nothing Then
      _prevWindow = New ExplorerPreview(_exploreWatcher, _pstruct)
    Else
      If _prevWindow.IsDisposed Then
        _prevWindow = New ExplorerPreview(_exploreWatcher, _pstruct)
      End If
    End If
    _prevWindow.Show()
  End Sub

  Private Sub mnuAlwaysOnTop_Click(sender As Object, e As EventArgs) Handles mnuAlwaysOnTop.Click
    Main.TopMost = mnuAlwaysOnTop.Checked
  End Sub

  Private _plugins As List(Of IWatcherPlugin)
  Private Sub PopulatePluginList()
    Dim objPlugin As IWatcherPlugin
    Dim plugins() As PluginServices.AvailablePlugin = PluginServices.FindPlugins(My.Settings.strPluginDirectory, "IWatcherPlugin")

    If Not IsNothing(plugins) Then
      '' Clear all plugins from main
      mnuPlugins.DropDownItems.Clear()
      _plugins = New List(Of IWatcherPlugin)
      Dim lstShortcutKeys As New List(Of System.Windows.Forms.Keys)
      For Each plugin As Object In plugins
        objPlugin = DirectCast(PluginServices.CreateInstance(plugin), IWatcherPlugin)
        objPlugin.ReferenceStructure = _pstruct
        Dim tsi As ToolStripMenuItem = Nothing
        If Not String.IsNullOrEmpty(objPlugin.Suite) Then
          If Not mnuPlugins.Has(objPlugin.Suite) Then
            mnuPlugins.DropDownItems.Add(objPlugin.Suite)
            'Else
            '  Debug.WriteLine("Plugin '" & objPlugin.Name & "' has no Suite reference '" & objPlugin.Suite & "'")
          End If
          tsi = mnuPlugins.Find(objPlugin.Suite).DropDownItems.Add(objPlugin.Name)
        Else
          Debug.WriteLine("Plugin '" & objPlugin.Name & "' has no Suite reference '" & objPlugin.Suite & "'")
        End If
        If tsi Is Nothing Then
          tsi = mnuPlugins.DropDownItems.Add(objPlugin.Name)
        End If
        tsi.ToolTipText = objPlugin.Description
        If Not objPlugin.ShortcutKeys = Keys.None And Not lstShortcutKeys.Contains(objPlugin.ShortcutKeys) Then
          tsi.ShortcutKeys = objPlugin.ShortcutKeys '' Apply shortcut keys from plugin
          tsi.ShowShortcutKeys = True '' Show the shortcut key string
          lstShortcutKeys.Add(objPlugin.ShortcutKeys) '' Prevent duplicate shortcut key combos
        End If
        AddHandler tsi.Click, AddressOf objPlugin.Run
        AddHandler objPlugin.Set_CurrentPath, Sub(FSPath As String)
                                                CurrentPath = New Path(_pstruct, FSPath)
                                              End Sub
        AddHandler objPlugin.Get_CurrentPath, Sub(ByRef CurPath As Path)
                                                CurPath = CurrentPath
                                              End Sub
        AddHandler objPlugin.Get_SelectedPath, Sub(ByRef SelPath As Path)
                                                 If trvFileSystem.SelectedNode IsNot Nothing Then
                                                   SelPath = trvFileSystem.SelectedNode.Tag
                                                 End If
                                               End Sub
        _plugins.Add(objPlugin) '' Add plugin to list for later processing


      Next
    End If
  End Sub
  Private Sub Plugins_ChangedCurrentPath(ByVal CurPath As Path)
    If _plugins IsNot Nothing Then
      If _plugins.Count > 0 Then
        For Each plugin As IWatcherPlugin In _plugins
          plugin.Changed_CurrentPath(CurPath)
        Next
      End If
    End If
  End Sub

End Class
Public Module Watcher_Helper
  <System.Runtime.CompilerServices.Extension()> _
  Public Function Find(ByVal mnuItem As ToolStripMenuItem, ByVal Text As String) As ToolStripMenuItem
    If mnuItem.HasDropDownItems Then
      For i = 0 To mnuItem.DropDownItems.Count - 1 Step 1
        If mnuItem.DropDownItems(i).Text = Text Then
          Return mnuItem.DropDownItems(i)
        End If
      Next
    End If
    Return Nothing
  End Function
  <System.Runtime.CompilerServices.Extension()> _
  Public Function Has(ByVal mnuItem As ToolStripMenuItem, ByVal Text As String) As Boolean
    If mnuItem.HasDropDownItems Then
      For i = 0 To mnuItem.DropDownItems.Count - 1 Step 1
        If mnuItem.DropDownItems(i).Text = Text Then
          Return True
        End If
      Next
    End If
    Return False
  End Function
End Module