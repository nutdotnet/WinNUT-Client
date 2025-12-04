<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WinNUT
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WinNUT))
        Me.NotifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ContextMenu_Systray = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.Menu_Sys_Settings = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Sys_Sep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.Menu_Sys_Update = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Sys_Sep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.Menu_Sys_About = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Sys_Sep3 = New System.Windows.Forms.ToolStripSeparator()
        Me.Menu_Sys_Exit = New System.Windows.Forms.ToolStripMenuItem()
        Me.Main_Menu = New System.Windows.Forms.MenuStrip()
        Me.Menu_File = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_UPS_Var = New System.Windows.Forms.ToolStripMenuItem()
        Me.ManageOldPrefsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Quit = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Connection = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Persist = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.Menu_Connect = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Disconnect = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Settings = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Help = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_About = New System.Windows.Forms.ToolStripMenuItem()
        Me.Menu_Help_Sep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.Menu_Update = New System.Windows.Forms.ToolStripMenuItem()
        Me.GB_Status = New System.Windows.Forms.GroupBox()
        Me.Lbl_VSerial = New System.Windows.Forms.Label()
        Me.Lbl_VFirmware = New System.Windows.Forms.Label()
        Me.Lbl_VName = New System.Windows.Forms.Label()
        Me.Lbl_VMfr = New System.Windows.Forms.Label()
        Me.Lbl_VRTime = New System.Windows.Forms.Label()
        Me.Lbl_VOB = New System.Windows.Forms.Label()
        Me.Lbl_VOLoad = New System.Windows.Forms.Label()
        Me.Lbl_VBL = New System.Windows.Forms.Label()
        Me.Lbl_VOL = New System.Windows.Forms.Label()
        Me.Lbl_Firmware = New System.Windows.Forms.Label()
        Me.Lbl_Serial = New System.Windows.Forms.Label()
        Me.Lbl_Name = New System.Windows.Forms.Label()
        Me.Lbl_Mfr = New System.Windows.Forms.Label()
        Me.Lbl_RTime = New System.Windows.Forms.Label()
        Me.Lbl_BL = New System.Windows.Forms.Label()
        Me.Lbl_OLoad = New System.Windows.Forms.Label()
        Me.Lbl_OB = New System.Windows.Forms.Label()
        Me.Lbl_OL = New System.Windows.Forms.Label()
        Me.GB_InV_Dial = New System.Windows.Forms.GroupBox()
        Me.AG_InV = New WinNUT_Client.Controls.UPSVarGauge()
        Me.Lbl_InV_Dial = New System.Windows.Forms.Label()
        Me.GB_OutV_Dial = New System.Windows.Forms.GroupBox()
        Me.AG_OutV = New WinNUT_Client.Controls.UPSVarGauge()
        Me.Lbl_OutV_Dial = New System.Windows.Forms.Label()
        Me.GB_BattCh_Dial = New System.Windows.Forms.GroupBox()
        Me.PBox_Battery_State = New System.Windows.Forms.PictureBox()
        Me.Lbl_BattCh_Dial = New System.Windows.Forms.Label()
        Me.AG_BattCh = New WinNUT_Client.Controls.UPSVarGauge()
        Me.GB_Load_Dial = New System.Windows.Forms.GroupBox()
        Me.AG_Load = New WinNUT_Client.Controls.UPSVarGauge()
        Me.Lbl_Load_Dial = New System.Windows.Forms.Label()
        Me.GB_BattV_Dial = New System.Windows.Forms.GroupBox()
        Me.AG_BattV = New WinNUT_Client.Controls.UPSVarGauge()
        Me.Lbl_BattV_Dial = New System.Windows.Forms.Label()
        Me.GB_InF_Dial = New System.Windows.Forms.GroupBox()
        Me.AG_InF = New WinNUT_Client.Controls.UPSVarGauge()
        Me.Lbl_InF_Dial = New System.Windows.Forms.Label()
        Me.CB_CurrentLog = New System.Windows.Forms.ComboBox()
        Me.ContextMenu_Systray.SuspendLayout()
        Me.Main_Menu.SuspendLayout()
        Me.GB_Status.SuspendLayout()
        Me.GB_InV_Dial.SuspendLayout()
        Me.GB_OutV_Dial.SuspendLayout()
        Me.GB_BattCh_Dial.SuspendLayout()
        CType(Me.PBox_Battery_State, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GB_Load_Dial.SuspendLayout()
        Me.GB_BattV_Dial.SuspendLayout()
        Me.GB_InF_Dial.SuspendLayout()
        Me.SuspendLayout()
        '
        'NotifyIcon
        '
        Me.NotifyIcon.ContextMenuStrip = Me.ContextMenu_Systray
        resources.ApplyResources(Me.NotifyIcon, "NotifyIcon")
        '
        'ContextMenu_Systray
        '
        Me.ContextMenu_Systray.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Menu_Sys_Settings, Me.Menu_Sys_Sep1, Me.Menu_Sys_Update, Me.Menu_Sys_Sep2, Me.Menu_Sys_About, Me.Menu_Sys_Sep3, Me.Menu_Sys_Exit})
        Me.ContextMenu_Systray.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.ContextMenu_Systray, "ContextMenu_Systray")
        '
        'Menu_Sys_Settings
        '
        Me.Menu_Sys_Settings.Name = "Menu_Sys_Settings"
        resources.ApplyResources(Me.Menu_Sys_Settings, "Menu_Sys_Settings")
        '
        'Menu_Sys_Sep1
        '
        Me.Menu_Sys_Sep1.Name = "Menu_Sys_Sep1"
        resources.ApplyResources(Me.Menu_Sys_Sep1, "Menu_Sys_Sep1")
        '
        'Menu_Sys_Update
        '
        Me.Menu_Sys_Update.Name = "Menu_Sys_Update"
        resources.ApplyResources(Me.Menu_Sys_Update, "Menu_Sys_Update")
        '
        'Menu_Sys_Sep2
        '
        Me.Menu_Sys_Sep2.Name = "Menu_Sys_Sep2"
        resources.ApplyResources(Me.Menu_Sys_Sep2, "Menu_Sys_Sep2")
        '
        'Menu_Sys_About
        '
        Me.Menu_Sys_About.Name = "Menu_Sys_About"
        resources.ApplyResources(Me.Menu_Sys_About, "Menu_Sys_About")
        '
        'Menu_Sys_Sep3
        '
        Me.Menu_Sys_Sep3.Name = "Menu_Sys_Sep3"
        resources.ApplyResources(Me.Menu_Sys_Sep3, "Menu_Sys_Sep3")
        '
        'Menu_Sys_Exit
        '
        Me.Menu_Sys_Exit.Name = "Menu_Sys_Exit"
        resources.ApplyResources(Me.Menu_Sys_Exit, "Menu_Sys_Exit")
        '
        'Main_Menu
        '
        resources.ApplyResources(Me.Main_Menu, "Main_Menu")
        Me.Main_Menu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Menu_File, Me.Menu_Connection, Me.Menu_Settings, Me.Menu_Help})
        Me.Main_Menu.Name = "Main_Menu"
        '
        'Menu_File
        '
        Me.Menu_File.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Menu_UPS_Var, Me.ManageOldPrefsToolStripMenuItem, Me.Menu_Quit})
        Me.Menu_File.Name = "Menu_File"
        resources.ApplyResources(Me.Menu_File, "Menu_File")
        '
        'Menu_UPS_Var
        '
        resources.ApplyResources(Me.Menu_UPS_Var, "Menu_UPS_Var")
        Me.Menu_UPS_Var.Name = "Menu_UPS_Var"
        '
        'ManageOldPrefsToolStripMenuItem
        '
        resources.ApplyResources(Me.ManageOldPrefsToolStripMenuItem, "ManageOldPrefsToolStripMenuItem")
        Me.ManageOldPrefsToolStripMenuItem.Image = Global.WinNUT_Client.My.Resources.Resources.regedit_exe_14_100_0
        Me.ManageOldPrefsToolStripMenuItem.Name = "ManageOldPrefsToolStripMenuItem"
        '
        'Menu_Quit
        '
        Me.Menu_Quit.Name = "Menu_Quit"
        resources.ApplyResources(Me.Menu_Quit, "Menu_Quit")
        '
        'Menu_Connection
        '
        Me.Menu_Connection.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Menu_Persist, Me.ToolStripSeparator1, Me.Menu_Connect, Me.Menu_Disconnect})
        Me.Menu_Connection.Name = "Menu_Connection"
        resources.ApplyResources(Me.Menu_Connection, "Menu_Connection")
        '
        'Menu_Persist
        '
        Me.Menu_Persist.Checked = Global.WinNUT_Client.My.MySettings.Default.NUT_AutoReconnect
        Me.Menu_Persist.CheckOnClick = True
        Me.Menu_Persist.Image = Global.WinNUT_Client.My.Resources.Resources.RepeatHS
        Me.Menu_Persist.Name = "Menu_Persist"
        resources.ApplyResources(Me.Menu_Persist, "Menu_Persist")
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'Menu_Connect
        '
        Me.Menu_Connect.Image = Global.WinNUT_Client.My.Resources.Resources.internetconnection
        Me.Menu_Connect.Name = "Menu_Connect"
        resources.ApplyResources(Me.Menu_Connect, "Menu_Connect")
        '
        'Menu_Disconnect
        '
        resources.ApplyResources(Me.Menu_Disconnect, "Menu_Disconnect")
        Me.Menu_Disconnect.Image = Global.WinNUT_Client.My.Resources.Resources.disconnect2
        Me.Menu_Disconnect.Name = "Menu_Disconnect"
        '
        'Menu_Settings
        '
        Me.Menu_Settings.Name = "Menu_Settings"
        resources.ApplyResources(Me.Menu_Settings, "Menu_Settings")
        '
        'Menu_Help
        '
        Me.Menu_Help.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Menu_About, Me.Menu_Help_Sep1, Me.Menu_Update})
        Me.Menu_Help.Name = "Menu_Help"
        resources.ApplyResources(Me.Menu_Help, "Menu_Help")
        '
        'Menu_About
        '
        Me.Menu_About.Name = "Menu_About"
        resources.ApplyResources(Me.Menu_About, "Menu_About")
        '
        'Menu_Help_Sep1
        '
        Me.Menu_Help_Sep1.Name = "Menu_Help_Sep1"
        resources.ApplyResources(Me.Menu_Help_Sep1, "Menu_Help_Sep1")
        '
        'Menu_Update
        '
        Me.Menu_Update.Name = "Menu_Update"
        resources.ApplyResources(Me.Menu_Update, "Menu_Update")
        '
        'GB_Status
        '
        resources.ApplyResources(Me.GB_Status, "GB_Status")
        Me.GB_Status.Controls.Add(Me.Lbl_VSerial)
        Me.GB_Status.Controls.Add(Me.Lbl_VFirmware)
        Me.GB_Status.Controls.Add(Me.Lbl_VName)
        Me.GB_Status.Controls.Add(Me.Lbl_VMfr)
        Me.GB_Status.Controls.Add(Me.Lbl_VRTime)
        Me.GB_Status.Controls.Add(Me.Lbl_VOB)
        Me.GB_Status.Controls.Add(Me.Lbl_VOLoad)
        Me.GB_Status.Controls.Add(Me.Lbl_VBL)
        Me.GB_Status.Controls.Add(Me.Lbl_VOL)
        Me.GB_Status.Controls.Add(Me.Lbl_Firmware)
        Me.GB_Status.Controls.Add(Me.Lbl_Serial)
        Me.GB_Status.Controls.Add(Me.Lbl_Name)
        Me.GB_Status.Controls.Add(Me.Lbl_Mfr)
        Me.GB_Status.Controls.Add(Me.Lbl_RTime)
        Me.GB_Status.Controls.Add(Me.Lbl_BL)
        Me.GB_Status.Controls.Add(Me.Lbl_OLoad)
        Me.GB_Status.Controls.Add(Me.Lbl_OB)
        Me.GB_Status.Controls.Add(Me.Lbl_OL)
        Me.GB_Status.Name = "GB_Status"
        Me.GB_Status.TabStop = False
        '
        'Lbl_VSerial
        '
        Me.Lbl_VSerial.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VSerial, "Lbl_VSerial")
        Me.Lbl_VSerial.Name = "Lbl_VSerial"
        '
        'Lbl_VFirmware
        '
        Me.Lbl_VFirmware.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VFirmware, "Lbl_VFirmware")
        Me.Lbl_VFirmware.Name = "Lbl_VFirmware"
        Me.Lbl_VFirmware.UseCompatibleTextRendering = True
        '
        'Lbl_VName
        '
        Me.Lbl_VName.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VName, "Lbl_VName")
        Me.Lbl_VName.Name = "Lbl_VName"
        '
        'Lbl_VMfr
        '
        Me.Lbl_VMfr.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VMfr, "Lbl_VMfr")
        Me.Lbl_VMfr.Name = "Lbl_VMfr"
        '
        'Lbl_VRTime
        '
        Me.Lbl_VRTime.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VRTime, "Lbl_VRTime")
        Me.Lbl_VRTime.Name = "Lbl_VRTime"
        '
        'Lbl_VOB
        '
        Me.Lbl_VOB.BackColor = System.Drawing.Color.White
        Me.Lbl_VOB.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Lbl_VOB.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VOB, "Lbl_VOB")
        Me.Lbl_VOB.Name = "Lbl_VOB"
        '
        'Lbl_VOLoad
        '
        Me.Lbl_VOLoad.BackColor = System.Drawing.Color.White
        Me.Lbl_VOLoad.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Lbl_VOLoad.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VOLoad, "Lbl_VOLoad")
        Me.Lbl_VOLoad.Name = "Lbl_VOLoad"
        '
        'Lbl_VBL
        '
        Me.Lbl_VBL.BackColor = System.Drawing.Color.White
        Me.Lbl_VBL.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Lbl_VBL.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VBL, "Lbl_VBL")
        Me.Lbl_VBL.Name = "Lbl_VBL"
        '
        'Lbl_VOL
        '
        Me.Lbl_VOL.BackColor = System.Drawing.Color.White
        Me.Lbl_VOL.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Lbl_VOL.CausesValidation = False
        resources.ApplyResources(Me.Lbl_VOL, "Lbl_VOL")
        Me.Lbl_VOL.Name = "Lbl_VOL"
        '
        'Lbl_Firmware
        '
        Me.Lbl_Firmware.CausesValidation = False
        resources.ApplyResources(Me.Lbl_Firmware, "Lbl_Firmware")
        Me.Lbl_Firmware.Name = "Lbl_Firmware"
        Me.Lbl_Firmware.UseCompatibleTextRendering = True
        '
        'Lbl_Serial
        '
        Me.Lbl_Serial.CausesValidation = False
        resources.ApplyResources(Me.Lbl_Serial, "Lbl_Serial")
        Me.Lbl_Serial.Name = "Lbl_Serial"
        '
        'Lbl_Name
        '
        Me.Lbl_Name.CausesValidation = False
        resources.ApplyResources(Me.Lbl_Name, "Lbl_Name")
        Me.Lbl_Name.Name = "Lbl_Name"
        '
        'Lbl_Mfr
        '
        Me.Lbl_Mfr.CausesValidation = False
        resources.ApplyResources(Me.Lbl_Mfr, "Lbl_Mfr")
        Me.Lbl_Mfr.Name = "Lbl_Mfr"
        '
        'Lbl_RTime
        '
        Me.Lbl_RTime.CausesValidation = False
        resources.ApplyResources(Me.Lbl_RTime, "Lbl_RTime")
        Me.Lbl_RTime.Name = "Lbl_RTime"
        '
        'Lbl_BL
        '
        Me.Lbl_BL.CausesValidation = False
        resources.ApplyResources(Me.Lbl_BL, "Lbl_BL")
        Me.Lbl_BL.Name = "Lbl_BL"
        '
        'Lbl_OLoad
        '
        Me.Lbl_OLoad.CausesValidation = False
        resources.ApplyResources(Me.Lbl_OLoad, "Lbl_OLoad")
        Me.Lbl_OLoad.Name = "Lbl_OLoad"
        '
        'Lbl_OB
        '
        Me.Lbl_OB.CausesValidation = False
        resources.ApplyResources(Me.Lbl_OB, "Lbl_OB")
        Me.Lbl_OB.Name = "Lbl_OB"
        '
        'Lbl_OL
        '
        Me.Lbl_OL.CausesValidation = False
        resources.ApplyResources(Me.Lbl_OL, "Lbl_OL")
        Me.Lbl_OL.Name = "Lbl_OL"
        '
        'GB_InV_Dial
        '
        resources.ApplyResources(Me.GB_InV_Dial, "GB_InV_Dial")
        Me.GB_InV_Dial.Controls.Add(Me.AG_InV)
        Me.GB_InV_Dial.Controls.Add(Me.Lbl_InV_Dial)
        Me.GB_InV_Dial.Name = "GB_InV_Dial"
        Me.GB_InV_Dial.TabStop = False
        '
        'AG_InV
        '
        Me.AG_InV.BaseArcRadius = 45
        Me.AG_InV.BaseArcWidth = 5
        Me.AG_InV.GradientOrientation = WinNUT_Client.Controls.UPSVarGauge.GradientOrientationEnum.BottomToTop
        Me.AG_InV.GradientType = WinNUT_Client.Controls.UPSVarGauge.GradientTypeEnum.RedGreen
        resources.ApplyResources(Me.AG_InV, "AG_InV")
        Me.AG_InV.MaxValue = 100
        Me.AG_InV.MinValue = 0
        Me.AG_InV.Name = "AG_InV"
        Me.AG_InV.NeedleRadius = 32
        Me.AG_InV.ScaleLinesInterInnerRadius = 40
        Me.AG_InV.ScaleLinesInterOuterRadius = 48
        Me.AG_InV.ScaleLinesMajorInnerRadius = 40
        Me.AG_InV.ScaleLinesMajorOuterRadius = 48
        Me.AG_InV.ScaleLinesMinorInnerRadius = 42
        Me.AG_InV.ScaleLinesMinorOuterRadius = 48
        Me.AG_InV.ScaleNumbersFormat = Nothing
        Me.AG_InV.ScaleNumbersRadius = 60
        Me.AG_InV.UnitValue1 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Volts
        Me.AG_InV.UnitValue2 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.None
        Me.AG_InV.Value = 0!
        Me.AG_InV.Value1 = 0!
        Me.AG_InV.Value2 = 0!
        '
        'Lbl_InV_Dial
        '
        resources.ApplyResources(Me.Lbl_InV_Dial, "Lbl_InV_Dial")
        Me.Lbl_InV_Dial.Name = "Lbl_InV_Dial"
        '
        'GB_OutV_Dial
        '
        resources.ApplyResources(Me.GB_OutV_Dial, "GB_OutV_Dial")
        Me.GB_OutV_Dial.Controls.Add(Me.AG_OutV)
        Me.GB_OutV_Dial.Controls.Add(Me.Lbl_OutV_Dial)
        Me.GB_OutV_Dial.Name = "GB_OutV_Dial"
        Me.GB_OutV_Dial.TabStop = False
        '
        'AG_OutV
        '
        Me.AG_OutV.BaseArcRadius = 45
        Me.AG_OutV.BaseArcWidth = 5
        Me.AG_OutV.GradientOrientation = WinNUT_Client.Controls.UPSVarGauge.GradientOrientationEnum.BottomToTop
        Me.AG_OutV.GradientType = WinNUT_Client.Controls.UPSVarGauge.GradientTypeEnum.RedGreen
        resources.ApplyResources(Me.AG_OutV, "AG_OutV")
        Me.AG_OutV.MaxValue = 100
        Me.AG_OutV.MinValue = 0
        Me.AG_OutV.Name = "AG_OutV"
        Me.AG_OutV.NeedleRadius = 32
        Me.AG_OutV.ScaleLinesInterInnerRadius = 40
        Me.AG_OutV.ScaleLinesInterOuterRadius = 48
        Me.AG_OutV.ScaleLinesMajorInnerRadius = 40
        Me.AG_OutV.ScaleLinesMajorOuterRadius = 48
        Me.AG_OutV.ScaleLinesMinorInnerRadius = 42
        Me.AG_OutV.ScaleLinesMinorOuterRadius = 48
        Me.AG_OutV.ScaleNumbersFormat = Nothing
        Me.AG_OutV.ScaleNumbersRadius = 60
        Me.AG_OutV.UnitValue1 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Volts
        Me.AG_OutV.UnitValue2 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.None
        Me.AG_OutV.Value = 0!
        Me.AG_OutV.Value1 = 0!
        Me.AG_OutV.Value2 = 0!
        '
        'Lbl_OutV_Dial
        '
        resources.ApplyResources(Me.Lbl_OutV_Dial, "Lbl_OutV_Dial")
        Me.Lbl_OutV_Dial.Name = "Lbl_OutV_Dial"
        '
        'GB_BattCh_Dial
        '
        resources.ApplyResources(Me.GB_BattCh_Dial, "GB_BattCh_Dial")
        Me.GB_BattCh_Dial.Controls.Add(Me.PBox_Battery_State)
        Me.GB_BattCh_Dial.Controls.Add(Me.Lbl_BattCh_Dial)
        Me.GB_BattCh_Dial.Controls.Add(Me.AG_BattCh)
        Me.GB_BattCh_Dial.Name = "GB_BattCh_Dial"
        Me.GB_BattCh_Dial.TabStop = False
        '
        'PBox_Battery_State
        '
        resources.ApplyResources(Me.PBox_Battery_State, "PBox_Battery_State")
        Me.PBox_Battery_State.Name = "PBox_Battery_State"
        Me.PBox_Battery_State.TabStop = False
        '
        'Lbl_BattCh_Dial
        '
        resources.ApplyResources(Me.Lbl_BattCh_Dial, "Lbl_BattCh_Dial")
        Me.Lbl_BattCh_Dial.Name = "Lbl_BattCh_Dial"
        '
        'AG_BattCh
        '
        Me.AG_BattCh.BaseArcRadius = 45
        Me.AG_BattCh.BaseArcWidth = 5
        Me.AG_BattCh.GradientOrientation = WinNUT_Client.Controls.UPSVarGauge.GradientOrientationEnum.LeftToRight
        Me.AG_BattCh.GradientType = WinNUT_Client.Controls.UPSVarGauge.GradientTypeEnum.RedGreen
        resources.ApplyResources(Me.AG_BattCh, "AG_BattCh")
        Me.AG_BattCh.MaxValue = 100
        Me.AG_BattCh.MinValue = 0
        Me.AG_BattCh.Name = "AG_BattCh"
        Me.AG_BattCh.NeedleRadius = 32
        Me.AG_BattCh.ScaleLinesInterInnerRadius = 40
        Me.AG_BattCh.ScaleLinesInterOuterRadius = 48
        Me.AG_BattCh.ScaleLinesMajorInnerRadius = 40
        Me.AG_BattCh.ScaleLinesMajorOuterRadius = 48
        Me.AG_BattCh.ScaleLinesMinorInnerRadius = 42
        Me.AG_BattCh.ScaleLinesMinorOuterRadius = 48
        Me.AG_BattCh.ScaleNumbersFormat = Nothing
        Me.AG_BattCh.ScaleNumbersRadius = 60
        Me.AG_BattCh.UnitValue1 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Percent
        Me.AG_BattCh.UnitValue2 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.None
        Me.AG_BattCh.Value = 0!
        Me.AG_BattCh.Value1 = 0!
        Me.AG_BattCh.Value2 = 0!
        '
        'GB_Load_Dial
        '
        resources.ApplyResources(Me.GB_Load_Dial, "GB_Load_Dial")
        Me.GB_Load_Dial.Controls.Add(Me.AG_Load)
        Me.GB_Load_Dial.Controls.Add(Me.Lbl_Load_Dial)
        Me.GB_Load_Dial.Name = "GB_Load_Dial"
        Me.GB_Load_Dial.TabStop = False
        '
        'AG_Load
        '
        Me.AG_Load.BaseArcRadius = 45
        Me.AG_Load.BaseArcWidth = 5
        Me.AG_Load.GradientOrientation = WinNUT_Client.Controls.UPSVarGauge.GradientOrientationEnum.RightToLeft
        Me.AG_Load.GradientType = WinNUT_Client.Controls.UPSVarGauge.GradientTypeEnum.RedGreen
        resources.ApplyResources(Me.AG_Load, "AG_Load")
        Me.AG_Load.MaxValue = 100
        Me.AG_Load.MinValue = 0
        Me.AG_Load.Name = "AG_Load"
        Me.AG_Load.NeedleRadius = 32
        Me.AG_Load.ScaleLinesInterInnerRadius = 40
        Me.AG_Load.ScaleLinesInterOuterRadius = 48
        Me.AG_Load.ScaleLinesMajorInnerRadius = 40
        Me.AG_Load.ScaleLinesMajorOuterRadius = 48
        Me.AG_Load.ScaleLinesMinorInnerRadius = 42
        Me.AG_Load.ScaleLinesMinorOuterRadius = 48
        Me.AG_Load.ScaleNumbersFormat = Nothing
        Me.AG_Load.ScaleNumbersRadius = 60
        Me.AG_Load.UnitValue1 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Percent
        Me.AG_Load.UnitValue2 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Watts
        Me.AG_Load.Value = 0!
        Me.AG_Load.Value1 = 0!
        Me.AG_Load.Value2 = 0!
        '
        'Lbl_Load_Dial
        '
        resources.ApplyResources(Me.Lbl_Load_Dial, "Lbl_Load_Dial")
        Me.Lbl_Load_Dial.Name = "Lbl_Load_Dial"
        '
        'GB_BattV_Dial
        '
        resources.ApplyResources(Me.GB_BattV_Dial, "GB_BattV_Dial")
        Me.GB_BattV_Dial.Controls.Add(Me.AG_BattV)
        Me.GB_BattV_Dial.Controls.Add(Me.Lbl_BattV_Dial)
        Me.GB_BattV_Dial.Name = "GB_BattV_Dial"
        Me.GB_BattV_Dial.TabStop = False
        '
        'AG_BattV
        '
        Me.AG_BattV.BaseArcRadius = 45
        Me.AG_BattV.BaseArcWidth = 5
        Me.AG_BattV.GradientOrientation = WinNUT_Client.Controls.UPSVarGauge.GradientOrientationEnum.BottomToTop
        Me.AG_BattV.GradientType = WinNUT_Client.Controls.UPSVarGauge.GradientTypeEnum.RedGreen
        resources.ApplyResources(Me.AG_BattV, "AG_BattV")
        Me.AG_BattV.MaxValue = 100
        Me.AG_BattV.MinValue = 0
        Me.AG_BattV.Name = "AG_BattV"
        Me.AG_BattV.NeedleRadius = 32
        Me.AG_BattV.ScaleLinesInterInnerRadius = 40
        Me.AG_BattV.ScaleLinesInterOuterRadius = 48
        Me.AG_BattV.ScaleLinesMajorInnerRadius = 40
        Me.AG_BattV.ScaleLinesMajorOuterRadius = 48
        Me.AG_BattV.ScaleLinesMinorInnerRadius = 42
        Me.AG_BattV.ScaleLinesMinorOuterRadius = 48
        Me.AG_BattV.ScaleNumbersFormat = Nothing
        Me.AG_BattV.ScaleNumbersRadius = 60
        Me.AG_BattV.UnitValue1 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Volts
        Me.AG_BattV.UnitValue2 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.None
        Me.AG_BattV.Value = 0!
        Me.AG_BattV.Value1 = 0!
        Me.AG_BattV.Value2 = 0!
        '
        'Lbl_BattV_Dial
        '
        resources.ApplyResources(Me.Lbl_BattV_Dial, "Lbl_BattV_Dial")
        Me.Lbl_BattV_Dial.Name = "Lbl_BattV_Dial"
        '
        'GB_InF_Dial
        '
        resources.ApplyResources(Me.GB_InF_Dial, "GB_InF_Dial")
        Me.GB_InF_Dial.Controls.Add(Me.AG_InF)
        Me.GB_InF_Dial.Controls.Add(Me.Lbl_InF_Dial)
        Me.GB_InF_Dial.Name = "GB_InF_Dial"
        Me.GB_InF_Dial.TabStop = False
        '
        'AG_InF
        '
        Me.AG_InF.BaseArcRadius = 45
        Me.AG_InF.BaseArcWidth = 5
        Me.AG_InF.GradientOrientation = WinNUT_Client.Controls.UPSVarGauge.GradientOrientationEnum.BottomToTop
        Me.AG_InF.GradientType = WinNUT_Client.Controls.UPSVarGauge.GradientTypeEnum.RedGreen
        resources.ApplyResources(Me.AG_InF, "AG_InF")
        Me.AG_InF.MaxValue = 100
        Me.AG_InF.MinValue = 0
        Me.AG_InF.Name = "AG_InF"
        Me.AG_InF.NeedleRadius = 32
        Me.AG_InF.ScaleLinesInterInnerRadius = 40
        Me.AG_InF.ScaleLinesInterOuterRadius = 48
        Me.AG_InF.ScaleLinesMajorInnerRadius = 40
        Me.AG_InF.ScaleLinesMajorOuterRadius = 48
        Me.AG_InF.ScaleLinesMinorInnerRadius = 42
        Me.AG_InF.ScaleLinesMinorOuterRadius = 48
        Me.AG_InF.ScaleNumbersFormat = Nothing
        Me.AG_InF.ScaleNumbersRadius = 60
        Me.AG_InF.UnitValue1 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.Hertz
        Me.AG_InF.UnitValue2 = WinNUT_Client.Controls.UPSVarGauge.UnitValueEnum.None
        Me.AG_InF.Value = 0!
        Me.AG_InF.Value1 = 0!
        Me.AG_InF.Value2 = 0!
        '
        'Lbl_InF_Dial
        '
        resources.ApplyResources(Me.Lbl_InF_Dial, "Lbl_InF_Dial")
        Me.Lbl_InF_Dial.Name = "Lbl_InF_Dial"
        '
        'CB_CurrentLog
        '
        Me.CB_CurrentLog.CausesValidation = False
        Me.CB_CurrentLog.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        resources.ApplyResources(Me.CB_CurrentLog, "CB_CurrentLog")
        Me.CB_CurrentLog.Name = "CB_CurrentLog"
        '
        'WinNUT
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.AutoValidate = System.Windows.Forms.AutoValidate.Disable
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.CB_CurrentLog)
        Me.Controls.Add(Me.GB_InF_Dial)
        Me.Controls.Add(Me.GB_InV_Dial)
        Me.Controls.Add(Me.GB_BattV_Dial)
        Me.Controls.Add(Me.GB_Load_Dial)
        Me.Controls.Add(Me.GB_OutV_Dial)
        Me.Controls.Add(Me.GB_Status)
        Me.Controls.Add(Me.GB_BattCh_Dial)
        Me.Controls.Add(Me.Main_Menu)
        Me.DoubleBuffered = True
        Me.MainMenuStrip = Me.Main_Menu
        Me.MaximizeBox = False
        Me.Name = "WinNUT"
        Me.ContextMenu_Systray.ResumeLayout(False)
        Me.Main_Menu.ResumeLayout(False)
        Me.Main_Menu.PerformLayout()
        Me.GB_Status.ResumeLayout(False)
        Me.GB_InV_Dial.ResumeLayout(False)
        Me.GB_InV_Dial.PerformLayout()
        Me.GB_OutV_Dial.ResumeLayout(False)
        Me.GB_OutV_Dial.PerformLayout()
        Me.GB_BattCh_Dial.ResumeLayout(False)
        Me.GB_BattCh_Dial.PerformLayout()
        CType(Me.PBox_Battery_State, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GB_Load_Dial.ResumeLayout(False)
        Me.GB_Load_Dial.PerformLayout()
        Me.GB_BattV_Dial.ResumeLayout(False)
        Me.GB_BattV_Dial.PerformLayout()
        Me.GB_InF_Dial.ResumeLayout(False)
        Me.GB_InF_Dial.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents Menu_Check_Update As ToolStripMenuItem
    Friend WithEvents NotifyIcon As NotifyIcon
    Friend WithEvents Main_Menu As MenuStrip
    Friend WithEvents Menu_File As ToolStripMenuItem
    Friend WithEvents Menu_UPS_Var As ToolStripMenuItem
    Friend WithEvents Menu_Quit As ToolStripMenuItem
    Friend WithEvents Menu_Connection As ToolStripMenuItem
    Friend WithEvents Menu_Settings As ToolStripMenuItem
    Friend WithEvents Menu_Help As ToolStripMenuItem
    Friend WithEvents Menu_About As ToolStripMenuItem
    Friend WithEvents Menu_Help_Sep1 As ToolStripSeparator
    Friend WithEvents Menu_Update As ToolStripMenuItem
    Friend WithEvents Menu_Connect As ToolStripMenuItem
    Friend WithEvents Menu_Disconnect As ToolStripMenuItem
    Friend WithEvents ContextMenu_Systray As ContextMenuStrip
    Friend WithEvents Menu_Sys_Settings As ToolStripMenuItem
    Friend WithEvents Menu_Sys_Sep1 As ToolStripSeparator
    Friend WithEvents Menu_Sys_Update As ToolStripMenuItem
    Friend WithEvents Menu_Sys_Sep2 As ToolStripSeparator
    Friend WithEvents Menu_Sys_About As ToolStripMenuItem
    Friend WithEvents Menu_Sys_Sep3 As ToolStripSeparator
    Friend WithEvents Menu_Sys_Exit As ToolStripMenuItem
    Friend WithEvents GB_Status As GroupBox
    Friend WithEvents Lbl_VSerial As Label
    Friend WithEvents Lbl_VFirmware As Label
    Friend WithEvents Lbl_VName As Label
    Friend WithEvents Lbl_VMfr As Label
    Friend WithEvents Lbl_VRTime As Label
    Friend WithEvents Lbl_VOB As Label
    Friend WithEvents Lbl_VOLoad As Label
    Friend WithEvents Lbl_VBL As Label
    Friend WithEvents Lbl_VOL As Label
    Friend WithEvents Lbl_Firmware As Label
    Friend WithEvents Lbl_Serial As Label
    Friend WithEvents Lbl_Name As Label
    Friend WithEvents Lbl_Mfr As Label
    Friend WithEvents Lbl_RTime As Label
    Friend WithEvents Lbl_BL As Label
    Friend WithEvents Lbl_OLoad As Label
    Friend WithEvents Lbl_OB As Label
    Friend WithEvents Lbl_OL As Label
    Friend WithEvents GB_BattCh_Dial As GroupBox
    Friend WithEvents Lbl_BattCh_Dial As Label
    Friend WithEvents GB_OutV_Dial As GroupBox
    Friend WithEvents Lbl_OutV_Dial As Label
    Friend WithEvents GB_Load_Dial As GroupBox
    Friend WithEvents Lbl_Load_Dial As Label
    Friend WithEvents GB_BattV_Dial As GroupBox
    Friend WithEvents Lbl_BattV_Dial As Label
    Friend WithEvents GB_InV_Dial As GroupBox
    Friend WithEvents Lbl_InV_Dial As Label
    Friend WithEvents AG_InV As WinNUT_Client.Controls.UPSVarGauge
    Friend WithEvents AG_OutV As WinNUT_Client.Controls.UPSVarGauge
    Friend WithEvents AG_BattCh As WinNUT_Client.Controls.UPSVarGauge
    Friend WithEvents AG_Load As WinNUT_Client.Controls.UPSVarGauge
    Friend WithEvents AG_BattV As WinNUT_Client.Controls.UPSVarGauge
    Friend WithEvents GB_InF_Dial As GroupBox
    Friend WithEvents AG_InF As WinNUT_Client.Controls.UPSVarGauge
    Friend WithEvents Lbl_InF_Dial As Label
    Friend WithEvents PBox_Battery_State As PictureBox
    Friend WithEvents ManageOldPrefsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents Menu_Persist As ToolStripMenuItem
    Private WithEvents CB_CurrentLog As ComboBox
End Class
