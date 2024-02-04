' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Namespace OldParams
    ''' <summary>
    ''' Helper class to upgrade parameters.
    ''' </summary>
    Public Class UpgradableParams
        Inherits WinNUT_Params

        Private _PrefSettingsLookup As New List(Of PrefSettingPair)({
            New PrefSettingPair("ServerAddress", "NUT_ServerAddress"),
            New PrefSettingPair("Port", "NUT_ServerPort"),
            New PrefSettingPair("UPSName", "NUT_UPSName"),
            New PrefSettingPair("Delay", "NUT_PollIntervalMsec"),
            New PrefSettingPair("NutLogin", "NUT_Username"),
            New PrefSettingPair("NutPassword", "NUT_Password"),
            New PrefSettingPair("AutoReconnect", "NUT_AutoReconnect"),
            New PrefSettingPair("MinInputVoltage", "CAL_VoltInMin"),
            New PrefSettingPair("MaxInputVoltage", "CAL_VoltInMax"),
            New PrefSettingPair("FrequencySupply", "CAL_FreqInNom"),
            New PrefSettingPair("MinInputFrequency", "CAL_FreqInMin"),
            New PrefSettingPair("MaxInputFrequency", "CAL_FreqInMax"),
            New PrefSettingPair("MinOutputVoltage", "CAL_VoltOutMin"),
            New PrefSettingPair("MaxOutputVoltage", "CAL_VoltOutMax"),
            New PrefSettingPair("MinBattVoltage", "CAL_BattVMin"),
            New PrefSettingPair("MaxBattVoltage", "CAL_BattVMax"),
            New PrefSettingPair("MinimizeToTray", "MinimizeToTray"),
            New PrefSettingPair("MinimizeOnStart", "MinimizeOnStart"),
            New PrefSettingPair("CloseToTray", "CloseToTray"),
            New PrefSettingPair("StartWithWindows", "StartWithWindows"),
            New PrefSettingPair("UseLogFile", "LG_LogToFile"),
            New PrefSettingPair("Log Level", "LG_LogLevel"),
            New PrefSettingPair("ShutdownLimitBatteryCharge", "PW_BattChrgFloor"),
            New PrefSettingPair("ShutdownLimitUPSRemainTime", "PW_RuntimeFloor"),
            New PrefSettingPair("ImmediateStopAction", "PW_Immediate"),
            New PrefSettingPair("Follow_FSD", "PW_RespectFSD"),
            New PrefSettingPair("TypeOfStop", "PW_StopType"),
            New PrefSettingPair("DelayToShutdown", "PW_StopDelaySec"),
            New PrefSettingPair("AllowExtendedShutdownDelay", "PW_UserExtendStopTimer"),
            New PrefSettingPair("ExtendedShutdownDelay", "PW_ExtendDelaySec"),
            New PrefSettingPair("VerifyUpdate", "UP_AutoUpdate"),
            New PrefSettingPair("VerifyUpdateAtStart", "UP_CheckAtStart"),
            New PrefSettingPair("DelayBetweenEachVerification", "UP_AutoChkDelay"),
            New PrefSettingPair("StableOrDevBranch", "UP_Branch"),
            New PrefSettingPair("LastDateVerification", "UP_LastCheck")
        })

        Public Property PrefSettingsLookup As List(Of PrefSettingPair)
            Get
                Return _PrefSettingsLookup
            End Get
            Set(value As List(Of PrefSettingPair))
                _PrefSettingsLookup = value
            End Set
        End Property

        Private lastCount As Integer = -1
        Public ReadOnly Property TotalPrefs As Integer
            Get
                If lastCount = -1 Then ' Calculate, then cache the result.
                    lastCount = 0

                    For Each collection As KeyValuePair(Of String, Dictionary(Of String, Object)) In Parameters
                        lastCount += collection.Value.Count
                    Next
                End If

                Return lastCount
            End Get
        End Property

        Public Sub New()
            Parameters = LoadParams(Me)
        End Sub

        Public Class PrefSettingPair
            Public Property OldPreferenceName As String
            Public Property NewSettingsName As String

            Public Sub New(oldPrefName As String, newSettingName As String)
                OldPreferenceName = oldPrefName
                NewSettingsName = newSettingName
            End Sub
        End Class

        Friend Class UpgradeWorkerArguments
            Public ReadOnly Property DoImport As Boolean
            Public ReadOnly Property DoBackup As Boolean
            Public ReadOnly Property DoDelete As Boolean

            Public Sub New(doImport As Boolean, doBackup As Boolean, doDelete As Boolean)
                Me.DoImport = doImport
                Me.DoBackup = doBackup
                Me.DoDelete = doDelete
            End Sub
        End Class
        Friend Class UpgradeWorkerProgressReport
            Public Property LogOutput As String
            Public Property LogLevel As LogLvl
            Public Property Sender As Object
            Public Property LogResourceString As String

            Public Sub New(logOutput As String, logLevel As LogLvl, sender As Object, Optional logRes As String = Nothing)
                Me.LogOutput = logOutput
                Me.LogLevel = logLevel
                Me.Sender = sender
                Me.LogResourceString = logRes
            End Sub
        End Class
    End Class
End Namespace
