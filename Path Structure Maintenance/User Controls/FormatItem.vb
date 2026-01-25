Imports System.Xml
Imports PathStructureClass

Public Class Format_Item
  Private _CurrentPath As Path
  Private _overridePath As Path
  'Private myXML As New XmlDocument
  Private fileName As String
  Private _blnAutoClose As Boolean = True
  Private _struct As XmlElement

  Public Event Accepted(ByVal sender As Object, ByVal e As FormatItemAcceptedEventArgs)

  Public Sub New(ByVal CurrentPath As String, Optional ByVal QuickSelect As Boolean = False, Optional ByVal AutoClose As Boolean = True, Optional ByVal OverridePath As Path = Nothing)
    '' This call is required by the designer.
    InitializeComponent()
    _CurrentPath = New Path(Main.PathStruct, CurrentPath)
    _blnAutoClose = AutoClose

    If OverridePath IsNot Nothing Then
      _overridePath = OverridePath
    Else
      _overridePath = _CurrentPath
    End If

    If _CurrentPath.Type = Path.PathType.File Then
      Log(vbTab & "Directory: " & _overridePath.ParentPath)

      For Each var As Variable In _overridePath.Variables.Items
        Log(vbTab & var.Name & ": " & var.Value)
      Next
      If QuickSelect Then
        Log(vbTab & "IsNameStructured: " & _overridePath.IsNameStructured().ToString)
      End If

      pnlVariables.Controls.Clear()
      cmbFiles.Items.Clear()
      Dim slist As New SortedList(Of String, String)
      If _overridePath.StructureCandidates.Count > 0 Then
        Dim searchNode As XmlElement
        For Each struct As StructureCandidate In _overridePath.StructureCandidates.Items
          searchNode = Nothing
          If struct.XElement.Name = "File" Then
            If struct.XElement.ParentNode IsNot Nothing Then
              searchNode = struct.XElement.ParentNode
            End If
          ElseIf struct.XElement.Name = "Option" Then
            If struct.XElement.ParentNode.ParentNode IsNot Nothing Then
              searchNode = struct.XElement.ParentNode.ParentNode
            End If
          ElseIf struct.XElement.Name = "Folder" Then
            If struct.XElement.HasChildNodes Then
              searchNode = struct.XElement
            End If
          End If
          If Not IsNothing(searchNode) Then
            For Each nod As XmlElement In searchNode.SelectNodes("File")
              If nod.HasAttribute("name") Then
                If Not slist.ContainsKey(nod.Attributes("name").Value) Then
                  slist.Add(nod.Attributes("name").Value, nod.Attributes("name").Value)
                End If
              End If
            Next
          End If
        Next
      Else
        For Each fil As XmlElement In Main.PathStruct.Settings.SelectNodes("//File")
          If Not slist.ContainsKey(fil.Attributes("name").Value) Then
            slist.Add(fil.Attributes("name").Value, fil.Attributes("name").Value)
          End If
        Next
      End If
      If slist.Count > 0 Then
        cmbFiles.Items.AddRange(slist.Values.ToArray())
      End If
    Else
      MessageBox.Show("You must select a file to complete the 'Format' function!", "Invalid FileSystemObject type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      If _blnAutoClose Then Application.Exit()
    End If
  End Sub

  Private Sub cmbFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFiles.SelectedIndexChanged
    Dim FileType As String = cmbFiles.Items(cmbFiles.SelectedIndex).ToString
    cmbOptions.Items.Clear()
    pnlVariables.Controls.Clear()
    Dim slist As New List(Of String)
    Dim xPath As String = ".//File[@name='" & FileType & "']/Option"
    If SetStruct(FileType) Then
      If String.Equals(_struct.Name, "File", StringComparison.OrdinalIgnoreCase) Then xPath = "Option"
      For Each opt As XmlElement In _struct.SelectNodes(xPath)
        Debug.WriteLine(_overridePath.PStructure.GetURIfromXPath(opt.FindXPath()))
        slist.Add(opt.Attributes("name").Value) ', opt.Attributes("name").Value)
        'cmbOptions.Items.Add(opt.Attributes("name").Value)
      Next
    End If
    cmbOptions.Items.AddRange(slist.ToArray())
    If cmbOptions.Items.Count > 0 Then
      pnlOptions.Enabled = True
    Else
      pnlOptions.Enabled = False
      LoadFileSyntax(FileType)
    End If

  End Sub

  Private Sub cmbOptions_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbOptions.SelectedIndexChanged
    pnlVariables.Controls.Clear()
    LoadFileSyntax(cmbFiles.Items(cmbFiles.SelectedIndex).ToString, cmbOptions.Items(cmbOptions.SelectedIndex).ToString)
  End Sub

  Private Function SetStruct(Optional ByVal nameFile As String = "", Optional ByVal nameOption As String = "") As Boolean
    If _overridePath.Type = Path.PathType.File And _overridePath.IsNameStructured() Then
      _struct = _overridePath.StructureCandidates.GetHighestMatch().XElement
      Return True
    ElseIf Not String.IsNullOrEmpty(nameFile) Then
      If String.IsNullOrEmpty(nameOption) Then
        _struct = _overridePath.PathStructure.SelectSingleNode(".//File[@name='" & nameFile & "']")
        Return True
      Else
        _struct = _overridePath.PathStructure.SelectSingleNode(".//File[@name='" & nameFile & "']/Option[@name='" & nameOption & "']")
        Return True
      End If
    End If

    Return False
  End Function
  Private Sub LoadFileSyntax(ByVal nameFile As String, Optional ByVal nameOption As String = "")
    If SetStruct(nameFile, nameOption) Then
      If Not String.IsNullOrEmpty(nameOption) Then
        fileName = _struct.InnerText
        If fileName.Contains("{name}") Then fileName = fileName.Replace("{name}", nameOption)
      Else
        fileName = _struct.InnerText
        If fileName.Contains("{name}") Then fileName = fileName.Replace("{name}", nameFile)
      End If
    End If

    fileName = _overridePath.Variables.Replace(fileName) ' _CurrentPath.ReplaceVariables(fileName)
    If fileName.Contains("{Date}") Then fileName = fileName.Replace("{Date}", DateTime.Now.ToString("MM-dd-yyyy"))
    If fileName.Contains("{Time}") Then fileName = fileName.Replace("{Time}", DateTime.Now.ToString("hh-mm-ss tt"))

    fileName += _CurrentPath.Extension ' _CurrentPath.FileInfo.Extension

    Dim input As String() = GetListOfInternalStrings(fileName, "{", "}")
    If input.Length > 0 Then
      For Each str As String In input
        If String.IsNullOrEmpty(str) Then Continue For

        Dim pnl As New Panel
        Dim lbl As New Label
        Dim txt As New TextBox

        pnl.Dock = DockStyle.Top
        pnl.Height = 50

        lbl.Dock = DockStyle.Left
        lbl.AutoSize = False
        lbl.TextAlign = ContentAlignment.MiddleRight
        lbl.Text = str
        lbl.Size = New Size(pnlVariables.Width * 0.3, 30)

        txt.Dock = DockStyle.Right
        txt.Size = New Size(pnlVariables.Width * 0.6, 30)
        txt.Tag = str

        If _overridePath.Variables.ContainsName("{" & str & "}") Then
          txt.Text = _overridePath.Variables("{" & str & "}").Value
        End If
        AddHandler txt.TextChanged, AddressOf Variable_Changed

        pnl.Controls.Add(lbl)
        pnl.Controls.Add(txt)

        pnlVariables.Controls.Add(pnl)
        pnlVariables.Controls.SetChildIndex(pnl, 0)
      Next
    End If

    Variable_Changed(Nothing, Nothing)
  End Sub

  Private Sub Variable_Changed(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Dim vals As New SortedList(Of String, String)
    txtPreview.Text = _overridePath.Variables.Replace(fileName) ' _CurrentPath.ReplaceVariables(fileName)
    For Each pnl As Control In pnlVariables.Controls
      vals.Add(pnl.Controls(1).Tag, pnl.Controls(1).Text)
      If Not String.IsNullOrEmpty(pnl.Controls(1).Text) Then
        txtPreview.Text = txtPreview.Text.Replace("{" & pnl.Controls(1).Tag & "}", pnl.Controls(1).Text)
      End If
    Next
    txtPreview.Text = txtPreview.Text.Replace("{}", _CurrentPath.PathName.Replace(_CurrentPath.Extension, String.Empty))
  End Sub

  Private Sub btnAccept_Click(sender As Object, e As EventArgs) Handles btnAccept.Click
    If pnlOptions.Enabled And cmbOptions.SelectedIndex < 0 Then
      MessageBox.Show("You must select a file type option!", "Invalid Option", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Exit Sub
    End If
    Dim strDir As String = _overridePath.ParentPath
    If _struct IsNot Nothing Then
      If Not _struct.Name = "Folder" Then
        Do Until String.Equals(_struct.Name, "Folder", StringComparison.OrdinalIgnoreCase) Or String.Equals(_struct.ParentNode.Name, "Structure")
          _struct = _struct.ParentNode
        Loop
      End If
      If String.Equals(_struct.Name, "Folder", StringComparison.OrdinalIgnoreCase) Then
        If _overridePath.IsNameStructured() Then
          Debug.WriteLine("Is name structured")
          If _overridePath.Type = Path.PathType.File Then
            'Debug.WriteLine("Replacing variables of parent directory")
            strDir = _overridePath.Parent.Variables.Replace(_overridePath.PStructure.GetURIfromXPath(FindXPath(_overridePath.Parent.StructureCandidates.GetHighestMatch().XElement)))
          ElseIf _overridePath.Type = Path.PathType.Folder Then
            strDir = _overridePath.Variables.Replace(_overridePath.PStructure.GetURIfromXPath(FindXPath(_overridePath.StructureCandidates.GetHighestMatch().XElement)))
          End If
        ElseIf _CurrentPath.IsNameStructured() Then
          strDir = _overridePath.Variables.Replace(_overridePath.PStructure.GetURIfromXPath(FindXPath(_CurrentPath.StructureCandidates.GetHighestMatch().XElement)))
        Else
          strDir = _overridePath.Variables.Replace(_overridePath.PStructure.GetURIfromXPath(FindXPath(_struct)))
        End If
        If Not IO.Directory.Exists(strDir) Then
          IO.Directory.CreateDirectory(strDir)
        End If
      End If
    End If
    Debug.WriteLine("Settings strDir: " & strDir)
    Dim dts As String = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")
    Try
      If IO.File.Exists(strDir & txtPreview.Text) And Not String.Equals((strDir & txtPreview.Text), _CurrentPath.UNCPath, StringComparison.OrdinalIgnoreCase) Then
        Dim archive As String = _overridePath.FindNearestArchive()
        If Not String.IsNullOrEmpty(archive) Then
          IO.File.Move(strDir & txtPreview.Text, archive & "\" & dts & "_" & txtPreview.Text)
          If strDir.IndexOf("Archive", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            IO.File.Move(_CurrentPath.UNCPath, strDir & dts & "_" & txtPreview.Text) '' Adding DateTime stamp
            _CurrentPath.LogData(strDir & dts & "_" & txtPreview.Text, "Format Filename")
          Else
            IO.File.Move(_CurrentPath.UNCPath, strDir & dts & "_" & txtPreview.Text) '' Not transfering to archive
            _CurrentPath.LogData(strDir & txtPreview.Text, "Format Filename")
          End If
        Else
          MessageBox.Show("The new name seems to exist already and an archive folder could not be found to place the old file.", "Aborting", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
      Else
        If strDir.IndexOf("Archive", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
          IO.File.Move(_CurrentPath.UNCPath, strDir & dts & "_" & txtPreview.Text) '' Adding DateTime stamp
          _CurrentPath.LogData(strDir & dts & "_" & txtPreview.Text, "Format Filename")
        Else
          IO.File.Move(_CurrentPath.UNCPath, strDir & txtPreview.Text) '' Not transfering to archive
          _CurrentPath.LogData(strDir & txtPreview.Text, "Format Filename")
        End If
      End If
    Catch ex As Exception
      Log("{FormatItem} Move/Archive Fail: " & ex.Message)
    End Try

    If _blnAutoClose Then Application.Exit()
    RaiseEvent Accepted(Me, New FormatItemAcceptedEventArgs(_CurrentPath.UNCPath))
  End Sub
End Class
Public Class FormatItemAcceptedEventArgs
  Inherits EventArgs

  Public Property Path As String

  Public Sub New(ByVal path As String)
    MyBase.New()

    Me.Path = path
  End Sub
End Class
