Imports System.Xml
Imports System.Security.Principal
Imports HTML, HTML.HTMLWriter
Imports PathStructureClass
Imports System.IO

Imports System.Runtime.InteropServices

Imports Shell32               ' for ShellFolderView
Imports SHDocVw               ' for IShellWindows

Public Class Main
  'Public _xml As XmlDocument
  Const MemoryThreshold As ULong = 3000
  Public Shared PathStruct As PathStructure
  Private myXML As XmlDocument
  Private _exploreWatcher As ExplorerWatcher

  Public Sub LogWrapper(ByVal sender As Object, ByVal e As System.String)
    Try
      IO.File.AppendAllText(My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\Path Structure Log.log", e & vbLf)
    Catch ex As Exception
      Debug.WriteLine("Failed to write: " & e)
    End Try
  End Sub

  Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
    Try
      UnregisterHotKey(Me.Handle, 100) '' Unregister Ctrl+F3 HotKey
      If toolExplorerSearch IsNot Nothing Then
        toolExplorerSearch.ActiveWatcher.StopWatcher()
      End If
    Catch ex As Exception
      Log("{Main}(Closing) Error occurred while attempting to close the application: " & vbCrLf & ex.Message)
    End Try
  End Sub
  Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    My.Application.SaveMySettingsOnExit = True

    PathStruct = New PathStructure(My.Settings.SettingsPath,
                                   My.Settings.ERPConnection,
                                   My.Settings.blnERPCheck,
                                   My.Settings.blnDeleteThumbsDb,
                                   True,
                                   True,
                                   True)
    AddHandler PathStructureLog, AddressOf LogWrapper

    '' Set application context menu dynamically
    If Not Settings.CheckRegistryItems() Then
      If Settings.RemoveAllRegistryItems() Then
        If Settings.AddAllRegistryItems() Then
          statStatus.Text = "Added Context Menu"
        Else
          statStatus.Text = "Failed to Add Context Menu. See Settings."
        End If
      Else
        statStatus.Text = "Failed to Remove Context Menu. See Settings."
      End If
    Else
      statStatus.Text = "Registry is valid."
    End If

    Try
      Log(SurroundJoin(Environment.GetCommandLineArgs, "[", "]" & vbTab, True))
    Catch ex As Exception
      LogWrapper(Nothing, "Startup Error: " & ex.Message)
      Exit Sub
    End Try

    If Environment.GetCommandLineArgs.Length > 0 Then
      Dim args As String() = Environment.GetCommandLineArgs
      Dim dg As DialogResult
      Dim strTemp As String = ""
      For Each arg As String In args
        'Log("Command Line Argument: '" & arg & "'")
      Next
      If args.Length >= 2 Then
        Select Case args(1)
          Case "-add"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = GetUNCPath(args(2))
                Log("{Main}(Load) Add Command Received")
                Dim fi As New Add_Folder(GetUNCPath(args(2)))
                fi.Dock = DockStyle.Fill

                pnlContainer.Controls.Add(fi)
                'AddFolder(args(2), args(?))
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
              End If
            End If
          Case "-addall"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = GetUNCPath(args(2))
                dg = MessageBox.Show("Are you sure you wish to create all main folders in the selected directory?", "Verify", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If dg = Windows.Forms.DialogResult.Yes Then
                  Log("{Main}(Load) Add All Command Received")
                  Dim c As New PathStructureClass.Path(PathStruct, GetUNCPath(args(2)))
                  For Each fold As XmlElement In c.PathStructure.SelectNodes("//Folder")
                    If Not fold.Attributes("name").Value.Contains("{") And Not fold.Attributes("name").Value.Contains("}") Then
                      If Not IO.Directory.Exists(PathStruct.ReplaceVariables(PathStruct.GetURIfromXPath(FindXPath(fold)), c.UNCPath)) Then 'c.ReplaceVariables(c.GetURIfromXPath(FindXPath(fold)))) Then
                        IO.Directory.CreateDirectory(PathStruct.ReplaceVariables(PathStruct.GetURIfromXPath(FindXPath(fold)), c.UNCPath)) 'c.ReplaceVariables(c.GetURIfromXPath(FindXPath(fold))))
                      End If
                      'AddFolder(args(2), fold.Attributes("name").Value)
                    End If
                  Next
                End If
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
              End If
              Application.Exit()
            End If
          Case "-format"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = GetUNCPath(args(2))
                Log("{Main}(Load) Format Command Received")
                Dim fi As New Format_Item(GetUNCPath(args(2)))
                fi.Dock = DockStyle.Fill

                pnlContainer.Controls.Add(fi)
                'FormatFile(args(2), args(?))
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
              End If
            End If
          Case "-audit"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = GetUNCPath(args(2))
                Log("{Main}(Load) Audit Command Received")

                Dim pt As New PathStructureClass.Path(PathStruct, GetUNCPath(args(2)))
                Dim bads As New List(Of String)

                '' If audit report exists, then delete it
                Dim arc As String = pt.FindNearestArchive()
                Dim strTS As String = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt")

                If IO.File.Exists(arc & "\" & strTS & "Audit Report.html") Then IO.File.Delete(arc & "\" & strTS & "Audit Report.html")
                Dim auditRpt As New PathStructureClass.Path.AuditVisualReport(pt)
                auditRpt.Audit()
                IO.File.WriteAllText(arc & "\" & strTS & "Audit Report.html", auditRpt.ReportMarkup)
                MessageBox.Show("Audit Successful!", "Audit Complete!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Process.Start(arc & "\" & strTS & "Audit Report.html")
                pt.LogData(arc & "\" & strTS & "Audit Report.html", "Generic Audit")
                Application.Exit()
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
              End If
            End If
          Case "-clipboard"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = GetUNCPath(args(2))
                Log("{Main}(Load) Clipboard Command Received")
                Dim fi As New FileClipboard(GetUNCPath(args(2)))
                fi.Dock = DockStyle.Fill

                pnlContainer.Controls.Add(fi)
                'FormatFile(args(2), args(?))
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
              End If
            End If
          Case "-transfer"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = args(2)
                Log("{Main}(Load) Transfer_Files_By_Extension Command Received")
                Dim fi As New Transfer_FilesByExtension(args(2))
                fi.Dock = DockStyle.Fill

                pnlContainer.Controls.Add(fi)
                'FormatFile(args(2), args(?))
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Application.Exit()
              End If
            End If
          Case "-preview"
            If args.Length >= 3 Then
              If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(args(2)).ToLower.StartsWith(defaultPath) Then
                statCurrentPath.Text = GetUNCPath(args(2))
                Log("{Main}(Load) Preview Command Received")

                '' Load preview control
                Me.WindowState = FormWindowState.Maximized
                Application.DoEvents()
                Dim prev As New Preview(GetUNCPath(args(2)))
                prev.Dock = DockStyle.Fill
                pnlContainer.Controls.Add(prev)
                prev.lstFolders.Focus()
              Else
                Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Application.Exit()
              End If
            End If
          Case "-heatmap"
            If args.Length >= 3 Then
              statCurrentPath.Text = GetUNCPath(args(2))
              Log("{Main}(Load) Folder Heat Map Command Received")

              '' Load preview control
              Me.WindowState = FormWindowState.Maximized
              Application.DoEvents()
              Dim fhm As New FolderHeatMap(GetUNCPath(args(2)))
              fhm.Dock = DockStyle.Fill
              pnlContainer.Controls.Add(fhm)
            End If
          Case "-permissions"
            If args.Length >= 3 Then
              If IO.Directory.Exists(args(2)) Then
                If PathStruct.IsInDefaultPath(GetUNCPath(args(2))) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) And Not GetUNCPath(opn.CurrentDirectory) = defaultPath Then
                  Dim pt As New PathStructureClass.Path(PathStruct, GetUNCPath(args(2)))
                  If pt.IsNameStructured() Then
                    If pt.StructureCandidates.Count = 1 Then
                      If pt.StructureCandidates(0).XElement.HasAttribute("permissionsgroup") Then
                        '' If allowed, check if this path requires permissions and set the permissions if allowed to
                        If pt.Users IsNot Nothing Then
                          pt.Users.SetPermissionsByGroup(pt.UNCPath, pt.StructureCandidates(0).XElement.Attributes("permissionsgroup").Value)
                          MessageBox.Show("Complete!")
                        Else
                          MessageBox.Show("Couldn't set permissions because no users could be read from the Path Structure settings", "No Users", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        End If
                      Else
                        MessageBox.Show("The detected Path Structure (" & pt.StructureCandidates(0).StructurePath & ") did not have a permissions group attribute applied in the Path Structure settings. Talk to an administrator to see that this is resolved.", "Missing 'permissionsgroup' attribute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                      End If
                    Else
                      MessageBox.Show("Too many potential Path Structures detected and couldn't determine a valid Path Structure from the following path: " & vbLf & pt.UNCPath, "Invalid Path Structure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    End If
                  Else
                    MessageBox.Show("Couldn't determine a valid Path Structure from the following path: " & vbLf & pt.UNCPath, "Invalid Path Structure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                  End If
                Else
                  Log("{Main}(Load) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(args(2)) & "'")
                  MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
              End If
            End If
            Application.Exit()
          Case "-archive"
            If args.Length >= 3 Then
              Dim cnt As Integer = 0
              For i = 2 To args.Length - 1 Step 1
                'Log("{Main}(Load) Archive command received '" & args(i) & "'")
                If Not String.IsNullOrEmpty(args(i)) Then
                  strTemp = GetUNCPath(args(i))
                  'Log("{Main}(Load) Going to try to archive '" & strTemp & "'")
                  If PathStruct.IsInDefaultPath(strTemp) Then
                    statCurrentPath.Text = strTemp

                    '' Define path object
                    Dim tmp As New PathStructureClass.Path(PathStruct, strTemp)
                    If tmp.Type = PathStructureClass.Path.PathType.File Then
                      Dim arc As String = tmp.FindNearestArchive()
                      'Log(vbTab & "{Main}(Load) Archive path '" & arc & "'")
                      If Not String.IsNullOrEmpty(arc) Then
                        If Not IO.Directory.Exists(arc) Then
                          IO.Directory.CreateDirectory(arc)
                        End If
                        If IO.Directory.Exists(arc) Then
                          IO.File.Move(tmp.UNCPath, arc & "\" & DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt") & "_" & tmp.PathName)
                          cnt += 1
                        End If
                      Else
                        statStatus.Text = "Couldn't find a valid Archive directory"
                      End If
                    Else
                      statStatus.Text = "Can only process files!"
                    End If
                  Else
                    statStatus.Text = "Path not part of a path structure"
                  End If
                End If
              Next
              If cnt > 0 Then
                MessageBox.Show("Moved '" & cnt.ToString & "' files to the closest archive", "Archive Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
              Else
                MessageBox.Show("No files were archived", "Archive Unsuccessful", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
              End If
            End If
            Application.Exit()
          Case "-watcher"
            mnuExplorerWatcher_Click(mnuExplorerWatcher, Nothing)
        End Select
      End If
    End If
  End Sub

  Private Sub mnuSettings_Click(sender As Object, e As EventArgs) Handles mnuSettings.Click
    Dim identity = WindowsIdentity.GetCurrent
    Dim principal = New WindowsPrincipal(identity)
    If principal.IsInRole(WindowsBuiltInRole.Administrator) Then
      If Not Settings.Visible Then
        Settings.Show()
      Else
        Settings.Focus()
      End If
    Else
      If MessageBox.Show("You must have Administrative rights to access this window. Would you like to restart the application as admin?",
                         "Admin Only", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = Windows.Forms.DialogResult.Yes Then
        Dim procSI As New ProcessStartInfo
        Dim procExe As New Process
        With procSI
          .UseShellExecute = True
          .FileName = System.Reflection.Assembly.GetExecutingAssembly().Location
          .WindowStyle = ProcessWindowStyle.Normal
          .Verb = "runas"
        End With
        procExe = Process.Start(procSI)
        Application.Exit()
      End If
    End If
  End Sub

  Private Sub mnuToolsAddAll_Click(sender As Object, e As EventArgs) Handles mnuToolsAddAll.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If

    pnlContainer.Controls.Clear()
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory), defaultPath) Then ' IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) Then
        Debug.WriteLine("Adding folders for '" & GetUNCPath(opn.CurrentDirectory) & "'")
        If IsNothing(myXML) Then
          myXML = New XmlDocument
          myXML.Load(My.Settings.SettingsPath)
        End If
        Log("{Main}(ToolsAddAll) Add All Command Received")
        Dim c As New PathStructureClass.Path(PathStruct, GetUNCPath(opn.CurrentDirectory))
        For Each fold As XmlElement In myXML.SelectNodes("//Folder")
          If Not fold.Attributes("name").Value.Contains("{") And Not fold.Attributes("name").Value.Contains("}") Then
            If Not IO.Directory.Exists(PathStruct.ReplaceVariables(PathStruct.GetURIfromXPath(FindXPath(fold)), c.UNCPath)) Then 'c.ReplaceVariables(c.GetURIfromXPath(FindXPath(fold)))) Then
              IO.Directory.CreateDirectory(PathStruct.ReplaceVariables(PathStruct.GetURIfromXPath(FindXPath(fold)), c.UNCPath)) 'c.ReplaceVariables(c.GetURIfromXPath(FindXPath(fold))))
            End If
            'AddFolder(args(2), fold.Attributes("name").Value)
          End If
        Next
      Else
        Log("{Main}(ToolsAddAll)Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Sub mnuToolAddFolder_Click(sender As Object, e As EventArgs) Handles mnuToolAddFolder.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    pnlContainer.Controls.Clear()
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) And Not GetUNCPath(opn.CurrentDirectory) = defaultPath Then
        Log("{Main}(ToolsAddFolder)Add Command Received")
        Dim fi As New Add_Folder(GetUNCPath(opn.CurrentDirectory))
        fi.Dock = DockStyle.Fill

        pnlContainer.Controls.Add(fi)
        'AddFolder(args(2), args(?))
      Else
        Log("{Main}(ToolsAddFolder)Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must select a folder within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Sub mnuToolsFormat_Click(sender As Object, e As EventArgs) Handles mnuToolsFormat.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    pnlContainer.Controls.Clear()
    Dim opn As New OpenFileDialog
    opn.Title = "Select a file under the 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.File.Exists(opn.FileName) Then
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.FileName)) Then ' GetUNCPath(opn.FileName).StartsWith(defaultPath) Then
        Log("{Main}(ToolsFormat)Format Command Received")
        Dim fi As New Format_Item(GetUNCPath(opn.FileName))
        fi.Dock = DockStyle.Fill

        pnlContainer.Controls.Add(fi)
        'FormatFile(args(2), args(?))
      Else
        Log("{Main}(ToolsFormat)Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.FileName) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Sub mnuToolsAuditFile_Click(sender As Object, e As EventArgs) Handles mnuToolsAuditFile.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    Dim opn As New OpenFileDialog
    opn.Title = "Select a file under the 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.File.Exists(opn.FileName) Then
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.FileName)) Then ' GetUNCPath(opn.FileName).StartsWith(defaultPath) Then
        Dim pt As New PathStructureClass.Path(PathStruct, GetUNCPath(opn.FileName))
        Dim bads As New List(Of String)

        Dim arc As String = pt.FindNearestArchive()
        Dim strTS As String = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt")
        '' If audit report exists, then delete it
        If IO.File.Exists(arc & "\" & strTS & "Audit Report.html") Then IO.File.Delete(arc & "\" & strTS & "Audit Report.html")
        Dim auditRpt As New PathStructureClass.Path.AuditVisualReport(pt)
        auditRpt.Audit()
        'pt.LogData(pt.StartPath & "\" & strTS & "Audit Report.html", "Generic Audit")
        Try
          IO.File.WriteAllText(arc & "\" & strTS & "Audit Report.html", auditRpt.ReportMarkup)
          Process.Start(arc & "\" & strTS & "Audit Report.html")
        Catch ex As Exception
          MessageBox.Show("An error occurred while attempting to create the audit report: " & vbLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
      Else
        Log("{Main}(ToolsAuditFile) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.FileName) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Sub mnuToolsAuditFolder_Click(sender As Object, e As EventArgs) Handles mnuToolsAuditFolder.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) And Not GetUNCPath(opn.CurrentDirectory) = defaultPath Then
        Dim pt As New PathStructureClass.Path(PathStruct, GetUNCPath(opn.CurrentDirectory))

        Dim strTS As String = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt")
        '' If audit report exists, then delete it
        Dim auditRpt As New PathStructureClass.Path.AuditVisualReport(pt)
        auditRpt.Audit()
        Dim arc As String = pt.FindNearestArchive()
        If IO.File.Exists(arc & "\" & strTS & "Audit Report.html") Then IO.File.Delete(arc & "\" & strTS & "Audit Report.html")
        Try
          IO.File.WriteAllText(arc & "\" & strTS & "Audit Report.html", auditRpt.ReportMarkup)
          Process.Start(arc & "\" & strTS & "Audit Report.html")
        Catch ex As Exception
          MessageBox.Show("An error occurred while attempting to create the audit report: " & vbLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        pt.LogData(arc & "\" & strTS & "Audit Report.html", "Generic Audit")
      Else
        Log("{Main}(ToolsAuditFolder) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub

  Private cancelAudit As Boolean = False
  Private Sub CancelAudit_Clicked(ByVal sender As System.Object, ByVal e As EventArgs)
    cancelAudit = True
    If auditRpt IsNot Nothing Then auditRpt.Quit()
    If bgwAudit.IsBusy Then bgwAudit.CancelAsync()
  End Sub
  'Private Sub mnuToolsAuditDefaultPath_Click(sender As Object, e As EventArgs)
  '  pnlContainer.Controls.Clear()
  '  Dim rtb As New RichTextBox
  '  rtb.Dock = DockStyle.Fill
  '  Dim btnCancel As New Button
  '  btnCancel.Text = "Cancel Audit"
  '  btnCancel.Dock = DockStyle.Bottom
  '  AddHandler btnCancel.Click, AddressOf CancelAudit_Clicked
  '  pnlContainer.Controls.Add(btnCancel)
  '  pnlContainer.Controls.Add(rtb)


  '  Dim tot, bad As Integer
  '  Dim stp As New Stopwatch
  '  stp.Start()
  '  Dim auditRpt As New PathStructure.AuditReport(New PathStructure(defaultPath))
  '  Dim blnSuccess As Boolean = True
  '  Dim curIndex As Integer = 0
  '  Dim totCustomers As Integer = IO.Directory.EnumerateDirectories(defaultPath).Count
  '  Dim lstTime As New List(Of Double)
  '  Dim lstAverageHistory As New List(Of Double)
  '  Dim stpWatch As New Stopwatch
  '  Dim averageTime As Double

  '  Dim ERPVariables As New SortedList(Of String, String)

  '  For Each cust As String In IO.Directory.GetDirectories(defaultPath)
  '    statProgress.Value = (curIndex / totCustomers) * 100
  '    curIndex += 1
  '    '' If parsed Customer is invalid in E2, then continue by skipping
  '    If My.Settings.blnERPCheck Then
  '      Dim c As New PathStructure(cust)

  '      Dim ERPCustCodeTable As String = ""
  '      ERPVariables = GetERPVariables("Audit_CustCode", ERPCustCodeTable, c)
  '      If Not IsValidERP(ERPCustCodeTable, c.Variables) Then
  '        auditRpt.Report("Skipped '" & cust & "' as the Customer Code could not be found in the E2 database.", PathStructure.AuditReport.StatusCode.ErrorStatus)
  '        rtb.AppendText("Skipping '" & cust & "' because could not be found in ERP system." & vbCrLf)
  '        Continue For
  '      Else
  '        rtb.AppendText("Found '" & cust & "' in ERP system. ")
  '      End If
  '    End If

  '    For Each part As String In IO.Directory.GetDirectories(cust)
  '      stpWatch.Restart()
  '      rtb.AppendText("Auditing '" & part & "'..." & vbLf)
  '      statCurrentPath.Text = "'" & part & "'"
  '      Application.DoEvents()

  '      Dim pt As New PathStructure(part)

  '      Dim ERPPartNoTable As String = ""
  '      '' Check if user want to check ERP system
  '      ERPVariables = GetERPVariables("Audit_PartNo", ERPPartNoTable, pt)
  '      If IsValidERP(ERPPartNoTable, ERPVariables) Then
  '        If Not pt.Audit(auditRpt) Then
  '          bad += 1
  '          blnSuccess = False
  '        End If
  '        rtb.ScrollToCaret()
  '        tot += 1

  '        '' Do math to display
  '        lstTime.Add(stpWatch.ElapsedMilliseconds)
  '        averageTime += stpWatch.ElapsedMilliseconds
  '        lstAverageHistory.Add((averageTime / lstTime.Count))
  '        statStatus.Text = "Average Processing Time (ms): " & (averageTime / lstTime.Count).ToString & vbTab & "Last Processing Time (ms): " & stpWatch.ElapsedMilliseconds.ToString

  '      End If
  '      If cancelAudit Then Exit For
  '    Next

  '    System.GC.Collect()

  '    '' Keep track of RichTextBox length and clear frequently to avoid bogging down computer
  '    If rtb.Lines.Count > 100 Then
  '      rtb.Clear()
  '    End If
  '    If cancelAudit Then Exit For
  '  Next
  '  cancelAudit = False


  '  'auditRpt.Raw(New HTML.HTMLWriter.HTMLCanvas("canvGraph", New HTML.AttributeList({New HTML.AttributeList.AttributeItem("width", "400"), New HTML.AttributeList.AttributeItem("height", "400")}, {New HTML.AttributeList.StyleItem("border", "1px solid black")})).Markup)
  '  'auditRpt.Raw("<script>" & My.Resources.DrawGraph)
  '  'Dim dataPoints As New System.Text.StringBuilder
  '  'dataPoints.Append(String.Join(",", lstTime.ToArray))
  '  'If dataPoints.Chars(dataPoints.Length - 1) = "," Then dataPoints.Remove(dataPoints.Length - 1, 1)
  '  'auditRpt.Raw("var dataPoints = [" & dataPoints.ToString & "];")
  '  'dataPoints = New System.Text.StringBuilder
  '  'dataPoints.Append(String.Join(",", lstAverageHistory.ToArray))
  '  'If dataPoints.Chars(dataPoints.Length - 1) = "," Then dataPoints.Remove(dataPoints.Length - 1, 1)
  '  'auditRpt.Raw("var dataAverage = [" & dataPoints.ToString & "];")
  '  'auditRpt.Raw("DrawPoints(dataAverage, 'yellow');")
  '  'auditRpt.Raw("DrawPoints(dataPoints, 'black');</script>")

  '  stp.Stop()
  '  Dim ts As New TimeSpan(0, 0, 0, 0, stp.ElapsedMilliseconds)
  '  rtb.AppendText(vbLf & "Audited '" & auditRpt.FileCount.ToString & "' files in '" & ts.Hours.ToString & ":" & ts.Minutes.ToString & ":" & ts.Seconds.ToString & "." & ts.Milliseconds.ToString & "'" & vbLf)
  '  rtb.AppendText(bad.ToString & "/" & tot.ToString & " part folders contained Path Structure error(s)." & vbLf)

  '  If Not blnSuccess Then
  '    MessageBox.Show("Invalid path(s) found. Click OK to open temporary report...", "Audit Failed!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
  '    IO.File.WriteAllText(defaultPath & "\Audit Report.html", auditRpt.ReportMarkup)
  '    Try
  '      Process.Start(defaultPath & "\Audit Report.html")
  '    Catch ex As Exception
  '      MessageBox.Show("An error occurred while attempting to create the audit report: " & vbLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
  '    End Try
  '  Else
  '    MessageBox.Show("Audit Successful!" & vbLf & "No errors found in the Path Structure", "Audit Success!", MessageBoxButtons.OK, MessageBoxIcon.Information)
  '  End If
  'End Sub
  Private Sub mnuToolsClipboard_Click(sender As Object, e As EventArgs) Handles mnuToolsClipboard.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      statStatus.Text = opn.CurrentDirectory
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory), defaultPath) Then ' IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) Then
        Log("{Main}(ToolsClipboard) Clipboard Command Received")
        Dim fi As New FileClipboard(GetUNCPath(opn.CurrentDirectory))
        fi.Dock = DockStyle.Fill

        pnlContainer.Controls.Add(fi)
        'FormatFile(args(2), args(?))
      Else
        Log("{Main}(ToolsClipboard) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Sub mnuToolsTransferFilesByExtension_Click(sender As Object, e As EventArgs) Handles mnuToolsTransferFilesByExtension.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog()
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    Debug.WriteLine("After dialog: " & defaultPath)
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a 'parts' folder to search for transfer files:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      statStatus.Text = opn.CurrentDirectory
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory), defaultPath) Then ' IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) Then
        Log("{Main}(ToolsTransferFileExt) Transfer_Files_By_Extension Command Received")
        Dim fi As New Transfer_FilesByExtension(GetUNCPath(opn.CurrentDirectory))
        fi.Dock = DockStyle.Fill

        pnlContainer.Controls.Add(fi)
        'FormatFile(args(2), args(?))
      Else
        Log("{Main}(ToolsTransferFileExt) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Sub mnuToolsPreview_Click(sender As Object, e As EventArgs) Handles mnuToolsPreview.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a folder to search for preview:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      statStatus.Text = opn.CurrentDirectory
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory), defaultPath) Then ' IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) Then
        Log("{Main}(ToolsPreview) Preview Command Received")
        Dim fi As New Preview(GetUNCPath(opn.CurrentDirectory))
        fi.Dock = DockStyle.Fill

        pnlContainer.Controls.Add(fi)
        fi.lstFolders.Focus()
        'FormatFile(args(2), args(?))
      Else
        Log("{Main}(ToolsPreview) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub
  Private Function RecursivePathStructure(ByVal node As XmlElement) As HTMLList.ListItem
    Dim li As HTMLList.ListItem
    Dim cls As String = ""
    If node.Name = "Folder" Then
      cls = "folder"
    ElseIf node.Name = "File" Or node.Name = "Option" Then
      cls = "file"
    End If
    li = New HTMLList.ListItem("<b>" & node.Attributes("name").Value.ToString & "</b>: " & node.Attributes("description").Value.ToString, New AttributeList({"class"}, {cls}))
    If node.SelectNodes("./*").Count > 0 Then
      Dim lst As New HTMLList(HTMLList.ListType.Unordered)
      For Each chld As XmlElement In node.SelectNodes("./*")
        lst.AddListItem(RecursivePathStructure(chld))
      Next
      lst.SetList()
      li.AddInnerHTML(lst.Markup)
      li.AttributeList = New AttributeList({"class"}, {cls & " contains"})
    End If
    Return li
  End Function
  Private Sub mnuToolsFolderHeatMap_Click(sender As Object, e As EventArgs) Handles mnuToolsFolderHeatMap.Click
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a folder to search for preview:"
    'If IO.Directory.Exists(defaultPath) Then
    '  opn.InitialDirectory = defaultPath
    'End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      statCurrentPath.Text = GetUNCPath(opn.CurrentDirectory)
      Log("{Main}(ToolsFolderHeatMap) Preview Command Received")

      '' Load preview control
      Me.WindowState = FormWindowState.Maximized
      Application.DoEvents()
      Dim fhm As New FolderHeatMap(GetUNCPath(opn.CurrentDirectory))
      fhm.Dock = DockStyle.Fill
      pnlContainer.Controls.Add(fhm)
    End If
  End Sub
  Private Sub mnuToolsSetPermissions_Click(sender As Object, e As EventArgs) Handles mnuToolsSetPermissions.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If
    Dim opn As New SelectFolderDialog
    opn.Title = "Select a 'parts' folder to format:"
    If IO.Directory.Exists(defaultPath) Then
      opn.InitialDirectory = defaultPath
    End If
    opn.ShowDialog()
    If IO.Directory.Exists(opn.CurrentDirectory) And opn.DialogResult = Windows.Forms.DialogResult.OK Then
      If PathStruct.IsInDefaultPath(GetUNCPath(opn.CurrentDirectory)) Then ' GetUNCPath(opn.CurrentDirectory).StartsWith(defaultPath) And Not GetUNCPath(opn.CurrentDirectory) = defaultPath Then
        Dim pt As New PathStructureClass.Path(PathStruct, GetUNCPath(opn.CurrentDirectory))
        If pt.IsNameStructured() Then
          If pt.StructureCandidates.Count = 1 Then
            If pt.StructureCandidates(0).XElement.HasAttribute("permissionsgroup") Then
              '' If allowed, check if this path requires permissions and set the permissions if allowed to
              If pt.Users IsNot Nothing Then
                pt.Users.SetPermissionsByGroup(pt.UNCPath, pt.StructureCandidates(0).XElement.Attributes("permissionsgroup").Value)
                MessageBox.Show("Complete!")
              Else
                MessageBox.Show("Couldn't set permissions because no users could be read from the Path Structure settings", "No Users", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
              End If
            Else
              MessageBox.Show("The detected Path Structure (" & pt.StructureCandidates(0).StructurePath & ") did not have a permissions group attribute applied in the Path Structure settings. Talk to an administrator to see that this is resolved.", "Missing 'permissionsgroup' attribute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End If
          Else
            MessageBox.Show("Too many potential Path Structures detected and couldn't determine a valid Path Structure from the following path: " & vbLf & pt.UNCPath, "Invalid Path Structure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
          End If
        Else
          MessageBox.Show("Couldn't determine a valid Path Structure from the following path: " & vbLf & pt.UNCPath, "Invalid Path Structure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
      Else
        Log("{Main}(ToolsSetPermissions) Path does not match default path!" & vbCrLf & vbTab & "'" & GetUNCPath(opn.CurrentDirectory) & "'")
        MessageBox.Show("You must be within a default path!", "Wrong Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    End If
  End Sub

  Dim auditList As ListBox
  Dim auditRpt As PathStructureClass.Path.AuditVisualReport
  Private Sub mnuToolsAuditVisualDefaultPath_Click(sender As Object, e As EventArgs) Handles mnuToolsAuditVisualDefaultPath.Click
    Dim dialogSelect As New Select_Default_Path()
    Dim dg As DialogResult = dialogSelect.ShowDialog()
    Dim defaultPath As String
    If dg = Windows.Forms.DialogResult.OK And Not String.IsNullOrEmpty(dialogSelect.DefaultPath) Then
      defaultPath = dialogSelect.DefaultPath
    Else
      Exit Sub
    End If

    pnlContainer.Controls.Clear()
    auditList = New ListBox
    auditList.Dock = DockStyle.Fill
    Dim btnCancel As New Button
    btnCancel.Text = "Cancel Audit"
    btnCancel.Dock = DockStyle.Bottom
    AddHandler btnCancel.Click, AddressOf CancelAudit_Clicked
    pnlContainer.Controls.Add(btnCancel)
    pnlContainer.Controls.Add(auditList)


    auditRpt = New PathStructureClass.Path.AuditVisualReport(New PathStructureClass.Path(PathStruct, defaultPath))
    AddHandler auditRpt.ChildAudited, AddressOf AuditChildUpdate
    AddHandler auditRpt.GrandChildAudited, AddressOf AuditGrandchildUpdate
    cancelAudit = False

    Dim totCustomers As Integer = IO.Directory.EnumerateDirectories(defaultPath).Count
    Dim tot As Integer
    Dim stp As New Stopwatch
    stp.Start()

    bgwAudit.RunWorkerAsync()

    AddHandler bgwAudit.RunWorkerCompleted, Sub()
                                              stp.Stop()
                                              Dim ts As New TimeSpan(0, 0, 0, 0, stp.ElapsedMilliseconds)
                                              auditList.Items.Insert(0, "Audited '" & auditRpt.FileCount.ToString & "' files in '" & ts.Hours.ToString & ":" & ts.Minutes.ToString & ":" & ts.Seconds.ToString & "." & ts.Milliseconds.ToString & "'")
                                              auditList.Items.Insert(0, tot.ToString & " part folders audited.")
                                              MessageBox.Show("Click OK to open temporary report...", "Audit Complete!", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                              Dim strTS As String = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss tt")
                                              IO.File.WriteAllText(defaultPath & "\" & strTS & "Audit Report.html", auditRpt.ReportMarkup)
                                              Try
                                                Process.Start(defaultPath & "\" & strTS & "Audit Report.html")
                                              Catch ex As Exception
                                                MessageBox.Show("An error occurred while attempting to create the audit report: " & vbLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                              End Try
                                            End Sub

  End Sub
  Private Sub RunAudit() Handles bgwAudit.DoWork
    auditRpt.Audit()
  End Sub
  'Public Sub ThreadPart(ByVal params As Object)
  '  params(0).AuditVisualChildren(params(1), params(2), params(0))
  '  params(0).Dispose()
  'End Sub
  'Private Sub AuditChildUpdate(ByVal sender As Object, ByVal e As PathStructure.AuditVisualReport.AuditedEventArgs)
  '  'statProgress.Value = (e.Index / e.ParentTotal) * 100
  '  'If auditList IsNot Nothing Then
  '  '  If Not auditList.InvokeRequired Then
  '  '    auditList.Items.Insert(0, "Child audited '" & e.Path & "'")
  '  '  End If
  '  'End If
  '  Me.AuditUpdate(e)
  '  Application.DoEvents()
  'End Sub
  'Private Sub AuditGrandChildUpdate(ByVal sender As Object, ByVal e As PathStructure.AuditVisualReport.AuditedEventArgs)
  '  statCurrentPath.Text = e.Path
  '  If auditGrandchildProgress IsNot Nothing Then
  '    auditGrandchildProgress.Value = (e.Index / e.ParentTotal) * 100
  '  End If
  '  If auditList IsNot Nothing Then
  '    If Not auditList.InvokeRequired Then
  '      auditList.Items.Insert(0, "Grandchild audited '" & e.Path & "'")
  '    End If
  '  End If
  'End Sub
  Delegate Sub AuditChildUpdateCallback(e As PathStructureClass.Path.AuditVisualReport.AuditedEventArgs)
  Private Sub AuditChildUpdate(ByVal e As PathStructureClass.Path.AuditVisualReport.AuditedEventArgs)
    If Me.auditList.InvokeRequired Then
      Dim d As New AuditChildUpdateCallback(AddressOf AuditChildUpdate)
      Me.Invoke(d, New Object() {e})
    Else
      If Me.auditList.Items.Count > 100 Then Me.auditList.Items.Clear()
      Me.auditList.Items.Insert(0, "Audited '" & e.Path & "'")
      Me.statProgress.Value = (e.Index / e.ParentTotal) * 100
      Me.statStatus.Text = e.Index.ToString & " / " & e.ParentTotal.ToString & " children"
      auditGrandchildCount = 0
      'Application.DoEvents()
    End If
  End Sub
  Delegate Sub AuditGrandchildUpdateCallback(e As PathStructureClass.Path.AuditVisualReport.AuditedEventArgs)
  Private auditGrandchildCount As Integer
  Private Sub AuditGrandchildUpdate(ByVal e As PathStructureClass.Path.AuditVisualReport.AuditedEventArgs)
    If Me.auditList.InvokeRequired Then
      Dim d As New AuditGrandchildUpdateCallback(AddressOf AuditGrandchildUpdate)
      Me.Invoke(d, New Object() {e})
    Else
      auditGrandchildCount += 1
      Me.statCurrentPath.Text = auditGrandchildCount.ToString & " objects in child"
      'Application.DoEvents()
    End If
  End Sub

  Public Function AddFolder(ByVal CurrentPath As String, ByVal PathName As String)
    Dim strTemp As String
    Dim dg As DialogResult
    strTemp = IO.Path.Combine(CurrentPath, PathName)

    Dim pt As New PathStructureClass.Path(PathStruct, CurrentPath)


    Dim folderName As String
    folderName = PathStruct.ReplaceVariables(strTemp, CurrentPath) 'pt.ReplaceVariables(strTemp)
    If folderName.Contains("{Date}") Then folderName = folderName.Replace("{Date}", DateTime.Now.ToString("MM-dd-yyyy"))
    If folderName.Contains("{Time}") Then folderName = folderName.Replace("{Time}", DateTime.Now.ToString("hh-mm-ss tt"))

    Log("{Main}(AddFolder) Pre-Folder: " & folderName)

    Dim inpt As String() = GetListOfInternalStrings(folderName, "{", "}")
    If inpt.Length > 0 Then
      Try
        Dim genDialog As New Generic_Dialog(inpt.Distinct.ToArray)
        genDialog.ShowDialog()
        If genDialog.DialogResult = Windows.Forms.DialogResult.OK Then
          For Each Val As KeyValuePair(Of String, String) In genDialog.Values
            folderName = folderName.Replace("{" & Val.Key & "}", Val.Value)
          Next
        Else
          Return False '' User chose not to continue somehow
        End If
      Catch ex As Exception
        Log("{Main}(AddFolder) Error showing dialog" & vbCrLf & vbTab & ex.Message)
        Return False
      End Try
    End If

    Log("{Main}(AddFolder) Post-FolderName: " & folderName)

    dg = MessageBox.Show("Are you sure you wish to create the following directory?" & vbLf & "'" & folderName & "'", "Verify", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
    If dg = Windows.Forms.DialogResult.Yes Then
      IO.Directory.CreateDirectory(folderName)
      pt.LogData(folderName, "Create Folder")
    End If
    Return True
  End Function

  Private Sub mnuExplorerWatcher_Click(sender As Object, e As EventArgs) Handles mnuExplorerWatcher.Click
    Me.SetDesktopLocation(My.Computer.Screen.WorkingArea.Width - Me.Width,
                                          My.Computer.Screen.WorkingArea.Height - Me.Height)
    pnlContainer.Controls.Clear()

    toolExplorerSearch = New Watcher(PathStruct)
    toolExplorerSearch.Dock = DockStyle.Fill
    pnlContainer.Controls.Add(toolExplorerSearch)

    RegisterHotKey(Me.Handle, 100, MOD_ALT, Keys.F3) '' Register Ctrl+F3 hotkey

    mnuExplorerWatcher.Enabled = False
  End Sub

  <DllImport("User32.dll")> _
  Public Shared Function RegisterHotKey(ByVal hwnd As IntPtr, _
                                        ByVal id As Integer, ByVal fsModifiers As Integer,
                                        ByVal vk As Integer) As Integer
  End Function
  <DllImport("User32.dll")> _
  Public Shared Function UnregisterHotKey(ByVal hwnd As IntPtr, _
                                          ByVal id As Integer) As Integer
  End Function

  Private toolExplorerSearch As Watcher
  Public Const MOD_ALT As Integer = &H1
  Public Const WM_HOTKEY As Integer = &H312

  Protected Overrides Sub WndProc(ByRef m As Message)
    If m.Msg = WM_HOTKEY Then
      Dim id As IntPtr = m.WParam
      Select Case (id.ToString)
        Case "100"
          If toolExplorerSearch Is Nothing Then
            If _exploreWatcher.CurrentFoundPaths IsNot Nothing Then
              'If _exploreWatcher.CurrentFoundPaths.FoundPaths.Count > 0 Then
              '  toolExplorerSearch = New Watcher(PathStruct)
              'Else
              toolExplorerSearch = New Watcher(PathStruct)
              'End If
              Me.SetDesktopLocation(My.Computer.Screen.WorkingArea.Width - Me.Width,
                                                    My.Computer.Screen.WorkingArea.Height - Me.Height)
              toolExplorerSearch.Show()
            End If
          Else
            If toolExplorerSearch.IsDisposed Then
              If _exploreWatcher.CurrentFoundPaths IsNot Nothing Then
                'If _exploreWatcher.CurrentFoundPaths.FoundPaths.Count > 0 Then
                '  toolExplorerSearch = New Watcher(PathStruct)
                'Else
                toolExplorerSearch = New Watcher(PathStruct)
                'End If
                Me.SetDesktopLocation(My.Computer.Screen.WorkingArea.Width - Me.Width,
                                                      My.Computer.Screen.WorkingArea.Height - Me.Height)
                toolExplorerSearch.Show()
              End If
            Else
              If toolExplorerSearch.Visible Then
                toolExplorerSearch.Hide()
              Else
                toolExplorerSearch.Show()
              End If
            End If
          End If
      End Select
    End If
    MyBase.WndProc(m)
  End Sub
End Class
