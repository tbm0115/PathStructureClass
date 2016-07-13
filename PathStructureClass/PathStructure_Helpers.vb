Imports System.IO, System.Security.AccessControl, System.Text
Imports System.Xml
Imports System.Runtime.CompilerServices

Public Module PathStructure_Helpers
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

  Public Enum StringContainsType
    ContainsOr
    ContainsAnd
  End Enum
  <Extension()>
  Public Function Contains(ByVal Input As String, ByVal Condition As String(), Optional ByVal ContainsType As StringContainsType = StringContainsType.ContainsOr) As Boolean
    Dim cond As Boolean = False
    For i = 0 To Condition.Length - 1 Step 1
      If Input.IndexOf(Condition(i), System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        cond = True
        If ContainsType = StringContainsType.ContainsOr Then
          Exit For
        End If
      Else
        If ContainsType = StringContainsType.ContainsAnd Then
          cond = False
          Exit For
        End If
      End If
    Next
    Return cond
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

  Declare Function WNetGetConnection Lib "mpr.dll" Alias "WNetGetConnectionA" (ByVal lpszLocalName As String, _
     ByVal lpszRemoteName As String, ByRef cbRemoteName As Integer) As Integer

  ''' <summary>
  ''' Gets the UNC representation of the provided path
  ''' </summary>
  ''' <param name="sFilePath"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetUNCPath(ByVal sFilePath As String) As String
    '' Check before allocating resources...
    If sFilePath.StartsWith("\\") Then Return sFilePath
    '' Now allocate resources for processing
    Static allDrives() As DriveInfo = DriveInfo.GetDrives()
    'Dim d As DriveInfo
    Dim DriveType, Ctr As Integer
    Dim DriveLtr, UNCName As String
    Dim StrBldr As New StringBuilder

    UNCName = Space(160)
    DriveLtr = sFilePath.Substring(0, 3)

    For i = 0 To allDrives.Length - 1 Step 1 ' Each d In allDrives
      If allDrives(i).Name = DriveLtr Then
        DriveType = allDrives(i).DriveType
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

  Public Event PathStructureLog(ByVal sender As Object, ByVal e As System.EventArgs)
  Public Sub Log(ByVal input As String)
    RaiseEvent PathStructureLog(input, Nothing)
    'IO.File.AppendAllText(My.Computer.FileSystem.SpecialDirectories.MyDocuments.ToString & "\Path Structure Log.txt", input & vbLf)
  End Sub

  Public Sub AddDirectorySecurity(ByVal Path As String, ByVal Account As String, ByVal Rights As FileSystemRights, ByVal ControlType As AccessControlType)
    Dim Dir As New DirectoryInfo(Path)
    AddDirectorySecurity(Dir, Account, Rights, ControlType)
  End Sub
  Public Sub AddDirectorySecurity(ByVal Dir As DirectoryInfo, ByVal Account As String, ByVal Rights As FileSystemRights, ByVal ControlType As AccessControlType)
    Dim sec As DirectorySecurity = Dir.GetAccessControl()

    sec.AddAccessRule(New FileSystemAccessRule(Account, Rights, ControlType))

    Dir.SetAccessControl(sec)
  End Sub
  Public Sub RemoveDirectorySecurity(ByVal Path As String, ByVal Account As String, ByVal Rights As FileSystemRights, ByVal ControlType As AccessControlType)
    Dim Dir As New DirectoryInfo(Path)
    RemoveDirectorySecurity(Dir, Account, Rights, ControlType)
  End Sub
  Public Sub RemoveDirectorySecurity(ByVal Dir As DirectoryInfo, ByVal Account As String, ByVal Rights As FileSystemRights, ByVal ControlType As AccessControlType)
    Dim sec As DirectorySecurity = Dir.GetAccessControl()

    sec.RemoveAccessRule(New FileSystemAccessRule(Account, Rights, ControlType))

    Dir.SetAccessControl(sec)
  End Sub

  ''' <summary>
  ''' Gets an array of raw variable names from the provided string
  ''' </summary>
  ''' <param name="Input"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetListOfRawVariables(ByVal Input As String) As String()
    Dim lst As New List(Of String)
    Do Until (Not Input.Contains("{")) And (Not Input.Contains("}"))
      If Input.IndexOf("{") < Input.IndexOf("}") Then
        Input = Input.Remove(0, Input.IndexOf("{") + 1)
        lst.Add(Input.Remove(Input.IndexOf("}")))
        Input = Input.Remove(0, Input.IndexOf("}") + 1)
      End If
    Loop
    Return lst.ToArray
  End Function
End Module
