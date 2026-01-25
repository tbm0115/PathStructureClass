Imports System.Xml
Imports PathStructureClass

Public Class Add_Folder
  Private _CurrentPath As Path
  'Private myXML As New XmlDocument
  Private fileName As String

  Public Sub New(ByVal CurrentPath As String, Optional ByVal QuickSelect As Boolean = False)
    '' This call is required by the designer.
    InitializeComponent()
    _CurrentPath = New Path(Main.PathStruct, CurrentPath)

    If _CurrentPath.Type = Path.PathType.Folder Then
      '' Add any initialization after the InitializeComponent() call.
      'Main.PathStruct.Settings.Load(My.Settings.SettingsPath)
      'Log(vbTab & "Directory: " & _CurrentPath.FolderInfo.Name)
      'Log(vbTab & "Extension: " & _CurrentPath.FolderInfo.Extension)
      Log(vbTab & "Directory: " & _CurrentPath.ParentPath)
      'For Each var As KeyValuePair(Of String, String) In _CurrentPath.Variables
      '  Log(vbTab & var.Key & ": " & var.Value)
      'Next
      For Each var As Variable In _CurrentPath.Variables.Items
        Log(vbTab & var.Name & ": " & var.Value)
      Next
      If QuickSelect Then
        Log(vbTab & "IsNameStructured: " & _CurrentPath.IsNameStructured().ToString)
      End If

      pnlVariables.Controls.Clear()
      cmbFiles.Items.Clear()
      If _CurrentPath.StructureCandidates.Count > 0 Then
        For Each struct As StructureCandidate In _CurrentPath.StructureCandidates.Items
          If struct.XElement.Name = "File" Then
            If struct.XElement.ParentNode IsNot Nothing Then
              For Each nod As XmlElement In struct.XElement.ParentNode.SelectNodes("Folder")
                If nod.HasAttribute("name") Then
                  If Not cmbFiles.Items.Contains(nod.Attributes("name").Value) Then
                    cmbFiles.Items.Add(nod.Attributes("name").Value)
                  End If
                End If
              Next
            End If
          ElseIf struct.XElement.Name = "Option" Then
            If struct.XElement.ParentNode.ParentNode IsNot Nothing Then
              For Each nod As XmlElement In struct.XElement.ParentNode.SelectNodes("Folder")
                If nod.HasAttribute("name") Then
                  If Not cmbFiles.Items.Contains(nod.Attributes("name").Value) Then
                    cmbFiles.Items.Add(nod.Attributes("name").Value)
                  End If
                End If
              Next
            End If
          ElseIf struct.XElement.Name = "Folder" Then
            If struct.XElement.HasChildNodes Then
              For Each nod As XmlElement In struct.XElement.SelectNodes("Folder")
                If nod.HasAttribute("name") Then
                  If Not cmbFiles.Items.Contains(nod.Attributes("name").Value) Then
                    cmbFiles.Items.Add(nod.Attributes("name").Value)
                  End If
                End If
              Next
            End If
          End If
        Next
      Else
        For Each fil As XmlElement In Main.PathStruct.Settings.SelectNodes("//Folder")
          cmbFiles.Items.Add(fil.Attributes("name").Value)
        Next
      End If
    Else
      MessageBox.Show("You must select a file to complete the 'Format' function!", "Invalid FileSystemObject type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Application.Exit()
    End If

  End Sub
  Private Sub cmbFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFiles.SelectedIndexChanged
    Dim FileType As String = cmbFiles.Items(cmbFiles.SelectedIndex).ToString
    pnlVariables.Controls.Clear()
    LoadFileSyntax(FileType)
  End Sub

  Private Sub LoadFileSyntax(ByVal nameFile As String, Optional ByVal nameOption As String = "")
    fileName = nameFile
    Dim parent As XmlElement
    If Not IsNothing(Main.PathStruct.Settings.SelectSingleNode("//Folder[@name='" & nameFile & "']")) Then
      parent = Main.PathStruct.Settings.SelectSingleNode("//Folder[@name='" & nameFile & "']")
      Do Until IsNothing(parent.ParentNode.Attributes("name"))
        fileName = parent.ParentNode.Attributes("name").Value & "\" & fileName
        parent = parent.ParentNode
      Loop
    End If
    fileName = _CurrentPath.UNCPath 'StartPath & fileName
    fileName = _CurrentPath.Variables.Replace(fileName) ' _CurrentPath.ReplaceVariables(fileName)
    If fileName.Contains("{Date}") Then fileName = fileName.Replace("{Date}", DateTime.Now.ToString("MM-dd-yyyy"))
    If fileName.Contains("{Time}") Then fileName = fileName.Replace("{Time}", DateTime.Now.ToString("hh-mm-ss tt"))

    'fileName += filInfo.Extension

    Dim input As String() = GetListOfInternalStrings(fileName, "{", "}")
    If input.Length > 0 Then
      For Each str As String In input
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
        AddHandler txt.TextChanged, AddressOf Variable_Changed

        pnl.Controls.Add(lbl)
        pnl.Controls.Add(txt)

        pnlVariables.Controls.Add(pnl)
        pnlVariables.Controls.SetChildIndex(pnl, 0)
      Next
    End If

    txtPreview.Text = _CurrentPath.Variables.Replace(fileName) ' _CurrentPath.ReplaceVariables(fileName)
  End Sub

  Private Sub Variable_Changed(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Dim vals As New SortedList(Of String, String)
    txtPreview.Text = _CurrentPath.Variables.Replace(fileName) ' _CurrentPath.ReplaceVariables(fileName)
    For Each pnl As Control In pnlVariables.Controls
      vals.Add(pnl.Controls(1).Tag, pnl.Controls(1).Text)
      If Not String.IsNullOrEmpty(pnl.Controls(1).Text) Then
        txtPreview.Text = txtPreview.Text.Replace("{" & pnl.Controls(1).Tag & "}", pnl.Controls(1).Text)
      End If
    Next
  End Sub

  Private Sub btnAccept_Click(sender As Object, e As EventArgs) Handles btnAccept.Click
    If cmbFiles.SelectedIndex < 0 Then
      MessageBox.Show("You must select a file type option!", "Invalid Option", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Exit Sub
    End If
    IO.Directory.CreateDirectory(_CurrentPath.Variables.Replace(txtPreview.Text)) ' _CurrentPath.ReplaceVariables(txtPreview.Text))
    _CurrentPath.LogData(txtPreview.Text, "Create Folder")
    Application.Exit()
  End Sub

End Class
