Imports PathStructureClass

Public Class FixAudit
  Private _errPaths As List(Of PathStructureClass.Path)
  Public Sub New(ByVal Paths As List(Of PathStructureClass.Path)) ' PathStructure.AuditReport)
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    _errPaths = Paths
    If Not IsNothing(_errPaths) Then
      If _errPaths.Count > 0 Then
        lstFSO.Items.Clear()
        For Each p As PathStructureClass.Path In _errPaths
          lstFSO.Items.Add(p.UNCPath)
        Next
        If lstFSO.Items.Count = 1 Then
          lstFSO.SelectedIndex = 0
        End If
      End If
    End If
  End Sub

  Private Sub lstFSO_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstFSO.SelectedIndexChanged
    If lstFSO.SelectedIndex = -1 Then Exit Sub

    pnlFormat.Controls.Clear()
    Dim fsoFormat As New Format_Item(lstFSO.SelectedItem.ToString, , False)
    fsoFormat.Dock = DockStyle.Fill
    AddHandler fsoFormat.Accepted, AddressOf FormatAccepted
    pnlFormat.Controls.Add(fsoFormat)
  End Sub

  Private Sub FormatAccepted(ByVal sender As Object, ByVal e As FormatItemAcceptedEventArgs)
    lstFSO.Items.RemoveAt(lstFSO.SelectedIndex)
    pnlFormat.Controls.Clear()
  End Sub
End Class
