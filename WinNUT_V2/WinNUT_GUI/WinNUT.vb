﻿Public Enum LogLvl
    LOG_NOTICE
    LOG_WARNING
    LOG_ERROR
    LOG_DEBUG
End Enum

Public Class WinNUT
    Public WithEvents LogFile As New Logger(False, 0)
    Public LogLvls As Logger.LogLvl
    Public WithEvents UPS_Network As New UPS_Network(LogFile)
    Public UPS_Mfr As String
    Public UPS_Model As String
    Public UPS_Serial As String
    Public UPS_Firmware As String
    Public UPS_BattCh As Double
    Public UPS_BattV As Double
    Public UPS_BattRuntime As Double
    Public UPS_BattCapacity As Double
    Public UPS_InputF As Double
    Public UPS_InputV As Double
    Public UPS_OutputV As Double
    Public UPS_Load As Double
    Public UPS_Status As String
    Public UPS_OutPower As Double
    Public UPS_InputA As Double
    Private LastAppIconIdx As Integer
    Private ActualAppIconIdx As Integer
    Private WinDarkMode As Boolean = False
    Private AppDarkMode As Boolean = False
    Private HasFocus As Boolean = True
    Private frmBuild As Form
    Private mUpdate As Boolean = False
    Declare Function SetSystemPowerState Lib "kernel32" (ByVal fSuspend As Integer, ByVal fForce As Integer) As Integer
    Public Enum AppIconIdx
        IDX_BATT_0 = 1
        IDX_BATT_25 = 2
        IDX_BATT_50 = 4
        IDX_BATT_75 = 8
        IDX_BATT_100 = 16
        IDX_OL = 32
        WIN_DARK = 64
        IDX_ICO_OFFLINE = 128
        IDX_ICO_RETRY = 256
        IDX_ICO_VIEWLOG = 2001
        IDX_ICO_DELETELOG = 2002
        IDX_OFFSET = 1024
    End Enum
    Public Property UpdateMethod() As String
        Get
            If Me.mUpdate Then
                Me.mUpdate = False
                Return True
            Else
                Return False
            End If
        End Get
        Set(ByVal Value As String)
            Me.mUpdate = Value
        End Set
    End Property
    Private Sub WinNUT_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Microsoft.Win32.SystemEvents.PowerModeChanged, AddressOf SystemEvents_PowerModeChanged

        'Init WinNUT Variables
        WinNUT_Globals.Init_Globals()

        'Init WinNUT PArameters
        WinNUT_Params.Init_Params()

        'Load WinNUT PArameters
        WinNUT_Params.Load_Params()

        'Init Log File
        LogFile.WriteLog = WinNUT_Params.Arr_Reg_Key.Item("UseLogFile")
        LogFile.LogLevel = WinNUT_Params.Arr_Reg_Key.Item("Log Level")
        Me.LogLvls = New LogLvl
        LogFile.LogTracing("Initialisation Globals Variables Complete", LogLvl.LOG_DEBUG, Me)
        'Auto Import of old Ini File
        Dim IniFile = AppDomain.CurrentDomain.BaseDirectory & "ups.ini"

        If System.IO.File.Exists(IniFile) Then
            If WinNUT_Params.ImportIni(IniFile) Then
                LogFile.LogTracing("Import Old IniFile : Success", LogLvl.LOG_DEBUG, Me)
                My.Computer.FileSystem.MoveFile(IniFile, IniFile & ".old")
                MsgBox("Old ups.ini imported" & vbNewLine & "Ini File Moved to " & IniFile & ".old")
            Else
                LogFile.LogTracing("Failed To import old IniFile", LogLvl.LOG_DEBUG, Me)
                LogFile.LogTracing("Initialisation Params Complete", LogLvl.LOG_DEBUG, Me)
                LogFile.LogTracing("Loaded Params Complete", LogLvl.LOG_DEBUG, Me)
            End If
        Else
            LogFile.LogTracing("Initialisation Params Complete", LogLvl.LOG_DEBUG, Me)
            LogFile.LogTracing("Loaded Params Complete", LogLvl.LOG_DEBUG, Me)
        End If

        'Init Systray
        Me.NotifyIcon.Text = WinNUT_Globals.LongProgramName & " - " & WinNUT_Globals.ProgramVersion
        Me.NotifyIcon.Visible = False
        LogFile.LogTracing("NotifyIcons Initialised", 1, Me)

        'Init Connexion to UPS
        UPS_Network.NutHost = WinNUT_Params.Arr_Reg_Key.Item("ServerAddress")
        UPS_Network.NutPort = WinNUT_Params.Arr_Reg_Key.Item("Port")
        UPS_Network.NutUPS = WinNUT_Params.Arr_Reg_Key.Item("UPSName")
        UPS_Network.NutDelay = WinNUT_Params.Arr_Reg_Key.Item("Delay")
        UPS_Network.AutoReconnect = WinNUT_Params.Arr_Reg_Key.Item("AutoReconnect")
        UPS_Network.Battery_Limit = WinNUT_Params.Arr_Reg_Key.Item("ShutdownLimitBatteryCharge")
        UPS_Network.Backup_Limit = WinNUT_Params.Arr_Reg_Key.Item("ShutdownLimitUPSRemainTime")

        'Force Positionning Text Label Because Unknow auto positionning property ???
        Lbl_RTime.Location = New Point(6, 116)
        Lbl_VRTime.Location = New Point(6, 136)
        Lbl_Mfr.Location = New Point(6, 156)
        Lbl_VMfr.Location = New Point(6, 176)
        Lbl_Name.Location = New Point(6, 196)
        Lbl_VName.Location = New Point(6, 216)
        Lbl_Serial.Location = New Point(6, 236)
        Lbl_VSerial.Location = New Point(6, 256)
        Lbl_Firmware.Location = New Point(6, 276)
        Lbl_VFirmware.Location = New Point(6, 296)

        'Add DialGraph
        With AG_InV
            .Location = New Point(6, 26)
            .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxInputVoltage")
            .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinInputVoltage")
            .Value1 = Me.UPS_InputV
            .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
        End With
        With AG_InF
            .Location = New Point(6, 26)
            .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxInputFrequency")
            .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinInputFrequency")
            .Value1 = Me.UPS_InputF
            .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
        End With
        With AG_OutV
            .Location = New Point(6, 26)
            .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxOutputVoltage")
            .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinOutputVoltage")
            .Value1 = Me.UPS_OutputV
            .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
        End With
        With AG_BattCh
            .Location = New Point(6, 26)
            .MaxValue = 100
            .MinValue = 0
            .Value1 = Me.UPS_BattCh
            .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
        End With
        With AG_Load
            .Location = New Point(6, 26)
            .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxUPSLoad")
            .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinUPSLoad")
            .Value1 = Me.UPS_Load
            .Value2 = Me.UPS_OutPower
            .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
        End With
        With AG_BattV
            .Location = New Point(6, 26)
            .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxBattVoltage")
            .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinBattVoltage")
            .Value1 = Me.UPS_BattV
            .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
        End With

        'Verify Is Windows is In Dark Mode
        If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0) = 0 Then
            LogFile.LogTracing("Windows App Use Dark Theme", LogLvl.LOG_DEBUG, Me)
            AppDarkMode = True
        Else
            LogFile.LogTracing("Windows App Use Light Theme", LogLvl.LOG_DEBUG, Me)
            AppDarkMode = False
        End If
        If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0) = 0 Then
            LogFile.LogTracing("Windows Use Dark Theme", LogLvl.LOG_DEBUG, Me)
            WinDarkMode = True
        Else
            LogFile.LogTracing("Windows Use Light Theme", LogLvl.LOG_DEBUG, Me)
            WinDarkMode = False
        End If

        'Adapts the icon according to the Win / App Dark Mode 
        Dim Start_App_Icon
        Dim Start_Tray_Icon
        If AppDarkMode Then
            Start_App_Icon = AppIconIdx.IDX_ICO_OFFLINE Or AppIconIdx.WIN_DARK Or AppIconIdx.IDX_OFFSET
        Else
            Start_App_Icon = AppIconIdx.IDX_ICO_OFFLINE Or AppIconIdx.IDX_OFFSET
        End If
        Me.Icon = GetIcon(Start_App_Icon)
        If WinDarkMode Then
            Start_Tray_Icon = AppIconIdx.IDX_ICO_OFFLINE Or AppIconIdx.WIN_DARK Or AppIconIdx.IDX_OFFSET
        Else
            Start_Tray_Icon = AppIconIdx.IDX_ICO_OFFLINE Or AppIconIdx.IDX_OFFSET
        End If

        'Initializes the state of the NotifyICon, the connection to the Nut server and the application icons
        NotifyIcon.Visible = False
        NotifyIcon.Icon = GetIcon(Start_Tray_Icon)
        Dim NotifyStr As String
        NotifyStr = WinNUT_Globals.LongProgramName & " - " & WinNUT_Globals.ProgramVersion & vbNewLine
        NotifyStr &= "Not Connected"
        Me.NotifyIcon.Text = NotifyStr
        LogFile.LogTracing("NotifyIcon Text => " & NotifyStr, LogLvl.LOG_DEBUG, Me)
        LogFile.LogTracing("Init Connexion to NutServer", LogLvl.LOG_DEBUG, Me)
        UPS_Network.Connect()
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
        Start_Tray_Icon = Nothing
        NotifyStr = Nothing

        'Run Update
        If WinNUT_Params.Arr_Reg_Key.Item("VerifyUpdate") = True And WinNUT_Params.Arr_Reg_Key.Item("VerifyUpdateAtStart") = True Then
            Dim th As System.Threading.Thread = New Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf Run_Update))

            th.SetApartmentState(System.Threading.ApartmentState.STA)
            th.Start(Me.UpdateMethod)
        End If
    End Sub

    Private Sub SystemEvents_PowerModeChanged(ByVal sender As Object, ByVal e As Microsoft.Win32.PowerModeChangedEventArgs)
        Select Case e.Mode
            Case Microsoft.Win32.PowerModes.Resume
                If WinNUT_Params.Arr_Reg_Key.Item("AutoReconnect") = True Then
                    UPS_Network.Connect()
                End If
            'Case PowerModes.StatusChange
            Case Microsoft.Win32.PowerModes.Suspend
                UPS_Network.Disconnect()
                RemoveHandler Microsoft.Win32.SystemEvents.PowerModeChanged, AddressOf SystemEvents_PowerModeChanged
        End Select
    End Sub

    Private Sub WinNUT_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
        If WinNUT_Params.Arr_Reg_Key.Item("MinimizeOnStart") = True Then
            LogFile.LogTracing("Minimize WinNut On Start", LogLvl.LOG_DEBUG, Me)
            Me.WindowState = FormWindowState.Minimized
            Me.NotifyIcon.Visible = True
        Else
            LogFile.LogTracing("Show WinNut Main Gui", LogLvl.LOG_DEBUG, Me)
            Me.NotifyIcon.Visible = False
        End If
        If WinNUT_Params.Arr_Reg_Key.Item("VerifyUpdate") = True Then
            Me.Menu_Help_Sep1.Visible = True
            Me.Menu_Update.Visible = True
            Me.Menu_Update.Visible = Enabled = True
        Else
            Me.Menu_Help_Sep1.Visible = False
            Me.Menu_Update.Visible = False
            Me.Menu_Update.Visible = Enabled = False
        End If
    End Sub

    Private Sub WinNUT_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If WinNUT_Params.Arr_Reg_Key.Item("CloseToTray") = True Then
            LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
            UpdateIcon_NotifyIcon()
            LogFile.LogTracing("Minimize Main Gui To Notify Icon", LogLvl.LOG_DEBUG, Me)
            Me.WindowState = FormWindowState.Minimized
            Me.Visible = False
            Me.NotifyIcon.Visible = True
            e.Cancel = True
        Else
            LogFile.LogTracing("Init Disconnecting Before Close WinNut", LogLvl.LOG_DEBUG, Me)
            UPS_Network.Disconnect()
            LogFile.LogTracing("WinNut Is now Closed", LogLvl.LOG_DEBUG, Me)
            RemoveHandler Microsoft.Win32.SystemEvents.PowerModeChanged, AddressOf SystemEvents_PowerModeChanged
            End
        End If

    End Sub

    Private Sub Menu_Quit_Click_1(sender As Object, e As EventArgs) Handles Menu_Quit.Click
        LogFile.LogTracing("Close WinNut From Menu Quit", LogLvl.LOG_DEBUG, Me)
        End
    End Sub

    Private Sub Menu_Settings_Click(sender As Object, e As EventArgs) Handles Menu_Settings.Click
        LogFile.LogTracing("Open Pref Gui From Menu", LogLvl.LOG_DEBUG, Me)
        Pref_Gui.Activate()
        Pref_Gui.Visible = True
        HasFocus = False
    End Sub

    Private Sub Menu_Sys_Exit_Click(sender As Object, e As EventArgs) Handles Menu_Sys_Exit.Click
        LogFile.LogTracing("Close WinNut From Systray", LogLvl.LOG_DEBUG, Me)
        End
    End Sub

    Private Sub Menu_Sys_Settings_Click(sender As Object, e As EventArgs) Handles Menu_Sys_Settings.Click
        LogFile.LogTracing("Open Pref Gui From Systray", LogLvl.LOG_DEBUG, Me)
        Pref_Gui.Activate()
        Pref_Gui.Visible = True
        HasFocus = False
    End Sub

    Private Sub NotifyIcon_DoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon.DoubleClick
        LogFile.LogTracing("Restore Main Gui On Double Click Notify Icon", LogLvl.LOG_DEBUG, Me)
        Me.Visible = True
        Me.NotifyIcon.Visible = False
        Me.WindowState = FormWindowState.Normal
    End Sub
    Private Sub WinNUT_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If sender.WindowState = FormWindowState.Minimized Then
            If WinNUT_Params.Arr_Reg_Key.Item("MinimizeToTray") = True Then
                LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
                UpdateIcon_NotifyIcon()
                LogFile.LogTracing("Minimize Main Gui To Notify Icon", LogLvl.LOG_DEBUG, Me)
                Me.WindowState = FormWindowState.Minimized
                Me.Visible = False
                Me.NotifyIcon.Visible = True
            Else
                LogFile.LogTracing("Close WinNut", LogLvl.LOG_DEBUG, Me)
                End
            End If
        End If
    End Sub

    Private Sub NewRetry_NotifyIcon() Handles UPS_Network.NewRetry
        Dim NotifyStr As String
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
        NotifyStr = "WinNUT - " & WinNUT_Globals.ProgramVersion & vbNewLine
        NotifyStr &= "Reconnection In Progress" & vbNewLine
        NotifyStr &= String.Format("Try {0} of {1}", UPS_Network.UPS_Retry, UPS_Network.UPS_MaxRetry) & vbNewLine
        Me.NotifyIcon.Text = NotifyStr
        LogFile.LogTracing("NotifyIcon Text => " & NotifyStr, LogLvl.LOG_DEBUG, Me)
        NotifyStr = Nothing
    End Sub

    Private Sub Deconnected_NotifyIcon() Handles UPS_Network.Deconnected
        Dim NotifyStr As String
        ActualAppIconIdx = AppIconIdx.IDX_ICO_OFFLINE
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
        NotifyStr = WinNUT_Globals.LongProgramName & " - " & WinNUT_Globals.ProgramVersion & vbNewLine
        NotifyStr &= "Not Connected"
        Me.NotifyIcon.Text = NotifyStr
        LogFile.LogTracing("NotifyIcon Text => " & NotifyStr, LogLvl.LOG_DEBUG, Me)
        NotifyStr = Nothing
    End Sub

    Private Sub Menu_About_Click(sender As Object, e As EventArgs) Handles Menu_About.Click
        LogFile.LogTracing("Open About Gui From Menu", LogLvl.LOG_DEBUG, Me)
        About_Gui.Activate()
        About_Gui.Visible = True
        HasFocus = False
    End Sub

    Private Sub Menu_Sys_About_Click(sender As Object, e As EventArgs) Handles Menu_Sys_About.Click
        LogFile.LogTracing("Open About Gui From Systray", LogLvl.LOG_DEBUG, Me)
        About_Gui.Activate()
        About_Gui.Visible = True
        HasFocus = False
    End Sub

    Private Sub UPS_Lostconnect() Handles UPS_Network.LostConnect
        LogFile.LogTracing("Nut Server Lost Connection", LogLvl.LOG_ERROR, Me)
        LogFile.LogTracing("Fix All data to null/empty String", LogLvl.LOG_DEBUG, Me)
        Me.UPS_Mfr = ""
        Me.UPS_Model = ""
        Me.UPS_Serial = ""
        Me.UPS_Firmware = ""
        Lbl_VOL.BackColor = Color.White
        Lbl_VOB.BackColor = Color.White
        Lbl_VOLoad.BackColor = Color.White
        Lbl_VBL.BackColor = Color.White
        Lbl_VRTime.Text = ""
        Lbl_VMfr.Text = Me.UPS_Mfr
        Lbl_VName.Text = Me.UPS_Model
        Lbl_VSerial.Text = Me.UPS_Serial
        Lbl_VFirmware.Text = Me.UPS_Firmware
        If UPS_Network.AutoReconnect And UPS_Network.UPS_Retry <= UPS_Network.UPS_MaxRetry Then
            ActualAppIconIdx = AppIconIdx.IDX_ICO_RETRY
        Else
            ActualAppIconIdx = AppIconIdx.IDX_ICO_OFFLINE
        End If
        LogFile.LogTracing("Fix All Dial Data to Min Value/0", LogLvl.LOG_DEBUG, Me)
        AG_InV.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinInputVoltage")
        AG_InF.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinInputFrequency")
        AG_OutV.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinOutputVoltage")
        AG_BattCh.Value1 = 0
        AG_Load.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinUPSLoad")
        AG_Load.Value2 = 0
        AG_BattV.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinBattVoltage")
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
    End Sub

    Private Sub Update_UPS_Data() Handles UPS_Network.DataUpdated
        If Me.UPS_Mfr = "" And Me.UPS_Model = "" And Me.UPS_Serial = "" And Me.UPS_Firmware = "" Then
            LogFile.LogTracing("Retrieve UPS Informations", LogLvl.LOG_DEBUG, Me)
            Me.UPS_Mfr = UPS_Network.UPS_Mfr
            Me.UPS_Model = UPS_Network.UPS_Model
            Me.UPS_Serial = UPS_Network.UPS_Serial
            Me.UPS_Firmware = UPS_Network.UPS_Firmware
            Lbl_VMfr.Text = Me.UPS_Mfr
            Lbl_VName.Text = Me.UPS_Model
            Lbl_VSerial.Text = Me.UPS_Serial
            Lbl_VFirmware.Text = Me.UPS_Firmware
        End If
        Me.UPS_BattCh = UPS_Network.UPS_BattCh
        Me.UPS_BattV = UPS_Network.UPS_BattV
        Me.UPS_BattRuntime = UPS_Network.UPS_BattRuntime
        Me.UPS_BattCapacity = UPS_Network.UPS_BattCapacity
        Me.UPS_InputF = UPS_Network.UPS_InputF
        Me.UPS_InputV = UPS_Network.UPS_InputV
        Me.UPS_OutputV = UPS_Network.UPS_OutputV
        Me.UPS_Load = UPS_Network.UPS_Load
        Me.UPS_Status = UPS_Network.UPS_Status
        Me.UPS_OutPower = UPS_Network.UPS_OutPower

        If Me.UPS_Status <> Nothing Then
            If Me.UPS_Status.Trim().StartsWith("OL") Then
                LogFile.LogTracing("UPS is plugged", LogLvl.LOG_DEBUG, Me)
                Lbl_VOL.BackColor = Color.Green
                Lbl_VOB.BackColor = Color.White
                ActualAppIconIdx = AppIconIdx.IDX_OL
            Else
                LogFile.LogTracing("UPS is unplugged", LogLvl.LOG_DEBUG, Me)
                Lbl_VOL.BackColor = Color.Yellow
                Lbl_VOB.BackColor = Color.Green
                ActualAppIconIdx = 0
            End If

            If Me.UPS_Load > 100 Then
                LogFile.LogTracing("UPS Overload", LogLvl.LOG_ERROR, Me)
                Lbl_VOLoad.BackColor = Color.Red
            Else
                Lbl_VOLoad.BackColor = Color.White
            End If

            Select Case Me.UPS_BattCh
                Case 76 To 100
                    Lbl_VBL.BackColor = Color.White
                    ActualAppIconIdx = ActualAppIconIdx Or AppIconIdx.IDX_BATT_100
                    LogFile.LogTracing("Battery Charged", LogLvl.LOG_DEBUG, Me)
                Case 51 To 75
                    Lbl_VBL.BackColor = Color.White
                    ActualAppIconIdx = ActualAppIconIdx Or AppIconIdx.IDX_BATT_75
                    LogFile.LogTracing("Battery Charged", LogLvl.LOG_DEBUG, Me)
                Case 40 To 50
                    Lbl_VBL.BackColor = Color.White
                    ActualAppIconIdx = ActualAppIconIdx Or AppIconIdx.IDX_BATT_50
                    LogFile.LogTracing("Battery Charged", LogLvl.LOG_DEBUG, Me)
                Case 26 To 39
                    Lbl_VBL.BackColor = Color.Red
                    ActualAppIconIdx = ActualAppIconIdx Or AppIconIdx.IDX_BATT_50
                    LogFile.LogTracing("Low Battery", LogLvl.LOG_DEBUG, Me)
                Case 11 To 25
                    Lbl_VBL.BackColor = Color.Red
                    ActualAppIconIdx = ActualAppIconIdx Or AppIconIdx.IDX_BATT_25
                    LogFile.LogTracing("Low Battery", LogLvl.LOG_DEBUG, Me)
                Case 0 To 10
                    Lbl_VBL.BackColor = Color.Red
                    ActualAppIconIdx = ActualAppIconIdx Or AppIconIdx.IDX_BATT_0
                    LogFile.LogTracing("Low Battery", LogLvl.LOG_DEBUG, Me)
            End Select

            Dim iSpan As TimeSpan = TimeSpan.FromSeconds(Me.UPS_BattRuntime)
            Lbl_VRTime.Text = iSpan.Hours.ToString.PadLeft(2, "0"c) & ":" &
                                    iSpan.Minutes.ToString.PadLeft(2, "0"c) & ":" &
                                    iSpan.Seconds.ToString.PadLeft(2, "0"c)
            iSpan = Nothing
        End If
        Dim NotifyStr As String
        NotifyStr = "WinNUT - " & WinNUT_Globals.ProgramVersion & vbNewLine
        NotifyStr &= "Connected" & vbNewLine
        If UPS_Network.UPS_Status = "OL" Then
            NotifyStr &= "On Line" & vbNewLine
        Else
            NotifyStr &= "On Battery " & "(" & UPS_Network.UPS_BattCh & "%)" & vbNewLine
        End If
        Select Case UPS_Network.UPS_BattCh
            Case 0 To 40
                NotifyStr &= "Low Battery"
            Case 41 To 100
                NotifyStr &= "Battery OK"
        End Select
        Me.NotifyIcon.Text = NotifyStr
        LogFile.LogTracing("NotifyIcon Text => " & NotifyStr, LogLvl.LOG_DEBUG, Me)
        NotifyStr = Nothing

        LogFile.LogTracing("Update Dial", LogLvl.LOG_DEBUG, Me)
        AG_InV.Value1 = Me.UPS_InputV
        AG_InF.Value1 = Me.UPS_InputF
        AG_OutV.Value1 = Me.UPS_OutputV
        AG_BattCh.Value1 = Me.UPS_BattCh
        AG_Load.Value1 = Me.UPS_Load
        AG_Load.Value2 = Me.UPS_OutPower
        AG_BattV.Value1 = Me.UPS_BattV
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
    End Sub

    Private Sub Menu_Disconnect_Click(sender As Object, e As EventArgs) Handles Menu_Disconnect.Click
        LogFile.LogTracing("Force Disconnect from menu", LogLvl.LOG_DEBUG, Me)
        UPS_Network.Disconnect(True)
        ReInitDisplayValues()
        ActualAppIconIdx = AppIconIdx.IDX_ICO_OFFLINE
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
    End Sub

    Private Sub ReInitDisplayValues()
        LogFile.LogTracing("Update all informations displayed ton empty values", LogLvl.LOG_DEBUG, Me)
        Me.UPS_Mfr = ""
        Me.UPS_Model = ""
        Me.UPS_Serial = ""
        Me.UPS_Firmware = ""
        Lbl_VOL.BackColor = Color.White
        Lbl_VOB.BackColor = Color.White
        Lbl_VOLoad.BackColor = Color.White
        Lbl_VBL.BackColor = Color.White
        Lbl_VRTime.Text = ""
        Lbl_VMfr.Text = Me.UPS_Mfr
        Lbl_VName.Text = Me.UPS_Model
        Lbl_VSerial.Text = Me.UPS_Serial
        Lbl_VFirmware.Text = Me.UPS_Firmware
        AG_InV.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinInputVoltage")
        AG_InF.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinInputFrequency")
        AG_OutV.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinOutputVoltage")
        AG_BattCh.Value1 = 0
        AG_Load.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinUPSLoad")
        AG_Load.Value2 = 0
        AG_BattV.Value1 = WinNUT_Params.Arr_Reg_Key.Item("MinBattVoltage")
    End Sub

    Private Sub Menu_Reconnect_Click(sender As Object, e As EventArgs) Handles Menu_Reconnect.Click
        LogFile.LogTracing("Force Reconnect from menu", LogLvl.LOG_DEBUG, Me)
        UPS_Network.Connect()
    End Sub

    Private Sub WinNUT_Deactivate(sender As Object, e As EventArgs) Handles MyBase.Deactivate
        LogFile.LogTracing("Main Gui Lose Focus", LogLvl.LOG_DEBUG, Me)
        HasFocus = False
        Dim Tmp_App_Mode As Integer
        If Not AppDarkMode Then
            Tmp_App_Mode = AppIconIdx.WIN_DARK Or AppIconIdx.IDX_OFFSET
        Else
            Tmp_App_Mode = AppIconIdx.IDX_OFFSET
        End If
        Dim TmpGuiIDX = ActualAppIconIdx Or Tmp_App_Mode
        Me.Icon = GetIcon(TmpGuiIDX)
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
        Tmp_App_Mode = Nothing
        TmpGuiIDX = Nothing
    End Sub

    Private Sub WinNUT_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated
        LogFile.LogTracing("Main Gui Has Focus", LogLvl.LOG_DEBUG, Me)
        HasFocus = True
        Dim Tmp_App_Mode As Integer
        If AppDarkMode Then
            Tmp_App_Mode = AppIconIdx.WIN_DARK Or AppIconIdx.IDX_OFFSET
        Else
            Tmp_App_Mode = AppIconIdx.IDX_OFFSET
        End If
        Dim TmpGuiIDX = ActualAppIconIdx Or Tmp_App_Mode
        Me.Icon = GetIcon(TmpGuiIDX)
        LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
        UpdateIcon_NotifyIcon()
        Tmp_App_Mode = Nothing
        TmpGuiIDX = Nothing
    End Sub

    Public Sub WinNUT_PrefsChanged()
        LogFile.LogTracing("WinNut Preferences Changed", LogLvl.LOG_NOTICE, Me)
        Dim NeedReconnect As Boolean = False
        If WinNUT_Params.Arr_Reg_Key.Item("AutoReconnect") <> UPS_Network.AutoReconnect Then
            If WinNUT_Params.Arr_Reg_Key.Item("AutoReconnect") Then
                UPS_Network.AutoReconnect = True
            Else
                UPS_Network.AutoReconnect = False
            End If
        End If
        If UPS_Network.NutHost <> WinNUT_Params.Arr_Reg_Key.Item("ServerAddress") Then
            NeedReconnect = True
            UPS_Network.NutHost = WinNUT_Params.Arr_Reg_Key.Item("ServerAddress")
        End If
        If UPS_Network.NutPort <> WinNUT_Params.Arr_Reg_Key.Item("Port") Then
            NeedReconnect = True
            UPS_Network.NutPort = WinNUT_Params.Arr_Reg_Key.Item("Port")
        End If
        If UPS_Network.NutUPS <> WinNUT_Params.Arr_Reg_Key.Item("UPSName") Then
            NeedReconnect = True
            UPS_Network.NutUPS = WinNUT_Params.Arr_Reg_Key.Item("UPSName")
        End If
        If UPS_Network.NutDelay <> WinNUT_Params.Arr_Reg_Key.Item("Delay") Then
            NeedReconnect = True
            UPS_Network.NutDelay = WinNUT_Params.Arr_Reg_Key.Item("Delay")
        End If
        UPS_Network.Battery_Limit = WinNUT_Params.Arr_Reg_Key.Item("ShutdownLimitBatteryCharge")
        UPS_Network.Backup_Limit = WinNUT_Params.Arr_Reg_Key.Item("ShutdownLimitUPSRemainTime")
        If NeedReconnect And UPS_Network.IsConnected Then
            LogFile.LogTracing("Connection parameter Changed. Force Disconnect", LogLvl.LOG_DEBUG, Me)
            UPS_Network.Disconnect(True)
            ReInitDisplayValues()
            ActualAppIconIdx = AppIconIdx.IDX_ICO_OFFLINE
            LogFile.LogTracing("Update Icon", LogLvl.LOG_DEBUG, Me)
            UpdateIcon_NotifyIcon()
            LogFile.LogTracing("New Parameter Applyed. Force Reconnect", LogLvl.LOG_DEBUG, Me)
            UPS_Network.Connect()
        End If
        NeedReconnect = Nothing
        With AG_InV
            If (.MaxValue <> WinNUT_Params.Arr_Reg_Key.Item("MaxInputVoltage")) Or (.MinValue <> WinNUT_Params.Arr_Reg_Key.Item("MinInputVoltage")) Then
                LogFile.LogTracing("Parameter Dial Input Voltage Need to be Updated", LogLvl.LOG_DEBUG, Me)
                .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxInputVoltage")
                .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinInputVoltage")
                .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
                LogFile.LogTracing("Parameter Dial Input Voltage Updated", LogLvl.LOG_DEBUG, Me)
            End If
        End With
        With AG_InF
            If (.MaxValue <> WinNUT_Params.Arr_Reg_Key.Item("MaxInputFrequency")) Or (.MinValue <> WinNUT_Params.Arr_Reg_Key.Item("MinInputFrequency")) Then
                LogFile.LogTracing("Parameter Dial Input Frequency Need to be Updated", LogLvl.LOG_DEBUG, Me)
                .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxInputFrequency")
                .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinInputFrequency")
                .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
                LogFile.LogTracing("Parameter Dial Input Frequency Updated", LogLvl.LOG_DEBUG, Me)
            End If
        End With
        With AG_OutV
            If (.MaxValue <> WinNUT_Params.Arr_Reg_Key.Item("MaxOutputVoltage")) Or (.MinValue <> WinNUT_Params.Arr_Reg_Key.Item("MinOutputVoltage")) Then
                LogFile.LogTracing("Parameter Dial Output Voltage Need to be Updated", LogLvl.LOG_DEBUG, Me)
                .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxOutputVoltage")
                .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinOutputVoltage")
                .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
                LogFile.LogTracing("Parameter Dial Output Voltage Updated", LogLvl.LOG_DEBUG, Me)
            End If
        End With
        With AG_Load
            If (.MaxValue <> WinNUT_Params.Arr_Reg_Key.Item("MaxUPSLoad")) Or (.MinValue <> WinNUT_Params.Arr_Reg_Key.Item("MinUPSLoad")) Then
                LogFile.LogTracing("Parameter Dial UPS Load Need to be Updated", LogLvl.LOG_DEBUG, Me)
                .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxUPSLoad")
                .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinUPSLoad")
                .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
                LogFile.LogTracing("Parameter Dial UPS Load Updated", LogLvl.LOG_DEBUG, Me)
            End If
        End With
        With AG_BattV
            If (.MaxValue <> WinNUT_Params.Arr_Reg_Key.Item("MinBattVoltage")) Or (.MinValue <> WinNUT_Params.Arr_Reg_Key.Item("MinBattVoltage")) Then
                LogFile.LogTracing("Parameter Dial Voltage Battery Need to be Updated", LogLvl.LOG_DEBUG, Me)
                .MaxValue = WinNUT_Params.Arr_Reg_Key.Item("MaxBattVoltage")
                .MinValue = WinNUT_Params.Arr_Reg_Key.Item("MinBattVoltage")
                .ScaleLinesMajorStepValue = CInt((.MaxValue - .MinValue) / 5)
                LogFile.LogTracing("Parameter Dial Voltage Battery Updated", LogLvl.LOG_DEBUG, Me)
            End If
        End With
        If WinNUT_Params.Arr_Reg_Key.Item("VerifyUpdate") = True Then
            Me.Menu_Help_Sep1.Visible = True
            Me.Menu_Update.Visible = True
            Me.Menu_Update.Visible = Enabled = True
        Else
            Me.Menu_Help_Sep1.Visible = False
            Me.Menu_Update.Visible = False
            Me.Menu_Update.Visible = Enabled = False
        End If
    End Sub

    Private Sub UpdateIcon_NotifyIcon()
        Dim Tmp_Win_Mode As Integer
        Dim Tmp_App_Mode As Integer
        If (ActualAppIconIdx <> LastAppIconIdx) Then
            LogFile.LogTracing("Status Icon Changed", LogLvl.LOG_DEBUG, Me)
            If WinDarkMode Then
                Tmp_Win_Mode = AppIconIdx.WIN_DARK Or AppIconIdx.IDX_OFFSET
            Else
                Tmp_Win_Mode = AppIconIdx.IDX_OFFSET
            End If
            If AppDarkMode Then
                Tmp_App_Mode = AppIconIdx.WIN_DARK Or AppIconIdx.IDX_OFFSET
            Else
                Tmp_App_Mode = AppIconIdx.IDX_OFFSET
            End If
            Dim TmpGuiIDX = ActualAppIconIdx Or Tmp_App_Mode
            If Not HasFocus Then
                TmpGuiIDX = TmpGuiIDX Or AppIconIdx.WIN_DARK
            End If
            Dim TmpTrayIDX = ActualAppIconIdx Or Tmp_Win_Mode
            LogFile.LogTracing("New Icon Value For Systray : " & TmpTrayIDX.ToString, LogLvl.LOG_DEBUG, Me)
            LogFile.LogTracing("New Icon Value For Gui : " & TmpGuiIDX.ToString, LogLvl.LOG_DEBUG, Me)
            If NotifyIcon.Visible Then
                NotifyIcon.Icon = GetIcon(TmpTrayIDX)
            End If
            Me.Icon = GetIcon(TmpGuiIDX)
            LastAppIconIdx = ActualAppIconIdx
            TmpGuiIDX = Nothing
            TmpTrayIDX = Nothing
        End If
        Tmp_Win_Mode = Nothing
        Tmp_App_Mode = Nothing
    End Sub

    Private Function GetIcon(ByVal IconIdx As Integer) As Icon
        Select Case IconIdx
            Case 1025
                Return My.Resources._1025
            Case 1026
                Return My.Resources._1026
            Case 1028
                Return My.Resources._1028
            Case 1032
                Return My.Resources._1032
            Case 1040
                Return My.Resources._1040
            Case 1057
                Return My.Resources._1057
            Case 1058
                Return My.Resources._1058
            Case 1060
                Return My.Resources._1060
            Case 1064
                Return My.Resources._1064
            Case 1072
                Return My.Resources._1072
            Case 1079
                Return My.Resources._1079
            Case 1080
                Return My.Resources._1080
            Case 1092
                Return My.Resources._1092
            Case 1096
                Return My.Resources._1096
            Case 1104
                Return My.Resources._1096
            Case 1121
                Return My.Resources._1121
            Case 1122
                Return My.Resources._1122
            Case 1124
                Return My.Resources._1124
            Case 1128
                Return My.Resources._1128
            Case 1136
                Return My.Resources._1136
            Case 1152
                Return My.Resources._1152
            Case 1216
                Return My.Resources._1216
            Case 1280
                Return My.Resources._1280
            Case 1344
                Return My.Resources._1344
            Case Else
                Return My.Resources._1136
        End Select
    End Function

    Private Sub Menu_UPS_Var_Click(sender As Object, e As EventArgs) Handles Menu_UPS_Var.Click
        LogFile.LogTracing("Open List Var Gui", LogLvl.LOG_DEBUG, Me)
        List_Var_Gui.Activate()
        List_Var_Gui.Visible = True
        HasFocus = False
    End Sub

    Private Sub Update_InstantLog() Handles LogFile.NewData
        Dim Message As String = LogFile.CurrentLogData
        LogFile.LogTracing("New Log to CB_Current Log : " & Message, LogLvl.LOG_DEBUG, Me)
        CB_CurrentLog.Items.Insert(0, Message)
        CB_CurrentLog.SelectedIndex = 0
        If CB_CurrentLog.Items.Count > 10 Then
            For i = 10 To (CB_CurrentLog.Items.Count - 1) Step 1
                CB_CurrentLog.Items.Remove(i)
            Next
        End If
    End Sub

    Private Sub Shutdown_Event() Handles UPS_Network.Shutdown_Condition
        If WinNUT_Params.Arr_Reg_Key.Item("ImmediateStopAction") Then
            Shutdown_Action()
        Else
            LogFile.LogTracing("Open Shutdown Gui", LogLvl.LOG_DEBUG, Me)
            Shutdown_Gui.Activate()
            Shutdown_Gui.Visible = True
            HasFocus = False
        End If
    End Sub
    Private Sub Stop_Shutdown_Event() Handles UPS_Network.Stop_Shutdown
        Shutdown_Gui.Shutdown_Timer.Stop()
        Shutdown_Gui.Shutdown_Timer.Enabled = False
        Shutdown_Gui.Grace_Timer.Stop()
        Shutdown_Gui.Grace_Timer.Enabled = False
        Shutdown_Gui.Hide()
        Shutdown_Gui.Close()
    End Sub
    Public Sub Shutdown_Action()
        Select Case WinNUT_Params.Arr_Reg_Key.Item("TypeOfStop")
            Case 0
                Process.Start("C:\WINDOWS\system32\Shudown.exe", "-s -t 0")
            Case 1
                SetSystemPowerState(True, 0)
            Case 2
                SetSystemPowerState(False, 0)
        End Select
    End Sub

    Private Sub Menu_Update_Click(sender As Object, e As EventArgs) Handles Menu_Update.Click
        Me.mUpdate = True
        Dim th As System.Threading.Thread = New Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf Run_Update))
        th.SetApartmentState(System.Threading.ApartmentState.STA)
        th.Start(Me.UpdateMethod)
    End Sub
    Public Sub Run_Update(ByVal data As Object)
        Dim frmBuild As Update_Gui = New Update_Gui(data) ' Must be created on this thread!
        Application.Run(frmBuild)
        frmBuild = Nothing
    End Sub
End Class
