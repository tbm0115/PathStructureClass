Imports System.Xml
Imports Shell32               ' for ShellFolderView
Imports SHDocVw               ' for IShellWindows

''' <summary>
''' Uses Shell to get the users selection(s) within Windows Explorer/Internet Explorer
''' </summary>
''' <remarks></remarks>
Public Class ExplorerWatcher
  Private _pathStruct As PathStructure
  Private _pollRate As Integer
  Private _watcher As System.Threading.Thread
  Private _cancel As Boolean
  Private _evt As ExplorerWatcherFoundEventArgs

  Public ReadOnly Property CurrentFoundPaths As ExplorerWatcherFoundEventArgs
    Get
      Return _evt
    End Get
  End Property

  ''' <summary>
  ''' This event is raised whenever a path selected in Windows Explorer or navigated to in Internet Explorer 
  ''' is successfully validated in Path.IsNamedStructure() as it would in an audit.
  ''' </summary>
  ''' <param name="sender">Current instance of ExplorerWatcher</param>
  ''' <param name="e">An array of Path objects wrapped in a class</param>
  ''' <remarks></remarks>
  Public Event ExplorerWatcherFound(ByVal sender As Object, ByVal e As ExplorerWatcherFoundEventArgs)
  Public Event ExplorerWatcherAborted(ByVal sender As Object, ByVal e As EventArgs)

  Public Class ExplorerWatcherFoundEventArgs
    Private _paths As List(Of PathStructureClass.Path)
    Private _pathsbln As List(Of Boolean)

    Public ReadOnly Property FoundPaths As List(Of PathStructureClass.Path)
      Get
        Return _paths
      End Get
    End Property
    Public ReadOnly Property GoodPaths As List(Of PathStructureClass.Path)
      Get
        Dim lst As New List(Of PathStructureClass.Path)
        For i = 0 To FoundPaths.Count - 1 Step 1
          If _pathsbln(i) Then
            lst.Add(_paths(i))
          End If
        Next
        Return lst
      End Get
    End Property
    Public ReadOnly Property BadPaths As List(Of PathStructureClass.Path)
      Get
        Dim lst As New List(Of PathStructureClass.Path)
        For i = 0 To FoundPaths.Count - 1 Step 1
          If Not _pathsbln(i) Then
            lst.Add(_paths(i))
          End If
        Next
        Return lst
      End Get
    End Property
    Default Public ReadOnly Property Item(ByVal Index As Integer) As PathStructureClass.Path
      Get
        Return _paths(Index)
      End Get
    End Property

    Public Sub New(Optional ByVal ListOfPaths As List(Of PathStructureClass.Path) = Nothing, Optional ByVal ListOfBooleans As List(Of Boolean) = Nothing)
      _paths = IIf(ListOfPaths Is Nothing, New List(Of PathStructureClass.Path), ListOfPaths)
      _pathsbln = IIf(ListOfBooleans Is Nothing, New List(Of Boolean), ListOfBooleans)
    End Sub

    Public Sub Add(ByVal FoundPath As PathStructureClass.Path, ByVal Good As Boolean)
      _paths.Add(FoundPath)
      _pathsbln.Add(Good)
    End Sub
  End Class

  Public Sub New(ByVal PathStruct As PathStructure, Optional ByVal PollRate As Integer = 500)
    _pathStruct = PathStruct
    _pollRate = PollRate
    _watcher = New Threading.Thread(AddressOf ExplorerQuery)
    _cancel = False
  End Sub

  ''' <summary>
  ''' Begins polling Shell for current contexts of Windows Explorer/Internet Explorer
  ''' </summary>
  ''' <remarks></remarks>
  Public Sub StartWatcher()
    If _watcher.ThreadState = Threading.ThreadState.Unstarted Then
      _watcher.Start()
      _cancel = False
    Else
      Throw New ArgumentException("PathStructure: Can only start an initialized Explorer Watcher.")
    End If
  End Sub
  ''' <summary>
  ''' Requests that the polling abort
  ''' </summary>
  ''' <remarks></remarks>
  Public Sub StopWatcher()
    _cancel = True
    If _watcher.ThreadState = Threading.ThreadState.Running Or _watcher.ThreadState = Threading.ThreadState.WaitSleepJoin Or _watcher.ThreadState = Threading.ThreadState.Background Then
      If _watcher.ThreadState = Threading.ThreadState.WaitSleepJoin Then
        _watcher.Interrupt()
      End If
      _watcher.Abort()
    End If
  End Sub

  Private Function GetExplorerPath(Optional ByRef lst As SortedList(Of String, Boolean) = Nothing) As ExplorerWatcherFoundEventArgs
    Dim exShell As New Shell
    If lst Is Nothing Then
      lst = New SortedList(Of String, Boolean)
    End If
    Dim evt As New ExplorerWatcherFoundEventArgs()
    Dim tmp As PathStructureClass.Path
    Dim strTemp As String

    If _pathStruct IsNot Nothing Then
      '' Get all the open Explorer windows
      For Each w As ShellBrowserWindow In DirectCast(exShell.Windows, IShellWindows)
        strTemp = ""
        Try
          '' Somehow these are different. They're known to fail so try everything.
          If TryCast(w.Document, IShellFolderViewDual) IsNot Nothing Then
            Dim sh As IShellFolderViewDual = DirectCast(w.Document, IShellFolderViewDual)
            If sh.FocusedItem IsNot Nothing Then
              strTemp = GetUNCPath(sh.FocusedItem.Path)
            End If
          ElseIf TryCast(w.Document, ShellFolderView) IsNot Nothing Then
            Dim sh As ShellFolderView = DirectCast(w.Document, ShellFolderView)
            If sh.FocusedItem IsNot Nothing Then
              strTemp = GetUNCPath(sh.FocusedItem.Path)
            End If
          End If
        Catch ex As Exception
          _cancel = True
          '' Go ahead and quit. Chances are that someone closed/opened windows too quickly
          RaiseEvent ExplorerWatcherAborted(Me, New System.UnhandledExceptionEventArgs(ex, True))
          Return Nothing
        End Try
        If Not String.IsNullOrEmpty(strTemp) Then
          If _pathStruct.IsInDefaultPath(strTemp) Then
            tmp = New PathStructureClass.Path(_pathStruct, strTemp)
            Dim bln As Boolean = tmp.IsNameStructured()
            lst.Add(tmp.ToString, bln)
            evt.Add(tmp, bln)
          Else
            lst.Add(strTemp, False)
          End If
        End If
      Next
    End If

    Return evt
  End Function
  Private Sub ExplorerQuery()
    Static old As SortedList(Of String, Boolean)
    Dim tmp As New SortedList(Of String, Boolean)
    _evt = GetExplorerPath(tmp)
    '' Check that the GetExplorerPath did not error out
    If _evt IsNot Nothing Then
      Dim blnChanged As Boolean = False
      '' Check for initialized state
      If old Is Nothing Then
        '' Function has been initialized
        old = tmp
        blnChanged = True
      Else
        '' Check if the number of windows has changed
        If tmp.Count = old.Count Then
          '' Iterate through each window to check for changes in paths
          For i = 0 To old.Count - 1 Step 1
            If Not String.Equals(tmp.Keys(i).ToString, old.Keys(i).ToString, StringComparison.OrdinalIgnoreCase) Then
              blnChanged = True
            End If
          Next
        Else
          '' Different number of windows detected
          blnChanged = True
        End If
      End If
      If blnChanged Then
        old = tmp
        RaiseEvent ExplorerWatcherFound(Me, _evt)
      End If
    End If
    '' Check for request to cancel
    If Not _cancel Then
      System.Threading.Thread.Sleep(_pollRate)
      ExplorerQuery()
    Else
      _cancel = False
    End If
  End Sub
End Class
