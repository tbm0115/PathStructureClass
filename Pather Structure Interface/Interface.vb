Imports PathStructureClass

Public Interface IWatcherPlugin
  ReadOnly Property Name() As String
  ReadOnly Property Description() As String
  ReadOnly Property Suite() As String
  Property ReferenceStructure As PathStructure
  ReadOnly Property ShortcutKeys As System.Windows.Forms.Keys

  Sub Run()
  Sub Changed_CurrentPath(ByVal CurPath As Path)

  Event Set_CurrentPath(ByVal FSPath As String)
  Event Get_CurrentPath(ByRef CurPath As Path)
  Event Get_SelectedPath(ByRef SelPath As Path)
End Interface