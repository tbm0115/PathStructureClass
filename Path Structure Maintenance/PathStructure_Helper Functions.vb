Imports System.IO, System.Text
Imports System.Xml
Imports System.Runtime.CompilerServices

Module PathStructure_Helper_Functions
  Public defaultPaths As New List(Of String)
  Public ERPConnection As New DatabaseConnection(My.Settings.ERPConnection)
  Public myXML As XmlDocument

  Declare Function WNetGetConnection Lib "mpr.dll" Alias "WNetGetConnectionA" (ByVal lpszLocalName As String, _
     ByVal lpszRemoteName As String, ByRef cbRemoteName As Integer) As Integer

  Public Function GetUNCPath(ByVal sFilePath As String) As String
    Dim allDrives() As DriveInfo = DriveInfo.GetDrives()
    Dim d As DriveInfo
    Dim DriveType, Ctr As Integer
    Dim DriveLtr, UNCName As String
    Dim StrBldr As New StringBuilder

    If sFilePath.StartsWith("\\") Then Return sFilePath

    UNCName = Space(160)

    DriveLtr = sFilePath.Substring(0, 3)

    For Each d In allDrives
      If d.Name = DriveLtr Then
        DriveType = d.DriveType
        Exit For
      End If
    Next

    If DriveType = 4 Then

      Ctr = WNetGetConnection(sFilePath.Substring(0, 2), UNCName, UNCName.Length)

      If Ctr = 0 Then
        UNCName = UNCName.Trim
        For Ctr = 0 To UNCName.Length - 1
          Dim SingleChar As Char = UNCName(Ctr)
          Dim asciiValue As Integer = Asc(SingleChar)
          If asciiValue > 0 Then
            StrBldr.Append(SingleChar)
          Else
            Exit For
          End If
        Next
        StrBldr.Append(sFilePath.Substring(2))
        Return StrBldr.ToString
      Else
        Return sFilePath
        'MsgBox("Cannot Retrieve UNC path" & vbCrLf & "Must Use Mapped Drive of SQLServer", MsgBoxStyle.Critical)
      End If
    Else
      Return sFilePath
      'MsgBox("Cannot Use Local Drive" & vbCrLf & "Must Use Mapped Drive of SQLServer", MsgBoxStyle.Critical)
    End If
  End Function

  Public Sub Log(ByVal input As String)
    IO.File.AppendAllText(My.Computer.FileSystem.SpecialDirectories.MyDocuments.ToString & "\Path Structure Log.txt", input & vbLf)
  End Sub

  ''' <summary>
  ''' Gets the first occurance of an internal string between a left and right string
  ''' </summary>
  ''' <param name="Input">Base string class</param>
  ''' <param name="Left">The left-side comparitor</param>
  ''' <param name="Right">The right-side comparitor</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Public Function GetInternalString(ByVal Input As String, ByVal Left As String, ByVal Right As String) As String
    If Input.Contains(Left) And Input.Contains(Right) Then
      Return Input.Substring(Input.IndexOf(Left) + Left.Length, Input.IndexOf(Right, Input.IndexOf(Left) + Left.Length) - Input.IndexOf(Left) - Left.Length)
    Else
      Return Input
    End If
  End Function

  ''' <summary>
  ''' Surrounds the base array of strings with a prefix and suffix. Optionally, empty strings will be skipped.
  ''' </summary>
  ''' <param name="Arr">Base array of strings</param>
  ''' <param name="Prefix">String to prefix each string item in the base array</param>
  ''' <param name="Suffix">String to suffix each string item in the base array</param>
  ''' <param name="SkipEmpties">Optional, allows function to skip empty string from the base array</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Public Function SurroundJoin(ByVal Arr As String(), ByVal Prefix As String, ByVal Suffix As String, Optional ByVal SkipEmpties As Boolean = False) As String
    Dim out As New StringBuilder
    If Not IsNothing(Arr) Then
      For Each s As String In Arr
        If (SkipEmpties And Not String.IsNullOrEmpty(s)) Or Not SkipEmpties Then
          out.Append(Prefix & s & Suffix)
        End If
      Next
    End If
    Return out.ToString
  End Function

  ''' <summary>
  ''' Gets an array of occuring internal strings that are between a left and right string
  ''' </summary>
  ''' <param name="Input">Base string class</param>
  ''' <param name="Left">The left-side comparitor</param>
  ''' <param name="Right">The right-side comparitor</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Public Function GetListOfInternalStrings(ByVal Input As String, ByVal Left As String, ByVal Right As String) As String()
    Dim lst As New List(Of String)
    Do Until Not Input.Contains(Left)
      If Input.Contains(Left) And Input.Contains(Right) Then
        lst.Add(GetInternalString(Input, Left, Right))
        Input = Input.Replace(Left & lst(lst.Count - 1) & Right, "|" & lst(lst.Count - 1) & "|")
      Else
        Exit Do
      End If
    Loop
    Return lst.ToArray
  End Function

  ''' <summary>
  ''' Gets a count of how many times a given string occurs in the base string
  ''' </summary>
  ''' <param name="Input">Base string class</param>
  ''' <param name="Identifier">Comparitor string</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Public Function CountStringOccurance(ByVal Input As String, ByVal Identifier As String) As Integer
    Dim i As Integer = 0
    Do Until Not Input.Contains(Identifier)
      If Input.Contains(Identifier) Then
        Input = Input.Remove(0, Input.IndexOf(Identifier) + (Identifier.Length))
        i += 1
      End If
    Loop
    Return i
  End Function

  ''' <summary>
  ''' Gets the XPath string for the base XmlNode
  ''' </summary>
  ''' <param name="node">Base XmlNode</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Public Function FindXPath(ByVal node As XmlNode) As String
    Dim builder As New StringBuilder
    While Not IsNothing(node)
      Select Case node.NodeType
        Case XmlNodeType.Attribute
          builder.Insert(0, "/@" & node.Name)
          node = DirectCast(node, XmlAttribute).OwnerElement
          Continue While
        Case XmlNodeType.Element
          Dim index As Integer = FindElementIndex(DirectCast(node, XmlElement))
          builder.Insert(0, "/" & node.Name & "[" & index & "]")
          node = node.ParentNode
        Case XmlNodeType.Document
          Return builder.ToString
        Case Else
          Throw New ArgumentException("Only elements and attributes are supported")
      End Select
    End While
    Throw New ArgumentException("Node was not in a document")
  End Function

  ''' <summary>
  ''' Gets the index of the base element within its parent
  ''' </summary>
  ''' <param name="Element">Base element</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Private Function FindElementIndex(ByVal Element As XmlElement) As Integer
    Dim parentNode As XmlNode = Element.ParentNode
    If parentNode.NodeType = XmlNodeType.Document Then
      Return 1
    End If
    Dim parent As XmlElement = DirectCast(parentNode, XmlElement)
    Dim index As Integer = 1
    For Each candidate As XmlNode In parent.ChildNodes
      If candidate.NodeType = XmlNodeType.Element And candidate.Name = Element.Name Then
        If DirectCast(candidate, XmlElement).Equals(Element) Then
          Return index
        End If
        index += 1
      End If
    Next
    Throw New ArgumentException("Couldn't find element within parent")
  End Function

  ''' <summary>
  ''' Gets the index of the Nth occurance of a character
  ''' </summary>
  ''' <param name="Input">Base string</param>
  ''' <param name="Ch">Character to search for</param>
  ''' <param name="Index">Occurance index of the search character</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  <Extension()>
  Public Function GetNthIndexOf(ByVal Input As String, ByVal Ch As Char, ByVal Index As Integer) As Integer
    Dim count As Integer = 0
    For i = 0 To Input.Length - 1 Step 1
      If Input(i) = Ch Then
        count += 1
        If count = Index Then
          Return i
        End If
      End If
    Next
    Return -1
  End Function
  
  Public Function IsInDefaultPath(ByVal Input As String, Optional ByVal PreferredPath As String = "") As Boolean
    For i = 0 To defaultPaths.Count - 1 Step 1
      If (Input.IndexOf(defaultPaths(i), System.StringComparison.OrdinalIgnoreCase) >= 0) Or (defaultPaths(i).IndexOf(Input, System.StringComparison.OrdinalIgnoreCase) >= 0) Then
        If Not String.IsNullOrEmpty(PreferredPath) And Not String.Equals(defaultPaths(i), PreferredPath) Then
          Continue For
        End If
        Return True
      End If
    Next
    Return False
  End Function

  ''' <summary>
  ''' Replaces PathStructure variables with provided values.
  ''' </summary>
  ''' <param name="Input">Full or partial path string</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Function ReplaceVariables(ByVal Input As String, ByVal Path As String) As String
    Dim vars As New PathStructure.VariableArray("//Variables", Path)
    Return vars.Replace(Input)
  End Function

  ''' <summary>
  ''' Converts the XPath for the PathStructure into a valid FileSystem path.
  ''' </summary>
  ''' <param name="XPath">XML XPath</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Function GetURIfromXPath(ByVal XPath As String) As String
    If Not String.IsNullOrEmpty(XPath) Then
      Dim x As XmlElement = myXML.SelectSingleNode(XPath)
      '' Check if the element has the temporary URI for this session. If so, use it
      If x.HasAttribute("tmpURI") Then
        Return x.Attributes("tmpURI").Value
      Else
        Dim a As XmlAttribute = myXML.CreateAttribute("tmpURI")
        x.Attributes.Append(a)
        Dim u As New StringBuilder
        Do Until x.Name = "Structure"
          If String.Equals(x.Name, "Folder", StringComparison.OrdinalIgnoreCase) Then
            u.Insert(0, x.Attributes("name").Value & "\")
          ElseIf String.Equals(x.Name, "File", StringComparison.OrdinalIgnoreCase) Then
            u.Append(x.InnerText)
          ElseIf String.Equals(x.Name, "Option", StringComparison.OrdinalIgnoreCase) Then
            u.Append(x.InnerText)
            x = x.ParentNode '' Set parent node to 'File' so that the next x-set will set x to the folder
          End If
          x = x.ParentNode
        Loop
        'u = _startPath & u
        u.Insert(0, x.Attributes("defaultPath").Value & "\") ' & x.Attributes("path").Value & "\")
        a.Value = u.ToString
        Return u.ToString
      End If
    Else
      Return ""
    End If
  End Function
  Public Function GetDescriptionfromXPath(ByVal XPath As String) As String
    If Not String.IsNullOrEmpty(XPath) Then
      Dim x As XmlElement = myXML.SelectSingleNode(XPath)
      If Not IsNothing(x) Then
        If x.HasAttribute("description") Then
          Return x.Attributes("description").Value
        End If
      End If
    End If
    Return ""
  End Function
End Module
