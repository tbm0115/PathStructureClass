Imports System.Windows.Forms

Public Class FormatDialog

  Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Me.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.Close()
  End Sub

  Private _format As Format_Item
  Public Sub New(ByVal CurrentPath As String, ByVal PathStruct As PathStructureClass.PathStructure, ByVal PathStructureName As String, ByVal Path As PathStructureClass.Path)
    InitializeComponent()

    Me.DialogResult = Windows.Forms.DialogResult.Cancel

    _format = New Format_Item(CurrentPath, False, False, Path)
    _format.Dock = DockStyle.Fill
    AddHandler _format.Accepted, Sub()
                                   Me.Close()
                                 End Sub
    For i = 0 To _format.cmbFiles.Items.Count - 1 Step 1
      If _format.cmbFiles.Items(i) = PathStructureName Then
        _format.cmbFiles.SelectedIndex = i
      End If
    Next
    pnlContainer.Controls.Add(_format)

  End Sub
End Class
