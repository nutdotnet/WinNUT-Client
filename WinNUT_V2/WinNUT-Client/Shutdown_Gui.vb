' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports LogLvl = WinNUT_Client_Common.LogLvl
Imports AppResxStr = WinNUT_Client_Common.AppResxStr
Imports WinNUT_Client_Common

Public Class Shutdown_Gui
    Private RedText As Boolean = True
    Private ReadOnly Shutdown_PBar As New WinFormControls.CProgressBar
    Private Start_Shutdown As Date
    Private Offset_STimer As Double = 0
    Private STimer As Double = 0
    Private Remained As Double = 0
    Public Grace_Timer As New Timer
    Public Shutdown_Timer As New Timer

    Private Sub Grace_Button_Click(sender As Object, e As EventArgs) Handles Grace_Button.Click
        Shutdown_Timer.Stop()
        Shutdown_Timer.Enabled = False
        Grace_Button.Enabled = False
        Grace_Timer.Enabled = True
        Grace_Timer.Start()
        Offset_STimer = My.Settings.PW_ExtendDelaySec
    End Sub

    Private Sub Shutdown_Gui_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Icon = WinNUT.Icon
        LogFile.LogTracing("Load ShutDown Gui", LogLvl.LOG_DEBUG, Me)
        Grace_Timer.Enabled = False
        Grace_Timer.Stop()
        'If ExtendedShutdownDelay = 0 (the default value), the next line fails and the whole shutdown sequence fails - Thus no shutdown
        'Moved next line lower down
        'Me.Grace_Timer.Interval = (WinNUT_Params.Arr_Reg_Key.Item("ExtendedShutdownDelay") * 1000)
        Shutdown_Timer.Interval = (My.Settings.PW_StopDelaySec * 1000)
        STimer = My.Settings.PW_StopDelaySec
        Remained = STimer
        If My.Settings.PW_UserExtendStopTimer Then
            Grace_Button.Enabled = True
            'Moved here so it is only used if grace period is allowed
            Try
                Grace_Timer.Interval = (My.Settings.PW_ExtendDelaySec * 1000)
            Catch ex As Exception
                'Disable Grace peroid option if Interval is set to 0
                Grace_Button.Enabled = False
            End Try
        Else
            Grace_Button.Enabled = False
        End If
        Start_Shutdown = Now
        With Shutdown_PBar
            .Location = New Point(10, 150)
            .Size = New Point(400, 23)
            .Style = ProgressBarStyle.Continuous
            .Font = New Font(.Font, .Font.Style Or FontStyle.Bold)
            .ForeColor = Color.Black
            Dim TimeToShow As String
            Dim iSpan As TimeSpan = TimeSpan.FromSeconds(Shutdown_Timer.Interval / 1000)
            If Shutdown_Timer.Interval = (3600 * 1000) Then
                TimeToShow = iSpan.Hours.ToString.PadLeft(2, "0"c) & ":" &
                                        iSpan.Minutes.ToString.PadLeft(2, "0"c) & ":" &
                                        iSpan.Seconds.ToString.PadLeft(2, "0"c)
            Else
                TimeToShow = iSpan.Minutes.ToString.PadLeft(2, "0"c) & ":" &
                             iSpan.Seconds.ToString.PadLeft(2, "0"c)
            End If
            .Text = TimeToShow
            .Value = 0
        End With
        Controls.Add(Shutdown_PBar)
        AddHandler Grace_Timer.Tick, AddressOf Grace_Timer_Tick
        AddHandler Shutdown_Timer.Tick, AddressOf Shutdown_Timer_Tick
    End Sub

    Private Sub Shutdown_Gui_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Shutdown_Timer.Enabled = True
        Shutdown_Timer.Start()
        lbl_UPSStatus.Text = String.Format(StrLog.Item(AppResxStr.STR_SHUT_STAT), WinNUT.UPS_BattCh.ToString(), WinNUT.Lbl_VRTime.Text)
        LogFile.LogTracing("Shutdown GUI is shown and timer started for " & Shutdown_Timer.Interval / 1000 & " seconds.", LogLvl.LOG_NOTICE, Me)
    End Sub

    Private Sub Grace_Timer_Tick(sender As Object, e As EventArgs)
        Shutdown_Timer.Interval = Remained * 1000
        Shutdown_Timer.Enabled = True
        Shutdown_Timer.Start()
    End Sub

    Private Sub ShutDown_Btn_Click(sender As Object, e As EventArgs) Handles ShutDown_Btn.Click
        WinNUT.Shutdown_Action()
    End Sub

    Private Sub Shutdown_Timer_Tick(sender As Object, e As EventArgs)
        LogFile.LogTracing("Shutdown timer tick.", LogLvl.LOG_NOTICE, Me)
        Shutdown_PBar.Value = 100
        Threading.Thread.Sleep(1000)
        Run_Timer.Enabled = False
        Shutdown_Timer.Stop()
        Shutdown_Timer.Enabled = False
        Grace_Timer.Stop()
        Grace_Timer.Enabled = False
        Hide()
        WinNUT.Shutdown_Action()

        Close()
    End Sub

    Private Sub Run_Timer_Tick(sender As Object, e As EventArgs) Handles Run_Timer.Tick
        If RedText Then
            lbl_UPSStatus.ForeColor = Color.Red
            RedText = False
        Else
            lbl_UPSStatus.ForeColor = Color.Black
            RedText = True
        End If
        If Shutdown_Timer.Enabled = True And Remained > 0 Then
            Remained = Int(STimer + Offset_STimer - Now.Subtract(Start_Shutdown).TotalSeconds)
            Dim NewValue As Integer = 100
            If Remained > 0 Then
                NewValue -= (100 * (Remained / STimer))
                If NewValue > 100 Then
                    NewValue = 100
                End If
            End If
            Dim TimeToShow As String
            Dim iSpan As TimeSpan = TimeSpan.FromSeconds(Remained)
            If Shutdown_Timer.Interval = (3600 * 1000) Then
                TimeToShow = iSpan.Hours.ToString.PadLeft(2, "0"c) & ":" &
                                        iSpan.Minutes.ToString.PadLeft(2, "0"c) & ":" &
                                        iSpan.Seconds.ToString.PadLeft(2, "0"c)
            Else
                TimeToShow = iSpan.Minutes.ToString.PadLeft(2, "0"c) & ":" &
                             iSpan.Seconds.ToString.PadLeft(2, "0"c)
            End If
            Shutdown_PBar.Text = TimeToShow
            Shutdown_PBar.Value = NewValue
            lbl_UPSStatus.Text = String.Format(StrLog.Item(AppResxStr.STR_SHUT_STAT), WinNUT.UPS_BattCh.ToString(), WinNUT.Lbl_VRTime.Text)
        End If
    End Sub

    Private Sub Shutdown_Gui_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Visible Then
            e.Cancel = True
        End If
    End Sub
End Class
