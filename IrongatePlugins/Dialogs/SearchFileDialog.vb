Imports System.Windows.Forms
Imports System.Xml
Imports PathStructureClass.PathStructure_Helpers
Imports System.Speech, System.Speech.Recognition

Public Class SearchFileDialog
  Private _pstruct As PathStructureClass.PathStructure

  Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
    If Not String.IsNullOrEmpty(txtClosestMatch.Text) Then
      If IO.File.Exists(txtClosestMatch.Text) Or IO.Directory.Exists(txtClosestMatch.Text) Then
        Try
          Process.Start(txtClosestMatch.Text)
        Catch ex As Exception
          MessageBox.Show("Failed to open filesystem object: " & vbCrLf & txtClosestMatch.Text & vbCrLf & "Due to error: " & vbCrLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
      End If
      Me.DialogResult = System.Windows.Forms.DialogResult.OK
      Me.Close()
    Else
      MessageBox.Show("Please fill out appropriate information until a file or folder is found. Alternatively, cancel", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
    End If
  End Sub
  Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
    Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.Close()
  End Sub

  Public Sub New(ByVal PStructure As PathStructureClass.PathStructure)
    InitializeComponent()

    _pstruct = PStructure
  End Sub

  Private lstSFiles As SortedList(Of String, String)
  Private Sub SearchFileDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    Dim myXML As New XmlDocument
    myXML.Load(_pstruct.SettingsPath)
    Dim lstFiles As XmlNodeList = myXML.SelectNodes("//File[not(Option)]|//File/Option")
    lstSFiles = New SortedList(Of String, String)
    For i = 0 To lstFiles.Count - 1 Step 1
      If Not lstSFiles.ContainsKey(lstFiles(i).Attributes("name").Value) Then
        lstSFiles.Add(lstFiles(i).Attributes("name").Value, lstFiles(i).FindXPath())
      End If
    Next

    cmbFiles.Items.Clear()
    For Each k As String In lstSFiles.Keys
      cmbFiles.Items.Add(k)
    Next

    btnSpeech_Click(btnSpeech, Nothing)
  End Sub

  Private strUri As String
  Private Sub cmbFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFiles.SelectedIndexChanged
    strUri = String.Empty
    pnlVariables.Controls.Clear()
    If cmbFiles.SelectedIndex >= 0 And cmbFiles.SelectedIndex < lstSFiles.Count Then
      strUri = _pstruct.GetURIfromXPath(lstSFiles(cmbFiles.SelectedItem.ToString))
      FitTextbox(txtURI, strUri)

      Dim vars As String() = strUri.GetListOfInternalStrings("{", "}") '.Split({"{", "}"}, System.StringSplitOptions.RemoveEmptyEntries)
      For i = 0 To vars.Length - 1 Step 1
        Dim lbl As New Label
        lbl.Text = vars(i)
        lbl.Size = New Drawing.Size(pnlVariables.Width - (pnlVariables.Width * 0.1), 48)
        lbl.Location = New Drawing.Point(0, 0 + (96 * i))
        lbl.AutoSize = False
        pnlVariables.Controls.Add(lbl)
        
        Dim txt As New TextBox
        txt.Size = New Drawing.Size(pnlVariables.Width - (pnlVariables.Width * 0.1), 48)
        txt.Location = New Drawing.Point(50, 48 + (96 * i))
        txt.Tag = "{" & vars(i) & "}"
        pnlVariables.Controls.Add(txt)
        AddHandler txt.TextChanged, AddressOf DynamicText_Changed
      Next
      If pnlVariables.Controls.Count >= 2 Then
        pnlVariables.Controls(1).Focus()
      End If
    End If
  End Sub

  Private Sub DynamicText_Changed(ByVal sender As Object, ByVal e As System.EventArgs)
    Dim strTemp As String = strUri
    '' Setup replacement URI
    For i = 1 To pnlVariables.Controls.Count - 1 Step 2
      strTemp = strTemp.Replace(pnlVariables.Controls(i).Tag, pnlVariables.Controls(i).Text)
    Next
    Debug.WriteLine("About to test uri: " & strTemp)
    TestURI(strTemp) '' Test temp URI and slowly reduce the URI to see if there is a valid match

  End Sub

  Private Sub TestURI(ByVal Uri As String)
    If CheckFile(Uri) Then
      FitTextbox(txtClosestMatch, Uri)
      txtClosestMatch.BackColor = Drawing.Color.LightGreen
    ElseIf CheckDir(Uri) Then
      FitTextbox(txtClosestMatch, Uri)
      txtClosestMatch.BackColor = Drawing.Color.LightBlue
    Else
      If Uri.CountStringOccurance("\") > 2 Then
        TestURI(Uri.Remove(Uri.LastIndexOf("\")))
      Else
        txtClosestMatch.BackColor = DefaultBackColor
      End If
    End If
  End Sub

  Private Sub FitTextbox(ByVal cntrlTextbox As TextBox, ByVal strText As String)
    Dim fnt As New System.Drawing.Font("Arial", 12.0F)
    Do Until TextRenderer.MeasureText(strText, fnt).Width < cntrlTextbox.Width
      fnt = New System.Drawing.Font("Arial", fnt.Size - 0.1F)
    Loop
    cntrlTextbox.Font = fnt
    cntrlTextbox.Text = strText
  End Sub
  Private Function CheckFile(ByRef Uri As String) As Boolean
    If CheckDir(Uri.Remove(Uri.LastIndexOf("\"))) Then
      For Each fil As String In IO.Directory.GetFiles(Uri.Remove(Uri.LastIndexOf("\")))
        If fil.IndexOf(Uri, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
          Debug.WriteLine("Found File! " & fil)
          Uri = fil
          Return True
        End If
      Next
    Else
      'Debug.WriteLine("Directory didn't exists: " & Uri.Remove(Uri.LastIndexOf("\")))
      Return IO.File.Exists(Uri)
    End If
    Return False
  End Function
  Private Function CheckDir(ByVal Uri As String) As Boolean
    Return IO.Directory.Exists(Uri)
  End Function

  Private Sub btnSpeech_Click(sender As Object, e As EventArgs) Handles btnSpeech.Click
    Dim thrd As New Threading.Thread(Sub()
                                       Dim d As statStatusChangeTextCallback
                                       Using recognizer As New SpeechRecognitionEngine(New Globalization.CultureInfo("en-US"))
                                         Dim filChoices As New Choices(lstSFiles.Keys.ToArray)
                                         recognizer.LoadGrammar(New Grammar(filChoices.ToGrammarBuilder))
                                         recognizer.RequestRecognizerUpdate()
                                         AddHandler recognizer.SpeechRecognized, AddressOf recognizer_SpeechRecognized

                                         recognizer.SetInputToDefaultAudioDevice()

                                         Dim wait As Integer = 0
                                         Dim maxTime As Integer = 8
                                         recognizer.RecognizeAsync(RecognizeMode.Multiple)

                                         '' Try to set text from different thread
                                         If Me.InvokeRequired Then
                                           d = New statStatusChangeTextCallback(Sub(Text)
                                                                                  statSpeech.Text = Text
                                                                                End Sub)
                                           Me.Invoke(d, New Object() {"Listening..."})
                                         End If

                                         While wait < maxTime
                                           Threading.Thread.Sleep(1000) '' Wait one second

                                           wait += 1
                                           DrawOnMicButton(wait, maxTime)

                                           '' Try to set text from different thread
                                           If Me.InvokeRequired Then
                                             d = New statStatusChangeTextCallback(Sub(Text)
                                                                                    statSpeech.Text = Text
                                                                                  End Sub)
                                             Me.Invoke(d, New Object() {(maxTime - wait).ToString & "/" & maxTime.ToString & " seconds"})
                                           End If

                                           Application.DoEvents()
                                         End While
                                         recognizer.RecognizeAsyncCancel()
                                       End Using

                                       '' Try to set text from different thread
                                       If Me.InvokeRequired Then
                                         d = New statStatusChangeTextCallback(Sub(Text)
                                                                                statSpeech.Text = Text
                                                                              End Sub)
                                         Me.Invoke(d, New Object() {"Not Listening"})
                                       End If
                                     End Sub)
    thrd.Start()
  End Sub
  Private Sub DrawOnMicButton(ByVal Index As Integer, ByVal Max As Integer)
    Dim d As Integer = btnSpeech.Height * 2
    Dim r As Integer = btnSpeech.Height
    'Dim pn As New Drawing.Pen(Drawing.Brushes.MediumSeaGreen)
    Dim frstPnt As New Drawing.Point(0, -btnSpeech.Height)
    Using g As Drawing.Graphics = btnSpeech.CreateGraphics()
      g.Clear(Drawing.Color.WhiteSmoke)

      g.TranslateTransform(btnSpeech.Width / 2, btnSpeech.Height / 2) '' Set origin in center
      Dim lstPnts As New List(Of Drawing.Point)
      lstPnts.Add(New Drawing.Point(0, 0))
      For i = 0 To Index Step 1
        Dim rads As Double = (CInt((12 * i) / Max) * Math.PI) / 6 '' Calculate relative radians
        Dim x As Double = Math.Cos(rads) * (frstPnt.X - 0) - Math.Sin(rads) * (frstPnt.Y - 0) + 0
        Dim y As Double = Math.Sin(rads) * (frstPnt.X - 0) + Math.Cos(rads) * (frstPnt.Y - 0) + 0
        lstPnts.Add(New Drawing.Point(CInt(x), CInt(y)))
        'g.DrawLine(pn, New Drawing.Point(0, 0), New Drawing.Point(CInt(x), CInt(y)))
      Next
      g.FillPolygon(Drawing.Brushes.MediumSeaGreen, lstPnts.ToArray)
    End Using
  End Sub

  Delegate Sub statStatusChangeTextCallback(ByVal Text As String)
  Delegate Sub cmbFilesSelectionCallback(ByVal Index As Integer)
  Private Sub recognizer_SpeechRecognized(ByVal sender As Object, ByVal e As SpeechRecognizedEventArgs)
    Debug.WriteLine("Recognized text (" & e.Result.Confidence.ToString & "): " & e.Result.Text)
    statSpeech.Text = e.Result.Text
    If lstSFiles.Keys.Contains(e.Result.Text) Then
      If Me.InvokeRequired Then
        Dim d As New cmbFilesSelectionCallback(Sub(Index)
                                                 cmbFiles.SelectedIndex = Index
                                               End Sub)
        Me.Invoke(d, New Object() {lstSFiles.IndexOfKey(e.Result.Text)})
      End If
    End If
  End Sub
End Class
