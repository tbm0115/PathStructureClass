Imports System.Windows.Forms

Public Class Select_Default_Path
  Public ReadOnly Property DefaultPath As String
    Get
      Dim _x As Xml.XmlNode
      If drpDefaultPath.SelectedIndex >= 0 Then
        Debug.WriteLine("Selected: " & drpDefaultPath.SelectedItem.ToString)
        _x = Main.PathStruct.Settings.SelectSingleNode("//Structure[@name='" & drpDefaultPath.SelectedItem.ToString & "']/@defaultPath")
        If _x IsNot Nothing Then
          Return _x.Value
        End If
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

  Public Sub New()

    ' This call is required by the designer.
    InitializeComponent()

    ' Add any initialization after the InitializeComponent() call.
    drpDefaultPath.Items.Clear()
    Dim names As Xml.XmlNodeList = Main.PathStruct.Settings.SelectNodes("//Structure/@name")
    For i = 0 To names.Count - 1 Step 1
      drpDefaultPath.Items.Add(names(i).Value)
    Next
  End Sub
End Class
