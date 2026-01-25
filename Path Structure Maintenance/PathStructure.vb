Imports System.Xml, System.IO, System.Text
Imports HTML, HTML.HTMLWriter, HTML.HTMLWriter.HTMLTable

Public Class PathStructure : Implements IDisposable
  Private _type As PathType
  Private _path As String
  Private Shared _struct As XmlElement
  Private _infoFile As IO.FileInfo
  Private _infoFolder As IO.DirectoryInfo
  'Private _infoObject As IO.FileSystemInfo
  Private Shared myXML As XmlDocument
  Private _defaultPath, _startPath As String
  Private _variables As VariableArray
  Private _parent As PathStructure
  Private _children As PathStructure()
  Private _candidates As StructureCandidateArray

  ''' <summary>
  ''' Gets the PathStructure representation of the current path's parent directory.
  ''' </summary>
  ''' <value></value>
  ''' <returns>PathStructure</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Parent As PathStructure
    Get
      If IsNothing(_parent) Then
        'Dim strTemp As String = _path
        'If strTemp.EndsWith("\") Then strTemp = strTemp.Remove(strTemp.Length - 1) '' Fix last index issue
        'If strTemp.Contains("\") Then '' Verify the path is still valid
        '  strTemp = strTemp.Remove(strTemp.LastIndexOf("\"))
        '  Dim chk As Boolean = True
        '  _parent = New PathStructure(strTemp, chk)
        '  If Not chk Then _parent = Nothing
        'End If
        Dim chk As Boolean = True
        _parent = New PathStructure(Me.ParentPath, chk)
        If Not chk Then _parent = Nothing
      End If
      Return _parent
    End Get
  End Property
  Public ReadOnly Property ParentPath As String
    Get
      Dim splt As String() = _path.Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      If splt.Length > 1 Then
        ReDim Preserve splt(splt.Length - 2)
        'Debug.WriteLine("Parent path: " & "\\" & String.Join("\", splt))
        Return "\\" & String.Join("\", splt)
      Else
        Return ""
      End If
    End Get
  End Property
  Public ReadOnly Property CurrentDirectory As String
    Get
      If _type = PathType.File Then
        Return ParentPath
      Else
        Return _path
      End If
    End Get
  End Property
  ''' <summary>
  ''' Enumerates the PathStructure objects the represent the current path's child filesystem objects.
  ''' </summary>
  ''' <value></value>
  ''' <returns>PathStructure()</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Children As PathStructure()
    Get
      If _type = PathType.Folder Then
        If IsNothing(_children) Then
          Dim arr As New List(Of PathStructure)
          Dim chk As Boolean
          For Each fil As String In IO.Directory.EnumerateFiles(_path)
            chk = True
            Dim tmp As New PathStructure(fil, PathType.File, chk)
            If chk Then
              arr.Add(tmp)
            End If
          Next
          For Each fol As String In IO.Directory.EnumerateDirectories(_path)
            chk = True
            Dim tmp As New PathStructure(fol, PathType.Folder, chk)
            If chk Then
              arr.Add(tmp)
            End If
          Next
          _children = arr.ToArray
        End If
        Return _children
      Else
        Return Nothing
      End If
    End Get
  End Property
  Public ReadOnly Property FileInfo As IO.FileSystemInfo
    Get
      Return _infoFile
    End Get
  End Property
  Public ReadOnly Property FolderInfo As IO.FileSystemInfo
    Get
      Return _infoFolder
    End Get
  End Property
  ''' <summary>
  ''' Gets a list of variables in the current path and their values.
  ''' </summary>
  ''' <value>Key is variable name. Value is the variable value.</value>
  ''' <returns>SortedList(Of String, String)</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Variables As VariableArray
    Get
      Return _variables
    End Get
  End Property
  ''' <summary>
  ''' Gets the UNC formatted path of the current path
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property UNCPath As String
    Get
      Return _path
    End Get
  End Property
  ''' <summary>
  ''' Gets the current path's path type, an enum of PathType (File or Folder)
  ''' </summary>
  ''' <value></value>
  ''' <returns>PathType</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Type As PathType
    Get
      Return _type
    End Get
  End Property
  ''' <summary>
  ''' Gets the default (or root) directory for the current Structure.
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property DefaultPath As String
    Get
      Return _defaultPath
    End Get
  End Property
  ''' <summary>
  ''' Gets the current path with replaced variables.
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property StartPath As String
    Get
      Return _startPath
    End Get
  End Property
  Public ReadOnly Property PathName As String
    Get
      If _type = PathType.File Then
        Return _infoFile.Name
      ElseIf _type = PathType.Folder Then
        Return _infoFolder.Name
      Else
        Return ""
      End If
    End Get
  End Property
  Public ReadOnly Property Extension As String
    Get
      If _type = PathType.File Then
        Return _infoFile.Extension
      Else
        Return ""
      End If
    End Get
  End Property
  Public ReadOnly Property PathStructure As XmlElement
    Get
      Return _struct
    End Get
  End Property
  Public ReadOnly Property StructureCandidates As StructureCandidateArray
    Get
      If _candidates Is Nothing Then IsNameStructured()
      Return _candidates
    End Get
  End Property

  Public Enum PathType
    File
    Folder
  End Enum

  Public Overrides Function ToString() As String
    If _type = PathType.File Then
      Return _infoFile.Name
    ElseIf _type = PathType.Folder Then
      Return _infoFolder.Name
    Else
      Return _path
    End If
  End Function

  Public Sub New(ByVal Path As String, Optional ByVal SetType As PathStructure.PathType = Nothing, Optional ByRef Successful As Boolean = Nothing)
    '' Set path
    _path = GetUNCPath(Path)

    '' Determine/Set path type
    If SetType = Nothing Then
      If IO.File.Exists(_path) Then
        _type = PathType.File
        _infoFile = New IO.FileInfo(_path)
        Successful = True
      ElseIf IO.Directory.Exists(_path) Then
        _type = PathType.Folder
        _infoFolder = New IO.DirectoryInfo(_path)
        If Not _path.EndsWith("\") Then _path += "\"
        Successful = True
      Else
        If Not IsNothing(Successful) Then
          Successful = False
        Else
          Throw New ArgumentException("Path type not determinable from '" & _path & "'", "Invalid Path Type")
        End If
      End If
    Else
      _type = SetType
      If _type = PathType.File Then
        _infoFile = New IO.FileInfo(_path)
      ElseIf _type = PathType.Folder Then
        _infoFolder = New IO.DirectoryInfo(_path)
      End If
    End If

    If IsNothing(myXML) Then
      myXML = New XmlDocument
      myXML.Load(My.Settings.SettingsPath)
    Else
      myXML = myXML
    End If
    '' Find relevant Structure
    Dim structs As XmlNodeList = myXML.SelectNodes("//Structure")
    For i = 0 To structs.Count - 1 Step 1
      If _path.IndexOf(structs(i).Attributes("defaultPath").Value, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        _struct = structs(i)
        Exit For
      End If
    Next
    If _struct Is Nothing Then Throw New ArgumentException("PathStructure: Couldn't determine the default Structure node from '" & _path & "'. Searched " & structs.Count.ToString & " Structures in XmlDocument.")

    _defaultPath = _struct.Attributes("path").Value
    Dim defSeparator As Integer = CountStringOccurance(_defaultPath, IO.Path.DirectorySeparatorChar)
    '' Enumerate variables
    _variables = New VariableArray(_struct.SelectSingleNode("Variables"), _path)


    '' Set Start path
    _startPath = _defaultPath & "\" & ReplaceVariables(_struct.Attributes("path").Value, _path)
  End Sub

  Public Class VariableArray
    Private _x As XmlElement
    Private _path As String
    Private _lst As New List(Of Variable)

    Default Public Property Item(ByVal Index As Integer) As Variable
      Get
        Return _lst(Index)
      End Get
      Set(value As Variable)
        _lst(Index) = value
      End Set
    End Property
    Default Public Property Item(ByVal Name As String) As Variable
      Get
        Dim v As Variable = Nothing
        For i = 0 To _lst.Count - 1 Step 1
          If String.Equals(_lst(i).Name, Name, StringComparison.OrdinalIgnoreCase) Then
            v = _lst(i)
            Exit For
          End If
        Next
        Return v
      End Get
      Set(value As Variable)
        Dim v As Variable = Nothing
        For i = 0 To _lst.Count - 1 Step 1
          If String.Equals(_lst(i).Name, Name, StringComparison.OrdinalIgnoreCase) Then
            _lst(i) = value
            Exit For
          End If
        Next
      End Set
    End Property
    Public ReadOnly Property Count As Integer
      Get
        Return _lst.Count
      End Get
    End Property
    Public ReadOnly Property Items As Variable()
      Get
        Return _lst.ToArray
      End Get
    End Property

    Public Sub New(ByVal XPath As String, ByVal Path As String)
      _x = _struct.SelectSingleNode(XPath)
      _path = Path
      If _x IsNot Nothing Then
        If _x.HasChildNodes Then
          AddRange(_x.SelectNodes("Variable"))
        End If
      End If
    End Sub
    Public Sub New(ByVal XElement As XmlElement, ByVal Path As String)
      _x = XElement
      _path = Path
      If _x IsNot Nothing Then
        If _x.HasChildNodes Then
          AddRange(_x.SelectNodes("Variable"))
        End If
      End If
    End Sub

    Private Sub Add(ByVal XPath As String)
      _lst.Add(New Variable(XPath, _path))
    End Sub
    Private Sub Add(ByVal XElement As XmlElement)
      _lst.Add(New Variable(XElement, _path))
    End Sub
    Private Sub AddRange(ByVal XPaths As String())
      For i = 0 To XPaths.Length - 1 Step 1
        _lst.Add(New Variable(XPaths(i), _path))
      Next
    End Sub
    Private Sub AddRange(ByVal XElements As XmlNodeList)
      For i = 0 To XElements.Count - 1 Step 1
        _lst.Add(New Variable(XElements(i), _path))
      Next
    End Sub

    Public Function Replace(ByVal Input As String) As String
      If _lst.Count > 0 Then
        For i = 0 To _lst.Count - 1 Step 1
          Input = _lst(i).Replace(Input)
        Next
      End If
      Return Input
    End Function

    Public Function IsValid(ByVal Input As String) As Boolean
      Dim blnValid As Boolean = True
      Dim tmp As Boolean
      For i = 0 To _lst.Count - 1 Step 1
        tmp = _lst(i).HasVariable(Input)
        'Debug.WriteLine("Input has variable? " & tmp.ToString & " (" & _lst(i).Name & ")(" & Input & ")")
        If tmp Then
          If Not _lst(i).IsValid() Then
            blnValid = False
            Exit For
          End If
        End If
      Next
      Return blnValid
    End Function

    Public Function ContainsName(ByVal PathName As String) As Boolean
      Dim fnd As Boolean = False
      For i = 0 To _lst.Count - 1 Step 1
        If PathName.IndexOf(_lst(i).Name, System.StringComparison.OrdinalIgnoreCase) >= 0 Then ' String.Equals(, PathName, StringComparison.OrdinalIgnoreCase) Then
          fnd = True
          Exit For
        End If
      Next
      Return fnd
    End Function
    Public Function EndsWithName(ByVal PathName As String) As Boolean
      Dim fnd As Boolean = False
      For i = 0 To _lst.Count - 1 Step 1
        '' Check that the index of the variable name is towards the end. There is a tolerance of 2 characters
        If PathName.LastIndexOf(_lst(i).Name, System.StringComparison.OrdinalIgnoreCase) >= (PathName.Length - _lst(i).Name.Length - 2) Then ' String.Equals(_lst(i).Name, PathName, StringComparison.OrdinalIgnoreCase) Then
          fnd = True
          Exit For
        End If
      Next
      Return fnd
    End Function
  End Class
  Public Class Variable
    Private _x As XmlElement
    Private _name, _erptable As String
    Private _index As Integer
    Private _cmds As New List(Of ERPCommand)
    Private _path As String
    Public ReadOnly Property Value As String
      Get
        Dim nodes As String() = _path.Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
        If _index < nodes.Length Then
          Return nodes(_index)
        Else
          Return ""
        End If
      End Get
    End Property
    Public ReadOnly Property Name As String
      Get
        Return _name
      End Get
    End Property

    Public Sub New(ByVal XPath As String, ByVal Path As String)
      _x = _struct.SelectSingleNode(XPath)
      _name = _x.Attributes("name").Value
      If _x.HasAttribute("erp") Then
        _erptable = _x.Attributes("erp").Value
      End If
      _index = Convert.ToInt32(_x.Attributes("pathindex").Value)
      If _x.HasChildNodes Then
        Dim cmds As XmlNodeList = _x.SelectNodes("ERPCommand")
        For i = 0 To cmds.Count - 1 Step 1
          _cmds.Add(New ERPCommand(cmds(i)))
        Next
      End If
      _path = Path
    End Sub
    Public Sub New(ByVal XElement As XmlElement, ByVal Path As String)
      _x = XElement
      _name = _x.Attributes("name").Value
      If _x.HasAttribute("erp") Then
        _erptable = _x.Attributes("erp").Value
      End If
      _index = Convert.ToInt32(_x.Attributes("pathindex").Value)
      If _x.HasChildNodes Then
        Dim cmds As XmlNodeList = _x.SelectNodes("ERPCommand")
        For i = 0 To cmds.Count - 1 Step 1
          _cmds.Add(New ERPCommand(cmds(i)))
        Next
      End If
      _path = Path
    End Sub

    Public Function HasVariable(ByVal Input As String) As Boolean
      Return (Input.IndexOf(_name) >= 0)
    End Function

    Public Function IsValid() As Boolean
      Dim blnFound As Boolean = False
      If My.Settings.blnERPCheck Then
        Dim cond As String
        Dim selFields As String
        For i = 0 To _cmds.Count - 1 Step 1
          If _cmds(i).IsOR Then '' Check if the command has an OR operator
            selFields += String.Join(",", _cmds(i).Fields) & ","
            cond += "(" & _cmds(i).Fields.SurroundJoin("", "='" & Replace(_cmds(i).Value).Replace("'", "''") & "' OR ", True) & ")"
            If cond.EndsWith(" OR )") Then cond = cond.Remove(cond.LastIndexOf(" OR )")) & ")"
          Else
            selFields += _cmds(i).Field & ","
            cond += _cmds(i).Field & "='" & Replace(_cmds(i).Value).Replace("'", "''") & "' AND "
          End If
        Next
        If selFields.LastIndexOf(",") = selFields.Length - 1 Then selFields = selFields.Remove(selFields.Length - 1)
        If cond.EndsWith(" AND ") Then cond = cond.Remove(cond.LastIndexOf(" AND "))
        cond = cond.Replace("{", "").Replace("}", "")

        If ERPConnection.Successful Then
          blnFound = ERPConnection.CommandHasValues("SELECT " & selFields & " FROM [" & _erptable & "] WHERE " & cond & ";")
        End If
      Else
        blnFound = True '' Set to true if the flag isn't even set. No need to raise alarm
      End If
      Return blnFound
    End Function

    Public Function Replace(ByVal Input As String) As String
      If Not String.IsNullOrEmpty(Input) Then
        If Input.IndexOf(_name, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
          Input = Input.Replace(_name, Value)
        End If
      End If
      Return Input
    End Function

    Public Class ERPCommand
      Private _x As XmlElement
      Private _rawcmd As String
      Public ReadOnly Property Field As String
        Get
          Return _rawcmd.Remove(_rawcmd.IndexOf("="))
        End Get
      End Property
      Public ReadOnly Property Value As String
        Get
          Return _rawcmd.Remove(0, _rawcmd.IndexOf("=") + 1)
        End Get
      End Property
      Public ReadOnly Property IsOR As Boolean
        Get
          Return (Field.IndexOf("||") >= 0)
        End Get
      End Property
      Public ReadOnly Property Fields As String()
        Get
          Return Field.Split({"||"}, System.StringSplitOptions.RemoveEmptyEntries)
        End Get
      End Property

      Public Sub New(ByVal XPath As String)
        _x = _struct.SelectSingleNode(XPath)
        _rawcmd = _x.InnerText
      End Sub
      Public Sub New(ByVal XElement As XmlElement)
        _x = XElement
        _rawcmd = _x.InnerText
      End Sub
    End Class
  End Class

  ''' <summary>
  ''' Determines if the current instance of a PathStructure is a descendant of the DefaultPath (or root directory)
  ''' </summary>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Function IsDescendantOfDefaultPath() As Boolean
    If Not String.IsNullOrEmpty(_path) Then
      If _path.IndexOf(_defaultPath, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        Return True
      End If
    End If
    Return False
  End Function

  ''' <summary>
  ''' Determines whether the current path is part of the Path Structure format.
  ''' </summary>
  ''' <param name="MainStructureName">Sets the default search location within the Path Structure.</param>
  ''' <param name="Candidates">A reference to a list that will be filled with the XPath to any valid Path Structure nodes.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function IsNameStructured() As Boolean
    '' Initialize candidates
    _candidates = New StructureCandidateArray(_path)

    Dim strTemp As String
    Dim searchXPath As String
    Dim blnFound As Boolean = False
    Dim isMatch As Boolean
    If _type = PathType.File Then
      strTemp = ReplaceVariables(ParentPath, _path)
      searchXPath = ".//Folder[@name='" & Uri.EscapeDataString(strTemp) & "']/File[not(Option)]|.//Folder[@name='" & Uri.EscapeDataString(strTemp) & "']/File/Option"
      If _struct.SelectNodes(searchXPath).Count <= 0 Then searchXPath = "//File[not(Option)]|.//File/Option"
    ElseIf _type = PathType.Folder Then
      strTemp = ReplaceVariables(PathName, _path)
      searchXPath = ".//Folder[@name='" & Uri.EscapeDataString(strTemp) & "']"
      If _struct.SelectNodes(searchXPath).Count <= 0 Then searchXPath = ".//Folder"
    End If
    If Not String.IsNullOrEmpty(searchXPath) Then
      Dim objs As XmlNodeList = _struct.SelectNodes(searchXPath)
      If objs.Count > 0 Then
        _candidates.AddRange(objs) '' Add all of the matching XPaths. Each new object will run a match check.
        _candidates.RemoveMismatches(100) '' Remove any mismatches
      Else
        'Debug.WriteLine("No nodes found for '" & searchXPath & "'")
      End If
    Else
      Throw New ArgumentException("Couldn't determine path type", "Invalid Path Type")
    End If

    If String.Equals(_path, _startPath, StringComparison.OrdinalIgnoreCase) Then
      _candidates.Add(_struct)
      blnFound = True
    End If

    '' Check if more than one Candidate, remove wildcards if at least one does not end in wildcard
    If _candidates.Count > 1 Or _candidates.Count = 0 Then
      'Debug.WriteLine("Fail(" & _candidates.Count.ToString & "): " & _path & " = " & SurroundJoin(_candidates.ToArray, "[", "]"))
      blnFound = False
    ElseIf _candidates.Count = 1 Then
      blnFound = True
    End If

    Return blnFound
  End Function

  Public Class StructureCandidateArray
    Private _lst As New List(Of StructureCandidate)
    Private _path As String

    Default Public Property Item(ByVal Index As Integer) As StructureCandidate
      Get
        Return _lst(Index)
      End Get
      Set(value As StructureCandidate)
        _lst(Index) = value
      End Set
    End Property
    Public ReadOnly Property Items As List(Of StructureCandidate)
      Get
        Return _lst
      End Get
    End Property
    Public ReadOnly Property Count As Integer
      Get
        Return _lst.Count
      End Get
    End Property

    Public Sub New(ByVal Path As String)
      _path = Path
    End Sub

    Public Sub Add(ByVal XPath As String)
      If Not String.IsNullOrEmpty(XPath) Then
        _lst.Add(New StructureCandidate(XPath, _path))
      End If
    End Sub
    Public Sub Add(ByVal XElement As XmlElement)
      _lst.Add(New StructureCandidate(XElement, _path))
    End Sub
    Public Sub Add(ByVal Candidate As StructureCandidate)
      _lst.Add(Candidate)
    End Sub
    Public Sub AddRange(ByVal XNodes As XmlNodeList)
      For i = 0 To XNodes.Count - 1 Step 1
        _lst.Add(New StructureCandidate(XNodes(i), _path))
      Next
    End Sub
    Public Sub AddRange(ByVal XPaths As String())
      For i = 0 To XPaths.Length - 1 Step 1
        _lst.Add(New StructureCandidate(XPaths(i), _path))
      Next
    End Sub
    Public Sub AddRange(ByVal Candidates As StructureCandidate())
      For i = 0 To Candidates.Length - 1 Step 1
        _lst.Add(Candidates(i))
      Next
    End Sub
    Public Sub AddRange(ByVal Candidates As List(Of StructureCandidate))
      For i = 0 To Candidates.Count - 1 Step 1
        _lst.Add(Candidates(i))
      Next
    End Sub
    Public Sub RemoveAt(ByVal Index As Integer)
      If Index < _lst.Count Then
        _lst.RemoveAt(Index)
      End If
    End Sub
    Public Sub Remove(ByVal Candidate As StructureCandidate)
      _lst.Remove(Candidate)
    End Sub

    Public Function ToArray() As String()
      Dim arr(_lst.Count - 1) As String
      For i = 0 To _lst.Count - 1 Step 1
        arr(i) = _lst(i).XPath
      Next
      Return arr
    End Function

    Public Sub RemoveMismatches(Optional ByVal MatchThreshold As Integer = 100)
      For i = _lst.Count - 1 To 0 Step -1
        If _lst(i).MatchPercentage < MatchThreshold Then
          _lst.RemoveAt(i)
        End If
      Next
    End Sub

    Public Function GetHighestMatch() As StructureCandidate
      Dim index As Integer = -1
      For i = 0 To _lst.Count - 1 Step 1
        If index = -1 Then index = i
        If _lst(i).MatchPercentage > _lst(index).MatchPercentage Then index = i
      Next
      If index >= 0 Then
        Return _lst(index)
      Else
        Return Nothing
      End If
    End Function
  End Class
  Public Class StructureCandidate
    Private _x As XmlElement
    Private _xpath, _path, _descr, _spath As String
    Private _match As Boolean
    Private _conf As Integer

    ''' <summary>
    ''' Gets the XPath for the candidate
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property XPath As String
      Get
        Return _xpath
      End Get
    End Property
    ''' <summary>
    ''' Gets the XmlElement reference for the candidate
    ''' </summary>
    ''' <value></value>
    ''' <returns>XmlElement</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property XElement As XmlElement
      Get
        Return _x
      End Get
    End Property
    ''' <summary>
    ''' Gets the URI string based on the structures XPath
    ''' </summary>
    ''' <value></value>
    ''' <returns>URI string</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property StructurePath As String
      Get
        Return _spath
      End Get
    End Property
    ''' <summary>
    ''' Gets the objects URI string
    ''' </summary>
    ''' <value></value>
    ''' <returns>URI string</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ObjectPath As String
      Get
        Return _path
      End Get
    End Property
    ''' <summary>
    ''' Gets the description as specified in the path structure
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property StructureDescription As String
      Get
        Return _descr
      End Get
    End Property
    Public ReadOnly Property IsMatch As Boolean
      Get
        Return _match
      End Get
    End Property
    Public ReadOnly Property MatchPercentage As Integer
      Get
        Return _conf
      End Get
    End Property
    Public ReadOnly Property PathName As String
      Get
        If _x.HasAttribute("name") Then
          Return _x.Attributes("name").Value
        Else
          Return ""
        End If
      End Get
    End Property

    Public Sub New(ByVal xPath As String, ByVal fPath As String)
      _x = _struct.SelectSingleNode(xPath)
      _xpath = xPath
      _path = fPath
      _spath = GetURIfromXPath(_xpath)
      _match = Match()
      _descr = GetDescriptionfromXPath(_xpath)
    End Sub
    Public Sub New(ByVal xElement As XmlElement, ByVal fPath As String)
      _x = xElement
      _xpath = FindXPath(xElement)
      _path = fPath
      _spath = GetURIfromXPath(_xpath)
      _match = Match()
      _descr = GetDescriptionfromXPath(_xpath)
    End Sub

    Private Function Match() As Boolean
      _conf = -1
      Dim msg As New StringBuilder
      Dim strTemp As String = _path
      '' Fix if a file
      If strTemp.LastIndexOf(".") > strTemp.LastIndexOf("\") Then strTemp = strTemp.Remove(strTemp.LastIndexOf("."))

      '' Iterate through each character and compare. Watch out for variables and peek into _path for next character
      Dim s As String() = ReplaceVariables(_spath, _path).Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      Dim f As String() = strTemp.Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      Dim si As Integer
      msg.AppendLine("Comparing '" & SurroundJoin(s, "[", "]") & "'(" & s.Length.ToString & ") to '" & SurroundJoin(f, "[", "]") & "'(" & f.Length.ToString & ")")
      msg.AppendLine("XPath: " & _xpath)
      If s.Length = f.Length Then
        For i = 0 To s.Length - 1 Step 1
          msg.AppendLine(vbTab & "(" & i.ToString & ")'" & s(i) & "' = '" & f(i) & "'")
          If i < f.Length Then
            If s(i).IndexOf("{") >= 0 And s(i).IndexOf("}") >= 0 Then
              '' Iterate through each character and compare. Watch out for variables and peek into _path for next character
              si = 0
              For j = 0 To f(i).Length - 1 Step 1
                If String.Equals(s(i)(si), "{") Then
                  Do Until String.Equals(s(i)(si), "}") Or si = (s(i).Length - 1) '' Try to skip to the end of the variable
                    si += 1
                  Loop
                  If Not si = (s(i).Length - 1) Then si += 1 '' Move index just past the end of the variable
                  Do Until String.Equals(f(i)(j), s(i)(si), StringComparison.OrdinalIgnoreCase) Or (j = f(i).Length - 1) '' Try to get to the next matching character or end of string
                    j += 1
                  Loop
                End If
                msg.AppendLine("'" & s(i)(si) & "'(" & si.ToString & "/" & (s(i).Length - 1).ToString & ") = '" & f(i)(j) & "'(" & j.ToString & "/" & (f(i).Length - 1).ToString & ")")
                '' If the last string is '}' then it doesn't matter, continue without error
                If (si = (s(i).Length - 1)) And String.Equals(s(i)(si), "}") Then
                  Continue For
                End If
                '' Now check if the current character is equal at all. If not, then setup confidence score. Skip if both indices are at end
                'If Not (j = (f(i).Length - 1)) And (si = (s(i).Length - 1)) Then
                If Not String.Equals(s(i)(si), f(i)(j), StringComparison.OrdinalIgnoreCase) Then
                  _conf = (((i / (s.Length)) * 100) + ((j / (f(i).Length)) * 10)) '' Confidence score 1's
                  Exit For
                Else
                  If Not (si = (s(i).Length - 1)) Then
                    si += 1 '' Increment the structure index
                  End If
                End If
                'End If
              Next
              If Not si = (s(i).Length - 1) Then '' Check if the standard was incomplete, meaning that the object path didn't have the next required character
                _conf = (i / (s.Length)) * 100 '' Confidence score 10's
              End If
              If _conf > -1 Then '' If the confidence has changed, exit the loop as it has failed
                Exit For
              End If
            ElseIf Not String.Equals(s(i), f(i), StringComparison.OrdinalIgnoreCase) Then '' Full comparison check
              _conf = (i / (s.Length)) * 100 '' Confidence score 10's
              Exit For
            End If
          End If
        Next
      Else
        _conf = 0 '' Ensure positive
      End If
      msg.AppendLine(vbTab & "Match percentage: " & _conf.ToString)
      'Debug.WriteLine(msg.ToString)
      If _conf = -1 Then
        _conf = 100
        Return True
      Else
        Return False
      End If
    End Function
  End Class

  ''' <summary>
  ''' Determines if the current path is valid according to the provided Path Structure node.
  ''' </summary>
  ''' <param name="Folder">The Path Structure node to check against the current path.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Overloads Function IsLocationValid(ByVal Folder As XmlElement) As Boolean
    Return IsLocationValid(New Uri(GetURIfromXPath(FindXPath(Folder))))
  End Function
  Public Overloads Function IsLocationValid(ByVal FolderXPath As String) As Boolean
    Return IsLocationValid(myXML.SelectSingleNode(FolderXPath))
  End Function
  Public Overloads Function IsLocationValid(ByVal FolderPath As Uri) As Boolean
    Dim pattern As String = FolderPath.AbsolutePath '& "(.*?)"
    If pattern.Contains("\") Then pattern = pattern.Replace("\", "\\")
    If pattern.Contains("{") And pattern.Contains("}") Then pattern = New RegularExpressions.Regex("{(.*?)}").Replace(pattern, "(.*?)")
    If _type = PathType.File Then
      Return New RegularExpressions.Regex(pattern, RegularExpressions.RegexOptions.IgnoreCase).IsMatch(ParentPath) ' _infoFile.DirectoryName & "\")
    ElseIf _type = PathType.Folder Then
      Return New RegularExpressions.Regex(pattern, RegularExpressions.RegexOptions.IgnoreCase).IsMatch(ParentPath & "\" & Me.PathName & "\") '_infoFolder.FullName)
    Else
      Throw New ArgumentException("Couldn't determine path type", "Invalid Path Type")
    End If
  End Function

  ''' <summary>
  ''' Gets a full XPath from a absolute or relative XPath.
  ''' </summary>
  ''' <param name="XPath">Absolute or relative XPath</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Function GetStructureTypefromXPath(ByVal XPath As String) As String
    Dim x As XmlElement = _struct.SelectSingleNode(XPath)
    Dim out As New StringBuilder
    If Not IsNothing(x) Then
      If x.HasAttribute("tmpStruct") Then
        Return x.Attributes("tmpStruct").Value
      Else
        Do Until x.ParentNode.Name = "Structure"
          If x.Name = "Structure" Then Exit Do
          If x.Name = "Option" Then
            out.Insert(0, x.ParentNode.Attributes("name").Value & "-" & x.Attributes("name").Value & "/")
            x = x.ParentNode
          Else
            out.Insert(0, x.Attributes("name").Value & "/")
          End If
          x = x.ParentNode
        Loop
        Dim a As XmlAttribute = myXML.CreateAttribute("tmpStruct")
        a.Value = "Structure/" & out.ToString
        x.Attributes.Append(a)
        Return a.Value
      End If
    Else
      Return ""
    End If
  End Function

  Public Function FindNearestArchive(Optional ByVal FocusPath As PathStructure = Nothing) As String
    If FocusPath Is Nothing Then FocusPath = Me
    If FocusPath.Type = PathType.Folder Then
      For i = 0 To FocusPath.Children.Length - 1 Step 1
        If String.Equals(FocusPath.Children(i).PathName, "Archive", StringComparison.OrdinalIgnoreCase) And FocusPath.Children(i).Type = PathType.Folder Then
          Return FocusPath.Children(i).UNCPath
        End If
      Next
    End If
    If IsInDefaultPath(FocusPath.Parent.UNCPath) Then
      Return FindNearestArchive(FocusPath.Parent)
    Else
      Return ""
    End If
  End Function

  Public Sub LogData(ByVal ChangedPath As String, ByVal Method As String)
    Try
      IO.File.AppendAllText(_defaultPath & "\PathStructure Changes.csv",
                            DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") & "," & _path & "," & ChangedPath & "," & Method & "," & My.User.Name & vbCrLf)
    Catch ex As Exception
      Log("Error while appending change log:" & vbCrLf & vbTab & ex.Message)
    End Try
  End Sub

  Public Class AuditVisualReport
    Private _report As HTML.HTMLWriter
    Private _fileCount As Integer
    Private _errPaths, _optPaths As List(Of String)
    Private fileSystem As HTML.HTMLWriter.HTMLList
    Private _path As PathStructure
    Private ERPVariables As New SortedList(Of String, String)
    Private _quit As Boolean = False

    ''' <summary>
    ''' Gets the HTML markup for the report.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ReportMarkup As String
      Get
        _report.AddBootstrapReference()
        _report.AddToHTMLMarkup(My.Resources.FileSystemVisualAuditTemplate.Replace("{PLACEHOLDER:FILESYSTEM}", fileSystem.Markup))
        Return _report.HTMLMarkup
      End Get
    End Property
    Public Event ChildAudited(ByVal e As AuditedEventArgs)
    Public Event GrandChildAudited(ByVal e As AuditedEventArgs)
    Public Property FileCount As Integer
      Get
        Return _fileCount
      End Get
      Set(value As Integer)
        _fileCount += 1
      End Set
    End Property

    Public Class AuditedEventArgs
      Private _path As String
      Private _index, _tot As Integer
      Private _audit As AuditVisualReport

      Public ReadOnly Property Path As String
        Get
          Return _path
        End Get
      End Property
      Public ReadOnly Property Index As Integer
        Get
          Return _index
        End Get
      End Property
      Public ReadOnly Property ParentTotal As Integer
        Get
          Return _tot
        End Get
      End Property
      Public ReadOnly Property Audit As AuditVisualReport
        Get
          Return _audit
        End Get
      End Property
      Public Sub New(ByVal AuditReport As AuditVisualReport, ByVal ChildPath As String, ByVal ChildIndex As Integer, ByVal ChildCount As Integer)
        _audit = AuditReport
        _path = ChildPath
        _index = ChildIndex
        _tot = ChildCount
      End Sub
    End Class

    Public Sub New(ByVal Path As PathStructure)
      _report = New HTML.HTMLWriter
      _path = Path
      fileSystem = New HTMLList(HTMLList.ListType.Unordered)
    End Sub

    Public Enum StatusCode
      ValidPath = 0
      InvalidPath = 1
      Other = 2
    End Enum

    ''' <summary>
    ''' Appends to the current instance of an AuditReport
    ''' </summary>
    ''' <param name="Message">Text to be displayed in report.</param>
    ''' <param name="Code">(Optional) Status code for the message.</param>
    ''' <param name="Path">(Optional) Provides the path to be added to the internal list of error or optimal path(s).</param>
    ''' <remarks></remarks>
    Public Function Report(ByVal Message As String, ByVal Code As StatusCode, ByVal Path As PathStructure) As HTMLList.ListItem
      Dim spn As HTMLSpan
      If Path.Type = PathType.Folder Then
        spn = New HTMLSpan("", New AttributeList({"class"}, {"folder"}))
      ElseIf Path.Type = PathType.File Then
        spn = New HTMLSpan("", New AttributeList({"class"}, {"file"}))
      Else
        Return Nothing
      End If

      Dim a As New HTMLAnchor(Path.PathName, , , , , New AttributeList({"data-message", "data-uncpath"}, {Message, Path.UNCPath}))

      Dim li As HTMLList.ListItem
      If Code = StatusCode.ValidPath Then
        li = New HTMLList.ListItem(spn.Markup & a.Markup, New AttributeList({"class"}, {"valid"}))
      ElseIf Code = StatusCode.InvalidPath Then
        li = New HTMLList.ListItem(spn.Markup & a.Markup, New AttributeList({"class"}, {"invalid"}))
      Else
        li = New HTMLList.ListItem(spn.Markup & a.Markup)
      End If

      Return li
    End Function

    Public Function CreateNewList(ByVal path As PathStructure) As HTMLList
      Return New HTMLList(HTMLList.ListType.Unordered, New AttributeList({"style"}, {"display: none;"}))
    End Function
    ''' <summary>
    ''' Adds the raw markup of a list to the innerHTML of a list item.
    ''' </summary>
    ''' <param name="LI"></param>
    ''' <param name="UL"></param>
    ''' <remarks></remarks>
    Public Sub AddListToListItem(ByRef LI As HTMLList.ListItem, ByVal UL As HTMLList)
      LI.AddInnerHTML(UL.Markup)
    End Sub
    ''' <summary>
    ''' Adds the list item to the provided list. If not list is provided, the main list will be used.
    ''' </summary>
    ''' <param name="UL"></param>
    ''' <param name="LI"></param>
    ''' <remarks></remarks>
    Public Sub AddListItemToList(ByRef UL As HTMLList, ByVal LI As HTMLList.ListItem)
      If IsNothing(UL) Then
        UL = fileSystem
      End If
      UL.AddListItem(LI)
    End Sub

    ''' <summary>
    ''' Appends to the current instance of an AuditReport
    ''' </summary>
    ''' <param name="HTMLMarkup">Raw HTML markup string</param>
    ''' <remarks></remarks>
    Public Sub Raw(ByVal HTMLMarkup As String)
      _report += HTMLMarkup
    End Sub

    Public Sub Audit()
      Dim found As Boolean = True '' Determines whether any valid locations were found, assume false
      Dim cand As StructureCandidate

      Dim li As HTMLList.ListItem
      Dim ul As HTMLList

      _path.IsNameStructured() '' Don't run in logic check because we're applying our own logic
      cand = _path.StructureCandidates.GetHighestMatch()
      If cand IsNot Nothing Then
        If _path.Variables.ContainsName(cand.PathName) Then '' Check that this path needs to be verified, then verify it in the ERP system.
          If Not _path.Variables.IsValid(cand.StructurePath) Then
            li = Report("'" & _path.UNCPath & "' was not valid in the ERP system",
                          AuditVisualReport.StatusCode.InvalidPath,
                          _path)
            found = False
          End If
        End If
        If cand.MatchPercentage = 100 Or _path.StructureCandidates.Count = 1 And found Then
          li = Report("'" & cand.ObjectPath & "' matched " & cand.MatchPercentage.ToString & "%. '" & cand.StructurePath & "':" & cand.StructureDescription,
                        AuditVisualReport.StatusCode.ValidPath,
                        _path)
        Else
          li = Report("'" & cand.ObjectPath & "' matched " & cand.MatchPercentage.ToString & "%, but there were too many similar paths '" & SurroundJoin(_path.StructureCandidates.ToArray, " {", "} ") & "'",
                        AuditVisualReport.StatusCode.Other,
                        _path)
        End If
      Else
        li = Report("'" & _path.UNCPath & "' does not adhere to any paths.",
                      AuditVisualReport.StatusCode.InvalidPath,
                      _path)
      End If

      '' Check status of children paths
      If Not IsNothing(_path.Children) And found Then

        ul = CreateNewList(_path)
        For i = 0 To _path.Children.Count - 1 Step 1
          Dim cli As HTMLList.ListItem
          FileCount += 1
          '' Check if user wants Thumbs.Db deleted
          If My.Settings.blnDeleteThumbsDb Then
            If _path.Children(i).Type = PathType.File And String.Equals(_path.Children(i).PathName, "thumbs", StringComparison.OrdinalIgnoreCase) And String.Equals(_path.Children(i).Extension, ".db", StringComparison.OrdinalIgnoreCase) Then
              IO.File.Delete(_path.Children(i).UNCPath)
              cli = Report("Deleted Thumbs.Db from '" & _path.Children(i).UNCPath & "'.",
                            AuditVisualReport.StatusCode.Other,
                            _path.Children(i))
              AddListItemToList(ul, cli)
              Continue For
            End If
          End If
          AuditVisualChildren(ul, _path.Children(i))
          RaiseEvent ChildAudited(New AuditedEventArgs(Me, _path.Children(i).UNCPath, i, _path.Children.Count))
          If _quit Then
            '' Add final objects
            Exit For
          End If
        Next
        AddListToListItem(li, ul)
      End If

      '' Add item to main list
      AddListItemToList(Nothing, li)
    End Sub
    Private Sub AuditVisualChildren(ByRef ParentList As HTMLList, ByVal Child As PathStructure)
      Dim found As Boolean = True '' Determines whether any valid locations were found, assume false
      Dim cand As StructureCandidate

      Dim li As HTMLList.ListItem
      Dim ul As HTMLList

      Child.IsNameStructured() '' Don't run in logic check because we're applying our own logic
      cand = Child.StructureCandidates.GetHighestMatch()
      If cand IsNot Nothing Then
        If _path.Variables.ContainsName(cand.PathName) Then
          If Not Child.Variables.IsValid(cand.StructurePath) Then
            '' Check that this path needs to be verified, then verify it in the ERP system.

            li = Report("'" & Child.UNCPath & "' was not valid in the ERP system",
                          AuditVisualReport.StatusCode.InvalidPath,
                          Child)
            found = False
          End If
        End If
        If cand.MatchPercentage = 100 Or Child.StructureCandidates.Count = 1 And found Then
          li = Report("'" & cand.ObjectPath & "' matched " & cand.MatchPercentage.ToString & "% '" & cand.StructurePath & "':" & cand.StructureDescription,
                        AuditVisualReport.StatusCode.ValidPath,
                        Child)
        Else
          li = Report("'" & cand.ObjectPath & "' matched " & cand.MatchPercentage.ToString & "%, but there were too many similar paths '" & SurroundJoin(Child.StructureCandidates.ToArray, " {", "} ") & "'",
                        AuditVisualReport.StatusCode.Other,
                        Child)
        End If
      Else
        li = Report("'" & Child.UNCPath & "' does not adhere to any paths.",
                      AuditVisualReport.StatusCode.InvalidPath,
                      Child)
      End If

      If Child.Children IsNot Nothing And found Then
        ul = CreateNewList(Child)
        For i = 0 To Child.Children.Length - 1 Step 1
          Dim cli As HTMLList.ListItem
          '' Check if user wants Thumbs.Db deleted
          FileCount += 1
          If My.Settings.blnDeleteThumbsDb Then
            If Child.Type = PathType.File And String.Equals(Child.PathName, "thumbs", StringComparison.OrdinalIgnoreCase) And String.Equals(Child.Extension, ".db", StringComparison.OrdinalIgnoreCase) Then
              IO.File.Delete(Child.Children(i).UNCPath)
              cli = Report("Deleted Thumbs.Db from '" & Child.Children(i).UNCPath & "'.",
                            AuditVisualReport.StatusCode.Other,
                            Child.Children(i))
              AddListItemToList(ul, cli)
              Continue For
            End If
          End If
          '' Setup thread
          'If System.Math.Round((My.Computer.Info.AvailablePhysicalMemory / (1024 * 1024)), 2) > 2000 Then
          '  Dim thrd As New System.Threading.Thread(AddressOf ThreadGrandChildren)
          '  thrd.Start({ul, Child.Children(i), i, Child.Children.Count})
          'Else
          AuditVisualChildren(ul, Child.Children(i))
          RaiseEvent GrandChildAudited(New AuditedEventArgs(Me, Child.Children(i).UNCPath, i, Child.Children.Count))
          'End If
          If _quit Then
            '' Add final objects and exit
            Exit For
          End If
        Next
        AddListToListItem(li, ul)
      End If

      '' Add item to main list
      AddListItemToList(ParentList, li)
    End Sub
    Private Sub ThreadGrandChildren(ByVal params As Object)
      AuditVisualChildren(params(0), params(1))
      RaiseEvent GrandChildAudited(New AuditedEventArgs(Me, params(1).UNCPath, params(2), params(3)))
    End Sub

    Public Sub Quit()
      _quit = True
    End Sub
  End Class

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        ' TODO: dispose managed state (managed objects).
        _path = String.Empty
        _infoFile = Nothing
        _infoFolder = Nothing
        _defaultPath = String.Empty
        _startPath = String.Empty
        _variables = Nothing
        _parent = Nothing
        _children = Nothing
      End If

      ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
      ' TODO: set large fields to null.
    End If
    Me.disposedValue = True
  End Sub

  ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
  Protected Overrides Sub Finalize()
    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    Dispose(False)
    MyBase.Finalize()
  End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region

End Class
