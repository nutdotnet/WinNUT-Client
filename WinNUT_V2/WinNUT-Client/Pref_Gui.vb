' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports LogLvl = WinNUT_Client_Common.LogLvl
Imports WinNUT_Client_Common

Public Class Pref_Gui
    Private IsShowed As Boolean = False
    Private PrefsModified As Boolean = False

    Public Event SavedPreferences()

    Private Sub Btn_Cancel_Click(sender As Object, e As EventArgs) Handles Btn_Cancel.Click
        LogFile.LogTracing("Close Pref Gui from Button Cancel", LogLvl.LOG_DEBUG, Me)
        Close()
    End Sub

    Private Sub Save_Params()
        Try
            LogFile.LogTracing("Save Parameters.", LogLvl.LOG_DEBUG, Me)
            My.Settings.NUT_ServerAddress = Tb_Server_IP.Text
            My.Settings.CAL_PowerFactor = CDbl(Tb_Cal_PowerFactor.Value)
            My.Settings.CAL_ApparentPowerNom = CInt(Tb_Cal_ApparentPowerNom.Value)
            My.Settings.NUT_ServerPort = CInt(Tb_Port.Text)
            My.Settings.NUT_UPSName = Tb_UPS_Name.Text
            My.Settings.NUT_PollIntervalMsec = CInt(pollingIntervalValue.Value * 1000D)
            My.Settings.NUT_Username = Tb_Login_Nut.Text
            My.Settings.NUT_Password = Tb_Pwd_Nut.Text
            My.Settings.NUT_AutoReconnect = Cb_Reconnect.Checked
            My.Settings.CAL_VoltInMin = CInt(Tb_InV_Min.Text)
            My.Settings.CAL_VoltInMax = CInt(Tb_InV_Max.Text)
            My.Settings.CAL_FreqInNom = Cbx_Freq_Input.SelectedItem
            My.Settings.CAL_FreqInMin = CInt(Tb_InF_Min.Text)
            My.Settings.CAL_FreqInMax = CInt(Tb_InF_Max.Text)
            My.Settings.CAL_VoltOutMin = CInt(Tb_OutV_Min.Text)
            My.Settings.CAL_VoltOutMax = CInt(Tb_OutV_Max.Text)
            My.Settings.CAL_BattVMin = CInt(Tb_BattV_Min.Text)
            My.Settings.CAL_BattVMax = CInt(Tb_BattV_Max.Text)
            My.Settings.MinimizeToTray = CB_Systray.Checked
            My.Settings.MinimizeOnStart = CB_Start_Mini.Checked
            My.Settings.CloseToTray = CB_Close_Tray.Checked
            My.Settings.StartWithWindows = CB_Start_W_Win.Checked
            My.Settings.LG_LogToFile = CB_Use_Logfile.Checked
            My.Settings.LG_LogLevel = Cbx_LogLevel.SelectedIndex
            My.Settings.PW_BattChrgFloor = CInt(Tb_BattLimit_Load.Text)
            My.Settings.PW_RuntimeFloor = CInt(Tb_BattLimit_Time.Text)
            My.Settings.PW_Immediate = Cb_ImmediateStop.Checked
            My.Settings.PW_RespectFSD = CB_Follow_FSD.Checked
            My.Settings.PW_StopType = Cbx_TypeStop.SelectedIndex
            My.Settings.PW_StopDelaySec = CInt(Tb_Delay_Stop.Text)
            My.Settings.PW_UserExtendStopTimer = Cb_ExtendTime.Checked
            My.Settings.PW_ExtendDelaySec = CInt(Tb_GraceTime.Text)
            My.Settings.UP_CheckAtStart = Cb_Update_At_Start.Checked
            My.Settings.UP_AutoChkDelay = Cbx_Delay_Verif.SelectedIndex
            My.Settings.UP_Branch = Cbx_Branch_Update.SelectedIndex

            My.Settings.Save()
            If CB_Start_W_Win.Checked Then
                If My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", Application.ProductName, Nothing) Is Nothing Then
                    My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True).SetValue(Application.ProductName, Application.ExecutablePath)
                    LogFile.LogTracing("WinNUT Added to Startup.", LogLvl.LOG_DEBUG, Me)
                End If
            Else
                If Not My.Computer.Registry.GetValue("HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", Application.ProductName, Nothing) Is Nothing Then
                    My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True).DeleteValue(Application.ProductName)
                    LogFile.LogTracing("WinNUT Removed From Startup.", LogLvl.LOG_DEBUG, Me)
                End If
            End If

            RaiseEvent SavedPreferences()

            SetLogControlsStatus()
            LogFile.LogTracing("WinNut Preferences Saved.", LogLvl.LOG_NOTICE, Me, StrLog.Item(AppResxStr.STR_LOG_PREFS))

            PrefsModified = False
        Catch e As Exception
            LogFile.LogTracing("Error when trying to save preferences.", LogLvl.LOG_ERROR, Me)
            LogFile.LogException(e, Me)
            MessageBox.Show(e.ToString(), "Error while saving")
        End Try
    End Sub

    Private Sub Btn_Apply_Click(sender As Object, e As EventArgs) Handles Btn_Apply.Click
        Save_Params()
        If Not PrefsModified Then
            Btn_Apply.Enabled = False
        End If
    End Sub

    Private Sub Btn_Ok_Click(sender As Object, e As EventArgs) Handles Btn_Ok.Click
        If PrefsModified Then
            Save_Params()
        End If
        Close()
    End Sub

    Private Sub Pref_Gui_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Try
            IsShowed = False
            Tb_Server_IP.Text = My.Settings.NUT_ServerAddress
            Tb_Port.Text = My.Settings.NUT_ServerPort
            Tb_UPS_Name.Text = My.Settings.NUT_UPSName
            pollingIntervalValue.Value = My.Settings.NUT_PollIntervalMsec / 1000D
            Tb_Login_Nut.Text = My.Settings.NUT_Username
            Tb_Pwd_Nut.Text = My.Settings.NUT_Password
            Cb_Reconnect.Checked = My.Settings.NUT_AutoReconnect
            Tb_InV_Min.Text = My.Settings.CAL_VoltInMin
            Tb_InV_Max.Text = My.Settings.CAL_VoltInMax
            Cbx_Freq_Input.SelectedIndex = Cbx_Freq_Input.FindStringExact(My.Settings.CAL_FreqInNom)
            Tb_InF_Min.Text = My.Settings.CAL_FreqInMin
            Tb_InF_Max.Text = My.Settings.CAL_FreqInMax
            Tb_OutV_Min.Text = My.Settings.CAL_VoltOutMin
            Tb_OutV_Max.Text = My.Settings.CAL_VoltOutMax
            Tb_BattV_Min.Text = My.Settings.CAL_BattVMin
            Tb_BattV_Max.Text = My.Settings.CAL_BattVMax
            CB_Systray.Checked = My.Settings.MinimizeToTray
            CB_Start_Mini.Checked = My.Settings.MinimizeOnStart
            CB_Close_Tray.Checked = My.Settings.CloseToTray
            CB_Start_W_Win.Checked = My.Settings.StartWithWindows
            CB_Use_Logfile.Checked = My.Settings.LG_LogToFile
            Cbx_LogLevel.SelectedIndex = My.Settings.LG_LogLevel
            Tb_BattLimit_Load.Text = My.Settings.PW_BattChrgFloor
            Tb_BattLimit_Time.Text = My.Settings.PW_RuntimeFloor
            Cb_ImmediateStop.Checked = My.Settings.PW_Immediate
            CB_Follow_FSD.Checked = My.Settings.PW_RespectFSD
            Cbx_TypeStop.SelectedIndex = My.Settings.PW_StopType
            Tb_Delay_Stop.Text = My.Settings.PW_StopDelaySec
            Cb_ExtendTime.Checked = My.Settings.PW_UserExtendStopTimer
            Tb_GraceTime.Text = My.Settings.PW_ExtendDelaySec
            Cb_Update_At_Start.Checked = My.Settings.UP_CheckAtStart
            Cbx_Delay_Verif.SelectedIndex = My.Settings.UP_AutoChkDelay
            Cbx_Branch_Update.SelectedIndex = My.Settings.UP_Branch
            If CB_Systray.Checked Then
                CB_Start_Mini.Enabled = True
                CB_Close_Tray.Enabled = True
            Else
                CB_Start_Mini.Enabled = False
                CB_Close_Tray.Enabled = False
            End If
            If Cb_ImmediateStop.Checked Then
                Tb_Delay_Stop.Enabled = False
            Else
                Tb_Delay_Stop.Enabled = True
            End If
            If Cb_ExtendTime.Checked Then
                Tb_GraceTime.Enabled = True
            Else
                Tb_GraceTime.Enabled = False
            End If

            Cb_Update_At_Start_CheckedChanged(Me, EventArgs.Empty)

            For Each TabCtrl In TabControl_Options.Controls.OfType(Of TabPage)()
                Dim TBoxes = TabCtrl.Controls.OfType(Of TextBox)()
                Dim ChkBoxes = TabCtrl.Controls.OfType(Of CheckBox)()
                Dim CmbBoxes = TabCtrl.Controls.OfType(Of ComboBox)()
                For Each TBox In TBoxes
                    AddHandler TBox.TextChanged, AddressOf Event_Ctrl_Value_Changed
                Next
                For Each ChkBox In ChkBoxes
                    AddHandler ChkBox.CheckedChanged, AddressOf Event_Ctrl_Value_Changed
                Next
                For Each CmbBox In CmbBoxes
                    AddHandler CmbBox.SelectedIndexChanged, AddressOf Event_Ctrl_Value_Changed
                Next
            Next
            AddHandler pollingIntervalValue.ValueChanged, AddressOf Event_Ctrl_Value_Changed

            SetLogControlsStatus()
            IsShowed = True
            LogFile.LogTracing("Pref Gui Opened.", LogLvl.LOG_DEBUG, Me)
        Catch Except As Exception
            IsShowed = False
            Close()
            LogFile.LogTracing("Error on Opening Pref_Gui:" & vbNewLine & Except.ToString(), LogLvl.LOG_ERROR, Me)
        End Try
    End Sub

    Private Sub CB_Systray_CheckedChanged(sender As Object, e As EventArgs) Handles CB_Systray.CheckedChanged
        If CB_Systray.Checked Then
            CB_Start_Mini.Enabled = True
            CB_Close_Tray.Enabled = True
        Else
            CB_Start_Mini.Enabled = False
            CB_Close_Tray.Enabled = False
        End If
    End Sub

    Private Sub Cb_ImmediateStop_CheckedChanged(sender As Object, e As EventArgs) Handles Cb_ImmediateStop.CheckedChanged
        If Cb_ImmediateStop.Checked Then
            Tb_Delay_Stop.Enabled = False
        Else
            Tb_Delay_Stop.Enabled = True
            Number_Validating(Tb_Delay_Stop, New System.ComponentModel.CancelEventArgs())
        End If
    End Sub

    Private Sub Cb_ExtendTime_CheckedChanged(sender As Object, e As EventArgs) Handles Cb_ExtendTime.CheckedChanged
        If Cb_ExtendTime.Checked Then
            Tb_GraceTime.Enabled = True
            Number_Validating(Tb_GraceTime, New System.ComponentModel.CancelEventArgs())
        Else
            Tb_GraceTime.Enabled = False
        End If
    End Sub

    Private Sub Cb_Update_At_Start_CheckedChanged(sender As Object, e As EventArgs) Handles Cb_Update_At_Start.CheckedChanged
        Cbx_Delay_Verif.Enabled = Cb_Update_At_Start.Checked
    End Sub

    Private Sub Number_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles Tb_Port.Validating, Tb_OutV_Min.Validating, Tb_OutV_Max.Validating, Tb_InV_Min.Validating, Tb_InV_Max.Validating, Tb_InF_Min.Validating, Tb_InF_Max.Validating, Tb_GraceTime.Validating, Tb_Delay_Stop.Validating, Tb_BattV_Min.Validating, Tb_BattV_Max.Validating, Tb_BattLimit_Time.Validating, Tb_BattLimit_Load.Validating
        If IsShowed Then
            Dim StrTest As String = sender.Text
            Dim Result As Object = 0
            Dim MinValue, MaxValue As Integer

            LogFile.LogTracing(String.Format("Check that the value of {0} for {1} is correct.", sender.Text, sender.Name), LogLvl.LOG_DEBUG, Me)
            Select Case sender.Name
                Case "Tb_Port"
                    MinValue = 1
                    MaxValue = 65536
                Case "Tb_OutV_Min", "Tb_OutV_Max", "Tb_InV_Min", "Tb_InV_Max", "Tb_BattV_Min", "Tb_BattV_Max"
                    MinValue = 0
                    MaxValue = 999
                Case "Tb_InF_Min", "Tb_InF_Max", "Tb_BattLimit_Load"
                    MinValue = 0
                    MaxValue = 100
                Case "Tb_BattLimit_Time"
                    MinValue = 0
                    MaxValue = 3600
            'Min value has to be 1 as 0 can't be assigned to a timer interval (used in Shutdown_Gui)
                Case "Tb_GraceTime", "Tb_Delay_Stop"
                    MinValue = 1
                    MaxValue = 3600
            End Select

            If sender.Text = "" Then
                sender.Text = MinValue
            End If

            If Integer.TryParse(sender.Text, Result) Then
                If (Result >= MinValue And Result <= MaxValue) Then
                    LogFile.LogTracing(String.Format("Value of {0} for {1} is valid.", Result, sender.Name), LogLvl.LOG_DEBUG, Me)
                    sender.BackColor = Color.White
                Else
                    LogFile.LogTracing(String.Format("Value of {0} for {1} is invalid.", Result, sender.Name), LogLvl.LOG_ERROR, Me)
                    e.Cancel = True
                    sender.BackColor = Color.Red
                End If
            Else
                e.Cancel = True
                sender.BackColor = Color.Red
            End If
        End If
    End Sub
    Private Sub Correct_IP_Validating(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles Tb_Server_IP.Validating
        LogFile.LogTracing("Check that the Nut Host address is valid.", LogLvl.LOG_DEBUG, Me)
        Dim Pattern As String
        Dim StrTest As String = sender.Text
        Dim Is_Correct As Boolean = False
        'Test IPV4
        Pattern = "^(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$"
        If System.Text.RegularExpressions.Regex.IsMatch(sender.Text, Pattern) Then
            Is_Correct = True
            LogFile.LogTracing("The Nut Host address is a valid IPV4 address.", LogLvl.LOG_WARNING, Me)
        End If
        'Test IPV6
        Pattern = "^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$"
        If (System.Text.RegularExpressions.Regex.IsMatch(sender.Text, Pattern) And Not Is_Correct) Then
            Is_Correct = True
            LogFile.LogTracing("The Nut Host address is a valid IPV6 address.", LogLvl.LOG_WARNING, Me)
        End If
        'Test fqdn
        Pattern = "^(?:(?!\d+\.|-)[a-zA-Z0-9_\-]{1,63}(?<!-)\.?)+(?:[a-zA-Z]{2,})$"
        If (System.Text.RegularExpressions.Regex.IsMatch(sender.Text, Pattern) And Not Is_Correct) Then
            Is_Correct = True
            LogFile.LogTracing("The Nut Host address is a valid FQDN address.", LogLvl.LOG_WARNING, Me)
        End If

        'Result
        If Is_Correct Then
            sender.BackColor = Color.White
        Else
            LogFile.LogTracing("The Nut Host address is a invalid", LogLvl.LOG_ERROR, Me)
            e.Cancel = True
            sender.BackColor = Color.Red
        End If
    End Sub
    Private Sub Pref_Gui_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        e.Cancel = False
    End Sub

    Private Sub TabControl_Options_Selecting(sender As Object, e As TabControlCancelEventArgs) Handles TabControl_Options.Selecting
        If TabControl_Options.SelectedTab Is Tab_Miscellanous Then
            SetLogControlsStatus()
        End If
    End Sub

    Private Sub Pref_Gui_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Icon = WinNUT.Icon
        LogFile.LogTracing("Load Pref Gui", LogLvl.LOG_DEBUG, Me)
        Try
            Tb_Cal_PowerFactor.Value = CDbl(My.Settings.CAL_PowerFactor)
        Catch
        End Try
        Try
            Tb_Cal_ApparentPowerNom.Value = CInt(My.Settings.CAL_ApparentPowerNom)
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Handle any value in the form changing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Event_Ctrl_Value_Changed(sender As Object, e As EventArgs)
        If IsShowed Then
            PrefsModified = True
            Btn_Apply.Enabled = True
        End If
    End Sub

#Region "Logging controls"

    Private Sub Btn_ViewLog_Click(sender As Object, e As EventArgs) Handles Btn_ViewLog.Click
        LogFile.LogTracing("User clicked ViewLog button.", LogLvl.LOG_DEBUG, Me)
        Try
            Process.Start(LogFile.LogFilePath)
            LogFile.LogTracing("Opened UI window to log location.", LogLvl.LOG_NOTICE, Me)
        Catch ex As Exception
            LogFile.LogException(ex, Me)
            SetLogControlsStatus()
        End Try
    End Sub

    Private Sub Btn_DeleteLog_Click(sender As Object, e As EventArgs) Handles Btn_DeleteLog.Click
        LogFile.LogTracing("User clicked DeleteLog button.", LogLvl.LOG_DEBUG, Me)

        Try
            LogFile.DeleteLogFile()
            PrefsModified = True ' Will help reinitialize log file later if user still wants it.
        Catch ex As Exception
            LogFile.LogException(ex, Me)
        End Try

        SetLogControlsStatus()
    End Sub

    ''' <summary>
    ''' Enable or disable controls to view and delete log data if it's available.
    ''' </summary>
    Private Sub SetLogControlsStatus()
        If LogFile.IsWritingToFile Then
            Btn_ViewLog.Enabled = True
            Btn_DeleteLog.Enabled = True
        Else
            Btn_ViewLog.Enabled = False
            Btn_DeleteLog.Enabled = False
        End If
    End Sub

#End Region
End Class
