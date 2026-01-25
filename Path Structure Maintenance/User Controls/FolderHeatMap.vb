Imports System.Drawing
Imports System.IO ', System.Linq

Public Class FolderHeatMap
  Const freqRed = 0.1
  Const freqGreen = 0.1
  Const freqBlue = 0.1
  Const phasRed = 10
  Const phasGreen = 6
  Const phasBlue = 0
  Const indxInterval = 7
  Const amplification = 126
  Const center = 130
  Const length = 55

  Public Shared colors As List(Of Color)

  Public Sub FillColors()
    colors = New List(Of Color)
    For i = 0 To length Step indxInterval
      Dim tmpRed = Math.Sin(freqRed * i + phasRed) * amplification + center
      Dim tmpGreen = Math.Sin(freqGreen * i + phasGreen) * amplification + center
      Dim tmpblue = Math.Sin(freqBlue * i + phasBlue) * amplification + center
      If tmpRed > 255 Then tmpRed = 255
      If tmpRed < 0 Then tmpRed = 0
      If tmpGreen > 255 Then tmpGreen = 255
      If tmpGreen < 0 Then tmpGreen = 0
      If tmpblue > 255 Then tmpblue = 255
      If tmpblue < 0 Then tmpblue = 0

      colors.Add(Color.FromArgb(Math.Round(tmpRed), Math.Round(tmpGreen), Math.Round(tmpblue)))
    Next
  End Sub

  Dim lstFSO As List(Of FileSystemObject)
  Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
    Cursor = Cursors.WaitCursor
    lstObjects.Items.Clear()
    If Not String.IsNullOrEmpty(txtFolder.Text) Then
      If IO.Directory.Exists(txtFolder.Text) Then
        Dim fold As New IO.DirectoryInfo(txtFolder.Text)
        statFolder.Text = fold.Name

        Dim li As ListViewItem
        If Not IsNothing(fold.Parent) Then
          li = New ListViewItem("../", 0)
          li.Tag = fold.Parent.FullName
          lstObjects.Items.Add(li)
        End If

        lstFSO = New List(Of FileSystemObject)
        Dim currentSize As Long = DirectorySize(fold, True, True)
        statSize.Text = ConvertSize2Text(currentSize)

        If lstFSO.Count > 0 Then
          For Each obj As FileSystemObject In lstFSO
            obj.SetRatio(currentSize)
            lstObjects.Items.Add(obj.ListItem)
          Next
        End If

        statProgress.Value = 0
        'Debug.WriteLine(sumSize.ToString & "/" & currentSize.ToString & " (" & ((sumSize / currentSize) * 100) & "%) analyzed")
      Else
        Debug.WriteLine("'" & txtFolder.Text & "' is not a valid directory!")
      End If
    Else
      Debug.WriteLine("Folder box cannot be empty!")
    End If
    Cursor = Cursors.Default
  End Sub

  Public Class FileSystemObject
    Private _name, _path As String
    Private _size As Long
    Private _ratio As Double
    Private _li As ListViewItem
    Public Property Name As String
      Get
        Return _name
      End Get
      Set(value As String)
        _name = value
      End Set
    End Property
    Public Property Path As String
      Get
        Return _path
      End Get
      Set(value As String)
        _path = value
      End Set
    End Property
    Public Property Size As Long
      Get
        Return _size
      End Get
      Set(value As Long)
        _size = value
      End Set
    End Property
    Public ReadOnly Property Ratio As Double
      Get
        Return _ratio
      End Get
    End Property
    Public ReadOnly Property ListItem As ListViewItem
      Get
        Return _li
      End Get
    End Property

    Public Sub New(ByVal File As FileInfo)
      _name = File.Name
      _path = File.FullName
      _li = New ListViewItem(_name, 1)
      _li.Tag = _path
      _size = File.Length
    End Sub
    Public Sub New(ByVal Folder As DirectoryInfo)
      _name = Folder.Name
      _path = Folder.FullName
      _li = New ListViewItem(_name, 0)
      _li.Tag = _path
    End Sub

    Public Function SetRatio(ByVal TopLevelSize As Long) As Integer
      _ratio = (_size / TopLevelSize) * 100
      _li.Text = _name & " (" & CInt(_ratio).ToString & "% " & ConvertSize2Text(_size) & ")"
      _li.BackColor = colors(CInt((_ratio / 100) * (colors.Count - 1)))
      Return _ratio
    End Function
  End Class

  Public Sub UpdateProgressBar(ByVal Value As Integer)
    Static lastValue = Value
    If (Value Mod 10) = 0 And Not lastValue = Value Then
      Debug.WriteLine("Updating Progress Bar at " & Value.ToString)
      '' Set progress bar
      Application.DoEvents()
      statProgress.Value = Value
      lastValue = Value
    End If
  End Sub

  Public Shared Function ConvertSize2Text(ByVal size As Long) As String
    If (size / 1000000000) > 1 Then
      Return (size / 1000000000).ToString("#.##") & "gb"
    ElseIf (size / 1000000) > 1 Then
      Return (size / 1000000).ToString("#.##") & "mb"
    ElseIf (size / 1000) > 1 Then
      Return (size / 1000).ToString("#.##") & "kb"
    Else
      Return size.ToString("#.##") & "bytes"
    End If
  End Function

  Public Function DirectorySize(ByVal dInfo As IO.DirectoryInfo, ByVal includeSubDir As Boolean, Optional ByVal IsTopLevel As Boolean = False) As Long
    Dim totalSize As Long
    If IsTopLevel Then
      Dim dList As DirectoryInfo() = dInfo.GetDirectories()
      Dim dlen = dList.Length
      For i = 0 To dlen - 1 Step 1
        statObject.Text = dList(i).Name
        statSize.Text = ConvertSize2Text(totalSize)
        UpdateProgressBar((i / dlen) * 100)
        'Application.DoEvents()

        lstFSO.Add(New FileSystemObject(dList(i)))
        lstFSO(lstFSO.Count - 1).Size = DirectorySize(dList(i), True)
        totalSize += lstFSO(lstFSO.Count - 1).Size
      Next
      Dim fList As FileInfo() = dInfo.GetFiles()
      dlen = fList.Length
      For i = 0 To dlen - 1 Step 1
        statObject.Text = fList(i).Name
        UpdateProgressBar((i / dlen) * 100)

        lstFSO.Add(New FileSystemObject(fList(i)))
        totalSize += fList(i).Length
      Next
    ElseIf includeSubDir Then
      totalSize = dInfo.EnumerateFiles().Sum(Function(file) file.Length)
      totalSize += dInfo.EnumerateDirectories().Sum(Function(dir) DirectorySize(dir, True))
    End If
    Return totalSize
  End Function

  Private Sub lstObjects_ItemActivate(sender As Object, e As EventArgs) Handles lstObjects.ItemActivate
    If Not IsNothing(lstObjects.FocusedItem.Tag) Then
      If IO.Directory.Exists(lstObjects.FocusedItem.Tag) Then
        txtFolder.Text = lstObjects.FocusedItem.Tag
        btnBrowse_Click(btnBrowse, Nothing)
      ElseIf IO.File.Exists(lstObjects.FocusedItem.Tag) Then
        Process.Start(lstObjects.FocusedItem.Tag)
      End If
    End If
  End Sub

  Private Sub FolderHeatMap_Load(sender As Object, e As EventArgs) Handles Me.Load
    '' Definitely need this....
    FillColors()

    If Not String.IsNullOrEmpty(txtFolder.Text) Then
      btnBrowse_Click(btnBrowse, Nothing)
    End If
  End Sub

  Public Sub New(Optional ByVal Folder As String = "")
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    If Not String.IsNullOrEmpty(Folder) Then
      txtFolder.Text = Folder
    Else
      txtFolder.Text = My.Computer.FileSystem.SpecialDirectories.MyDocuments
    End If
  End Sub
End Class
