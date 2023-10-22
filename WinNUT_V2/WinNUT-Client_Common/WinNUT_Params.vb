' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.Win32

Public Class WinNUT_Params
    Public Const REG_KEY_ROOT = "SOFTWARE\WinNUT\"
    Public Shared ReadOnly REG_HIVE As RegistryKey = Registry.CurrentUser

    Public Arr_Reg_Key As New Dictionary(Of String, Object)
    Private Arr_Reg_Key_Base As New Dictionary(Of String, Dictionary(Of String, Object))

    Public Sub New()
        With Arr_Reg_Key
            .Add("ServerAddress", "") ' NUT_ServerAddress
            .Add("Port", 0) ' NUT_ServerPort
            .Add("UPSName", "") ' NUT_UPSName
            .Add("Delay", 1000) ' NUT_PollIntervalMsec
            .Add("NutLogin", String.Empty) ' NUT_Username
            .Add("NutPassword", String.Empty) ' NUT_PasswordEnc
            .Add("AutoReconnect", vbFalse) ' NUT_AutoReconnect
            .Add("MinInputVoltage", 0) ' CAL_VoltInMin
            .Add("MaxInputVoltage", 0) ' CAL_VoltInMax
            .Add("FrequencySupply", 1) ' CAL_FreqInNom
            .Add("MinInputFrequency", 0) ' CAL_FreqInMin
            .Add("MaxInputFrequency", 0) ' CAL_FreqInMax
            .Add("MinOutputVoltage", 0) ' CAL_VoltOutMin
            .Add("MaxOutputVoltage", 0) ' CAL_VoltOutMax
            .Add("MinBattVoltage", 0) ' CAL_BattVMin
            .Add("MaxBattVoltage", 0) ' CAL_BattVMax
            .Add("MinimizeToTray", vbFalse) ' ☑️
            .Add("MinimizeOnStart", vbFalse) ' ☑️
            .Add("CloseToTray", vbFalse) ' ☑️
            .Add("StartWithWindows", vbFalse) ' ☑️
            .Add("UseLogFile", vbFalse) ' LG_LogToFile
            .Add("Log Level", 1) ' LG_LogLevel
            .Add("ShutdownLimitBatteryCharge", 0) ' PW_BattChrgFloor
            .Add("ShutdownLimitUPSRemainTime", 0) ' PW_RuntimeFloor
            .Add("ImmediateStopAction", vbFalse) ' PW_Immediate
            .Add("Follow_FSD", vbFalse) ' PW_RespectFSD
            .Add("TypeOfStop", 1) ' PW_StopType
            .Add("DelayToShutdown", 0) ' PW_StopDelaySec
            .Add("AllowExtendedShutdownDelay", vbFalse) ' PW_UserExtendStopTimer
            .Add("ExtendedShutdownDelay", 0) ' PW_ExtendDelaySec
            .Add("VerifyUpdate", vbFalse) ' UP_AutoUpdate
            .Add("VerifyUpdateAtStart", vbFalse) ' UP_CheckAtStart
            .Add("DelayBetweenEachVerification", 1) ' UP_AutoChkDelay
            .Add("StableOrDevBranch", 1) ' UP_Branch
            .Add("LastDateVerification", "") ' UP_LastCheck
        End With
    End Sub

    Public Sub Load_Params()
        Dim Arr_Reg_Connexion As New Dictionary(Of String, Object)
        Dim Arr_Reg_Calibration As New Dictionary(Of String, Object)
        Dim Arr_Reg_Miscellanous As New Dictionary(Of String, Object)
        Dim Arr_Reg_Logging As New Dictionary(Of String, Object)
        Dim Arr_Reg_Power As New Dictionary(Of String, Object)
        Dim Arr_Reg_Update As New Dictionary(Of String, Object)


        With Arr_Reg_Connexion
            .Add("ServerAddress", "nutserver host")
            .Add("Port", 3493)
            .Add("UPSName", "UPSName")
            .Add("Delay", 1000)
            .Add("NutLogin", String.Empty)
            .Add("NutPassword", String.Empty)
            .Add("AutoReconnect", vbFalse)
        End With
        With Arr_Reg_Calibration
            .Add("MinInputVoltage", 210)
            .Add("MaxInputVoltage", 270)
            .Add("FrequencySupply", 0)
            .Add("MinInputFrequency", 40)
            .Add("MaxInputFrequency", 60)
            .Add("MinOutputVoltage", 210)
            .Add("MaxOutputVoltage", 250)
            .Add("MinBattVoltage", 6)
            .Add("MaxBattVoltage", 18)
        End With
        With Arr_Reg_Miscellanous
            .Add("MinimizeToTray", vbFalse)
            .Add("MinimizeOnStart", vbFalse)
            .Add("CloseToTray", vbFalse)
            .Add("StartWithWindows", vbFalse)
        End With
        With Arr_Reg_Logging
            .Add("UseLogFile", vbFalse)
            .Add("Log Level", 0)
        End With
        With Arr_Reg_Power
            .Add("ShutdownLimitBatteryCharge", 30)
            .Add("ShutdownLimitUPSRemainTime", 120)
            .Add("ImmediateStopAction", vbFalse)
            .Add("Follow_FSD", vbFalse)
            .Add("TypeOfStop", 0)
            .Add("DelayToShutdown", 15)
            .Add("AllowExtendedShutdownDelay", vbFalse)
            .Add("ExtendedShutdownDelay", 15)
        End With
        With Arr_Reg_Update
            .Add("VerifyUpdate", vbFalse)
            .Add("VerifyUpdateAtStart", vbFalse)
            .Add("DelayBetweenEachVerification", 2)
            .Add("StableOrDevBranch", 0)
            .Add("LastDateVerification", "")
        End With
        With Arr_Reg_Key_Base
            .Add("Connexion", Arr_Reg_Connexion)
            .Add("Appareance", Arr_Reg_Miscellanous)
            .Add("Calibration", Arr_Reg_Calibration)
            .Add("Power", Arr_Reg_Power)
            .Add("Logging", Arr_Reg_Logging)
            .Add("Update", Arr_Reg_Update)
        End With

        ' Fill in parameter structures with data read in from the Registry (or defaults.)

        For Each RegKeys As KeyValuePair(Of String, Dictionary(Of String, Object)) In Arr_Reg_Key_Base
            For Each RegValue As KeyValuePair(Of String, Object) In RegKeys.Value
                Dim WinReg = REG_HIVE.GetValue(REG_KEY_ROOT & RegKeys.Key, RegValue.Key, RegValue.Value)

                If WinReg Is Nothing Then
                    LogFile.LogTracing("Registry key " & RegKeys.Key & RegValue.Key & " did not exist at load time. Creating and setting default value.", LogLvl.LOG_NOTICE, Nothing)
                    REG_HIVE.CreateSubKey(RegKeys.Key)
                    REG_HIVE.SetValue(REG_KEY_ROOT & RegKeys.Key, RegValue.Key, RegValue.Value)
                    WinReg = RegValue.Value
                End If

                If (RegValue.Key = "NutLogin" Or RegValue.Key = "NutPassword") Then
                    If Not String.IsNullOrEmpty(WinReg) Then
                        Try
                            WinReg = DecryptData(WinReg)
                        Catch ex As Exception
                            WinReg = RegValue.Value
                            LogFile.LogTracing(ex.GetType().ToString() & " encountered trying to decrypt " & RegValue.Key &
                                               ". Using default value (" & WinReg & ")", LogLvl.LOG_ERROR, Nothing)
                        End Try
                    End If
                End If

                Arr_Reg_Key.Item(RegValue.Key) = WinReg
            Next
        Next
    End Sub

    Public Sub Save_Params()
        For Each RegKeys As KeyValuePair(Of String, Dictionary(Of String, Object)) In Arr_Reg_Key_Base
            For Each RegValue As KeyValuePair(Of String, Object) In RegKeys.Value
                Dim saveValue = Arr_Reg_Key.Item(RegValue.Key)

                If (RegValue.Key = "NutLogin" Or RegValue.Key = "NutPassword") Then
                    saveValue = EncryptData(saveValue)
                End If

                REG_HIVE.SetValue(REG_KEY_ROOT & RegKeys.Key, RegValue.Key, saveValue)
            Next
        Next
    End Sub

    Private Function EncryptData(plaintext As String) As String
        Dim encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(plaintext),
                                                            Nothing, DataProtectionScope.CurrentUser)

        Return Convert.ToBase64String(encryptedData)
    End Function

    ''' <summary>
    ''' Decrypt a string that was previously encrypted by the <see cref="EncryptData(String)"/> function call.
    ''' </summary>
    ''' <param name="encryptedtext">A Base64-encoded and encrypted string to decrypt.</param>
    ''' <returns>An unencrypted Unicode string.</returns>
    ''' <exception cref="ArgumentNullException">The parameter was null.</exception>
    ''' <exception cref="CryptographicException">Decryption failed, likely because the argument was not an encrypted string.</exception>"
    Private Function DecryptData(encryptedtext As String) As String
        Dim decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(encryptedtext),
                                                    Nothing, DataProtectionScope.CurrentUser)

        Return Encoding.Unicode.GetString(decryptedData)
    End Function
End Class
