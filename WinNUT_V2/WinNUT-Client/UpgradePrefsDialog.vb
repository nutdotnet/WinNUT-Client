' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.Threading
Imports WinNUT_Client_Common
Imports WinNUT_Client_Common.Utils

Friend Class UpgradePrefsDialog
    Private paramUpgrade As UpgradePrefs
    Private importOperationComplete = False

    Private _ImportPreviousSettings As Boolean = True
    Private _DeletePreviousSettings As Boolean
    Private _OKButtonEnabled As Boolean
    Private _ProgressPercent = 0

    Public Property ImportPreviousSettigns As Boolean
        Get
            Return _ImportPreviousSettings
        End Get
        Protected Set(value As Boolean)
            _ImportPreviousSettings = value
            CalculateOKButtonState()
        End Set
    End Property

    Public Property DeletePreviousSettings As Boolean
        Get
            Return _DeletePreviousSettings
        End Get
        Protected Set(value As Boolean)
            _DeletePreviousSettings = value
            CalculateOKButtonState()
        End Set
    End Property

    Public Property OKButtonEnabled As Boolean
        Get
            Return _OKButtonEnabled
        End Get
        Set(value As Boolean)
            _OKButtonEnabled = value
            LogFile.LogTracing("OKButton state changed to " & value, LogLvl.LOG_DEBUG, Me)
        End Set
    End Property

    Public Property ProgressPercent As Integer
        Get
            Return _ProgressPercent
        End Get
        Set(value As Integer)
            If value < 0 OrElse value > 100 Then
                Throw New ArgumentOutOfRangeException(
                    "Progress must be reported as a percentage inbetween 0 and 100.")
            Else
                _ProgressPercent = value
            End If
        End Set
    End Property

    Private Sub CalculateOKButtonState()
        If ImportPreviousSettigns OrElse DeletePreviousSettings Then
            OKButtonEnabled = True
        Else
            OKButtonEnabled = False
        End If
    End Sub

    Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
        Enabled = False
        ' OK_Button.Enabled = False
        ' Cancel_Button.Enabled = False ' We currently don't support cancellation.
        UseWaitCursor = True

        UpgradeWorker.RunWorkerAsync()
    End Sub

    Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub UpgradeWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) _
        Handles UpgradeWorker.DoWork

#Region "Import work"
        If ImportPreviousSettigns Then
            ReportProgress(0, "Import operation beginning.", LogLvl.LOG_NOTICE, Me,
                               My.Resources.UpgradePrefsDialog_ImportProcedureBeginning)
            paramUpgrade = New UpgradePrefs()
            ReportProgress(50, "UpgradePrefs helper object instantiated.", LogLvl.LOG_DEBUG, Me)
            paramUpgrade.OldParams.Load_Params()
            Dim totalPrefs = paramUpgrade.OldParams.Arr_Reg_Key.Count
            ReportProgress(100, totalPrefs & " old parameters loaded.", LogLvl.LOG_NOTICE, Me)

            Dim progress = 0
            For Each oldPref As KeyValuePair(Of String, Object) In paramUpgrade.OldParams.Arr_Reg_Key
                Dim percentComplete = (progress / totalPrefs) * 100
                Try
                    Dim pairLookupRes = paramUpgrade.PrefSettingsLookup.First(Function(p As PrefSettingPair)
                                                                                  Return p.OldPreferenceName.Equals(oldPref.Key)
                                                                              End Function)
                    ' Handle special case of encrypted password.
                    Dim importData
                    If oldPref.Key = "NutPassword" Then
                        importData = New SerializedProtectedString(oldPref.Value, True)
                    Else
                        importData = oldPref.Value
                    End If

                    Try
                        My.Settings.Item(pairLookupRes.NewSettingsName) = importData
                        ReportProgress(percentComplete, "Imported " & oldPref.Key & " into " & pairLookupRes.NewSettingsName,
                        LogLvl.LOG_NOTICE, Me)
                    Catch ex As Exception
                        ReportProgress(percentComplete, "Error importing " & oldPref.Key, LogLvl.LOG_ERROR, Me)
                    End Try

                    ' Remove found entry since we no longer need it.
                    paramUpgrade.PrefSettingsLookup.Remove(pairLookupRes)
                    progress += 1
                    Thread.Sleep(1000)
                Catch ex As Exception
                    ReportProgress(percentComplete, String.Format("Error importing {0}:{2}{1}",
                oldPref.Key, vbNewLine, ex.ToString()), LogLvl.LOG_ERROR, Me)
                End Try
            Next

            Dim failedSettingsPairs = paramUpgrade.PrefSettingsLookup

            If failedSettingsPairs.Count > 0 Then
                Dim unmatchedPairsList = String.Empty
                For Each failedPair In failedSettingsPairs
                    unmatchedPairsList &= String.Format("[{0}, {1}]", failedPair.OldPreferenceName, failedPair.NewSettingsName)
                Next

                ReportProgress(95, String.Format("{0} unmatched settings pairs: {1}", failedSettingsPairs.Count,
                unmatchedPairsList), LogLvl.LOG_ERROR, Me,
                String.Format(My.Resources.UpgradePrefsDialog_UnmatchedPairs, failedSettingsPairs.Count,
                              unmatchedPairsList))
            End If

            ReportProgress(100, "Import procedure complete.", LogLvl.LOG_NOTICE, Me,
                           My.Resources.UpgradePrefsDialog_ImportProcedureCompleted)
            Thread.Sleep(2000)
        End If
#End Region

        If DeletePreviousSettings Then
            ReportProgress(0, "Starting delete procedure.", LogLvl.LOG_NOTICE, Me,
                           My.Resources.UpgradePrefsDialog_DeleteProcedureBeginning)

            Try
                WinNUT_Params.REG_HIVE.DeleteSubKeyTree(WinNUT_Params.REG_KEY_ROOT)
                ReportProgress(100, "Delete procedure completed successfully.", LogLvl.LOG_NOTICE, Me,
                    My.Resources.UpgradePrefsDialog_DeleteProcedureComplete)
            Catch ex As Exception
                ReportProgress(100, "Error occurred while deleteing old preferences from registry: " &
                               ex.ToString(), LogLvl.LOG_ERROR, Me, String.Format(
                                My.Resources.UpgradePrefsDialog_DeleteProcedureError, ex.ToString()))
            End Try
        End If

        Thread.Sleep(2000)
    End Sub

    Private Sub UpgradeWorker_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) _
        Handles UpgradeWorker.ProgressChanged

        Dim progReport As UpgradeWorkerProgressReport = CType(e.UserState, UpgradeWorkerProgressReport)
        LogFile.LogTracing(progReport.LogOutput, progReport.LogLevel, progReport.Sender, progReport.LogResourceString)
    End Sub

    Private Sub ReportProgress(percentComplete As Integer, logMsg As String, logLevel As LogLvl, sender As Object,
                               Optional logRes As String = Nothing)
        UpgradeWorker.ReportProgress(percentComplete,
                                     New UpgradeWorkerProgressReport(logMsg, logLevel, sender, logRes))
    End Sub

    Private Sub UpgradeWorker_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles UpgradeWorker.RunWorkerCompleted
        If e.Cancelled Then
            DialogResult = DialogResult.Cancel
        ElseIf e.Error IsNot Nothing Then
            DialogResult = DialogResult.Abort
        Else
            DialogResult = DialogResult.OK
        End If

        Close()
    End Sub

    Private Class UpgradeWorkerProgressReport
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

    'Private Sub UpgradePrefsDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    '    CalculateOKButtonState()
    'End Sub
End Class

