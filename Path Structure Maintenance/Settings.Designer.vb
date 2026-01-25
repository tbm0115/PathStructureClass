<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Settings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Settings))
    Me.TabControl1 = New System.Windows.Forms.TabControl()
    Me.TabPage1 = New System.Windows.Forms.TabPage()
    Me.chkDeleteThumbsDb = New System.Windows.Forms.CheckBox()
    Me.btnBrowse = New System.Windows.Forms.Button()
    Me.txtPathStructure = New System.Windows.Forms.TextBox()
    Me.Label1 = New System.Windows.Forms.Label()
    Me.TabPage2 = New System.Windows.Forms.TabPage()
    Me.GroupBox1 = New System.Windows.Forms.GroupBox()
    Me.CheckBox12 = New System.Windows.Forms.CheckBox()
    Me.CheckBox11 = New System.Windows.Forms.CheckBox()
    Me.CheckBox10 = New System.Windows.Forms.CheckBox()
    Me.CheckBox6 = New System.Windows.Forms.CheckBox()
    Me.chkTransfer = New System.Windows.Forms.CheckBox()
    Me.CheckBox5 = New System.Windows.Forms.CheckBox()
    Me.CheckBox4 = New System.Windows.Forms.CheckBox()
    Me.CheckBox3 = New System.Windows.Forms.CheckBox()
    Me.CheckBox2 = New System.Windows.Forms.CheckBox()
    Me.CheckBox1 = New System.Windows.Forms.CheckBox()
    Me.btnRemoveContextMenu = New System.Windows.Forms.Button()
    Me.btnAddContextMenu = New System.Windows.Forms.Button()
    Me.TabPage3 = New System.Windows.Forms.TabPage()
    Me.CheckBox9 = New System.Windows.Forms.CheckBox()
    Me.CheckBox8 = New System.Windows.Forms.CheckBox()
    Me.CheckBox7 = New System.Windows.Forms.CheckBox()
    Me.TabPage4 = New System.Windows.Forms.TabPage()
    Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
    Me.Label2 = New System.Windows.Forms.Label()
    Me.txtERPConnectionString = New System.Windows.Forms.TextBox()
    Me.txtTimeStampFormat = New System.Windows.Forms.TextBox()
    Me.Label3 = New System.Windows.Forms.Label()
    Me.TabControl1.SuspendLayout()
    Me.TabPage1.SuspendLayout()
    Me.TabPage2.SuspendLayout()
    Me.GroupBox1.SuspendLayout()
    Me.TabPage3.SuspendLayout()
    Me.TabPage4.SuspendLayout()
    Me.TableLayoutPanel1.SuspendLayout()
    Me.SuspendLayout()
    '
    'TabControl1
    '
    Me.TabControl1.Controls.Add(Me.TabPage1)
    Me.TabControl1.Controls.Add(Me.TabPage2)
    Me.TabControl1.Controls.Add(Me.TabPage3)
    Me.TabControl1.Controls.Add(Me.TabPage4)
    Me.TabControl1.Dock = System.Windows.Forms.DockStyle.Fill
    Me.TabControl1.Location = New System.Drawing.Point(0, 0)
    Me.TabControl1.Name = "TabControl1"
    Me.TabControl1.SelectedIndex = 0
    Me.TabControl1.Size = New System.Drawing.Size(465, 503)
    Me.TabControl1.TabIndex = 0
    '
    'TabPage1
    '
    Me.TabPage1.Controls.Add(Me.txtTimeStampFormat)
    Me.TabPage1.Controls.Add(Me.Label3)
    Me.TabPage1.Controls.Add(Me.chkDeleteThumbsDb)
    Me.TabPage1.Controls.Add(Me.btnBrowse)
    Me.TabPage1.Controls.Add(Me.txtPathStructure)
    Me.TabPage1.Controls.Add(Me.Label1)
    Me.TabPage1.Location = New System.Drawing.Point(4, 34)
    Me.TabPage1.Name = "TabPage1"
    Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
    Me.TabPage1.Size = New System.Drawing.Size(457, 465)
    Me.TabPage1.TabIndex = 0
    Me.TabPage1.Text = "Basic Settings"
    Me.TabPage1.UseVisualStyleBackColor = True
    '
    'chkDeleteThumbsDb
    '
    Me.chkDeleteThumbsDb.AutoSize = True
    Me.chkDeleteThumbsDb.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnDeleteThumbsDb
    Me.chkDeleteThumbsDb.CheckState = System.Windows.Forms.CheckState.Checked
    Me.chkDeleteThumbsDb.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnDeleteThumbsDb", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.chkDeleteThumbsDb.Location = New System.Drawing.Point(13, 77)
    Me.chkDeleteThumbsDb.Name = "chkDeleteThumbsDb"
    Me.chkDeleteThumbsDb.Size = New System.Drawing.Size(413, 29)
    Me.chkDeleteThumbsDb.TabIndex = 12
    Me.chkDeleteThumbsDb.Text = "Delete any 'Thumbs.db' files that are found?"
    Me.chkDeleteThumbsDb.UseVisualStyleBackColor = True
    '
    'btnBrowse
    '
    Me.btnBrowse.Location = New System.Drawing.Point(337, 31)
    Me.btnBrowse.Name = "btnBrowse"
    Me.btnBrowse.Size = New System.Drawing.Size(108, 30)
    Me.btnBrowse.TabIndex = 11
    Me.btnBrowse.Text = "Browse..."
    Me.btnBrowse.UseVisualStyleBackColor = True
    '
    'txtPathStructure
    '
    Me.txtPathStructure.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.Path_Structure_Maintenance.My.MySettings.Default, "SettingsPath", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.txtPathStructure.Location = New System.Drawing.Point(7, 31)
    Me.txtPathStructure.Name = "txtPathStructure"
    Me.txtPathStructure.ReadOnly = True
    Me.txtPathStructure.Size = New System.Drawing.Size(324, 30)
    Me.txtPathStructure.TabIndex = 10
    Me.txtPathStructure.Text = Global.Path_Structure_Maintenance.My.MySettings.Default.SettingsPath
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(8, 3)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(142, 25)
    Me.Label1.TabIndex = 9
    Me.Label1.Text = "Path Structure:"
    '
    'TabPage2
    '
    Me.TabPage2.Controls.Add(Me.GroupBox1)
    Me.TabPage2.Controls.Add(Me.btnRemoveContextMenu)
    Me.TabPage2.Controls.Add(Me.btnAddContextMenu)
    Me.TabPage2.Location = New System.Drawing.Point(4, 34)
    Me.TabPage2.Name = "TabPage2"
    Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
    Me.TabPage2.Size = New System.Drawing.Size(457, 465)
    Me.TabPage2.TabIndex = 1
    Me.TabPage2.Text = "Windows Context Menu"
    Me.TabPage2.UseVisualStyleBackColor = True
    '
    'GroupBox1
    '
    Me.GroupBox1.Controls.Add(Me.CheckBox12)
    Me.GroupBox1.Controls.Add(Me.CheckBox11)
    Me.GroupBox1.Controls.Add(Me.CheckBox10)
    Me.GroupBox1.Controls.Add(Me.CheckBox6)
    Me.GroupBox1.Controls.Add(Me.chkTransfer)
    Me.GroupBox1.Controls.Add(Me.CheckBox5)
    Me.GroupBox1.Controls.Add(Me.CheckBox4)
    Me.GroupBox1.Controls.Add(Me.CheckBox3)
    Me.GroupBox1.Controls.Add(Me.CheckBox2)
    Me.GroupBox1.Controls.Add(Me.CheckBox1)
    Me.GroupBox1.Location = New System.Drawing.Point(8, 6)
    Me.GroupBox1.Name = "GroupBox1"
    Me.GroupBox1.Size = New System.Drawing.Size(440, 379)
    Me.GroupBox1.TabIndex = 11
    Me.GroupBox1.TabStop = False
    Me.GroupBox1.Text = "Context Menu Options"
    '
    'CheckBox12
    '
    Me.CheckBox12.AutoSize = True
    Me.CheckBox12.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnSetPermissions
    Me.CheckBox12.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox12.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnSetPermissions", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox12.Location = New System.Drawing.Point(7, 344)
    Me.CheckBox12.Name = "CheckBox12"
    Me.CheckBox12.Size = New System.Drawing.Size(175, 29)
    Me.CheckBox12.TabIndex = 9
    Me.CheckBox12.Text = "Set Permissions"
    Me.CheckBox12.UseVisualStyleBackColor = True
    '
    'CheckBox11
    '
    Me.CheckBox11.AutoSize = True
    Me.CheckBox11.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnDirectArchive
    Me.CheckBox11.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox11.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnDirectArchive", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox11.Location = New System.Drawing.Point(7, 310)
    Me.CheckBox11.Name = "CheckBox11"
    Me.CheckBox11.Size = New System.Drawing.Size(173, 29)
    Me.CheckBox11.TabIndex = 8
    Me.CheckBox11.Text = "Send to Archive"
    Me.CheckBox11.UseVisualStyleBackColor = True
    '
    'CheckBox10
    '
    Me.CheckBox10.AutoSize = True
    Me.CheckBox10.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnFolderHeatMap
    Me.CheckBox10.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox10.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnFolderHeatMap", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox10.Location = New System.Drawing.Point(7, 275)
    Me.CheckBox10.Name = "CheckBox10"
    Me.CheckBox10.Size = New System.Drawing.Size(179, 29)
    Me.CheckBox10.TabIndex = 7
    Me.CheckBox10.Text = "Folder Heat Map"
    Me.CheckBox10.UseVisualStyleBackColor = True
    '
    'CheckBox6
    '
    Me.CheckBox6.AutoSize = True
    Me.CheckBox6.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnPreview
    Me.CheckBox6.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox6.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnPreview", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox6.Location = New System.Drawing.Point(7, 240)
    Me.CheckBox6.Name = "CheckBox6"
    Me.CheckBox6.Size = New System.Drawing.Size(241, 29)
    Me.CheckBox6.TabIndex = 6
    Me.CheckBox6.Text = "Preview File/Information"
    Me.CheckBox6.UseVisualStyleBackColor = True
    '
    'chkTransfer
    '
    Me.chkTransfer.AutoSize = True
    Me.chkTransfer.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnTransferByExtension
    Me.chkTransfer.CheckState = System.Windows.Forms.CheckState.Checked
    Me.chkTransfer.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnTransferByExtension", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.chkTransfer.Location = New System.Drawing.Point(7, 205)
    Me.chkTransfer.Name = "chkTransfer"
    Me.chkTransfer.Size = New System.Drawing.Size(272, 29)
    Me.chkTransfer.TabIndex = 5
    Me.chkTransfer.Text = "Transfer Files By Extension"
    Me.chkTransfer.UseVisualStyleBackColor = True
    '
    'CheckBox5
    '
    Me.CheckBox5.AutoSize = True
    Me.CheckBox5.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnClipboard
    Me.CheckBox5.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox5.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnClipboard", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox5.Location = New System.Drawing.Point(7, 170)
    Me.CheckBox5.Name = "CheckBox5"
    Me.CheckBox5.Size = New System.Drawing.Size(358, 29)
    Me.CheckBox5.TabIndex = 4
    Me.CheckBox5.Text = "Create/Send File Format to Clipboard"
    Me.CheckBox5.UseVisualStyleBackColor = True
    '
    'CheckBox4
    '
    Me.CheckBox4.AutoSize = True
    Me.CheckBox4.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnAudit
    Me.CheckBox4.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox4.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnAudit", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox4.Location = New System.Drawing.Point(7, 135)
    Me.CheckBox4.Name = "CheckBox4"
    Me.CheckBox4.Size = New System.Drawing.Size(79, 29)
    Me.CheckBox4.TabIndex = 3
    Me.CheckBox4.Text = "Audit"
    Me.CheckBox4.UseVisualStyleBackColor = True
    '
    'CheckBox3
    '
    Me.CheckBox3.AutoSize = True
    Me.CheckBox3.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnFormat
    Me.CheckBox3.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox3.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnFormat", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox3.Location = New System.Drawing.Point(7, 100)
    Me.CheckBox3.Name = "CheckBox3"
    Me.CheckBox3.Size = New System.Drawing.Size(188, 29)
    Me.CheckBox3.TabIndex = 2
    Me.CheckBox3.Text = "Format File Name"
    Me.CheckBox3.UseVisualStyleBackColor = True
    '
    'CheckBox2
    '
    Me.CheckBox2.AutoSize = True
    Me.CheckBox2.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnAddSingle
    Me.CheckBox2.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox2.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnAddSingle", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox2.Location = New System.Drawing.Point(7, 65)
    Me.CheckBox2.Name = "CheckBox2"
    Me.CheckBox2.Size = New System.Drawing.Size(190, 29)
    Me.CheckBox2.TabIndex = 1
    Me.CheckBox2.Text = "Add Single Folder"
    Me.CheckBox2.UseVisualStyleBackColor = True
    '
    'CheckBox1
    '
    Me.CheckBox1.AutoSize = True
    Me.CheckBox1.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnAddAll
    Me.CheckBox1.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox1.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnAddAll", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox1.Location = New System.Drawing.Point(7, 30)
    Me.CheckBox1.Name = "CheckBox1"
    Me.CheckBox1.Size = New System.Drawing.Size(215, 29)
    Me.CheckBox1.TabIndex = 0
    Me.CheckBox1.Text = "Add All Main Folders"
    Me.CheckBox1.UseVisualStyleBackColor = True
    '
    'btnRemoveContextMenu
    '
    Me.btnRemoveContextMenu.Location = New System.Drawing.Point(232, 391)
    Me.btnRemoveContextMenu.Name = "btnRemoveContextMenu"
    Me.btnRemoveContextMenu.Size = New System.Drawing.Size(216, 66)
    Me.btnRemoveContextMenu.TabIndex = 10
    Me.btnRemoveContextMenu.Text = "Remove Windows Context Menu"
    Me.btnRemoveContextMenu.UseVisualStyleBackColor = True
    '
    'btnAddContextMenu
    '
    Me.btnAddContextMenu.Location = New System.Drawing.Point(10, 391)
    Me.btnAddContextMenu.Name = "btnAddContextMenu"
    Me.btnAddContextMenu.Size = New System.Drawing.Size(216, 66)
    Me.btnAddContextMenu.TabIndex = 9
    Me.btnAddContextMenu.Text = "Add/Update Windows Context Menu"
    Me.btnAddContextMenu.UseVisualStyleBackColor = True
    '
    'TabPage3
    '
    Me.TabPage3.Controls.Add(Me.CheckBox9)
    Me.TabPage3.Controls.Add(Me.CheckBox8)
    Me.TabPage3.Controls.Add(Me.CheckBox7)
    Me.TabPage3.Location = New System.Drawing.Point(4, 34)
    Me.TabPage3.Name = "TabPage3"
    Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
    Me.TabPage3.Size = New System.Drawing.Size(457, 465)
    Me.TabPage3.TabIndex = 2
    Me.TabPage3.Text = "Reports"
    Me.TabPage3.UseVisualStyleBackColor = True
    '
    'CheckBox9
    '
    Me.CheckBox9.AutoSize = True
    Me.CheckBox9.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnReportOptimal
    Me.CheckBox9.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox9.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnReportOptimal", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox9.Location = New System.Drawing.Point(9, 79)
    Me.CheckBox9.Name = "CheckBox9"
    Me.CheckBox9.Size = New System.Drawing.Size(309, 29)
    Me.CheckBox9.TabIndex = 2
    Me.CheckBox9.Text = "Report Audit Optimal Messages"
    Me.CheckBox9.UseVisualStyleBackColor = True
    '
    'CheckBox8
    '
    Me.CheckBox8.AutoSize = True
    Me.CheckBox8.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnReportInformation
    Me.CheckBox8.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox8.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnReportInformation", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox8.Location = New System.Drawing.Point(9, 43)
    Me.CheckBox8.Name = "CheckBox8"
    Me.CheckBox8.Size = New System.Drawing.Size(338, 29)
    Me.CheckBox8.TabIndex = 1
    Me.CheckBox8.Text = "Report Audit Information Messages"
    Me.CheckBox8.UseVisualStyleBackColor = True
    '
    'CheckBox7
    '
    Me.CheckBox7.AutoSize = True
    Me.CheckBox7.Checked = Global.Path_Structure_Maintenance.My.MySettings.Default.blnReportErrors
    Me.CheckBox7.CheckState = System.Windows.Forms.CheckState.Checked
    Me.CheckBox7.DataBindings.Add(New System.Windows.Forms.Binding("Checked", Global.Path_Structure_Maintenance.My.MySettings.Default, "blnReportErrors", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.CheckBox7.Location = New System.Drawing.Point(9, 7)
    Me.CheckBox7.Name = "CheckBox7"
    Me.CheckBox7.Size = New System.Drawing.Size(294, 29)
    Me.CheckBox7.TabIndex = 0
    Me.CheckBox7.Text = "Report Audit Errors Messages"
    Me.CheckBox7.UseVisualStyleBackColor = True
    '
    'TabPage4
    '
    Me.TabPage4.Controls.Add(Me.TableLayoutPanel1)
    Me.TabPage4.Location = New System.Drawing.Point(4, 34)
    Me.TabPage4.Name = "TabPage4"
    Me.TabPage4.Padding = New System.Windows.Forms.Padding(3)
    Me.TabPage4.Size = New System.Drawing.Size(457, 465)
    Me.TabPage4.TabIndex = 3
    Me.TabPage4.Text = "ERP"
    Me.TabPage4.UseVisualStyleBackColor = True
    '
    'TableLayoutPanel1
    '
    Me.TableLayoutPanel1.ColumnCount = 2
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.79775!))
    Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.20225!))
    Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 0)
    Me.TableLayoutPanel1.Controls.Add(Me.txtERPConnectionString, 1, 0)
    Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top
    Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 3)
    Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
    Me.TableLayoutPanel1.Padding = New System.Windows.Forms.Padding(3)
    Me.TableLayoutPanel1.RowCount = 1
    Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
    Me.TableLayoutPanel1.Size = New System.Drawing.Size(451, 40)
    Me.TableLayoutPanel1.TabIndex = 0
    '
    'Label2
    '
    Me.Label2.Dock = System.Windows.Forms.DockStyle.Fill
    Me.Label2.Location = New System.Drawing.Point(6, 3)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(179, 34)
    Me.Label2.TabIndex = 0
    Me.Label2.Text = "Connection String"
    Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
    '
    'txtERPConnectionString
    '
    Me.txtERPConnectionString.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.Path_Structure_Maintenance.My.MySettings.Default, "ERPConnection", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.txtERPConnectionString.Dock = System.Windows.Forms.DockStyle.Fill
    Me.txtERPConnectionString.Location = New System.Drawing.Point(191, 6)
    Me.txtERPConnectionString.Name = "txtERPConnectionString"
    Me.txtERPConnectionString.Size = New System.Drawing.Size(254, 30)
    Me.txtERPConnectionString.TabIndex = 1
    Me.txtERPConnectionString.Text = Global.Path_Structure_Maintenance.My.MySettings.Default.ERPConnection
    '
    'txtTimeStampFormat
    '
    Me.txtTimeStampFormat.DataBindings.Add(New System.Windows.Forms.Binding("Text", Global.Path_Structure_Maintenance.My.MySettings.Default, "strTimeStampFormat", True, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged))
    Me.txtTimeStampFormat.Location = New System.Drawing.Point(5, 151)
    Me.txtTimeStampFormat.Name = "txtTimeStampFormat"
    Me.txtTimeStampFormat.ReadOnly = True
    Me.txtTimeStampFormat.Size = New System.Drawing.Size(324, 30)
    Me.txtTimeStampFormat.TabIndex = 14
    Me.txtTimeStampFormat.Text = Global.Path_Structure_Maintenance.My.MySettings.Default.strTimeStampFormat
    '
    'Label3
    '
    Me.Label3.AutoSize = True
    Me.Label3.Location = New System.Drawing.Point(6, 123)
    Me.Label3.Name = "Label3"
    Me.Label3.Size = New System.Drawing.Size(190, 25)
    Me.Label3.TabIndex = 13
    Me.Label3.Text = "Time Stamp Format:"
    '
    'Settings
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(465, 503)
    Me.Controls.Add(Me.TabControl1)
    Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
    Me.Name = "Settings"
    Me.Text = "Settings"
    Me.TabControl1.ResumeLayout(False)
    Me.TabPage1.ResumeLayout(False)
    Me.TabPage1.PerformLayout()
    Me.TabPage2.ResumeLayout(False)
    Me.GroupBox1.ResumeLayout(False)
    Me.GroupBox1.PerformLayout()
    Me.TabPage3.ResumeLayout(False)
    Me.TabPage3.PerformLayout()
    Me.TabPage4.ResumeLayout(False)
    Me.TableLayoutPanel1.ResumeLayout(False)
    Me.TableLayoutPanel1.PerformLayout()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
  Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
  Friend WithEvents btnBrowse As System.Windows.Forms.Button
  Friend WithEvents txtPathStructure As System.Windows.Forms.TextBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
  Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
  Friend WithEvents CheckBox6 As System.Windows.Forms.CheckBox
  Friend WithEvents chkTransfer As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox5 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox4 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox3 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox2 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
  Friend WithEvents btnRemoveContextMenu As System.Windows.Forms.Button
  Friend WithEvents btnAddContextMenu As System.Windows.Forms.Button
  Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
  Friend WithEvents CheckBox9 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox8 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox7 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox10 As System.Windows.Forms.CheckBox
  Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
  Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents txtERPConnectionString As System.Windows.Forms.TextBox
  Friend WithEvents chkDeleteThumbsDb As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox11 As System.Windows.Forms.CheckBox
  Friend WithEvents CheckBox12 As System.Windows.Forms.CheckBox
  Friend WithEvents txtTimeStampFormat As System.Windows.Forms.TextBox
  Friend WithEvents Label3 As System.Windows.Forms.Label
End Class
