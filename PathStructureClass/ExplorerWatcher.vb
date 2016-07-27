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
  Private _watcher As System.Timers.Timer
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
  Public Event ExplorerWatcherFound(ByVal URL As String)
  Public Event ExplorerWatcherAborted(ByVal sender As Object, ByVal e As EventArgs)

  Public Sub New(ByVal PathStruct As PathStructure, Optional ByVal PollRate As Integer = 500)
    _pathStruct = PathStruct
    _pollRate = PollRate
    _watcher = New Timers.Timer(_pollRate)
    AddHandler _watcher.Elapsed, AddressOf ExplorerQuery
    _cancel = False
  End Sub

  ''' <summary>
  ''' Begins polling Shell for current contexts of Windows Explorer/Internet Explorer
  ''' </summary>
  ''' <remarks></remarks>
  Public Sub StartWatcher()
    _watcher.Start()
  End Sub
  ''' <summary>
  ''' Requests that the polling abort
  ''' </summary>
  ''' <remarks></remarks>
  Public Sub StopWatcher()
    _cancel = True
    Try
      _watcher.Stop()
    Catch ex As Exception
      Log("{ExplorerWatcher} Abort Failed: " & ex.Message)
    End Try
  End Sub

  Private Sub WindowPathChanged(ByVal URL As String)
    RaiseEvent ExplorerWatcherFound(URL)
  End Sub

  Private Sub ExplorerQuery()
    Dim exShell As New Shell
    If _evt Is Nothing Then
      _evt = New ExplorerWatcherFoundEventArgs()
      AddHandler _evt.PathChanged, AddressOf WindowPathChanged
    End If

    If _pathStruct IsNot Nothing And Not _cancel Then
      '' Get all the open Explorer windows
      Try
        For Each w As ShellBrowserWindow In DirectCast(exShell.Windows, IShellWindows)
          If _cancel Then Exit For
          '' Somehow these are different. They're known to fail so try everything.
          If Not _evt.Contains(w.HWND) Then
            _evt.Add(w)
          End If
          If _evt.Item(_evt.IndexOf(w.HWND)).CheckWindow(w) Then
            RaiseEvent ExplorerWatcherFound(_evt.Item(_evt.IndexOf(w.HWND)).URL)
          End If
        Next
      Catch ex As Exception
        StopWatcher()
        ' '' Go ahead and quit. Chances are that someone closed/opened windows too quickly
        RaiseEvent ExplorerWatcherAborted(Me, New System.UnhandledExceptionEventArgs(ex, True))
      End Try
    Else
      '' Go ahead and quit. Chances are that someone closed/opened windows too quickly
      StopWatcher()
      RaiseEvent ExplorerWatcherAborted(Me, New System.UnhandledExceptionEventArgs(New Exception("Cancel Requested"), True))
    End If
  End Sub
End Class


Public Class ExplorerWatcherFoundEventArgs
  Private _wins As List(Of WindowWatch)

  Public Event PathChanged(ByVal URL As String)

  Default Public Property Item(ByVal Index As Integer) As WindowWatch
    Get
      If Index < _wins.Count Then
        Return _wins(Index)
      Else
        Throw New IndexOutOfRangeException
      End If
    End Get
    Set(value As WindowWatch)
      If Index < _wins.Count Then
        _wins(Index) = value
      Else
        Throw New IndexOutOfRangeException
      End If
    End Set
  End Property

  Public Sub Add(ByVal WindowObject As ShellBrowserWindow)
    If _wins Is Nothing Then _wins = New List(Of WindowWatch)
    If Not Contains(WindowObject.HWND) Then
      Dim tmp As New WindowWatch(WindowObject)

      _wins.Add(New WindowWatch(WindowObject))
    Else
      If _wins(IndexOf(WindowObject.HWND)).CheckWindow(WindowObject) Then
        RaiseEvent PathChanged(_wins(IndexOf(WindowObject.HWND)).URL)
      End If
    End If
  End Sub
  Public Sub Check(ByVal WindowObject As ShellBrowserWindow)
    If Contains(WindowObject.HWND) Then
      If _wins(IndexOf(WindowObject.HWND)).CheckWindow(WindowObject) Then
        RaiseEvent PathChanged(_wins(IndexOf(WindowObject.HWND)).URL)
      End If
    End If
  End Sub
  Public Sub Remove(ByVal Watch As WindowWatch)
    If Contains(Watch.WindowHandle) Then
      RemoveAt(IndexOf(Watch.WindowHandle))
    Else
      Throw New IndexOutOfRangeException
    End If
  End Sub
  Public Sub RemoveAt(ByVal Index As Integer)
    If Index < _wins.Count Then
      _wins.RemoveAt(Index)
    Else
      Throw New IndexOutOfRangeException
    End If
  End Sub

  Private Sub ChildPathChanged(ByVal URL As String, ByVal e As System.EventArgs)
    RaiseEvent PathChanged(URL)
  End Sub
  Private Sub RemoveWindow(ByVal hWindow As ShellBrowserWindow, ByVal e As System.EventArgs)
    If Contains(hWindow.HWND) Then
      RemoveAt(IndexOf(hWindow.HWND))
    End If
  End Sub

  Public Function Contains(ByVal WindowHandle As Integer) As Boolean
    Return IndexOf(WindowHandle) >= 0
  End Function
  Public Function IndexOf(ByVal WindowHandle As Integer) As Integer
    If _wins IsNot Nothing Then
      For i = 0 To _wins.Count - 1 Step 1
        If _wins(i).WindowHandle = WindowHandle Then
          Return i
        End If
      Next
    End If
    Return -1
  End Function

  Public Class WindowWatch
    Private _win As ShellBrowserWindow
    Private _path As String
    Private _handle As Integer

    Public ReadOnly Property URL As String
      Get
        Return _path
      End Get
    End Property
    Public Property Window As ShellBrowserWindow
      Get
        Return _win
      End Get
      Set(value As ShellBrowserWindow)
        _win = value
      End Set
    End Property
    Public ReadOnly Property WindowHandle As Integer
      Get
        Return _handle
      End Get
    End Property

    Public Sub New(ByVal hWindow As ShellBrowserWindow)
      _win = hWindow
      _handle = hWindow.HWND
      CheckWindow(hWindow)
    End Sub

    ''' <summary>
    ''' Return true when the path changes
    ''' </summary>
    ''' <param name="WindowObject"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CheckWindow(ByVal WindowObject As ShellBrowserWindow) As Boolean
      Try
        Dim newPath As String
        If TryCast(_win.Document, IShellFolderViewDual) IsNot Nothing Then
          Dim sh As IShellFolderViewDual = DirectCast(_win.Document, IShellFolderViewDual)

          If sh.FocusedItem IsNot Nothing Then
            newPath = GetUNCPath(sh.FocusedItem.Path)
          End If
        ElseIf TryCast(_win.Document, ShellFolderView) IsNot Nothing Then
          Dim sh As ShellFolderView = DirectCast(_win.Document, ShellFolderView)
          If sh.FocusedItem IsNot Nothing Then
            newPath = GetUNCPath(sh.FocusedItem.Path)
          End If
        End If
        If Not String.Equals(_path, newPath, StringComparison.OrdinalIgnoreCase) Then
          _path = newPath
          Return True
        Else
          Return False
        End If
      Catch ex As Exception
        Log("{Watcher} CheckWindow error: " & ex.Message)
        Return False
      End Try
    End Function
  End Class
End Class