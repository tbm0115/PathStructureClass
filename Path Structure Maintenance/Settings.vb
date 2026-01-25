Imports Microsoft.Win32
Imports System.Security, System.Security.Principal, System.Security.AccessControl

Public Class Settings
  Private Const regFoldMain As String = "AllFilesystemObjects\\shell\\PathStructure\\"
  Private Const regCommand As String = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\CommandStore\\shell\\"
  Private exePath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
  Private _regItems As New List(Of RegistryItem)

  Public Sub Settings_Load(sender As Object, e As EventArgs) Handles Me.Load
    CheckRegistryItems()
  End Sub

  Public Function CheckRegistryItems() As Boolean
    _regItems.Clear()
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Add", "Create", "", "", "PathStructure.Add.All;PathStructure.Add.Single;"))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Add.All", "Create All main folders", "%windir%\system32\shell32.dll,278", Chr(34) & exePath & Chr(34) & " -addall " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Add.Single", "Create a Folder...", "%windir%\system32\shell32.dll,279", Chr(34) & exePath & Chr(34) & " -add " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Format", "Rename selected file...", "%windir%\system32\comres.dll,6", Chr(34) & exePath & Chr(34) & " -format " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Audit", "Audit selected object...", "%windir%\system32\imageres.dll,109", Chr(34) & exePath & Chr(34) & " -audit " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Open", "Open Path Structure Application", "%windir%\system32\shell32.dll,261", Chr(34) & exePath & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Clipboard", "Generate Path to Clipboard...", "%windir%\system32\mmcndmgr.dll,21", Chr(34) & exePath & Chr(34) & " -clipboard " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.TransferByExtension", "Transfer Files by Extension...", "%windir%\system32\shell32.dll,132", Chr(34) & exePath & Chr(34) & " -transfer " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.Preview", "Preview Document(s)...", "%windir%\system32\ieframe.dll,66", Chr(34) & exePath & Chr(34) & " -preview " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.FolderHeatMap", "Heat Map...", "%windir%\system32\compstui.dll,51", Chr(34) & exePath & Chr(34) & " -heatmap " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.DirectArchive", "Send to Archive", "%windir%\system32\shell32.dll,280", Chr(34) & exePath & Chr(34) & " -archive " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.SetPermissions", "Set Permissions", "%windir%\system32\ieframe.dll,100", Chr(34) & exePath & Chr(34) & " -permissions " & Chr(34) & "%1" & Chr(34), ""))
    _regItems.Add(New RegistryItem(regCommand & "PathStructure.StartWatcher", "StartWatcher", "%windir%\system32\shell32.dll,261", Chr(34) & exePath & Chr(34) & " -watcher"))

    Try
      Dim cnt As Integer = 0
      For i = 0 To _regItems.Count - 1 Step 1
        If Not _regItems(i).Exists() And _regItems(i).ValueContains("", exePath, _regItems(i).ItemKey & "\\command") Then
          cnt += 1
        End If
      Next
      If cnt > 0 Then
        Debug.WriteLine(cnt.ToString & " invalid registry items")
        Return False
      Else
        Return True
      End If
    Catch ex As Exception
      Return False
    End Try
  End Function
  Public Function AddAllRegistryItems() As Boolean
    Dim blnValid As Boolean
    Dim regmenu As RegistryKey

    Try
      ''////////////// Add Folders \\\\\\\\\\\\\\\\\\\
      Dim view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
      '' Add main context menu item 'Path Structure'
      regmenu = Registry.ClassesRoot.CreateSubKey(regFoldMain)
      '' Add main values
      regmenu.SetValue("MUIVerb", "Path Structure")
      regmenu.SetValue("icon", "%windir%\system32\imageres.dll,153")
      regmenu.SetValue("SubCommands", "PathStructure.Open;" & _
                       IIf(My.Settings.blnAddAll Or My.Settings.blnAddSingle, "PathStructure.Add;", "") & _
                       IIf(My.Settings.blnFormat, "PathStructure.Format;", "") & _
                       IIf(My.Settings.blnAudit, "PathStructure.Audit;", "") & _
                       IIf(My.Settings.blnClipboard, "PathStructure.Clipboard;", "") & _
                       IIf(My.Settings.blnTransferByExtension, "PathStructure.TransferByExtension;", "") & _
                       IIf(My.Settings.blnPreview, "PathStructure.Preview;", "") & _
                       IIf(My.Settings.blnFolderHeatMap, "PathStructure.FolderHeatMap;", "") & _
                       IIf(My.Settings.blnDirectArchive, "PathStructure.DirectArchive;", "") & _
                       IIf(My.Settings.blnSetPermissions, "PathStructure.SetPermissions;", ""))

      For i = 0 To _regItems.Count - 1 Step 1
        If Not _regItems(i).AddToRegistry() Then
          Debug.WriteLine(_regItems(i).ItemKey & " could not be added")
        End If
      Next
      blnValid = True
    Catch ex As Exception
      blnValid = False
    End Try

    Return blnValid
  End Function
  Public Function RemoveAllRegistryItems() As Boolean
    Dim blnValid As Boolean

    Try
      For i = 0 To _regItems.Count - 1 Step 1
        If Not _regItems(i).RemoveFromRegistry() Then
          Debug.WriteLine(_regItems(i).ItemKey & " could not be removed")
        End If
      Next
      blnValid = True
    Catch ex As Exception
      blnValid = False
    End Try
    Return blnValid
  End Function

  Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
    Dim opn As New OpenFileDialog
    opn.Title = "Select the Path Structure XML file"
    opn.Filter = "XML|*.xml"
    opn.CheckFileExists = True
    opn.CheckPathExists = True
    opn.ShowDialog()

    If IO.File.Exists(opn.FileName) And Not opn.FileName = My.Settings.SettingsPath Then
      My.Settings.SettingsPath = opn.FileName
      My.Settings.Save()
      MessageBox.Show("The application will now restart to save these changes...", "Application Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Application.Restart()
    End If
  End Sub

  Public Sub btnAddContextMenu_Click(sender As Object, e As EventArgs) Handles btnAddContextMenu.Click
    If AddAllRegistryItems() Then
      Main.statStatus.Text = "Added Context Menu"
    Else
      Main.statStatus.Text = "Failed to Add Context Menu"
    End If
  End Sub
  Public Sub btnRemoveContextMenu_Click(sender As Object, e As EventArgs) Handles btnRemoveContextMenu.Click
    If RemoveAllRegistryItems() Then
      Main.statStatus.Text = "Removed Context Menu"
    Else
      Main.statStatus.Text = "Failed to Remove Context Menu"
    End If
  End Sub

  Public Class RegistryItem
    Private _key, _label, _icon, _cmd, _subcmd As String

    Public ReadOnly Property ItemKey As String
      Get
        Return _key
      End Get
    End Property
    Public ReadOnly Property ItemLabel As String
      Get
        Return _label
      End Get
    End Property
    Public ReadOnly Property ItemIcon As String
      Get
        Return _icon
      End Get
    End Property
    Public ReadOnly Property ItemCommand As String
      Get
        Return _cmd
      End Get
    End Property
    Public ReadOnly Property ItemSubCommands As String
      Get
        Return _subcmd
      End Get
    End Property

    Public Sub New(ByVal Key As String, ByVal Label As String, ByVal IconPath As String, ByVal Command As String, Optional ByVal SubCommands As String = "")
      _key = Key
      _label = Label
      _icon = IconPath
      _cmd = Command
      _subcmd = SubCommands
    End Sub

    Public Function AddToRegistry() As Boolean
      Try
        Dim regmenu As RegistryKey
        Dim regcmd As RegistryKey
        Dim view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
        regmenu = view32.CreateSubKey(_key)
        regmenu.SetValue("MUIVerb", _label)
        If Not String.IsNullOrEmpty(_icon) Then
          regmenu.SetValue("icon", _icon)
        End If
        If Not String.IsNullOrEmpty(_cmd) Then
          regcmd = view32.CreateSubKey(_key & "\\command")
          regcmd.SetValue("", _cmd)
        End If
        If Not String.IsNullOrEmpty(_subcmd) Then
          regmenu.SetValue("SubCommands", _subcmd)
        End If
        view32.Close()
        Return True
      Catch ex As Exception
        Return False
      End Try
    End Function

    Public Function RemoveFromRegistry() As Boolean
      Dim view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
      Dim reg = view32.OpenSubKey(_key, True)
      If Not IsNothing(reg) Then
        If Not reg.SubKeyCount > 0 Then
          reg.Close()
          view32.DeleteSubKey(_key)
        Else
          reg.Close()
          view32.DeleteSubKeyTree(_key)
        End If
        Return True
      Else
        Return False
      End If
    End Function

    Public Function ValueEquals(ByVal PropertyName As String, ByVal Value As String, Optional ByVal OverrideKey As String = "") As Boolean
      Dim strKey As String
      If String.IsNullOrEmpty(OverrideKey) Then
        strKey = _key
      Else
        strKey = OverrideKey
      End If
      Dim blnValid As Boolean
      Dim view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
      Dim reg = view32.OpenSubKey(strKey, True)
      If Not IsNothing(reg) Then
        If String.Equals(reg.GetValue(PropertyName).ToString, Value, StringComparison.OrdinalIgnoreCase) Then
          blnValid = True
        Else
          blnValid = False
        End If
        reg.Close()
        view32.Close()
      Else
        blnValid = False
      End If
      Return blnValid
    End Function
    Public Function ValueContains(ByVal PropertyName As String, ByVal Value As String, Optional ByVal OverrideKey As String = "") As Boolean
      Dim strKey As String
      If String.IsNullOrEmpty(OverrideKey) Then
        strKey = _key
      Else
        strKey = OverrideKey
      End If
      Dim blnValid As Boolean
      Dim view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
      Dim reg = view32.OpenSubKey(strKey, True)
      If Not IsNothing(reg) Then
        If reg.GetValue(PropertyName).ToString.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0 Then
          blnValid = True
        Else
          blnValid = False
        End If
        reg.Close()
        view32.Close()
      Else
        blnValid = False
      End If
      Return blnValid
    End Function

    Public Function Exists() As Boolean
      Dim view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
      Dim reg = view32.OpenSubKey(_key, True)
      If Not IsNothing(reg) Then
        reg.Close()
        view32.Close()
        Return True
      Else
        Debug.WriteLine(_key & " is nothing")
        Return False
      End If
    End Function
  End Class

End Class