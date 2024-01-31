' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Text
Imports System.Windows.Forms
Imports Microsoft.Win32

Namespace OldParams
    ''' <summary>
    ''' Previous system of persistent settings storage for WinNUT, in the Windows <see cref="Registry"/>.
    ''' </summary>
    Public Class WinNUT_Params
        Private Const REG_KEY_ROOT = "SOFTWARE\WinNUT\"
        Private Shared ReadOnly DEFAULT_PARAM_STRUCTURE As New Dictionary(Of String, Dictionary(Of String, Object)) From {
            {"Connexion", New Dictionary(Of String, Object) From {
                {"ServerAddress", "nutserver host"},
                {"Port", 3493},
                {"UPSName", "UPSName"},
                {"Delay", 1000},
                {"NutLogin", New SerializedProtectedString(String.Empty)},
                {"NutPassword", New SerializedProtectedString(String.Empty)},
                {"AutoReconnect", False}
            }},
            {"Appareance", New Dictionary(Of String, Object) From {
                {"MinimizeToTray", False},
                {"MinimizeOnStart", False},
                {"CloseToTray", False},
                {"StartWithWindows", False}
            }},
            {"Calibration", New Dictionary(Of String, Object) From {
                {"MinInputVoltage", 210},
                {"MaxInputVoltage", 270},
                {"FrequencySupply", 0},
                {"MinInputFrequency", 40},
                {"MaxInputFrequency", 60},
                {"MinOutputVoltage", 210},
                {"MaxOutputVoltage", 250},
                {"MinBattVoltage", 6},
                {"MaxBattVoltage", 18}
            }},
            {"Power", New Dictionary(Of String, Object) From {
                {"ShutdownLimitBatteryCharge", 30},
                {"ShutdownLimitUPSRemainTime", 120},
                {"ImmediateStopAction", False},
                {"Follow_FSD", False},
                {"TypeOfStop", 0},
                {"DelayToShutdown", 15},
                {"AllowExtendedShutdownDelay", False},
                {"ExtendedShutdownDelay", 15}
            }},
            {"Logging", New Dictionary(Of String, Object) From {
                {"UseLogFile", False},
                {"Log Level", 0}
            }},
            {"Update", New Dictionary(Of String, Object) From {
                {"VerifyUpdate", False},
                {"VerifyUpdateAtStart", False},
                {"DelayBetweenEachVerification", 2},
                {"StableOrDevBranch", 0},
                {"LastDateVerification", Date.MinValue}
            }}
        }

        Private _Parameters As Dictionary(Of String, Dictionary(Of String, Object))

        Public Property Parameters As Dictionary(Of String, Dictionary(Of String, Object))
            Get
                Return _Parameters
            End Get
            Protected Set(value As Dictionary(Of String, Dictionary(Of String, Object)))
                _Parameters = value
            End Set
        End Property

        Public Shared ReadOnly Property RegistryKeyRoot As RegistryKey
            Get
                Return Registry.CurrentUser.OpenSubKey(REG_KEY_ROOT, True)
            End Get
        End Property

        ''' <summary>
        ''' Load parameters from the Windows User Registry Hive.
        ''' </summary>
        ''' <returns>At a bare minimum, an empty Dictionary structure. Depending on set parameters, this will return
        ''' filled structures with either default preferences or loaded preferences.</returns>
        Protected Shared Function LoadParams(callingObj As Object) As Dictionary(Of String, Dictionary(Of String, Object))
            Dim newParams = New Dictionary(Of String, Dictionary(Of String, Object))

            If RegistryKeyRoot IsNot Nothing Then
                For Each ParamStructFolder As KeyValuePair(Of String, Dictionary(Of String, Object)) In DEFAULT_PARAM_STRUCTURE
                    Dim regKeyFolder = RegistryKeyRoot.OpenSubKey(ParamStructFolder.Key)

                    If regKeyFolder IsNot Nothing Then
                        Dim paramFolder As New Dictionary(Of String, Object)

                        For Each ParamItem As KeyValuePair(Of String, Object) In ParamStructFolder.Value
                            Try
                                Dim WinReg = CTypeDynamic(regKeyFolder.GetValue(ParamItem.Key), ParamItem.Value.GetType())
                                paramFolder.Add(ParamItem.Key, WinReg)
                                LogFile.LogTracing("Loaded parameter " & ParamItem.Key, LogLvl.LOG_NOTICE, callingObj)
                            Catch ex As Exception
                                LogFile.LogTracing(String.Format("Failed to load value from Registry [{0}\{1}]:{2}{3}",
                                        regKeyFolder.Name, ParamItem.Key, vbNewLine, ex.Message), LogLvl.LOG_WARNING, callingObj)
                            End Try
                        Next

                        newParams.Add(ParamStructFolder.Key, paramFolder)
                    Else
                        LogFile.LogTracing("Failed to open or create Registry Key for " & ParamStructFolder.Key, LogLvl.LOG_WARNING, callingObj)
                    End If
                Next
            Else
                LogFile.LogTracing("Failed to open the root WinNUT key.", LogLvl.LOG_ERROR, callingObj)
            End If

            Return newParams
        End Function

        ''' <summary>
        ''' Exports the WinNUT Registry Key structure using the reg executable to a .reg file.
        ''' </summary>
        ''' <param name="destinationPath">The location of the exported file.</param>
        Public Shared Sub ExportParams(destinationPath As String)
            ' Duplicate file must be removed now otherwise reg.exe will hang waiting for an input to overwrite.
            If IO.File.Exists(destinationPath) Then
                ' Allow any exceptions to be passed up to caller.
                IO.File.Delete(destinationPath)
            End If

            Dim regExpProcStartInfo As New ProcessStartInfo With {
                .FileName = "reg.exe",
                .UseShellExecute = False,
                .RedirectStandardError = True,
                .CreateNoWindow = True,
                .Arguments = "export """ & RegistryKeyRoot.Name & """ """ & destinationPath & """"
            }

            Dim proc = Process.Start(regExpProcStartInfo)
            proc.WaitForExit()
            If proc.ExitCode = 1 Then
                Throw New InvalidOperationException("reg.exe encountered an error: " & proc.StandardError.ReadToEnd())
            End If
        End Sub

        Public Shared Sub DeleteParams()
            RegistryKeyRoot.DeleteSubKeyTree("")
        End Sub
    End Class
End Namespace
