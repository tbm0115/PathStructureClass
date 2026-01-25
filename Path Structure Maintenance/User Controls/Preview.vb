Imports System.Xml
Imports AxAcroPDFLib.AxAcroPDF
Imports PathStructureClass

Public Class Preview
  Private _CurrentPath As Path
  Public Folders As New SortedList(Of String, String)
  Public Files As New SortedList(Of String, String)
  Public FileFilter As String = ""

  Public Sub New(ByVal SelectedPath As String)
    InitializeComponent()

    _CurrentPath = New Path(Main.PathStruct, SelectedPath)
    Folders.Clear()
    lstFolders.Items.Clear()
    lstFiles.Items.Clear()

    '' Check if valid preview item
    If _CurrentPath.Type = Path.PathType.File Then
      MessageBox.Show("The Preview function only applies to folders with a valid preview assignment.", "Invalid Type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Application.Exit()
    End If

    _CurrentPath.IsNameStructured()

    If _CurrentPath.StructureCandidates.Count = 1 Then
      '' Declare the reusable directory search string
      Dim dirSearch As String = ""
      '' Initialize Path Structure document
      Dim myXML As New XmlDocument
      myXML.Load(My.Settings.SettingsPath)

      '' Iterate through each relavant XPath
      For Each xp As String In _CurrentPath.StructureCandidates.ToArray
        '' Iterate through each relavant xmlelement
        For Each nod As XmlElement In myXML.SelectNodes(xp)
          '' Verify the xmlelement has the 'preview' attribute
          If nod.HasAttribute("preview") Then
            '' Iterate through each 'preview' xpath result
            For Each prev As XmlElement In nod.SelectNodes(nod.Attributes("preview").Value.ToString)
              '' Verify the xmlelement has the 'previewDocument' attribute
              If prev.HasAttribute("previewDocument") Then
                dirSearch = _CurrentPath.Variables.Replace(Main.PathStruct.GetURIfromXPath(FindXPath(prev))) ' _CurrentPath.ReplaceVariables(_CurrentPath.GetURIfromXPath(FindXPath(prev)))
                '' Remove last index of global variables
                If dirSearch.Contains("{") And dirSearch.Contains("}") Then
                  dirSearch = dirSearch.Remove(dirSearch.LastIndexOf("{"))
                End If

                '' If string still contains global variables, then the Path Structure is not formatted correctly
                If dirSearch.Contains("{") And dirSearch.Contains("}") Then
                  MessageBox.Show("The selected path did not provide enough information to correctly identify which folder to search.", "Invalid Path Structure Format", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                  Application.Exit()
                End If

                If IO.Directory.Exists(dirSearch) Then
                  For Each dirs As IO.DirectoryInfo In New IO.DirectoryInfo(dirSearch).GetDirectories
                    Folders.Add(dirs.Name, dirs.FullName)
                    lstFolders.Items.Add(dirs.Name)
                  Next
                  FileFilter = prev.Attributes("previewDocument").Value.ToString
                  Debug.WriteLine("Found '" & Folders.Count.ToString & "' folders in '" & dirSearch & "'")
                Else
                  MessageBox.Show("'" & dirSearch & "' does not appear to be a valid path.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                  Application.Exit()
                End If
              Else
                Debug.WriteLine("Node '" & prev.Name & ":" & prev.Attributes("name").Value & "' did not have a previewDocument attribute.")
              End If
            Next
          End If
        Next
      Next
    ElseIf _CurrentPath.StructureCandidates.Count = 0 Then
      MessageBox.Show("Couldn't not determine what type of folder '" & SelectedPath & "' based on the Path Structure.", "Invalid Path Structure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Application.Exit()
    ElseIf _CurrentPath.StructureCandidates.Count > 1 Then
      MessageBox.Show("Too many potential folder types for '" & SelectedPath & "' based on the Path Structure.", "Indeterminable Path Structure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      Application.Exit()
    End If
    Application.DoEvents()
    If lstFolders.Items.Count > 0 Then
      lstFolders.SelectedIndex = 0
      lstFolders.Focus() 'Me.TableLayoutPanel1.Controls("lstFolders").Focus()
      lstFolders_SelectedIndexChanged(lstFolders, Nothing)
    End If
  End Sub

  Private Sub lstFolders_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstFolders.SelectedIndexChanged
    pnlPreview.Controls.Clear()
    lstFiles.Items.Clear()
    If Not IsNothing(Folders(lstFolders.SelectedItem.ToString)) Then
      Dim fold As String = Folders(lstFolders.SelectedItem.ToString)
      If IO.Directory.Exists(fold) Then
        Files.Clear()
        For Each fil As IO.FileInfo In New IO.DirectoryInfo(fold).GetFiles
          Dim ps As New Path(Main.PathStruct, fold)
          Debug.WriteLine("Testing '" & (fil.Name.ToString) & "' against '" & ps.Variables.Replace(FileFilter) & "'") ' ps.ReplaceVariables(FileFilter) & "'")
          If (fil.Name.ToString).ToLower Like ps.Variables.Replace(FileFilter).ToLower Then ' ps.ReplaceVariables(FileFilter).ToLower Then
            Files.Add(fil.Name, fil.FullName)
            lstFiles.Items.Add(fil.Name)
          End If
        Next
      End If
    End If
    Application.DoEvents()
    If lstFiles.Items.Count > 0 Then
      lstFiles.SelectedIndex = 0
      lstFiles.Focus() 'Me.TableLayoutPanel1.Controls("lstFiles").Focus()
    End If

  End Sub

  Private Sub lstFiles_KeyDown(sender As Object, e As KeyEventArgs) Handles lstFiles.KeyDown
    If lstFiles.SelectedIndex = lstFiles.Items.Count - 1 And e.KeyCode = Keys.Down Then 'Down
      If Not lstFolders.SelectedIndex = lstFolders.Items.Count - 1 Then
        lstFolders.SelectedIndex = lstFolders.SelectedIndex + 1
      Else
        lstFolders.SelectedIndex = 0
      End If
      e.Handled = True
      Application.DoEvents()
      lstFolders.Focus() 'Me.TableLayoutPanel1.Controls("lstFolders").Focus()
      lstFolders_SelectedIndexChanged(lstFolders, Nothing)
    ElseIf lstFiles.SelectedIndex = 0 And e.KeyCode = Keys.Up Then 'Up
      If Not lstFolders.SelectedIndex = 0 Then
        lstFolders.SelectedIndex = lstFolders.SelectedIndex - 1
      Else
        lstFolders.SelectedIndex = lstFolders.Items.Count - 1
      End If
      e.Handled = True
      Application.DoEvents()
      lstFolders.Focus() 'Me.TableLayoutPanel1.Controls("lstFolders").Focus()
      lstFolders_SelectedIndexChanged(lstFolders, Nothing)
    ElseIf e.KeyCode = Keys.Enter Then
      btnOpenFile_Click(btnOpenFile, Nothing)
    End If
  End Sub


  Private Sub lstFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstFiles.SelectedIndexChanged
    '' Kill Adobe every 10th opened file
    'Static cntFileOpened As Integer = 0
    'cntFileOpened += 1
    'If (cntFileOpened Mod 10) = 0 Then
    '  KillAdobe()
    'End If
    For i = 0 To pnlPreview.Controls.Count - 1 Step 1
      pnlPreview.Controls(i).Dispose()
    Next
    pnlPreview.Controls.Clear()

    '' Begin loading controls
    If Not IsNothing(Files(lstFiles.SelectedItem.ToString)) Then
      If IO.File.Exists(Files(lstFiles.SelectedItem.ToString)) Then
        Dim fil As New IO.FileInfo(Files(lstFiles.SelectedItem.ToString))
        If Not IsNothing(fil.Extension) Then
          Select Case fil.Extension.ToLower
            Case ".pdf"
              Dim pdf As New WebBrowser
              pdf.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(pdf)
              pdf.Navigate("file:///" & fil.FullName)
            Case ".txt"
              Dim txt As New RichTextBox
              txt.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(txt)
              txt.LoadFile(fil.FullName)
            Case ".gcode"
              Dim txt As New RichTextBox
              txt.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(txt)
              txt.LoadFile(fil.FullName)
            Case ".eia"
              Dim txt As New RichTextBox
              txt.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(txt)
              txt.LoadFile(fil.FullName)
            Case ".rtf"
              Dim txt As New RichTextBox
              txt.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(txt)
              txt.LoadFile(fil.FullName)
            Case ".html"
              Dim html As New WebBrowser
              html.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(html)
              html.Navigate(fil.FullName)
            Case ".htm"
              Dim html As New WebBrowser
              html.Dock = DockStyle.Fill
              pnlPreview.Controls.Add(html)
              html.Navigate(fil.FullName)
            Case Else
              Dim lbl As New Label
              lbl.Text = "'" & fil.Extension & "' is not recognized"
              lbl.Dock = DockStyle.Fill
              lbl.TextAlign = ContentAlignment.MiddleCenter
              pnlPreview.Controls.Add(lbl)
          End Select
        End If
      End If
    End If

    lstFiles.Focus() 'Me.TableLayoutPanel1.Controls("lstFiles").Focus()
    Application.DoEvents()
  End Sub

  Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
    If lstFolders.SelectedIndex > -1 Then
      Try
        Process.Start("explorer.exe", Chr(34) & Folders(lstFolders.SelectedItem.ToString) & Chr(34))
      Catch ex As Exception
        MessageBox.Show("Failed to open '" & Folders(lstFolders.SelectedItem.ToString) & "' due to error:" & vbLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
      End Try
    End If
  End Sub

  Private Sub btnOpenFile_Click(sender As Object, e As EventArgs) Handles btnOpenFile.Click
    If lstFiles.SelectedIndex > -1 Then
      Try
        Process.Start(Chr(34) & Files(lstFiles.SelectedItem.ToString) & Chr(34))
      Catch ex As Exception
        MessageBox.Show("Failed to open '" & Files(lstFiles.SelectedItem.ToString) & "' due to error:" & vbLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
      End Try
    End If
  End Sub

  Private Sub KillAdobe()
    Debug.WriteLine("Killing Adobe")
    '' Clean Adobe Reader process(es)
    Dim procAdobe As New SortedList(Of Integer, Process)
    For Each proc As Process In Process.GetProcesses
      If proc.ProcessName.Contains("AcroRd") Then
        proc.Kill()
      End If
    Next
  End Sub

End Class
