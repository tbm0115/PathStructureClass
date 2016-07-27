Imports System.Xml, System.IO, System.Security.AccessControl, System.Text
Imports HTML, HTML.HTMLWriter, HTML.HTMLWriter.HTMLTable
Imports System.Data, System.Data.OleDb, System.Data.Sql, System.Data.SqlClient
Imports DSOFile

Public Class PathStructure
  Private _ERPCheck As Boolean = False
  Private _myXML As XmlDocument
  Private _DeleteThumbs As Boolean = False
  Private _HandleExtensions As Boolean = False
  Private _generateIcons As Boolean = False
  Private _setPermissions As Boolean = False
  Private _ERPConnection As String
  Public defaultPaths As New List(Of String)
  Private _structs As List(Of StructureStyle)
  Public ERPConnection As DatabaseConnection ' New DatabaseConnection(My.Settings.ERPConnection)
  Private _xmlPath As String

  Public ReadOnly Property SettingsPath As String
    Get
      Return _xmlPath
    End Get
  End Property
  Public Property CheckERPSystem As Boolean
    Get
      Return _ERPCheck
    End Get
    Set(value As Boolean)
      _ERPCheck = value
    End Set
  End Property
  Public Property Settings As XmlDocument
    Get
      Return _myXML
    End Get
    Set(value As XmlDocument)
      _myXML = value
    End Set
  End Property
  Public Property AllowDeletionOfThumbsDb As Boolean
    Get
      Return _DeleteThumbs
    End Get
    Set(value As Boolean)
      _DeleteThumbs = value
    End Set
  End Property
  Public Property ERPSystemConnectionString As String
    Get
      Return _ERPConnection
    End Get
    Set(value As String)
      _ERPConnection = value
    End Set
  End Property
  ''' <summary>
  ''' Sets whether or not a Path object will handle its extension during the IsNamedStructure() routine.
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Property HandleExtensions As Boolean
    Get
      Return _HandleExtensions
    End Get
    Set(value As Boolean)
      _HandleExtensions = value
    End Set
  End Property
  Public Property CanGenerateIcons As Boolean
    Get
      Return _generateIcons
    End Get
    Set(value As Boolean)
      _generateIcons = value
    End Set
  End Property
  Public Property CanSetPermissions As Boolean
    Get
      Return _setPermissions
    End Get
    Set(value As Boolean)
      _setPermissions = value
    End Set
  End Property

  Public Sub New(ByVal SettingsDocument As String,
                 Optional ByVal ERPConnectionString As String = "",
                 Optional ByVal ERPCheck As Boolean = False,
                 Optional ByVal DeleteThumbsDb As Boolean = False,
                 Optional ByVal ProcessExtensions As Boolean = False,
                 Optional ByVal GenerateIcons As Boolean = False,
                 Optional ByVal SetPermissions As Boolean = False)
    _xmlPath = SettingsDocument
    _myXML = New XmlDocument
    _myXML.Load(_xmlPath)
    If _myXML Is Nothing Then
      Throw New ArgumentException("PathStructure: XML Settings cannot be nothing!")
    Else
      '' Fix the settings file
      '' Remove any tmpURI's
      Dim attrs As XmlNodeList = _myXML.SelectNodes("//*[@tmpURI]")
      If attrs.Count > 0 Then
        For i = attrs.Count - 1 To 0 Step -1
          attrs(i).Attributes.Remove(attrs(i).Attributes.ItemOf("tmpURI"))
        Next
      End If
      '' Fix any links
      Dim lst As XmlNodeList = _myXML.SelectNodes("//Folder[@link]")
      If lst.Count > 0 Then
        For i = 0 To lst.Count - 1 Step 1
          If lst(i).Attributes("link") IsNot Nothing Then
            '' Re-add the childnodes (they could technically be different after all!
            Dim fold As XmlElement = _myXML.SelectSingleNode(lst(i).Attributes("link").Value).CloneNode(True)

            '' Remove childnodes
            If lst(i).HasChildNodes Then
              lst(i).InnerXml = ""
            End If
            If fold IsNot Nothing Then
              lst(i).AppendChild(fold)
            End If
          End If
        Next
      End If
      _myXML.Save(_xmlPath)

      _structs = New List(Of StructureStyle)
      For Each struct As XmlElement In _myXML.SelectNodes("//Structure")
        defaultPaths.Add(struct.Attributes("defaultPath").Value)
        _structs.Add(New StructureStyle(Me, struct))
      Next
    End If

    _ERPConnection = ERPConnectionString
    ERPConnection = New DatabaseConnection(_ERPConnection)
    _ERPCheck = ERPCheck

    _DeleteThumbs = DeleteThumbsDb
    _HandleExtensions = ProcessExtensions
    _generateIcons = GenerateIcons
    _setPermissions = SetPermissions
  End Sub

  Public Function IsInDefaultPath(ByVal Input As String, Optional ByVal PreferredPath As String = "") As Boolean
    If Not String.IsNullOrEmpty(Input) Then
      For i = 0 To defaultPaths.Count - 1 Step 1
        If (Input.IndexOf(defaultPaths(i), System.StringComparison.OrdinalIgnoreCase) >= 0) Or (defaultPaths(i).IndexOf(Input, System.StringComparison.OrdinalIgnoreCase) >= 0) Then
          If Not String.IsNullOrEmpty(PreferredPath) And Not String.Equals(defaultPaths(i), PreferredPath) Then
            Continue For
          End If
          Return True
        End If
      Next
    End If
    Return False
  End Function

  ''' <summary>
  ''' Replaces PathStructure variables with provided values.
  ''' </summary>
  ''' <param name="Input">Full or partial path string</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Function ReplaceVariables(ByVal Input As String, ByVal Path As String) As String
    Dim vars As New VariableArray("//Variables", New Path(Me, Path))
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
      Dim x As XmlElement = _myXML.SelectSingleNode(XPath)
      '' Check if the element has the temporary URI for this session. If so, use it
      If x.HasAttribute("tmpURI") Then
        Return x.Attributes("tmpURI").Value
      Else
        Dim a As XmlAttribute = _myXML.CreateAttribute("tmpURI")
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
    End If
    Return ""
  End Function
  ''' <summary>
  ''' Converts the XPath for the PathStructure into a valid FileSystem path.
  ''' </summary>
  ''' <param name="XElement">XML Element</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Function GetURIfromXPath(ByVal XElement As XmlElement) As String
    If XElement IsNot Nothing Then
      '' Check if the element has the temporary URI for this session. If so, use it
      If XElement.HasAttribute("tmpURI") Then
        Return XElement.Attributes("tmpURI").Value
      Else
        Dim a As XmlAttribute = _myXML.CreateAttribute("tmpURI")
        XElement.Attributes.Append(a)
        Dim u As New StringBuilder
        Do Until XElement.Name = "Structure"
          If String.Equals(XElement.Name, "Folder", StringComparison.OrdinalIgnoreCase) Then
            u.Insert(0, XElement.Attributes("name").Value & "\")
          ElseIf String.Equals(XElement.Name, "File", StringComparison.OrdinalIgnoreCase) Then
            u.Append(XElement.InnerText)
          ElseIf String.Equals(XElement.Name, "Option", StringComparison.OrdinalIgnoreCase) Then
            u.Append(XElement.InnerText)
            XElement = XElement.ParentNode '' Set parent node to 'File' so that the next x-set will set x to the folder
          End If
          XElement = XElement.ParentNode
        Loop
        'u = _startPath & u
        u.Insert(0, XElement.Attributes("defaultPath").Value & "\") ' & x.Attributes("path").Value & "\")
        a.Value = u.ToString
        Return u.ToString
      End If
    End If
    Return ""
  End Function
  ''' <summary>
  ''' Gets the value of the Description attribute from the given XPath element
  ''' </summary>
  ''' <param name="XPath"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetDescriptionfromXPath(ByVal XPath As String) As String
    If Not String.IsNullOrEmpty(XPath) Then
      Dim x As XmlElement = _myXML.SelectSingleNode(XPath)
      If Not IsNothing(x) Then
        If x.HasAttribute("description") Then
          Return x.Attributes("description").Value
        End If
      End If
    End If
    Return ""
  End Function
  ''' <summary>
  ''' Gets the value of the Description attribute from the given XmlElement
  ''' </summary>
  ''' <param name="XElement"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetDescriptionfromXPath(ByVal XElement As XmlElement) As String
    If XElement IsNot Nothing Then
      If XElement.HasAttribute("description") Then
        Return XElement.Attributes("description").Value
      End If
    End If
    Return ""
  End Function

  Public Function FindStructureStyle(ByVal UNCPath As String) As StructureStyle
    For i = 0 To _structs.Count - 1 Step 1
      If UNCPath.IndexOf(_structs(i).DefaultPath, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        Return _structs(i)
      End If
    Next
    Return Nothing
  End Function

  Public Class StructureStyle
    Private _struct As PathStructure
    Private _x As XmlElement
    Private _chld As List(Of PathStyle)

    Public Property Name As String
      Get
        If _x.HasAttribute("name") Then
          Return _x.Attributes("name").Value
        Else
          Return ""
        End If
      End Get
      Set(value As String)
        If Not _x.HasAttribute("name") Then
          Dim _attr As XmlAttribute = _struct.Settings.CreateAttribute("name")
          _x.Attributes.Append(_attr)
        End If
        _x.Attributes("name").Value = value
      End Set
    End Property
    Public Property DefaultPath As String
      Get
        If _x.HasAttribute("defaultPath") Then
          Return _x.Attributes("defaultPath").Value
        Else
          Return ""
        End If
      End Get
      Set(value As String)
        If Not _x.HasAttribute("defaultPath") Then
          Dim _attr As XmlAttribute = _struct.Settings.CreateAttribute("defaultPath")
          _x.Attributes.Append(_attr)
        End If
        _x.Attributes("defaultPath").Value = value
      End Set
    End Property
    Public ReadOnly Property Children As PathStyle()
      Get
        If _chld Is Nothing Then
          _chld = New List(Of PathStyle)
          Dim nds As XmlNodeList = _x.ChildNodes
          For i = 0 To nds.Count - 1 Step 1
            _chld.Add(New PathStyle(_struct, nds(i)))
          Next
        End If
        Return _chld.ToArray
      End Get
    End Property
    Public ReadOnly Property XElement As XmlElement
      Get
        Return _x
      End Get
    End Property

    Public Sub New(ByVal PathStruct As PathStructure, ByVal Node As XmlElement)
      _struct = PathStruct
      _x = Node
    End Sub

    Public Function GetStyle(ByVal XPath As String) As PathStyle
      Dim pth As PathStyle
      For i = 0 To _chld.Count - 1 Step 1
        pth = RecursiveGetStyle(XPath, _chld(i))
        If pth IsNot Nothing Then Exit For
      Next
      If pth Is Nothing Then
        Dim nds As XmlNodeList = _x.SelectNodes(XPath)
        If nds IsNot Nothing Then
          If nds.Count = 1 Then
            pth = New PathStyle(_struct, nds(0))
          End If
        End If
      End If
      Return pth
    End Function
    Private Function RecursiveGetStyle(ByVal XPath As String, ByVal Item As PathStyle) As PathStyle
      If Item.Children.Length > 0 Then
        For i = 0 To Item.Children.Length - 1 Step 1
          If String.Equals(Item.Children(i).XPath, XPath, StringComparison.OrdinalIgnoreCase) Then
            Return Item.Children(i)
          Else
            Return RecursiveGetStyle(XPath, Item.Children(i))
          End If
        Next
      End If
      Return Nothing
    End Function
  End Class
  Public Class PathStyle
    Private _struct As PathStructure
    Private _x As XmlElement
    Private _xpath, _uri As String
    Private _type As PathStyleType
    Private _parent As PathStyle
    Private _chld As List(Of PathStyle)

    Public ReadOnly Property XElement As XmlElement
      Get
        Return _x
      End Get
    End Property
    Public Property Name As String
      Get
        If _x.HasAttribute("name") Then
          Return _x.Attributes("name").Value
        Else
          Return ""
        End If
      End Get
      Set(value As String)
        If Not _x.HasAttribute("name") Then
          Dim _attr As XmlAttribute = _struct.Settings.CreateAttribute("name")
          _x.Attributes.Append(_attr)
        End If
        _x.Attributes("name").Value = value

        '' Reset fragile properties
        _xpath = Nothing
        _uri = Nothing
      End Set
    End Property
    Public Property Description As String
      Get
        If _x.HasAttribute("description") Then
          Return _x.Attributes("description").Value
        Else
          Return ""
        End If
      End Get
      Set(value As String)
        If Not _x.HasAttribute("description") Then
          Dim _attr As XmlAttribute = _struct.Settings.CreateAttribute("description")
          _x.Attributes.Append(_attr)
        End If
        _x.Attributes("description").Value = value

        '' Reset fragile properties
        _xpath = Nothing
        _uri = Nothing
      End Set
    End Property
    Public Property IsFreeForm As Boolean
      Get
        If _x.HasAttribute("freeform") Then
          Return Convert.ToBoolean(_x.Attributes("freeform").Value)
        Else
          Return False
        End If
      End Get
      Set(value As Boolean)
        If Not _x.HasAttribute("freeform") Then
          Dim _attr As XmlAttribute = _struct.Settings.CreateAttribute("freeform")
          _x.Attributes.Append(_attr)
        End If
        _x.Attributes("freeform").Value = value.ToString
      End Set
    End Property
    Public ReadOnly Property HasIcon As Boolean
      Get
        If _type = PathStyleType.FolderStyle Then
          If _x.HasAttribute("icon") And _x.HasAttribute("iconindex") Then
            Return True
          End If
        End If
        Return False
      End Get
    End Property
    Public ReadOnly Property HasPreview As Boolean
      Get
        If _type = PathStyleType.FolderStyle Then
          If _x.HasAttribute("preview") Then
            Return True
          End If
        End If
        Return False
      End Get
    End Property
    Public ReadOnly Property HasPermissionsGroup As Boolean
      Get
        If _type = PathStyleType.FolderStyle Then
          If _x.HasAttribute("permissionsgroup") Then
            Return True
          End If
        End If
        Return False
      End Get
    End Property
    Public ReadOnly Property XPath As String
      Get
        If String.IsNullOrEmpty(_xpath) Then
          _xpath = _x.FindXPath()
        End If
        Return _xpath
      End Get
    End Property
    Public ReadOnly Property URI As String
      Get
        If String.IsNullOrEmpty(_uri) Then
          _uri = _struct.GetURIfromXPath(Me.XPath)
        End If
        Return _uri
      End Get
    End Property

    Public ReadOnly Property Parent As PathStyle
      Get
        If _parent Is Nothing Then
          If _x.ParentNode IsNot Nothing Then
            _parent = New PathStyle(_struct, _x.ParentNode)
          End If
        End If
        Return _parent
      End Get
    End Property
    Public ReadOnly Property Children As PathStyle()
      Get
        If _chld Is Nothing Then
          _chld = New List(Of PathStyle)
          Dim nds As XmlNodeList = _x.ChildNodes
          For i = 0 To nds.Count - 1 Step 1
            _chld.Add(New PathStyle(_struct, nds(i)))
          Next
        End If
        Return _chld.ToArray
      End Get
    End Property

    Public Enum PathStyleType
      FolderStyle
      FileStyle
    End Enum

    Public Sub New(ByVal PathStruct As PathStructure, ByVal Node As XmlElement)
      _struct = PathStruct
      _x = Node

      If String.Equals(_x.Name, "Folder", StringComparison.OrdinalIgnoreCase) Then
        _type = PathStyleType.FolderStyle
      ElseIf String.Equals(_x.Name, "File", StringComparison.OrdinalIgnoreCase) Then
        _type = PathStyleType.FileStyle
      ElseIf String.Equals(_x.Name, "Option", StringComparison.OrdinalIgnoreCase) Then
        _type = PathStyleType.FileStyle
      End If
    End Sub

    Public Function GetExtensions() As Extensions
      Return New Extensions(_struct, _x)
    End Function
    Public Function GetIconFile() As String
      If _type = PathStyleType.FileStyle Then
        If _x.HasAttribute("icon") Then
          Return _x.Attributes("icon").Value
        End If
      End If
      Return ""
    End Function
    Public Function GetIconIndex() As String
      If _type = PathStyleType.FileStyle Then
        If _x.HasAttribute("iconindex") Then
          Return _x.Attributes("iconindex").Value
        End If
      End If
      Return ""
    End Function
    Public Function GetPermissionsGroup() As String
      If _type = PathStyleType.FileStyle Then
        If _x.HasAttribute("permissionsgroup") Then
          Return _x.Attributes("permissionsgroup").Value
        End If
      End If
      Return ""
    End Function
  End Class
End Class

''' <summary>
''' Represents a real Filesystem path and compares it to Path Structure
''' </summary>
''' <remarks></remarks>
Public Class Path : Implements IDisposable

#Region "Private Variables"
  Private _type As PathType
  Private _path As String
  Private Shared _struct As XmlElement
  Private _infoFile As IO.FileInfo
  Private _infoFolder As IO.DirectoryInfo
  Private Shared myXML As XmlDocument
  Private _variables As VariableArray
  Private _parent As Path
  Private _children As Path()
  Private _candidates As StructureCandidateArray
  Private Shared _pstruct As PathStructure
  Private _structStyle As PathStructure.StructureStyle
  Private _pathStyle As PathStructure.PathStyle
  Private _exts As Extensions
  Private _users As Users
#End Region

#Region "Properties"
  ''' <summary>
  ''' Returns the current Path's Path Structure reference.
  ''' </summary>
  ''' <value></value>
  ''' <returns>PathStructure</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property PStructure As PathStructure
    Get
      Return _pstruct
    End Get
  End Property
  ''' <summary>
  ''' Returns the Path of the current Path's parent directory.
  ''' </summary>
  ''' <value></value>
  ''' <returns>Path</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Parent As Path
    Get
      If IsNothing(_parent) Then
        Dim chk As Boolean = True
        _parent = New Path(_pstruct, Me.ParentPath, PathType.Folder, chk)
        If Not chk Then _parent = Nothing
      End If
      Return _parent
    End Get
  End Property
  ''' <summary>
  ''' Returns the UNC path of the current Path's parent directory
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property ParentPath As String
    Get
      Dim splt As String() = _path.Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      If splt.Length > 1 Then
        ReDim Preserve splt(splt.Length - 2)
        Return "\\" & String.Join("\", splt)
      Else
        Return ""
      End If
    End Get
  End Property
  ''' <summary>
  ''' Returns the Path objects the represent the current Path's child filesystem objects.
  ''' </summary>
  ''' <value></value>
  ''' <returns>Path()</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Children As Path()
    Get
      If _type = PathType.Folder Then
        If IsNothing(_children) Then
          Dim arr As New List(Of Path)
          Dim chk As Boolean
          Try
            For Each fil As String In IO.Directory.EnumerateFiles(_path)
              chk = True
              Dim tmp As New Path(_pstruct, fil, PathType.File, chk)
              If chk Then
                arr.Add(tmp)
              End If
            Next
            For Each fol As String In IO.Directory.EnumerateDirectories(_path)
              chk = True
              Dim tmp As New Path(_pstruct, fol, PathType.Folder, chk)
              If chk Then
                arr.Add(tmp)
              End If
            Next
            _children = arr.ToArray
          Catch ex As Exception
            Log("PathStructure: FSO child enumeration error. " & ex.Message)
          End Try
        End If
        Return _children
      Else
        Return Nothing
      End If
    End Get
  End Property
  ''' <summary>
  ''' Returns the IO.FileSystemInfo of a Path.PathType.File, returns nothing otherwise
  ''' </summary>
  ''' <value></value>
  ''' <returns>IO.FileSystemInfo</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property FileInfo As IO.FileSystemInfo
    Get
      Return _infoFile
    End Get
  End Property
  ''' <summary>
  ''' Returns the IO.FileSystemInfo of a Path.PathType.Folder, returns nothing otherwise
  ''' </summary>
  ''' <value></value>
  ''' <returns>IO.FileSystemInfo</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property FolderInfo As IO.FileSystemInfo
    Get
      Return _infoFolder
    End Get
  End Property
  ''' <summary>
  ''' Gets a list of variables in the current path and their values.
  ''' </summary>
  ''' <value></value>
  ''' <returns>VariableArray</returns>
  ''' <remarks></remarks>
  Public Property Variables As VariableArray
    Get
      Return _variables
    End Get
    Set(value As VariableArray)
      _variables = value
    End Set
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
  ''' Returns the name of the current Path. For PathType.File, returns the filename. For PathType.Folder, returns the folder name.
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
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
  ''' <summary>
  ''' Returns the extension of a Path of PathType.File, returns an empty string otherwise
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Extension As String
    Get
      If _type = PathType.File Then
        Return _infoFile.Extension
      Else
        Return ""
      End If
    End Get
  End Property
  ''' <summary>
  ''' Returns an Extensions object that manages a list of potential extensions for the current structured Path.
  ''' </summary>
  ''' <value></value>
  ''' <returns>Extensions</returns>
  ''' <remarks></remarks>
  Public Property Extensions As Extensions
    Get
      Return _exts
    End Get
    Set(value As Extensions)
      _exts = value
    End Set
  End Property
  ''' <summary>
  ''' Returns the XmlElement of the current Path's PathStructure reference in the XmlDocument settings.
  ''' </summary>
  ''' <value></value>
  ''' <returns>XmlElement</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property PathStructure As XmlElement
    Get
      Return _struct
    End Get
  End Property
  ''' <summary>
  ''' Returns a StructureCandidateArray object that manages a list of potential PathStructure candidates for the current Path.
  ''' </summary>
  ''' <value></value>
  ''' <returns>StructureCandidateArray</returns>
  ''' <remarks>This array helps determine which parts of the PathStructure the current Path matches.</remarks>
  Public ReadOnly Property StructureCandidates As StructureCandidateArray
    Get
      If _candidates Is Nothing Then IsNameStructured()
      Return _candidates
    End Get
  End Property
  ''' <summary>
  ''' Returns a Users object that manages a list of User permissions in case security settings must be applied.
  ''' </summary>
  ''' <value></value>
  ''' <returns>Users</returns>
  ''' <remarks></remarks>
  Public Property Users As Users
    Get
      Return _users
    End Get
    Set(value As Users)
      _users = value
    End Set
  End Property
  ''' <summary>
  ''' Returns a StructureStyle object that is set only if there is a perfect match of the current path with a default structure path.
  ''' </summary>
  ''' <value></value>
  ''' <returns>PathStructure.StructureStyle</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property StructureStyle As PathStructure.StructureStyle
    Get
      Return _structStyle
    End Get
  End Property
  ''' <summary>
  ''' Returns a PathStyle object that is set only if there is a perfect match of the current path with a single path structure.
  ''' </summary>
  ''' <value></value>
  ''' <returns>PathStructure.PathStyle</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property PathStyle As PathStructure.PathStyle
    Get
      Return _pathStyle
    End Get
  End Property
#End Region

#Region "Enumerations"
  ''' <summary>
  ''' Represents the FileSystem object type.
  ''' </summary>
  ''' <remarks></remarks>
  Public Enum PathType
    File
    Folder
  End Enum
#End Region

#Region "Overrides"
  ''' <summary>
  ''' Returns the UNC path of the current Path.
  ''' </summary>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Overrides Function ToString() As String
    Return _path
  End Function
  ''' <summary>
  ''' Compares the current Path's UNC path against the sender's UNC path.
  ''' </summary>
  ''' <param name="obj">Path or UNC path (as System.String)</param>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Overrides Function Equals(obj As Object) As Boolean
    Return String.Equals(obj.ToString, _path, StringComparison.OrdinalIgnoreCase)
  End Function
#End Region

  Public Sub New(ByVal PStructure As PathStructure,
                 ByVal Path As String,
                 Optional ByVal SetType As Path.PathType = Nothing,
                 Optional ByRef Successful As Boolean = Nothing)
    _pstruct = PStructure
    '' Set path
    If String.IsNullOrEmpty(Path) Then
      Successful = False
      Exit Sub
    End If
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
      myXML = _pstruct.Settings
    Else
      myXML = myXML
    End If
    '' Find relevant Structure
    _structStyle = _pstruct.FindStructureStyle(_path)
    _struct = _structStyle.XElement

    'Dim structs As XmlNodeList = myXML.SelectNodes("//Structure")
    'For i = 0 To structs.Count - 1 Step 1
    '  If _path.IndexOf(structs(i).Attributes("defaultPath").Value, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
    '    _struct = structs(i)
    '    Exit For
    '  End If
    'Next
    'If _struct Is Nothing Then Throw New ArgumentException("PathStructure: Couldn't determine the default Structure node from '" & _path & "'. Searched " & structs.Count.ToString & " Structures in XmlDocument.")

    '_defaultPath = _struct.Attributes("path").Value
    If _struct.SelectSingleNode("Users") IsNot Nothing Then
      _users = New Users(_struct.SelectSingleNode("Users"))
    End If

    'Dim defSeparator As Integer = CountStringOccurance(_defaultPath, IO.Path.DirectorySeparatorChar)
    '' Enumerate variables
    _variables = New VariableArray(_struct.SelectSingleNode("Variables"), Me)

    '' Set Start path
    '_startPath = _defaultPath & "\" & Variables.Replace(_struct.Attributes("path").Value)

    '' Compare to Path Structure
    IsNameStructured()
  End Sub

  ''' <summary>
  ''' Determines if the current instance of a Path is a descendant of the DefaultPath (or root directory)
  ''' </summary>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Function IsDescendantOfDefaultPath() As Boolean
    'If Not String.IsNullOrEmpty(_path) Then
    '  If _path.IndexOf(_defaultPath, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
    '    Return True
    '  End If
    'End If
    'Return False
    Return (_structStyle IsNot Nothing)
  End Function

  ''' <summary>
  ''' Determines whether the current path is part of the Path Structure format.
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function IsNameStructured() As Boolean
    Static blnFound As Boolean = False
    If blnFound Then Return blnFound '' Dont run this routine more than once

    '' Initialize candidates
    _candidates = New StructureCandidateArray(Me)

    Dim strTemp As String
    Dim searchXPath As String

    '' Create XPath depending on FSO type
    If _type = PathType.File Then
      strTemp = Variables.Replace(ParentPath) ' ReplaceVariables(ParentPath, _path)
      searchXPath = ".//Folder[@name='" & Uri.EscapeDataString(strTemp) & "']/File[not(Option)]|.//Folder[@name='" & Uri.EscapeDataString(strTemp) & "']/File/Option"
      If _struct.SelectNodes(searchXPath).Count <= 0 Then searchXPath = "//File[not(Option)]|.//File/Option"
    ElseIf _type = PathType.Folder Then
      strTemp = Variables.Replace(PathName) ' ReplaceVariables(PathName, _path)
      searchXPath = ".//Folder[@name='" & Uri.EscapeDataString(strTemp) & "']"
      If _struct.SelectNodes(searchXPath).Count <= 0 Then searchXPath = ".//Folder"
    End If

    '' Add default path to match
    _candidates.Add(_struct)

    If Not String.IsNullOrEmpty(searchXPath) Then
      Dim objs As XmlNodeList = _struct.SelectNodes(searchXPath)
      If objs.Count > 0 Then
        _candidates.AddRange(objs) '' Add all of the matching XPaths. Each new object will run a match check.
        _candidates.GetHighestMatch().CheckForLinkVariables() '' Try to fix variables with this
        _candidates.RemoveMismatches(100) '' Remove any mismatches below the threshold of a score of 100% (this excludes mismatches with extensions)
      End If
    Else
      '' This only occurs if the type was not set to either File or Folder
      Throw New ArgumentException("Couldn't determine path type", "Invalid Path Type")
    End If

    '' Check if more than one Candidate, remove wildcards if at least one does not end in wildcard
    If _candidates.Count > 1 Or _candidates.Count = 0 Then
      If _candidates.Count > 1 Then
        '' Try to remove any complete wildcard candidates to make room for non-complete wildcard candidates
        For i = _candidates.Count - 1 To 0 Step -1
          If String.Equals(_candidates(i).XElement.InnerText, "{}", StringComparison.OrdinalIgnoreCase) Then
            _candidates.RemoveAt(i)
          End If
        Next
        If _candidates.Count = 1 Then
          Dim m_file As DSOFile.OleDocumentProperties
          m_file = New DSOFile.OleDocumentProperties
          m_file.Open(_path, True, dsoFileOpenOptions.dsoOptionDefault)
          blnFound = True
        End If
      Else
        blnFound = False
      End If
    ElseIf _candidates.Count = 1 Then
      If _type = PathType.Folder Then
        If _pstruct.CanGenerateIcons Then
          '' If allowed, check if this path has a preferred icon and create the desktop.ini file
          If _candidates(0).XElement.HasAttribute("icon") Then
            Dim desktopINI As String = IO.Path.Combine({_path, "desktop.ini"})
            If Not IO.File.Exists(desktopINI) Then
              Dim iconIndex, iconTip As String
              If _candidates(0).XElement.HasAttribute("iconindex") Then
                iconIndex = _candidates(0).XElement.Attributes("iconindex").Value
              Else
                iconIndex = "0"
              End If
              If _candidates(0).XElement.HasAttribute("icontip") Then
                iconTip = _candidates(0).XElement.Attributes("icontip").Value
              ElseIf _candidates(0).XElement.HasAttribute("description") Then
                iconTip = _candidates(0).XElement.Attributes("description").Value
              End If
              IO.File.WriteAllText(desktopINI, "[.ShellClassInfo]" & vbCrLf & _
                                   "ConfirmFileOp=0" & vbCrLf & _
                                   "IconFile=" & _candidates(0).XElement.Attributes("icon").Value & vbCrLf & _
                                   "IconIndex=" & iconIndex & vbCrLf & _
                                   "InfoTip=" & iconTip,
                                   System.Text.Encoding.Unicode)
              SetAttr(_path, FileAttribute.System)
              SetAttr(desktopINI, FileAttribute.System Or FileAttribute.Hidden)
            End If
          End If
        End If
      End If
      blnFound = True '' Set to true because only one candidate was found
    End If

    Return blnFound
  End Function

  ''' <summary>
  ''' Determines if the current path is valid according to the provided Path Structure node.
  ''' </summary>
  ''' <param name="Folder">The Path Structure node to check against the current path.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Overloads Function IsLocationValid(ByVal Folder As XmlElement) As Boolean
    Return IsLocationValid(New Uri(_pstruct.GetURIfromXPath(FindXPath(Folder))))
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

  Public Function FindNearestArchive(Optional ByVal FocusPath As Path = Nothing) As String
    If FocusPath Is Nothing Then FocusPath = Me
    Log(vbTab & "Finding Archive: " & FocusPath.Type.ToString & vbTab & "'" & FocusPath.UNCPath & "'")
    If FocusPath.Type = PathType.Folder Then
      For i = 0 To FocusPath.Children.Length - 1 Step 1
        If FocusPath.Children(i).PathName.IndexOf("Archive", StringComparison.OrdinalIgnoreCase) >= 0 And FocusPath.Children(i).Type = PathType.Folder Then
          Return FocusPath.Children(i).UNCPath
        End If
      Next
    End If
    If _pstruct.IsInDefaultPath(FocusPath.Parent.UNCPath) Then
      Return FindNearestArchive(FocusPath.Parent)
    Else
      Return Me.ParentPath
    End If
  End Function

  Public Sub LogData(ByVal ChangedPath As String, ByVal Method As String)
    Try
      IO.File.AppendAllText(_structStyle.DefaultPath & "\PathStructure Changes.csv",
                            DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") & "," & _path & "," & ChangedPath & "," & Method & "," & My.User.Name & vbCrLf)
      Log("{LogData} " & Method & ": " & ChangedPath)
    Catch ex As Exception
      Log("Error while appending change log:" & vbCrLf & vbTab & ex.Message)
    End Try
  End Sub

  Public Class AuditVisualReport
    Implements IDisposable

    Private _report As HTML.HTMLWriter
    Private _fileCount As Integer
    Private _errPaths, _optPaths As List(Of String)
    Private fileSystem As HTML.HTMLWriter.HTMLList
    Private _auditpath As Path
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

    Public Sub New(ByVal Path As Path)
      _report = New HTML.HTMLWriter
      _auditpath = Path
      fileSystem = New HTMLList(HTMLList.ListType.Unordered)
      'Log("Audit initialized at '" & _auditpath.UNCPath & "'")
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
    Public Function Report(ByVal Message As String, ByVal Code As StatusCode, ByVal Path As Path) As HTMLList.ListItem
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

    Public Function CreateNewList(ByVal path As Path) As HTMLList
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
      'Log("Audit began")
      Dim found As Boolean = True '' Determines whether any valid locations were found, assume false
      Dim cand As StructureCandidate

      Dim li As HTMLList.ListItem
      Dim ul As HTMLList

      _auditpath.IsNameStructured() '' Don't run in logic check because we're applying our own logic
      cand = _auditpath.StructureCandidates.GetHighestMatch()
      If cand IsNot Nothing Then
        '' Because a solid candidate was found, we should re-initialize the current path's variables by calling "CheckVariables"
        cand.CheckForLinkVariables()
        If _auditpath.Variables.ContainsName(cand.PathName) Then
          If Not _auditpath.Variables.IsValid(cand.StructurePath) Then
            '' Check that this path needs to be verified, then verify it in the ERP system.

            li = Report("'" & _auditpath.UNCPath & "' was not valid in the ERP system",
                          AuditVisualReport.StatusCode.InvalidPath,
                          _auditpath)
            Dim out As New StringBuilder()
            For z = 0 To _auditpath.Variables.Count - 1 Step 1
              out.AppendLine(_auditpath.Variables(z).Name & "=" & _auditpath.Variables(z).Value)
            Next
            'Log("'" & _auditpath.UNCPath & "' not found in ERP system. Variables: " & vbLf & out.ToString)
            found = False
          End If
        End If
        If found Then
          If cand.MatchPercentage = 100 Then
            li = Report("'" & cand.UNCPath & "' matched " & cand.MatchPercentage.ToString & "% '" & cand.StructurePath & "':" & cand.StructureDescription,
                          AuditVisualReport.StatusCode.ValidPath,
                          _auditpath)
          Else
            li = Report("'" & cand.UNCPath & "' matched " & cand.MatchPercentage.ToString & "%, but was not high enough. Here are all of the candidates '" & SurroundJoin(_auditpath.StructureCandidates.ToArray, " {", "} ") & "'",
                          AuditVisualReport.StatusCode.Other,
                          _auditpath)
          End If
        End If
        If _auditpath.Type = PathType.Folder Then
          '' If the folder is a "FreeForm" folder (meaning that it can have whatever files/folders) then leave it alone
          If cand.XElement.HasAttribute("freeform") Then
            If String.Equals(cand.XElement.Attributes("freeform").Value, "true", StringComparison.OrdinalIgnoreCase) Then
              found = False '' This will allow iteration of child objects to be skipped.
              'Log("Freeform folder found at '" & _auditpath.UNCPath & "'. Force exiting the audit of this directory")
            End If
          End If
        End If
      Else
        li = Report("'" & _auditpath.UNCPath & "' does not adhere to any paths.",
                      AuditVisualReport.StatusCode.InvalidPath,
                      _auditpath)
        found = False
      End If

      '' Check status of children paths
      If Not IsNothing(_auditpath.Children) And found Then
        ul = CreateNewList(_auditpath)
        For i = 0 To _auditpath.Children.Length - 1 Step 1
          Dim cli As HTMLList.ListItem
          FileCount += 1
          '' Check if user wants Thumbs.Db deleted
          If _pstruct.AllowDeletionOfThumbsDb Then ' My.Settings.blnDeleteThumbsDb Then
            If _auditpath.Children(i).Type = PathType.File And _auditpath.Children(i).PathName.IndexOf("thumbs", StringComparison.OrdinalIgnoreCase) >= 0 And _auditpath.Children(i).Extension.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0 Then
              IO.File.Delete(_auditpath.Children(i).UNCPath)
              cli = Report("Deleted Thumbs.Db from '" & _auditpath.Children(i).UNCPath & "'.",
                            AuditVisualReport.StatusCode.Other,
                            _auditpath.Children(i))
              AddListItemToList(ul, cli)
              Continue For
            End If
          End If
          If _auditpath.Children(i).Type = PathType.File And _auditpath.Children(i).PathName.IndexOf("desktop", StringComparison.OrdinalIgnoreCase) >= 0 And _auditpath.Children(i).Extension.IndexOf(".ini", StringComparison.OrdinalIgnoreCase) >= 0 Then
            '' Ignore these files because we sometimes create them!
            Continue For
          End If
          AuditVisualChildren(ul, _auditpath.Children(i))
          RaiseEvent ChildAudited(New AuditedEventArgs(Me, _auditpath.Children(i).UNCPath, i, _auditpath.Children.Length))
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
    Private Sub AuditVisualChildren(ByRef ParentList As HTMLList, ByVal Child As Path)
      'Log(vbTab & "Audit started for path '" & Child.UNCPath & "'")
      Dim found As Boolean = True '' Determines whether any valid locations were found, assume false
      Dim cand As StructureCandidate

      Dim li As HTMLList.ListItem
      Dim ul As HTMLList

      Child.IsNameStructured() '' Don't run in logic check because we're applying our own logic
      cand = Child.StructureCandidates.GetHighestMatch()
      If cand IsNot Nothing Then
        '' Because a solid candidate was found, we should re-initialize the current path's variables by calling "CheckVariables"
        cand.CheckForLinkVariables()
        If Child.Variables.ContainsName(cand.PathName) Then
          If Not Child.Variables.IsValid(cand.StructurePath) Then
            '' Check that this path needs to be verified, then verify it in the ERP system.

            li = Report("'" & Child.UNCPath & "' was not valid in the ERP system",
                          AuditVisualReport.StatusCode.InvalidPath,
                          Child)
            Dim out As New StringBuilder()
            For z = 0 To Child.Variables.Count - 1 Step 1
              out.AppendLine(Child.Variables(z).Name & "=" & Child.Variables(z).Value)
            Next
            'Log("'" & Child.UNCPath & "' not found in ERP system. Variables: " & vbLf & out.ToString)
            found = False
          End If
        End If
        If found Then
          If cand.MatchPercentage = 100 Then
            li = Report("'" & cand.UNCPath & "' matched " & cand.MatchPercentage.ToString & "% '" & cand.StructurePath & "':" & cand.StructureDescription,
                          AuditVisualReport.StatusCode.ValidPath,
                          Child)
          Else
            li = Report("'" & cand.UNCPath & "' matched " & cand.MatchPercentage.ToString & "%, but was not high enough. Here are all of the candidates '" & SurroundJoin(Child.StructureCandidates.ToArray, " {", "} ") & "'",
                          AuditVisualReport.StatusCode.Other,
                          Child)
          End If
        End If
        If Child.Type = PathType.Folder Then
          '' If the folder is a "FreeForm" folder (meaning that it can have whatever files/folders) then leave it alone
          If cand.XElement.HasAttribute("freeform") Then
            If String.Equals(cand.XElement.Attributes("freeform").Value, "true", StringComparison.OrdinalIgnoreCase) Then
              found = False '' This will allow iteration of child objects to be skipped.
              'Log("Freeform folder found at '" & Child.UNCPath & "'. Force exiting the audit of this directory")
            End If
          End If
        End If
      Else
        li = Report("'" & Child.UNCPath & "' does not adhere to any paths.",
                      AuditVisualReport.StatusCode.InvalidPath,
                      Child)
        found = False
      End If

      If Child.Children IsNot Nothing And found Then
        ul = CreateNewList(Child)
        'Log("'" & Child.UNCPath & "' has " & Child.Children.Length.ToString & " children")
        For i = 0 To Child.Children.Length - 1 Step 1
          Dim cli As HTMLList.ListItem
          '' Check if user wants Thumbs.Db deleted
          FileCount += 1
          If _pstruct.AllowDeletionOfThumbsDb Then ' My.Settings.blnDeleteThumbsDb Then
            If Child.Children(i).Type = PathType.File And Child.Children(i).PathName.IndexOf("thumbs", StringComparison.OrdinalIgnoreCase) >= 0 And Child.Extension.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0 Then
              IO.File.Delete(Child.Children(i).UNCPath)
              cli = Report("Deleted Thumbs.Db from '" & Child.Children(i).UNCPath & "'.",
                            AuditVisualReport.StatusCode.Other,
                            Child.Children(i))
              AddListItemToList(ul, cli)
              Continue For
            End If
          End If
          If Child.Children(i).Type = PathType.File And Child.Children(i).PathName.IndexOf("desktop", StringComparison.OrdinalIgnoreCase) >= 0 And Child.Children(i).Extension.IndexOf(".ini", StringComparison.OrdinalIgnoreCase) >= 0 Then
            '' Ignore these files because we sometimes create them!
            Continue For
          End If
          AuditVisualChildren(ul, Child.Children(i))
          RaiseEvent GrandChildAudited(New AuditedEventArgs(Me, Child.Children(i).UNCPath, i, Child.Children.Length))
          If _quit Then
            '' Add final objects and exit
            Exit For
          End If
        Next
        AddListToListItem(li, ul)
      ElseIf Child.Children Is Nothing Then
        'Log(Child.UNCPath & " has no children")
      ElseIf Not found Then
        'Log("Not found triggered for '" & Child.UNCPath & "'")
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

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
      If Not Me.disposedValue Then
        If disposing Then
          ' TODO: dispose managed state (managed objects).
          _report.Dispose()
          _fileCount = Nothing
          _errPaths = Nothing
          _optPaths = Nothing
          fileSystem = Nothing
          _auditpath = Nothing
          ERPVariables = Nothing
          _quit = Nothing
        End If

        ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
        ' TODO: set large fields to null.
      End If
      Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
      ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
      Dispose(True)
      GC.SuppressFinalize(Me)
    End Sub
#End Region

  End Class

  ''' <summary>
  ''' Checks if the current path contains the specific object path
  ''' </summary>
  ''' <param name="Name">Path Structure object Name.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function HasPath(ByVal Name As String, Optional ByRef ReferencePath As Path = Nothing) As Boolean
    If ReferencePath Is Nothing Then
      Return (GetPathByStructure(Name) IsNot Nothing)
    Else
      ReferencePath = GetPathByStructure(Name)
      Return (ReferencePath IsNot Nothing)
    End If
  End Function

  ''' <summary>
  ''' Finds and returns a Path based on the provided Path Structure object name, if the path is found.
  ''' </summary>
  ''' <param name="Name">Path Structure object Name.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetPathByStructure(ByVal Name As String) As Path
    Dim objPath As Path = Nothing
    If _type = PathType.Folder Then
      For i = 0 To _children.Length - 1 Step 1
        If _children(i).IsNameStructured() Then
          If String.Equals(_children(i).StructureCandidates.GetHighestMatch().PathName, Name, StringComparison.OrdinalIgnoreCase) Then
            objPath = _children(i)
            Exit For
          End If
        End If
        If _children(i).Children.Length > 0 Then
          objPath = _children(i).GetPathByStructure(Name)
        End If
      Next
    End If
    Return objPath
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        ' TODO: dispose managed state (managed objects).
        _type = Nothing
        _path = Nothing
        _struct = Nothing
        _infoFile = Nothing
        _infoFolder = Nothing
        myXML = Nothing
        '_defaultPath = Nothing
        '_startPath = Nothing
        _variables = Nothing
        _parent = Nothing
        _children = Nothing
        _candidates = Nothing
        _pstruct = Nothing
        _exts = Nothing
        _users = Nothing
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

Public Class VariableArray
  Private _x As XmlElement
  Private _refpath As PathStructureClass.Path
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

  Public Sub New(ByVal XPath As String, ByVal Path As PathStructureClass.Path)
    _x = Path.PathStructure.SelectSingleNode(XPath)
    _refpath = Path
    If _x IsNot Nothing Then
      If _x.HasChildNodes Then
        AddRange(_x.SelectNodes("Variable"))
      End If
    End If

    'Log(New String("*", 20) & " New Path '" & _refpath.UNCPath & "'" & New String("*", 20))
  End Sub
  Public Sub New(ByVal XElement As XmlElement, ByVal Path As PathStructureClass.Path)
    _x = XElement
    _refpath = Path
    If _x IsNot Nothing Then
      If _x.HasChildNodes Then
        AddRange(_x.SelectNodes("Variable"))
      End If
    End If

    'Log(New String("*", 20) & " New Path '" & _refpath.UNCPath & "'" & New String("*", 20))
  End Sub

  Public Sub Initialize()
    _lst = New List(Of Variable)
    If _x IsNot Nothing Then
      If _x.HasChildNodes Then
        AddRange(_x.SelectNodes("Variable"))
      End If
    End If
  End Sub

  Private Sub Add(ByVal XPath As String)
    _lst.Add(New Variable(XPath, _refpath))
  End Sub
  Private Sub Add(ByVal XElement As XmlElement)
    _lst.Add(New Variable(XElement, _refpath))
  End Sub
  Private Sub AddRange(ByVal XPaths As String())
    For i = 0 To XPaths.Length - 1 Step 1
      _lst.Add(New Variable(XPaths(i), _refpath))
    Next
  End Sub
  Private Sub AddRange(ByVal XElements As XmlNodeList)
    For i = 0 To XElements.Count - 1 Step 1
      _lst.Add(New Variable(XElements(i), _refpath))
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
''' <summary>
''' Represents a Path's Path Structure variable and retains the values both globaly and through 'variablepipes'
''' </summary>
''' <remarks></remarks>
Public Class Variable
  Private _x As XmlElement
  Private _name, _erptable As String
  Private _index As Integer
  Private _cmds As New List(Of ERPCommand)
  Private _refVarPath As Path

  Public ReadOnly Property Value As String
    Get
      Dim nodes As String() = _refVarPath.UNCPath.Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      If _index < nodes.Length Then
        Return nodes(_index)
      Else
        'Log("Index is outside bounds of the array. " & _name & " at index " & _index.ToString & "/" & nodes.Length.ToString & " from variable path '" & _refVarPath.UNCPath & "'")
        Return ""
      End If
    End Get
  End Property
  Public ReadOnly Property Name As String
    Get
      Return _name
    End Get
  End Property
  Public Property Index As Integer
    Get
      Return _index
    End Get
    Set(value As Integer)
      _index = value
    End Set
  End Property

  Public Sub New(ByVal XPath As String, ByVal Path As Path)
    _x = Path.PathStructure.SelectSingleNode(XPath)
    _name = _x.Attributes("name").Value
    If _x.HasAttribute("erp") Then
      _erptable = _x.Attributes("erp").Value
    End If
    _index = Convert.ToInt32(_x.Attributes("pathindex").Value)
    If _x.HasChildNodes Then
      Dim cmds As XmlNodeList = _x.ChildNodes '.SelectNodes("ERPCommand")
      For i = 0 To cmds.Count - 1 Step 1
        _cmds.Add(New ERPCommand(cmds(i)))
      Next
    End If
    _refVarPath = Path
  End Sub
  Public Sub New(ByVal XElement As XmlElement, ByVal Path As Path)
    _x = XElement
    _name = _x.Attributes("name").Value
    If _x.HasAttribute("erp") Then
      _erptable = _x.Attributes("erp").Value
    End If
    _index = Convert.ToInt32(_x.Attributes("pathindex").Value)
    If _x.HasChildNodes Then
      Dim cmds As XmlNodeList = _x.ChildNodes '.SelectNodes("ERPCommand")
      For i = 0 To cmds.Count - 1 Step 1
        _cmds.Add(New ERPCommand(cmds(i)))
      Next
    End If
    _refVarPath = Path
  End Sub

  ''' <summary>
  ''' Determines whether or not the input value contains the name of the current Variable.
  ''' </summary>
  ''' <param name="Input"></param>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Function HasVariable(ByVal Input As String) As Boolean
    Return (Input.IndexOf(_name) >= 0)
  End Function

  ''' <summary>
  ''' Determines whether or not the variable value is valid in the ERP system.
  ''' </summary>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Function IsValid() As Boolean
    Dim blnFound As Boolean = False
    If _refVarPath.PStructure.CheckERPSystem Then ' My.Settings.blnERPCheck Then
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

      If _refVarPath.PStructure.ERPConnection.Successful Then ' ERPConnection.Successful Then
        blnFound = _refVarPath.PStructure.ERPConnection.CommandHasValues("SELECT " & selFields & " FROM [" & _erptable & "] WHERE " & cond & ";") 'ERPConnection.CommandHasValues("SELECT " & selFields & " FROM [" & _erptable & "] WHERE " & cond & ";")
      Else
        Log("Connection to ERP system unsuccessful!")
      End If
    Else
      blnFound = True '' Set to true if the flag isn't even set. No need to raise alarm
    End If
    Return blnFound
  End Function

  ''' <summary>
  ''' Replaces any variable name found in the provided string with a valid value of the variable.
  ''' </summary>
  ''' <param name="Input">String to which variable values will replace variable names</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Function Replace(ByVal Input As String) As String
    If Not String.IsNullOrEmpty(Input) Then
      If Input.IndexOf(_name, System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        Input = Input.Replace(_name, Me.Value)
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

    Public Sub New(ByVal XElement As XmlElement)
      _x = XElement
      _rawcmd = _x.InnerText
    End Sub
  End Class
End Class

''' <summary>
''' Represents and manages a list of StructureCanidate objects for a given Path to help determine what part of the PathStructure the path might be part of.
''' </summary>
''' <remarks></remarks>
Public Class StructureCandidateArray
  Private _lst As New List(Of StructureCandidate)
  Private _structarrpath As String
  Private _refPath As PathStructureClass.Path

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

  Public Sub New(ByVal ReferencePath As PathStructureClass.Path)
    'Log(vbTab & "StructureCandidateArray initialized with path '" & ReferencePath.UNCPath & "'")
    _refPath = ReferencePath
    _structarrpath = _refPath.UNCPath
  End Sub

  Public Sub Add(ByVal XPath As String)
    If Not String.IsNullOrEmpty(XPath) Then
      _lst.Add(New StructureCandidate(XPath, _refPath))
    End If
  End Sub
  Public Sub Add(ByVal XElement As XmlElement)
    _lst.Add(New StructureCandidate(XElement, _refPath))
  End Sub
  Public Sub Add(ByVal Candidate As StructureCandidate)
    _lst.Add(Candidate)
  End Sub
  Public Sub AddRange(ByVal XNodes As XmlNodeList)
    For i = 0 To XNodes.Count - 1 Step 1
      _lst.Add(New StructureCandidate(XNodes(i), _refPath))
    Next
  End Sub
  Public Sub AddRange(ByVal XPaths As String())
    For i = 0 To XPaths.Length - 1 Step 1
      _lst.Add(New StructureCandidate(XPaths(i), _refPath))
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
''' <summary>
''' A candidate object used to determine if the provided UNC path matches the provided Path Structure path.
''' </summary>
''' <remarks></remarks>
Public Class StructureCandidate
  Private _x As XmlElement
  'Private _x As PathStructure.PathStyle
  Private _xpath, _descr, _spath As String
  Private _structpath As Path
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
      'Return _x.XElement
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
  Public ReadOnly Property UNCPath As String
    Get
      Return _structpath.UNCPath
    End Get
  End Property
  ''' <summary>
  ''' Gets the reference path for the current candidate.
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property ReferencePath As Path
    Get
      Return _structpath
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
      If String.IsNullOrEmpty(_descr) Then
        _descr = _structpath.PStructure.GetDescriptionfromXPath(_xpath)
      End If
      Return _descr
    End Get
  End Property
  ''' <summary>
  ''' Gets whether or not the UNC path was a complete match with the provided Path Structure path
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property IsMatch As Boolean
    Get
      Return _match
    End Get
  End Property
  ''' <summary>
  ''' Gets the percentage that the UNC path matches the provided Path Structure path
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property MatchPercentage As Integer
    Get
      Return _conf
    End Get
  End Property
  ''' <summary>
  ''' Gets the Path Structure object's name attribute
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property PathName As String
    Get
      'If _x.HasAttribute("name") Then
      '  Return _x.Attributes("name").Value
      'Else
      '  Return ""
      'End If
      Return _x.Name
    End Get
  End Property

  Public Sub New(ByVal xPath As String, ByVal refPath As PathStructureClass.Path)
    _structpath = refPath
    _x = _structpath.PathStructure.SelectSingleNode(xPath)
    CheckForLinkVariables()
    '_x = refPath.StructureStyle.GetStyle(xPath)
    _xpath = xPath
    _spath = _structpath.PStructure.GetURIfromXPath(_x)
    '_spath = _x.URI
    _match = Match() '' Process the Path Structure
    If _match And refPath.Type = Path.PathType.File Then '' If a match, then check extensions
      refPath.Extensions = New Extensions(refPath.PStructure, _x)
      'refPath.Extensions = _x.GetExtensions()
      If Not refPath.Extensions.Contains(refPath.Extension) Then
        _match = False
        _conf -= 1
      End If
    End If
  End Sub
  Public Sub New(ByVal xElement As XmlElement, ByVal refPath As PathStructureClass.Path)
    _structpath = refPath
    _x = xElement
    CheckForLinkVariables()
    '_x = refPath.StructureStyle.GetStyle(xElement.FindXPath())
    _xpath = xElement.FindXPath()
    _spath = _structpath.PStructure.GetURIfromXPath(_x)
    '_spath = _x.URI
    _match = Match() '' Process the Path Structure
    If _match And refPath.Type = Path.PathType.File Then '' If a match, then check extensions
      refPath.Extensions = New Extensions(refPath.PStructure, _x)
      'refPath.Extensions = _x.GetExtensions()
      If Not refPath.Extensions.Contains(refPath.Extension) Then
        _match = False
        _conf -= 1
      End If
    End If
  End Sub

  ''' <summary>
  ''' Peaks for 'variablepipes' attribute(s) to dynamically adjust Path Structure Global variables starting at either the default node or the passed node variable.
  ''' </summary>
  ''' <param name="Node">(Optional) Used recursively, but passing a specific Path Structure node will set the starting search path.</param>
  ''' <remarks></remarks>
  Public Sub CheckForLinkVariables(Optional ByVal Node As XmlElement = Nothing)
    Static blnReplaced As Boolean = False
    If Node Is Nothing Then
      Node = _x '.XElement
      blnReplaced = False
      'Log(New String("v", 20))
    End If
    '' Check if the element is a link, and replace variables as necessary
    If Node.HasAttribute("variablepipes") Then
      '' Split all variable pipes
      Dim repVars As String() = Node.Attributes("variablepipes").Value.Split({"|"}, System.StringSplitOptions.RemoveEmptyEntries)
      If repVars.Length > 0 Then
        For i = 0 To repVars.Length - 1 Step 1
          '' Split name from value index
          Dim tmp As String() = repVars(i).Split({"="}, System.StringSplitOptions.RemoveEmptyEntries)
          If tmp.Length > 0 Then
            '' Set the variable's value index
            _structpath.Variables(tmp(0)).Index = Convert.ToInt32(tmp(1))
            blnReplaced = True
            'Log(vbTab & "Setting '" & tmp(0) & "'='" & tmp(1) & "'" & vbTab & "<" & Node.Name & " name='" & Node.Attributes("name").Value & "' />")
          End If
        Next
      End If
    End If
    If Not String.Equals(Node.ParentNode.Name, "Structure", StringComparison.OrdinalIgnoreCase) _
      And Not String.Equals(Node.Name, "Structure", StringComparison.OrdinalIgnoreCase) Then
      CheckForLinkVariables(Node.ParentNode)
    Else
      If Not blnReplaced Then
        'Log(New String("^", 20))
        _structpath.Variables.Initialize()
      End If
    End If
  End Sub

  ''' <summary>
  ''' Gets whether or not the current UNC path completely matches the provided Path Structure path.
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function Match(Optional ByVal CheckExtensions As Boolean = False) As Boolean
    _conf = -1
    '' Check if path is default path
    If _structpath.PStructure.defaultPaths.Contains(_structpath.UNCPath) Then
      Log("Path is default path")
      _conf = 100
      Return True
    End If

    Dim msg As New StringBuilder
    Dim strTemp As String = _structpath.UNCPath
    '' Fix if a file
    If strTemp.LastIndexOf(".") > strTemp.LastIndexOf("\") Then strTemp = strTemp.Remove(strTemp.LastIndexOf("."))
    If _spath.LastIndexOf(".") > _spath.LastIndexOf("\") Then _spath = _spath.Remove(_spath.LastIndexOf("."))

    If strTemp.LastIndexOf("\") = strTemp.Length - 1 Then strTemp = strTemp.Remove(strTemp.Length - 1)
    If _spath.LastIndexOf("\") = _spath.Length - 1 Then _spath = _spath.Remove(_spath.Length - 1)

    '' Iterate through each character and compare. Watch out for variables and peek into _path for next character
    If _spath.Split({"\\", "\"}, System.StringSplitOptions.None).Length = strTemp.Split({"\\", "\"}, System.StringSplitOptions.None).Length Then
      Dim s As String() = _structpath.Variables.Replace(_spath).Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries) '_pstruct.ReplaceVariables(_spath, _path).Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      Dim f As String() = strTemp.Split({"\\", "\"}, System.StringSplitOptions.RemoveEmptyEntries)
      Dim si As Integer
      msg.AppendLine("Comparing '" & SurroundJoin(s, "[", "]") & "'(" & s.Length.ToString & ") to '" & SurroundJoin(f, "[", "]") & "'(" & f.Length.ToString & ") from path '" & _structpath.UNCPath & "'")
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
        msg.AppendLine("_spath = " & _spath)
        _conf = 0 '' Ensure positive
      End If
      msg.AppendLine(vbTab & "Match percentage: " & _conf.ToString)
    Else
      msg.AppendLine(vbTab & "Aborted due to invalid array lengths")
      _conf = 0
    End If
    'Debug.WriteLine(msg.ToString)
    If _conf = -1 Then
      _conf = 100
      Return True
    Else
      Return False
    End If
  End Function

  ''' <summary>
  ''' Gets a list of successful StructureCandidates (real files) related to the current StructureCandidate
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetStructuredChildrenFiles() As List(Of StructureCandidate)
    Dim lst As New List(Of StructureCandidate)
    If _structpath.Type = Path.PathType.Folder Then
      If Me.IsMatch Then
        If Not IsNothing(_structpath.Children) Then
          For i = 0 To _structpath.Children.Length - 1 Step 1
            If _structpath.Children(i).IsNameStructured() Then
              Dim nwc As StructureCandidate = _structpath.Children(i).StructureCandidates.GetHighestMatch()
              If _structpath.Children(i).Type = Path.PathType.File Then
                lst.Add(nwc)
              ElseIf _structpath.Children(i).Type = Path.PathType.Folder Then
                lst.AddRange(nwc.GetStructuredChildrenFiles().ToArray)
              End If
            End If
          Next
        End If
      End If
    End If
    Return lst
  End Function
End Class

''' <summary>
''' Represents a list of valid extensions that can be associated with a Path.
''' </summary>
''' <remarks></remarks>
Public Class Extensions
  Private _pathStruct As PathStructure
  Private _namedStruct As XmlElement
  Private _exts As List(Of Extension)

  ''' <summary>
  ''' Returns an Extension object at the specified index.
  ''' </summary>
  ''' <param name="Index">Index of the Extension in the current Extensions list.</param>
  ''' <value></value>
  ''' <returns>Extension</returns>
  ''' <remarks></remarks>
  Default Public ReadOnly Property Item(ByVal Index As Integer)
    Get
      If _exts.Count > Index Then
        Return _exts(Index)
      Else
        Return Nothing
      End If
    End Get
  End Property
  ''' <summary>
  ''' Returns the current Extensions list of Extension objects.
  ''' </summary>
  ''' <value></value>
  ''' <returns>List(Of Extension)</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property ExtensionList As List(Of Extension)
    Get
      Return _exts
    End Get
  End Property

  Public Sub New(ByVal PathStruct As PathStructure, ByVal NamedStructure As XmlElement)
    _pathStruct = PathStruct
    _namedStruct = NamedStructure
    _exts = New List(Of Extension)

    If _namedStruct IsNot Nothing Then
      If _namedStruct.HasAttribute("exts") Then
        Dim strTemp As String = _namedStruct.Attributes("exts").Value
        Dim kyval As String()
        If strTemp.IndexOf("|") >= 0 Then
          Dim exts As String() = strTemp.Split({"|"}, System.StringSplitOptions.RemoveEmptyEntries)
          For i = 0 To exts.Length - 1 Step 1
            If exts(i).IndexOf(":") >= 0 Then
              kyval = exts(i).Split({":"}, System.StringSplitOptions.RemoveEmptyEntries)
              If kyval.Length = 2 Then
                Dim ext As New Extension(_pathStruct.Settings.SelectSingleNode("//Extension[@name='" & kyval(0) & "']"))
                ext.Occurance = Convert.ToInt32(kyval(1))
                _exts.Add(ext)
              End If
            End If
          Next
        Else
          If strTemp.IndexOf(":") >= 0 Then
            kyval = strTemp.Split({":"}, System.StringSplitOptions.RemoveEmptyEntries)
            If kyval.Length = 2 Then
              Dim ext As New Extension(_pathStruct.Settings.SelectSingleNode("//Extension[@name='" & kyval(0) & "']"))
              ext.Occurance = Convert.ToInt32(kyval(1))
              _exts.Add(ext)
            End If
          End If
        End If
      End If
    End If

    'Log("Extensions: " & _exts.Count.ToString & vbLf & vbTab & _namedStruct.OuterXml)
  End Sub

  Public Function GetHighestMatch() As Extension
    Dim ext As Extension
    Dim rating As Integer = -1
    For i = 0 To _exts.Count - 1 Step 1
      If _exts(i).Occurance > rating Then
        ext = _exts(i)
        rating = ext.Occurance
      End If
    Next
    Return ext
  End Function

  Public Sub SetExtensions()
    If _exts.Count > 0 Then
      Dim x As XmlElement = _pathStruct.Settings.SelectSingleNode(FindXPath(_namedStruct))
      If x IsNot Nothing Then
        Dim attr As XmlAttribute
        If x.HasAttribute("exts") Then
          attr = x.Attributes("exts")
        Else
          attr = _pathStruct.Settings.CreateAttribute("exts")
        End If
        Dim extlst As New List(Of String)
        For i = 0 To _exts.Count - 1 Step 1
          If Not String.IsNullOrEmpty(_exts(i).Name) Then
            extlst.Add(_exts(i).Name & ":" & _exts(i).Occurance.ToString)
          End If
        Next
        attr.Value = String.Join("|", extlst.ToArray)
        If Not String.IsNullOrEmpty(attr.Value) Then
          If x.HasAttribute("exts") Then
            x.Attributes("exts").Value = attr.Value
          Else
            x.Attributes.Append(attr)
          End If
        End If
      End If
    End If
    Try
      _pathStruct.Settings.Save(_pathStruct.SettingsPath)
    Catch ex As Exception
      Log("Couldn't save the settings file due to process error: " & ex.Message)
    End Try
  End Sub

  ''' <summary>
  ''' Adds a new extension to the Path Structure settings document.
  ''' </summary>
  ''' <param name="Name">The name of the extensions (ex. TXT or VB)</param>
  ''' <remarks></remarks>
  Public Sub AddExtension(ByVal Name As String)
    If Not String.IsNullOrEmpty(FormatExtension(Name)) Then
      Dim ext As XmlElement

      '' Check if Settings contains Extension node
      Dim exts As XmlNodeList = _pathStruct.Settings.SelectNodes("//Extension[@name='" & Name & "']")
      Dim attr As XmlAttribute
      If exts.Count = 0 Then
        '' Create the extension
        ext = _pathStruct.Settings.CreateElement("Extension")
        attr = _pathStruct.Settings.CreateAttribute("name")
        attr.Value = Name
        ext.Attributes.Append(attr)
        attr = _pathStruct.Settings.CreateAttribute("description")
        attr.Value = "{New Extension}"
        ext.Attributes.Append(attr)
        ext = _pathStruct.Settings.SelectSingleNode("//Extensions").AppendChild(ext)
        _pathStruct.Settings.Save(_pathStruct.SettingsPath)
      ElseIf exts.Count > 1 Then
        'Log("PathStructure_Extensions: There shouldn't be more than one extension in the Settings file.")
      ElseIf exts.Count = 1 Then
        ext = exts(0)
      Else
        'Log("PathStructure_Extensions: exts.Count is less than 0?")
      End If

      If Not Contains(Name) Then
        If ext IsNot Nothing Then
          Dim nwExt As New Extension(ext)
          nwExt.Occurance = 1
          _exts.Add(nwExt)
          'Log("PathStructure_Extensions: Adding '" & Name & "' to extension list")
        Else
          'Log("PathStructure_Extensions: Couldn't add '" & Name & "' to extension list because the Extension node could not be found")
        End If
      Else
        _exts.Item(IndexOf(Name)).Occurance += 1
        'Log("PathStructure_Extensions: Trying to increase the count of extension '" & Name & "' to " & _exts.Item(IndexOf(Name)).Occurance.ToString)
      End If

      SetExtensions()
    End If
  End Sub

  ''' <summary>
  ''' Removes unnecessary '.' from the name
  ''' </summary>
  ''' <param name="Name">Extension name (ex. TXT or .TXT or VB or .VB)</param>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Private Function FormatExtension(ByRef Name As String) As String
    If Name.IndexOf(".") = 0 Then
      Name = Name.Remove(0, 1)
    End If
    Name = Name.ToUpper
    Return Name
  End Function

  ''' <summary>
  ''' Returns whether or not the current Extensions list contains the provided name
  ''' </summary>
  ''' <param name="Name">Extension name (ex. TXT or VB)</param>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Overloads Function Contains(ByVal Name As String) As Boolean
    FormatExtension(Name)
    For i = 0 To _exts.Count - 1 Step 1
      If String.Equals(_exts(i).Name, Name, StringComparison.OrdinalIgnoreCase) Then
        Return True
      End If
    Next
    Return False
  End Function
  ''' <summary>
  ''' Returns whether or not the current Extensions list contains the provided Extension object.
  ''' </summary>
  ''' <param name="Ext">Extension object</param>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public Overloads Function Contains(ByVal Ext As Extension) As Boolean
    Return _exts.Contains(Ext)
  End Function

  ''' <summary>
  ''' Returns the index of the provided Extension
  ''' </summary>
  ''' <param name="Ext">Extension object</param>
  ''' <returns>Integer</returns>
  ''' <remarks></remarks>
  Public Overloads Function IndexOf(ByVal Ext As Extension) As Integer
    For i = 0 To _exts.Count - 1 Step 1
      If _exts(i).Equals(Ext) Then
        Return i
      End If
    Next
    Return -1
  End Function
  ''' <summary>
  ''' Returns the index of the provided extension name
  ''' </summary>
  ''' <param name="Name">Extension name (ex. TXT or VB)</param>
  ''' <returns>Integer</returns>
  ''' <remarks></remarks>
  Public Overloads Function IndexOf(ByVal Name As String) As Integer
    FormatExtension(Name)
    For i = 0 To _exts.Count - 1 Step 1
      If _exts(i).Name.Equals(Name, StringComparison.OrdinalIgnoreCase) Then
        Return i
      End If
    Next
    Return -1
  End Function

  ''' <summary>
  ''' Represents a PathStructure extension
  ''' </summary>
  ''' <remarks></remarks>
  Public Class Extension
    Private _x As XmlElement
    Private _cnt As Integer = 1

    ''' <summary>
    ''' Returns a reference to the PathStructure settings XmlElement
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property XElement As XmlElement
      Get
        Return _x
      End Get
    End Property
    Public ReadOnly Property Name As String
      Get
        If _x IsNot Nothing Then
          If _x.HasAttribute("name") Then
            Return _x.Attributes("name").Value
          End If
        End If
        Return ""
      End Get
    End Property
    Public ReadOnly Property Description As String
      Get
        If _x IsNot Nothing Then
          If _x.HasAttribute("description") Then
            Return _x.Attributes("description").Value
          End If
        End If
        Return ""
      End Get
    End Property
    Public Property Occurance As Integer
      Get
        Return _cnt
      End Get
      Set(value As Integer)
        _cnt = value
      End Set
    End Property

    Public Overrides Function ToString() As String
      Return Name
    End Function
    Public Overrides Function Equals(obj As Object) As Boolean
      Dim typ As Type = obj.GetType
      If typ.GetProperty("Name") IsNot Nothing Then
        Return String.Equals(Me.Name, obj.Name)
      Else
        Return False
      End If
    End Function
    Public Overloads Shared Operator =(ByVal cObject As Extension, ByVal aObject As Object)
      Return cObject.Equals(aObject)
    End Operator
    Public Overloads Shared Operator <>(ByVal cObject As Extension, ByVal aObject As Object)
      Return (Not cObject.Equals(aObject))
    End Operator

    Public Sub New(ByVal Element As XmlElement)
      _x = Element
    End Sub
  End Class
End Class

''' <summary>
''' An object that utilizes the connection string to a database in order to determine which .NET objects to use.
''' </summary>
''' <remarks></remarks>
Public Class DatabaseConnection
  Private _cnn As String
  Private _type As MediaType
  Private _oledb As OleDbConnection
  Private _sql As SqlConnection
  Private _cnnbldr As ConnectionBuilder
  Private _oledbcmd As OleDbCommand
  Private _sqlcmd As SqlCommand

  ''' <summary>
  ''' Gets the connection string that the current instance of the DatabaseConnection was initialized with.
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks>If a new connection string is needed, use a new instance of the DatabaseConnection</remarks>
  Public ReadOnly Property ConnectionString As String
    Get
      Return _cnn
    End Get
  End Property
  ''' <summary>
  ''' Gets which type of database the initialization was able to determine the connection string was referring to.
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public ReadOnly Property DatabaseType As MediaType
    Get
      Return _type
    End Get
  End Property
  ''' <summary>
  ''' Gets whether or not the initialization was able to open a connection based on the provided Connection String.
  ''' </summary>
  ''' <value></value>
  ''' <returns>Boolean</returns>
  ''' <remarks></remarks>
  Public ReadOnly Property Successful As Boolean
    Get
      If _type = MediaType.OLEDB And _oledb IsNot Nothing Then
        Return _oledb.State = ConnectionState.Open
      ElseIf _type = MediaType.SQL And _sql IsNot Nothing Then
        Return _sql.State = ConnectionState.Open
      Else
        Return False
      End If
    End Get
  End Property
  Public Property OleDbCommand As OleDbCommand
    Get
      Return _oledbcmd
    End Get
    Set(value As OleDbCommand)
      _oledbcmd = value
    End Set
  End Property
  Public Property SqlCommand As SqlCommand
    Get
      Return _sqlcmd
    End Get
    Set(value As SqlCommand)
      _sqlcmd = value
    End Set
  End Property
  ''' <summary>
  ''' Gets/Sets the command text currently set on the current instance of the DatabaseConnection
  ''' </summary>
  ''' <value></value>
  ''' <returns>String</returns>
  ''' <remarks></remarks>
  Public Property CommandText As String
    Get
      If _type = MediaType.OLEDB Then
        Return _oledbcmd.CommandText
      ElseIf _type = MediaType.SQL Then
        Return _sqlcmd.CommandText
      Else
        Return ""
      End If
    End Get
    Set(value As String)
      If _type = MediaType.OLEDB Then
        _oledbcmd.CommandText = value
      ElseIf _type = MediaType.SQL Then
        _sqlcmd.CommandText = value
      End If
    End Set
  End Property

  Public Enum MediaType
    SQL
    OLEDB
  End Enum

  ''' <summary>
  ''' Initializes a new instance of a DatabaseConnection. Initialization opens a database connection indefinitely during the lifecycle of the application.
  ''' </summary>
  ''' <param name="CnnStr">Connection string to the database. Looks at the 'Data Source' attribute to see if the connection string is towards an OleDb (.mdb, .accdb). Otherwise, assumes it's a SQL database.</param>
  ''' <remarks></remarks>
  Public Sub New(ByVal CnnStr As String)
    _cnn = CnnStr
    _cnnbldr = New ConnectionBuilder(_cnn)
    If _cnnbldr.HasKey("Data Source") Then
      If _cnnbldr.GetValue("data source").IndexOf(".mdb", System.StringComparison.OrdinalIgnoreCase) >= 0 Or _cnnbldr.GetValue("data source").IndexOf(".accdb", System.StringComparison.OrdinalIgnoreCase) >= 0 Then
        _type = MediaType.OLEDB
      Else
        _type = MediaType.SQL
      End If
    End If

    If _type = MediaType.OLEDB Then
      Try
        _oledb = New OleDbConnection(_cnn)
        _oledb.Open()
        _oledbcmd = New OleDbCommand("", _oledb)
      Catch ex As Exception
        Log("Error occurred opening OLEDB connection: " & ex.Message)
      End Try
    ElseIf _type = MediaType.SQL Then
      Try
        _sql = New SqlConnection(_cnn)
        _sql.Open()
        _sqlcmd = New SqlCommand("", _sql)
      Catch ex As Exception
        Log("Error occurred opening SQL connection: " & ex.Message)
      End Try
    End If
  End Sub

  ''' <summary>
  ''' Checks if the connection command will return by executing a reader and return HasRows on the [DataType]DataReader.HasRows()
  ''' </summary>
  ''' <param name="CommandString">Optional, command text to overwrite an existing command text</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function CommandHasValues(Optional ByVal CommandString As String = "") As Boolean
    Dim hasValues As Boolean = False
    If _type = MediaType.OLEDB Then
      If Not String.IsNullOrEmpty(CommandString) Then
        _oledbcmd.CommandText = CommandString
      End If
      If Not String.IsNullOrEmpty(_oledbcmd.CommandText) Then
        Using Rdr As OleDbDataReader = _oledbcmd.ExecuteReader
          hasValues = Rdr.HasRows
        End Using
      End If
    ElseIf _type = MediaType.SQL Then
      If Not String.IsNullOrEmpty(CommandString) Then
        _sqlcmd.CommandText = CommandString
      End If
      If Not String.IsNullOrEmpty(_sqlcmd.CommandText) Then
        Using Rdr As SqlDataReader = _sqlcmd.ExecuteReader
          hasValues = Rdr.HasRows
        End Using
      End If
    End If
    Return hasValues
  End Function

  ''' <summary>
  ''' Setup the parameters for your ExecuteReader before calling ExecuteReader by adding conditional parameters
  ''' </summary>
  ''' <param name="Key"></param>
  ''' <param name="Value"></param>
  ''' <remarks></remarks>
  Public Sub AddParameter(ByVal Key As String, ByVal Value As Object)
    If _type = MediaType.OLEDB Then
      _oledbcmd.Parameters.AddWithValue(Key, Value)
    ElseIf _type = MediaType.SQL Then
      _sqlcmd.Parameters.AddWithValue(Key, Value)
    End If
  End Sub

  Private Class ConnectionBuilder
    Private _lst As New SortedList(Of String, String)
    Public ReadOnly Property ConnectionVariables
      Get
        Return _lst
      End Get
    End Property

    Public Sub New(ByVal CnnStr As String)
      Dim keyvals As String() = CnnStr.Split({";"}, System.StringSplitOptions.RemoveEmptyEntries)
      Dim tmp As String()
      For i = 0 To keyvals.Length - 1 Step 1
        tmp = keyvals(i).Split({"="}, System.StringSplitOptions.RemoveEmptyEntries)
        If tmp.Length = 2 Then
          Debug.WriteLine("Adding KeyValue: " & tmp(0) & "=" & tmp(1))
          _lst.Add(tmp(0), tmp(1))
        End If
      Next
    End Sub

    Public Function HasKey(ByVal PropertyName As String) As Boolean
      If _lst.Count > 0 Then
        For i = 0 To _lst.Count - 1 Step 1
          Debug.WriteLine(_lst.Keys(i) & "=" & PropertyName & " (" & String.Equals(_lst.Keys(i), PropertyName, System.StringComparison.OrdinalIgnoreCase).ToString & ")")
          If String.Equals(_lst.Keys(i), PropertyName, System.StringComparison.OrdinalIgnoreCase) Then
            Return True
          End If
        Next
      End If
      Return False
    End Function
    Private Function GetIndexOfKey(ByVal PropertyName As String) As Integer
      If _lst.Count > 0 Then
        For i = 0 To _lst.Count - 1 Step 1
          If String.Equals(_lst.Keys(i), PropertyName, System.StringComparison.OrdinalIgnoreCase) Then
            Return i
          End If
        Next
      End If
      Return -1
    End Function
    Public Function GetValue(ByVal PropertyName As String) As String
      If _lst.Count > 0 Then
        Dim i As Integer = GetIndexOfKey(PropertyName)
        If i >= 0 Then
          Debug.WriteLine(_lst.Keys(i) & "=" & _lst.Values(i))
          Return _lst.Values(i)
        End If
      End If
      Return ""
    End Function
  End Class
End Class

''' <summary>
''' Represents a list of Windows user profiles/groups with specific permissions groups.
''' </summary>
''' <remarks></remarks>
Public Class Users
  Private _usrs As List(Of User)
  Private _node As XmlElement

  ''' <summary>
  ''' Returns a list of PathStructure User objects
  ''' </summary>
  ''' <value></value>
  ''' <returns>List(Of User)</returns>
  ''' <remarks></remarks>
  Public Property Users As List(Of User)
    Get
      Return _usrs
    End Get
    Set(value As List(Of User))
      _usrs = value
    End Set
  End Property

  Public Sub New(ByVal Node As XmlElement)
    If Node IsNot Nothing Then
      _node = Node
      Dim lst As XmlNodeList = _node.ChildNodes '.SelectNodes(".//User")
      _usrs = New List(Of User)
      If lst.Count > 0 Then
        For i = 0 To lst.Count - 1 Step 1
          _usrs.Add(New User(lst(i)))
        Next
      End If
    End If
  End Sub

  ''' <summary>
  ''' Sets all the PathStructure preferred permissions (referenced by permissions group name) for the provided filesystem path.
  ''' </summary>
  ''' <param name="Path">Filesystem path to receive permissions</param>
  ''' <param name="GroupName">Name of the PermissionsGroup to apply to the provided path</param>
  ''' <remarks></remarks>
  Public Sub SetPermissionsByGroup(ByVal Path As String, ByVal GroupName As String)
    If _usrs.Count > 0 Then
      For i = 0 To _usrs.Count - 1 Step 1
        If _usrs(i).PermissionGroups.Count > 0 Then
          For j = 0 To _usrs(i).PermissionGroups.Count - 1 Step 1
            If String.Equals(_usrs(i).PermissionGroups(j).GroupName, GroupName, StringComparison.OrdinalIgnoreCase) Then
              If _usrs(i).PermissionGroups(j).Permissions.Count > 0 Then
                For k = 0 To _usrs(i).PermissionGroups(j).Permissions.Count - 1 Step 1
                  If Not IsNothing(_usrs(i).PermissionGroups(j).Permissions(k).Rights) And Not IsNothing(_usrs(i).PermissionGroups(j).Permissions(k).Access) Then
                    If _usrs(i).PermissionGroups(j).Permissions(k).Rights > 0 And _usrs(i).PermissionGroups(j).Permissions(k).Access >= 0 Then
                      AddDirectorySecurity(Path, _usrs(i).UserName, _usrs(i).PermissionGroups(j).Permissions(k).Rights, _usrs(i).PermissionGroups(j).Permissions(k).Access)

                      '' If allowed, check if this path has a preferred icon and create the desktop.ini file
                      If _usrs(i).PermissionGroups(j).XElement.HasAttribute("icon") Then
                        Dim desktopINI As String = IO.Path.Combine({Path, "desktop.ini"})
                        If Not IO.File.Exists(desktopINI) Then
                          Dim iconIndex, iconTip As String
                          If _usrs(i).PermissionGroups(j).XElement.HasAttribute("iconindex") Then
                            iconIndex = _usrs(i).PermissionGroups(j).XElement.Attributes("iconindex").Value
                          Else
                            iconIndex = "0"
                          End If
                          If _usrs(i).PermissionGroups(j).XElement.HasAttribute("icontip") Then
                            iconTip = _usrs(i).PermissionGroups(j).XElement.Attributes("icontip").Value
                          ElseIf _usrs(i).PermissionGroups(j).XElement.HasAttribute("description") Then
                            iconTip = _usrs(i).PermissionGroups(j).XElement.Attributes("description").Value
                          End If
                          IO.File.WriteAllText(desktopINI, "[.ShellClassInfo]" & vbCrLf & _
                                               "ConfirmFileOp=0" & vbCrLf & _
                                               "IconFile=" & _usrs(i).PermissionGroups(j).XElement.Attributes("icon").Value & vbCrLf & _
                                               "IconIndex=" & iconIndex & vbCrLf & _
                                               "InfoTip=" & iconTip,
                                               System.Text.Encoding.Unicode)
                          SetAttr(Path, FileAttribute.System Or FileAttribute.Hidden)
                        End If
                      Else
                        Log("<" & _usrs(i).PermissionGroups(j).XElement.Name & " /> didn't contain the attribute 'icon'")
                      End If
                    Else
                      Log(vbTab & "Couldn't set permission: " & _usrs(i).PermissionGroups(j).Permissions(k).Rights.ToString & "=" & _usrs(i).PermissionGroups(j).Permissions(k).Access.ToString)
                    End If
                  Else
                    Log("Rights or Access Control is nothing")
                  End If
                Next
              Else
                'Log("No permissions in permissions list for user '" & _usrs(i).UserName & "' and permission group '" & _usrs(i).PermissionGroups(j).GroupName & "'")
              End If
            End If
          Next
        Else
          'Log("No permission groups in permission groups list for user '" & _usrs(i).UserName & "'")
        End If
      Next
    Else
      'Log("No users in users list")
    End If
  End Sub

  ''' <summary>
  ''' Represents a Windows user profile/group with a list permissions groups
  ''' </summary>
  ''' <remarks></remarks>
  Public Class User
    Private _name As String
    Private _perm As List(Of PermissionsGroup)
    Private _node As XmlElement

    ''' <summary>
    ''' Returns the name of the Windows Profile or Group
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UserName As String
      Get
        Return _name
      End Get
    End Property
    ''' <summary>
    ''' Returns a list of PermissionsGroup objects related to the current User
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PermissionGroups As List(Of PermissionsGroup)
      Get
        Return _perm
      End Get
      Set(value As List(Of PermissionsGroup))
        _perm = value
      End Set
    End Property

    Public Sub New(ByVal Node As XmlElement)
      If Node IsNot Nothing Then
        _node = Node
        _name = _node.Attributes("name").Value
        Dim lst As XmlNodeList = _node.ChildNodes '.SelectNodes(".//Permissions")
        _perm = New List(Of PermissionsGroup)
        If lst.Count > 0 Then
          For i = 0 To lst.Count - 1 Step 1
            _perm.Add(New PermissionsGroup(lst(i)))
          Next
        End If
      End If
    End Sub

    ''' <summary>
    ''' Represents a list of filesystem permissions relative to a common group name
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PermissionsGroup
      Private _grp As String
      Private _perms As List(Of Permission)
      Private _node As XmlElement

      ''' <summary>
      ''' Returns the PathStructure Permissions group name.
      ''' </summary>
      ''' <value></value>
      ''' <returns>String</returns>
      ''' <remarks></remarks>
      Public ReadOnly Property GroupName As String
        Get
          Return _grp
        End Get
      End Property
      ''' <summary>
      ''' Returns a list of PathStructure Permission objects
      ''' </summary>
      ''' <value></value>
      ''' <returns>List(Of Permission)</returns>
      ''' <remarks></remarks>
      Public Property Permissions As List(Of Permission)
        Get
          Return _perms
        End Get
        Set(value As List(Of Permission))
          _perms = value
        End Set
      End Property
      ''' <summary>
      ''' Returns the PathStructure settings reference XmlElement for the current Permissions group
      ''' </summary>
      ''' <value></value>
      ''' <returns>XmlElement</returns>
      ''' <remarks></remarks>
      Public Property XElement As XmlElement
        Get
          Return _node
        End Get
        Set(value As XmlElement)
          _node = value
        End Set
      End Property

      Public Sub New(ByVal Node As XmlElement)
        If Node IsNot Nothing Then
          _node = Node
          _grp = _node.Attributes("group").Value
          Dim lst As XmlNodeList = _node.ChildNodes '.SelectNodes(".//*[text()]")
          _perms = New List(Of Permission)
          If lst.Count > 0 Then
            For i = 0 To lst.Count - 1 Step 1
              If Not String.IsNullOrEmpty(lst(i).InnerText) Then
                _perms.Add(New Permission(lst(i)))
              End If
            Next
          End If
        End If
      End Sub

      ''' <summary>
      ''' Represents a filesystem permission and whether permission is allowed or denied.
      ''' </summary>
      ''' <remarks></remarks>
      Public Class Permission
        Private _node As XmlElement
        Private _fsr As System.Security.AccessControl.FileSystemRights
        Private _acc As System.Security.AccessControl.AccessControlType

        Public Property Rights As FileSystemRights
          Get
            Return _fsr
          End Get
          Set(value As FileSystemRights)
            _fsr = value
          End Set
        End Property
        Public Property Access As AccessControlType
          Get
            Return _acc
          End Get
          Set(value As AccessControlType)
            _acc = value
          End Set
        End Property
        Public Property XElement As XmlElement
          Get
            Return _node
          End Get
          Set(value As XmlElement)
            _node = value
          End Set
        End Property

        Public Sub New(ByVal Node As XmlElement)
          _node = Node
          Select Case _node.Name
            Case "AppendData"
              _fsr = FileSystemRights.AppendData
            Case "ChangePermissions"
              _fsr = FileSystemRights.ChangePermissions
            Case "CreateDirectories"
              _fsr = FileSystemRights.CreateDirectories
            Case "CreateFiles"
              _fsr = FileSystemRights.CreateFiles
            Case "Delete"
              _fsr = FileSystemRights.Delete
            Case "DeleteSubdirectoriesAndFiles"
              _fsr = FileSystemRights.DeleteSubdirectoriesAndFiles
            Case "ExecuteFile"
              _fsr = FileSystemRights.ExecuteFile
            Case "FullControl"
              _fsr = FileSystemRights.FullControl
            Case "ListDirectory"
              _fsr = FileSystemRights.ListDirectory
            Case "Modify"
              _fsr = FileSystemRights.Modify
            Case "Read"
              _fsr = FileSystemRights.Read
            Case "ReadAndExecute"
              _fsr = FileSystemRights.ReadAndExecute
            Case "ReadAttributes"
              _fsr = FileSystemRights.ReadAttributes
            Case "ReadData"
              _fsr = FileSystemRights.ReadData
            Case "ReadExtendedAttributes"
              _fsr = FileSystemRights.ReadExtendedAttributes
            Case "ReadPermissions"
              _fsr = FileSystemRights.ReadPermissions
            Case "TakeOwnership"
              _fsr = FileSystemRights.TakeOwnership
            Case "Traverse"
              _fsr = FileSystemRights.Traverse
            Case "Write"
              _fsr = FileSystemRights.Write
            Case "WriteAttributes"
              _fsr = FileSystemRights.WriteAttributes
            Case "WriteData"
              _fsr = FileSystemRights.WriteData
            Case "WriteExtendedAttributes"
              _fsr = FileSystemRights.WriteExtendedAttributes
            Case Else
              _fsr = -1
          End Select

          If String.Equals(_node.InnerText.Trim(), "Allow", StringComparison.OrdinalIgnoreCase) Then
            _acc = AccessControlType.Allow
          ElseIf String.Equals(_node.InnerText.Trim(), "Deny", StringComparison.OrdinalIgnoreCase) Then
            _acc = AccessControlType.Deny
          Else
            _acc = -1
          End If
        End Sub
      End Class
    End Class
  End Class
End Class