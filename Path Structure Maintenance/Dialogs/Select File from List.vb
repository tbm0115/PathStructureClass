Imports System.Windows.Forms

Public Class Select_File_from_List
  Public ReadOnly Property SelectedPath As String
    Get
      If drpFileSelect.SelectedIndex >= 0 Then
        Debug.WriteLine("Selected: " & drpFileSelect.SelectedItem.ToString)
          Return drpFileSelect.SelectedItem.ToString
      End If
      Return ""
    End Get
  End Property

  Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
    Me.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.Close()
  End Sub

  Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
    Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.Close()
  End Sub

  Public Sub New(ByVal PathList As List(Of PathStructureClass.Path))

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    drpFileSelect.Items.Clear()
    For i = 0 To PathList.Count - 1 Step 1
      drpFileSelect.Items.Add(PathList(i).UNCPath)
    Next
  End Sub
  Public Sub New(ByVal PathList As String())

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    drpFileSelect.Items.Clear()
    For i = 0 To PathList.Length - 1 Step 1
      drpFileSelect.Items.Add(PathList(i))
    Next
  End Sub
End Class
