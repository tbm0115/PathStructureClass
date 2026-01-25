Imports System.Windows.Forms

Public Class Generic_Dialog
  Private lbls As String()
  Public ReadOnly Property Values As SortedList(Of String, String)
    Get
      Dim vals As New SortedList(Of String, String)
      For Each pnl As Control In pnlContainer.Controls
        vals.Add(pnl.Controls(1).Tag, pnl.Controls(1).Text)
      Next
      Return vals
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

  Public Sub New(ByVal Labels As String())
    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    For Each str As String In Labels
      Dim pnl As New Panel
      Dim lbl As New Label
      Dim txt As New TextBox

      pnl.Dock = DockStyle.Top
      pnl.Height = 50

      lbl.Dock = DockStyle.Left
      lbl.AutoSize = False
      lbl.TextAlign = ContentAlignment.MiddleRight
      lbl.Text = str
      lbl.Size = New Size(pnlContainer.Width * 0.3, 30)

      txt.Dock = DockStyle.Right
      txt.Size = New Size(pnlContainer.Width * 0.6, 30)
      txt.Tag = str

      pnl.Controls.Add(lbl)
      pnl.Controls.Add(txt)

      pnlContainer.Controls.Add(pnl)
      pnlContainer.Controls.SetChildIndex(pnl, 0)
    Next
  End Sub
End Class
