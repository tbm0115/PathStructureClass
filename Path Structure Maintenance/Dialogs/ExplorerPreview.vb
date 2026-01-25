Imports PathStructureClass
Imports System.Xml

Public Class ExplorerPreview
  Private exp As ExplorerWatcher
  Private pstruct As PathStructure

  Public Sub New(ByVal Watcher As ExplorerWatcher, ByVal PathStruct As PathStructure)
    InitializeComponent()

    pstruct = PathStruct

    exp = Watcher
    AddHandler exp.ExplorerWatcherFound, AddressOf FoundPath
    exp.StartWatcher()
  End Sub

  Declare Function SetActiveWindow Lib "user32.dll" (ByVal hwnd As Integer) As Integer

  Delegate Sub ExplorerSearchCallback(ByVal URL As String)
  Private Sub FoundPath(ByVal URL As String)
    Try
      If Me IsNot Nothing Then
        If Me.InvokeRequired Then
          Dim d As New ExplorerSearchCallback(AddressOf FoundPath)
          Me.Invoke(d, New Object() {URL})
          Exit Sub
        End If
      End If
    Catch ex As Exception
      Log("{ExplorerPreview}(FoundPath)  Failed: " & ex.Message)
    End Try

    Try
      For i = 0 To Me.Controls.Count - 1 Step 1
        Me.Controls(i).Dispose()
      Next
    Catch ex As Exception
      Log("{ExplorerPreview}(FoundPath) Panel Clear Failed: " & ex.Message)
    End Try
    Me.Controls.Clear()

    RecursivePreview(New Path(pstruct, URL))

    Application.DoEvents()
    Try
      SetActiveWindow(exp.CurrentFoundPaths(0).WindowHandle)
    Catch ex As Exception
      Log("{ExplorerPreview}(FoundPath) Set focus failed: " & ex.Message)
    End Try
  End Sub
  Private Function RecursivePreview(ByVal focus As Path) As Boolean
    Debug.WriteLine(focus.UNCPath)
    Dim added As Boolean = True
    If focus.IsNameStructured And focus.Type = Path.PathType.Folder Then
      Dim lstCandidateXPaths As New List(Of String)
      For Each cand As StructureCandidate In focus.StructureCandidates.Items
        '' Verify the xmlelement has the 'preview' attribute
        If cand.XElement.HasAttribute("preview") Then
          '' Iterate through each 'preview' xpath result
          For Each nod As XmlNode In cand.XElement.SelectNodes(cand.XElement.Attributes("preview").Value)
            lstCandidateXPaths.Add(nod.FindXPath())
          Next
        End If
      Next

      If lstCandidateXPaths.Count > 0 Then
        For i = 0 To focus.Children.Length - 1 Step 1
          Dim pt As Path = focus.Children(i)
          '' Begin loading controls
          If pt.IsNameStructured Then
            If pt.Type = Path.PathType.File And lstCandidateXPaths.Contains(pt.StructureCandidates.GetHighestMatch().XPath) Then
              If Not IsNothing(pt.Extension) Then
                CheckAddByExtension(pt)
                If Me.Controls.Count > 0 Then
                  added = True
                End If
              End If
            ElseIf pt.Type = Path.PathType.Folder And lstCandidateXPaths.Contains(pt.StructureCandidates.GetHighestMatch().XPath) Then
              If RecursivePreview(pt) Then Exit For
              Debug.WriteLine(vbTab & vbTab & pt.UNCPath & " is either not a file or " & pt.StructureCandidates.GetHighestMatch().XPath & " doesn't meet any candidates")
            End If
          Else
            Debug.WriteLine(vbTab & vbTab & pt.UNCPath & " is not name structured")
          End If
        Next
      Else
        Debug.WriteLine(vbTab & "No candidates with preview")
      End If
    ElseIf focus.Type = Path.PathType.File Then
      CheckAddByExtension(focus)
    Else
      Debug.WriteLine(vbTab & "Not name structured")
    End If

    If Me.Controls.Count = 0 Then
      Dim lbl As New Label
      lbl.Text = "No Preview Document found in '" & focus.UNCPath & "'"
      lbl.Dock = DockStyle.Fill
      lbl.Font = New System.Drawing.Font("Arial", 24)
      Me.Controls.Add(lbl)
    Else
      Return True
    End If
    Return False
  End Function
  
  Private Sub CheckAddByExtension(ByVal Pt As Path)
    Select Case Pt.Extension.ToLower
      Case ".pdf"
        Try
          Dim axAcro As New AxAcroPDFLib.AxAcroPDF
          Me.Controls.Add(axAcro)
          axAcro.Dock = DockStyle.Fill
          axAcro.LoadFile(Pt.UNCPath)
          axAcro.src = Pt.UNCPath
          axAcro.setShowToolbar(False)
          axAcro.setView("Fit")
          axAcro.setLayoutMode("SinglePage")
          axAcro.Show()
        Catch ex As Exception
          Log("{ExplorerPreview}(AddControls) Failed to load Adobe PDF reader: " & ex.Message)
        Finally
          Dim pdf As New WebBrowser
          pdf.Dock = DockStyle.Fill
          Me.Controls.Add(pdf)
          pdf.Navigate("file:///" & Pt.UNCPath & "#page=1&view=Fit")
        End Try
      Case ".txt"
        Dim txt As New RichTextBox
        txt.Dock = DockStyle.Fill
        Me.Controls.Add(txt)
        txt.LoadFile(Pt.UNCPath)
      Case ".gcode"
        Dim txt As New RichTextBox
        txt.Dock = DockStyle.Fill
        Me.Controls.Add(txt)
        txt.LoadFile(Pt.UNCPath)
      Case ".eia"
        Dim txt As New RichTextBox
        txt.Dock = DockStyle.Fill
        Me.Controls.Add(txt)
        txt.LoadFile(Pt.UNCPath)
      Case ".rtf"
        Dim txt As New RichTextBox
        txt.Dock = DockStyle.Fill
        Me.Controls.Add(txt)
        txt.LoadFile(Pt.UNCPath)
      Case ".html"
        Dim html As New WebBrowser
        html.ScriptErrorsSuppressed = True
        html.Dock = DockStyle.Fill
        Me.Controls.Add(html)
        html.Navigate(Pt.UNCPath)
      Case ".htm"
        Dim html As New WebBrowser
        html.ScriptErrorsSuppressed = True
        html.Dock = DockStyle.Fill
        Me.Controls.Add(html)
        html.Navigate(Pt.UNCPath)
      Case ".url"
        Dim html As New WebBrowser
        html.ScriptErrorsSuppressed = True
        html.Dock = DockStyle.Fill
        Me.Controls.Add(html)
        Dim url As String
        For Each ln As String In IO.File.ReadAllLines(Pt.UNCPath)
          If ln.IndexOf("URL", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
            url = ln.Remove(0, ln.IndexOf("=") + 1)
          End If
        Next
        If Not String.IsNullOrEmpty(url) Then
          html.Navigate(url)
        End If
      Case ".jpg"
        Dim pic As New PictureBox
        pic.Dock = DockStyle.Fill
        pic.Image = Image.FromFile(Pt.UNCPath)
        pic.SizeMode = PictureBoxSizeMode.CenterImage
        Me.Controls.Add(pic)
      Case ".jpeg"
        Dim pic As New PictureBox
        pic.Dock = DockStyle.Fill
        pic.Image = Image.FromFile(Pt.UNCPath)
        pic.SizeMode = PictureBoxSizeMode.CenterImage
        Me.Controls.Add(pic)
      Case ".png"
        Dim pic As New PictureBox
        pic.Dock = DockStyle.Fill
        pic.Image = Image.FromFile(Pt.UNCPath)
        pic.SizeMode = PictureBoxSizeMode.CenterImage
        Me.Controls.Add(pic)
      Case ".bmp"
        Dim pic As New PictureBox
        pic.Dock = DockStyle.Fill
        pic.Image = Image.FromFile(Pt.UNCPath)
        pic.SizeMode = PictureBoxSizeMode.CenterImage
        Me.Controls.Add(pic)
    End Select
  End Sub

  Private Sub PreviewWindow_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
    RemoveHandler exp.ExplorerWatcherFound, AddressOf FoundPath
  End Sub
End Class