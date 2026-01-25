Imports System.Xml
Imports PathStructureClass

Public Class Transfer_FilesByExtension
  Private _CurrentPath As Path
  'Private myXML As New XmlDocument
  Private fileName As String

  Public Sub New(ByVal CurrentPath As String)
    InitializeComponent()
    _CurrentPath = New Path(Main.PathStruct, CurrentPath)

    'myXML.Load(My.Settings.SettingsPath)

    cmbFolder.Items.Clear()
    For Each fol As XmlElement In _CurrentPath.PathStructure.SelectNodes(".//Folder")
      cmbFolder.Items.Add(fol.Attributes("name").Value)
    Next

    txtFileExtension.AutoCompleteCustomSource.Clear()
    Main.statCurrentPath.Text = _CurrentPath.ParentPath
    If _CurrentPath.Type = Path.PathType.File Then
      txtFileExtension.AutoCompleteCustomSource.Add(_CurrentPath.Extension) ' _CurrentPath.FileInfo.Extension)
      txtFileExtension.Text = _CurrentPath.Extension ' _CurrentPath.FileInfo.Extension
      _CurrentPath = New PathStructureClass.Path(Main.PathStruct, _CurrentPath.Parent.UNCPath)
      txtFileExtension_TextChanged(txtFileExtension, Nothing)
    End If
    If _CurrentPath.Type = Path.PathType.Folder Then
      For Each fil As Path In _CurrentPath.Children
        If fil.Type = Path.PathType.File Then
          If Not txtFileExtension.AutoCompleteCustomSource.Contains(fil.Extension) Then ' fil.FileInfo.Extension) Then
            txtFileExtension.AutoCompleteCustomSource.Add(fil.Extension) ' fil.FileInfo.Extension)
          End If
        End If
      Next
    End If
  End Sub

  Private Sub btnTransfer_Click(sender As Object, e As EventArgs) Handles btnTransfer.Click
    Try
      If cmbFolder.SelectedIndex > 0 And Not String.IsNullOrEmpty(txtFileExtension.Text) Then
        Dim fold As XmlNodeList = _CurrentPath.PathStructure.SelectNodes(".//Folder[@name='" & cmbFolder.Items(cmbFolder.SelectedIndex).ToString & "']")
        If fold.Count = 1 Then
          Dim cntFiles As Integer = 0
          Dim xpath As String = ""
          Dim fpath As String = ""
          For Each fol As XmlNode In fold
            xpath = FindXPath(fol)
            fpath = _CurrentPath.Variables.Replace(Main.PathStruct.GetURIfromXPath(xpath))
            
            If Not IO.Directory.Exists(fpath) Then
              IO.Directory.CreateDirectory(fpath)
            End If
            If _CurrentPath.Children IsNot Nothing Then
              For Each child As Path In _CurrentPath.Children
                If child.Type = Path.PathType.File Then
                  If child.Extension.IndexOf(txtFileExtension.Text, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
                    If fpath.IndexOf("Archive", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
                      Dim f As New IO.FileInfo(child.UNCPath)
                      f.MoveTo(fpath & "\" & Now.ToString("yyyy-MM-dd hh-mm-ss tt") & "_" & child.PathName)
                      child.LogData(fpath & "\" & Now.ToString("yyyy-MM-dd hh-mm-ss tt") & "_" & child.PathName, "TransferByExtension")
                    Else
                      Dim f As New IO.FileInfo(child.UNCPath)
                      f.MoveTo(fpath & "\" & child.PathName)
                      child.LogData(fpath & "\" & child.PathName, "TransferByExtension")
                    End If
                    cntFiles += 1
                  End If
                End If
              Next
            End If
          Next
          MessageBox.Show("Moved '" & cntFiles.ToString & "' files to '" & fpath & "'", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
          Application.Exit()
        ElseIf fold.Count = 0 Then
          MessageBox.Show("You must select a valid folder name", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
          Exit Sub
        Else
          Log("XML Structure cannot have duplicate object names! Found '" & fold.Count.ToString & "' with the name '" & cmbFolder.Items(cmbFolder.SelectedIndex).ToString & "'.")
          MessageBox.Show("An error occurred in the XML Structure!" & vbLf & "Duplicate objects found. See log for more details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
          Exit Sub
        End If
      Else
        MessageBox.Show("You must provide a valid folder name and valid file extension!", "Invalid Folder/Extension", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
    Catch ex As Exception
      MessageBox.Show("An error occurred while attempting to transfer documents: " & vbCrLf & ex.Message)
    End Try
  End Sub

  Private Sub txtFileExtension_TextChanged(sender As Object, e As EventArgs) Handles txtFileExtension.TextChanged
    Dim cnt As Integer = 0
    If Not String.IsNullOrEmpty(txtFileExtension.Text) Then
      If _CurrentPath.Type = Path.PathType.Folder Then
        If _CurrentPath.Children IsNot Nothing Then
          For Each fil As Path In _CurrentPath.Children
            If fil.Type = Path.PathType.File And fil.Extension.IndexOf(txtFileExtension.Text, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
              cnt += 1
            End If
          Next
        Else
          Main.statStatus.Text = "No Children in this path..."
        End If
      End If
    End If
    Main.statStatus.Text = cnt.ToString & " files will be transfered"
  End Sub
End Class
