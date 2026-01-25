Imports System.Windows.Forms

Public Class SelectFolderDialog
  Private _init As String '= defaultPath
  Private _cur As String = _init
  Public Property Title As String
    Get
      Return Me.Text
    End Get
    Set(value As String)
      Me.Text = value
    End Set
  End Property
  Public Property InitialDirectory As String
    Get
      Return _init
    End Get
    Set(value As String)
      If IO.Directory.Exists(value) Then
        RaiseEvent DirectoryChanged(_init, value)
        _init = value
        _cur = _init
      Else
        Throw New ArgumentException("'" & value & "' is not a valid directory!")
      End If
    End Set
  End Property
  Public ReadOnly Property CurrentDirectory As String
    Get
      Return _cur
    End Get
  End Property

  Private Event DirectoryChanged(ByVal OldDirectory As String, ByVal NewDirectory As String)

  Private Sub DirectoryChanged_Raised(ByVal OldDirectory As String, ByVal NewDirectory As String) Handles Me.DirectoryChanged
    If Not String.IsNullOrEmpty(NewDirectory) Then
      If IO.Directory.Exists(NewDirectory) Then
        _cur = NewDirectory
        txtCurrentPath.Text = NewDirectory

        '' Clear listview
        lstFolders.Items.Clear()
        Dim fold As IO.DirectoryInfo
        For Each Dir As String In IO.Directory.GetDirectories(NewDirectory)
          fold = New IO.DirectoryInfo(Dir)
          lstFolders.Items.Add(fold.Name, 0).Tag = fold.FullName
        Next
      Else
        Throw New ArgumentException("'" & NewDirectory & "' is not a valid directory!")
      End If
    End If
  End Sub

  Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
    If Not IsNothing(lstFolders.FocusedItem) Then
      _cur = lstFolders.FocusedItem.Tag
    End If
    If Not _cur.EndsWith("\") Then _cur += "\"
    Me.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.Close()
  End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    RaiseEvent DirectoryChanged("", _init)
  End Sub

  Private Sub lstFolders_ItemActivate(sender As Object, e As EventArgs) Handles lstFolders.ItemActivate
    If Not IsNothing(lstFolders.FocusedItem) Then
      RaiseEvent DirectoryChanged(_cur, lstFolders.FocusedItem.Tag)
    End If
  End Sub

  Private Sub txtCurrentPath_TextChanged(sender As Object, e As EventArgs) Handles txtCurrentPath.TextChanged
    If IO.Directory.Exists(txtCurrentPath.Text) Then
      RaiseEvent DirectoryChanged(_cur, txtCurrentPath.Text)
    End If
  End Sub

  Private Sub btnParentFolder_Click(sender As Object, e As EventArgs) Handles btnParentFolder.Click
    Dim cur As New IO.DirectoryInfo(_cur)
    If Not IsNothing(cur.Parent) Then
      txtCurrentPath.Text = cur.Parent.FullName
    End If
  End Sub
End Class
