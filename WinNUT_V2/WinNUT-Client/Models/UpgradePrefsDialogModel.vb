' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports WinNUT_Client_Common

Namespace Models
    Friend Class UpgradePrefsDialogModel
        Implements INotifyPropertyChanged

        Private ReadOnly upgradeWorker As New BackgroundWorker With {
            .WorkerReportsProgress = True,
            .WorkerSupportsCancellation = True
        }
        Private oldPrefs As OldParams.UpgradableParams
        Private _parentForm As Form

#Region "Properties and backing fields"

        Private _ImportPreviousSettings As Boolean
        Private _BackupPreviousSettings As Boolean
        Private _DeletePreviousSettings As Boolean
        Private _OKButtonEnabled As Boolean
        Private _FormEnabled As Boolean
        Private _FormUseWaitCursor As Boolean
        Private _ProgressPercent = 0
        Private _Icon As Icon

        Public Property ImportPreviousSettigns As Boolean
            Get
                Return _ImportPreviousSettings
            End Get
            Set(value As Boolean)
                _ImportPreviousSettings = value
                NotifyPropertyChanged()
                ' CalculateOKButtonState()
            End Set
        End Property

        Public Property BackupPreviousSettings As Boolean
            Get
                Return _BackupPreviousSettings
            End Get
            Set(value As Boolean)
                If _BackupPreviousSettings <> value Then
                    _BackupPreviousSettings = value
                    NotifyPropertyChanged()
                    ' CalculateOKButtonState()
                End If
            End Set
        End Property

        Public Property DeletePreviousSettings As Boolean
            Get
                Return _DeletePreviousSettings
            End Get
            Set(value As Boolean)
                _DeletePreviousSettings = value
                NotifyPropertyChanged()
                ' CalculateOKButtonState()
            End Set
        End Property

        Public Property OKButtonEnabled As Boolean
            Get
                Return _OKButtonEnabled
            End Get
            Set(value As Boolean)
                If _OKButtonEnabled <> value Then
                    _OKButtonEnabled = value
                    NotifyPropertyChanged()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Enable/disable the main form and cascade changes down as necessary.
        ''' </summary>
        ''' <returns></returns>
        Public Property FormEnabled As Boolean
            Get
                Return _FormEnabled
            End Get
            Set(value As Boolean)
                If _FormEnabled <> value Then
                    _FormEnabled = value
                    NotifyPropertyChanged()
                End If
            End Set
        End Property

        Public Property FormUseWaitCursor As Boolean
            Get
                Return _FormUseWaitCursor
            End Get
            Set(value As Boolean)
                If _FormUseWaitCursor <> value Then
                    _FormUseWaitCursor = value
                    NotifyPropertyChanged()
                End If
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
                    NotifyPropertyChanged()
                End If
            End Set
        End Property

        Public Property Icon As Icon
            Get
                Return _Icon
            End Get
            Private Set(value As Icon)
                If _Icon IsNot Nothing AndAlso Not _Icon.Equals(value) Then
                    _Icon = value
                    NotifyPropertyChanged()
                End If
            End Set
        End Property

#End Region

#Region "INotifyPropertyChanged implementation"

        ' This method is called by the Set accessor of each property.  
        ' The CallerMemberName attribute that is applied to the optional propertyName  
        ' parameter causes the property name of the caller to be substituted as an argument.  
        Private Sub NotifyPropertyChanged(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

#End Region

        Friend Sub New(parentForm As Form)
            _parentForm = parentForm

            AddHandler upgradeWorker.DoWork, AddressOf ProcessUpgradeWork
            AddHandler upgradeWorker.ProgressChanged, AddressOf UpgradeProgressChanged
            AddHandler upgradeWorker.RunWorkerCompleted, AddressOf UpgradeWorkComplete
        End Sub

        Friend Sub InitializeProperties()
            FormEnabled = True
            FormUseWaitCursor = False
            ImportPreviousSettigns = True
            BackupPreviousSettings = True
            Icon = My.Resources.WinNut
        End Sub

        Friend Sub BeginUpgradeWorkAsync()
            Dim workerArgs As New UpgradeWorkerArguments()

            If BackupPreviousSettings Then
                ' Capture user's backup location preference on this thread before starting background worker.
                Dim saveFileDialog = New SaveFileDialog() With {
                    .FileName = "WinNUT-Prefs-Export",
                    .Filter = "Windows Registry files (*.reg)|*.reg|All files (*.*)|*.*",
                    .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    .Title = My.Resources.UpgradePrefsDialog_BackupLocationTitle
                }

                Dim dialogRes = saveFileDialog.ShowDialog()

                If dialogRes = DialogResult.Cancel Then
                    LogFile.LogTracing("User cancelled upgrade prefs process while selecting backup target.",
                                       LogLvl.LOG_NOTICE, Me)
                    Return
                End If

                workerArgs.BackupPath = saveFileDialog.FileName
            End If

            FormEnabled = False
            FormUseWaitCursor = True
            upgradeWorker.RunWorkerAsync(workerArgs)
        End Sub

        Friend Sub CancelButtonClicked()
            LogFile.LogTracing("Handling Cancel button click...", LogLvl.LOG_DEBUG, Me)

            If upgradeWorker IsNot Nothing AndAlso upgradeWorker.IsBusy Then
                LogFile.LogTracing("Requesting cancellation of upgradeWorker.", LogLvl.LOG_NOTICE, Me)
                upgradeWorker.CancelAsync()
            Else
                LogFile.LogTracing("Exiting out of upgrade dialog.", LogLvl.LOG_NOTICE, Me)
                _parentForm.DialogResult = DialogResult.Cancel
                My.Settings.UpgradePrefsCompleted = True
                _parentForm.Close()
            End If
        End Sub

        Private Sub CalculateOKButtonState(Optional sender As Object = Nothing, Optional args As PropertyChangedEventArgs = Nothing) _
            Handles Me.PropertyChanged

            If FormEnabled AndAlso ImportPreviousSettigns OrElse BackupPreviousSettings OrElse DeletePreviousSettings Then
                OKButtonEnabled = True
            Else
                OKButtonEnabled = False
            End If
        End Sub

#Region "BackgroundWorker"
        ''' <summary>
        ''' Perform the work of <see cref="upgradeWorker"/>
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ProcessUpgradeWork(sender As Object, e As DoWorkEventArgs)
            Dim args As UpgradeWorkerArguments = e.Argument

            If Not e.Cancel AndAlso ImportPreviousSettigns Then
                DoImportWork()
            End If

            If Not e.Cancel AndAlso BackupPreviousSettings Then
                DoBackupWork(args.BackupPath)
            End If

            If Not e.Cancel AndAlso DeletePreviousSettings Then
                DoDeleteWork()
            End If
        End Sub

        ''' <summary>
        ''' Execution of <see cref="ProcessUpgradeWork(Object, DoWorkEventArgs)"/> has ended and returned to the
        ''' UI thread.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub UpgradeWorkComplete(sender As Object, e As RunWorkerCompletedEventArgs)
            oldPrefs = Nothing

            FormEnabled = True
            FormUseWaitCursor = False

            If e.Error IsNot Nothing Then
                ProgressPercent = 0
                Dim localError = String.Format(My.Resources.UpgradePrefsDialog_ErrorEncountered, e.Error.Message)
                LogFile.LogTracing("UpgradeWorkComplete with error: " & vbNewLine & e.Error.ToString(),
                                   LogLvl.LOG_ERROR, Me, localError)
                MessageBox.Show(localError)
                Return
            End If

            If e.Cancelled Then
                ProgressPercent = 0
                LogFile.LogTracing("Upgrade work was cancelled.", LogLvl.LOG_WARNING, Me, My.Resources.UpgradePrefsDialog_Cancelled)
                Return
            End If

            ProgressPercent = 100

            My.Settings.UpgradePrefsCompleted = True
            _parentForm.Close()
        End Sub

        ''' <summary>
        ''' Reports progress from within the BackgroundWorker thread, back to the calling thread.
        ''' WARNING: This is not a reliably quick way to synchronize information back to the calling thread.
        ''' From testing, the <see cref="ProcessUpgradeWork"/> routine moves much faster than the main thread.
        ''' </summary>
        ''' <param name="percentComplete"></param>
        ''' <param name="logMsg"></param>
        ''' <param name="logLevel"></param>
        ''' <param name="sender"></param>
        ''' <param name="logRes"></param>
        Private Sub ReportProgress(percentComplete As Integer, logMsg As String, logLevel As LogLvl, sender As Object,
                                   Optional logRes As String = Nothing)
            upgradeWorker.ReportProgress(percentComplete,
                                     New UpgradeWorkerProgressReport(logMsg, logLevel, sender, logRes))
        End Sub

        Private Sub UpgradeProgressChanged(sender As Object, e As ProgressChangedEventArgs)
            Dim progReport As UpgradeWorkerProgressReport = CType(e.UserState, UpgradeWorkerProgressReport)
            LogFile.LogTracing(progReport.LogOutput, progReport.LogLevel, progReport.Sender, progReport.LogResourceString)
            ProgressPercent = e.ProgressPercentage
        End Sub

        Private Sub DoImportWork()
            ReportProgress(0, "Import operation beginning.", LogLvl.LOG_NOTICE, Me)
            oldPrefs = New OldParams.UpgradableParams()
            ReportProgress(100, "Old parameters loaded.", LogLvl.LOG_NOTICE, Me)

            Dim progress = 0
            For Each section As KeyValuePair(Of String, Dictionary(Of String, Object)) In oldPrefs.Parameters
                For Each oldPref As KeyValuePair(Of String, Object) In section.Value
                    Dim percentComplete = (progress / oldPrefs.TotalPrefs) * 100
                    Try
                        Dim pairLookupRes = oldPrefs.PrefSettingsLookup.First(Function(p As OldParams.UpgradableParams.PrefSettingPair)
                                                                                  Return p.OldPreferenceName.Equals(oldPref.Key)
                                                                              End Function)

                        Dim oldValue = oldPref.Value
                        ' Handle special case of converting the default input frequency calibration value
                        If oldPref.Key = "FrequencySupply" Then
                            oldValue = (oldValue * 10) + 50
                        End If

                        My.Settings.Item(pairLookupRes.NewSettingsName) = oldValue

                        ReportProgress(percentComplete, "Imported " & oldPref.Key & " into " & pairLookupRes.NewSettingsName,
                            LogLvl.LOG_NOTICE, Me)

                        ' Remove found entry since we no longer need it.
                        oldPrefs.PrefSettingsLookup.Remove(pairLookupRes)
                        progress += 1
                    Catch ex As Exception
                        ReportProgress(percentComplete, String.Format("Error importing {0}:{2}{1}",
                        oldPref.Key, vbNewLine, ex.ToString()), LogLvl.LOG_ERROR, Me)
                    End Try
                Next
            Next


            Dim failedSettingsPairs = oldPrefs.PrefSettingsLookup

            If failedSettingsPairs.Count > 0 Then
                Dim unmatchedPairsList = String.Empty
                For Each failedPair In failedSettingsPairs
                    unmatchedPairsList &= String.Format("[{0}, {1}]", failedPair.OldPreferenceName, failedPair.NewSettingsName)
                Next

                ReportProgress(95, String.Format("{0} unmatched settings pairs: {1}", failedSettingsPairs.Count,
                        unmatchedPairsList), LogLvl.LOG_ERROR, Me,
                        String.Format(My.Resources.UpgradePrefsDialog_UnmatchedPairs, failedSettingsPairs.Count))
            End If

            ReportProgress(100, "Import procedure complete.", LogLvl.LOG_NOTICE, Me,
                               My.Resources.UpgradePrefsDialog_ImportProcedureCompleted)
        End Sub

        ''' <summary>
        ''' Performs synchronous work to backup Registry data. Throws exceptions if any error occurs.
        ''' </summary>
        ''' <param name="targetPath">Full path and filename where registry items will be backed up to.</param>
        Private Sub DoBackupWork(targetPath As String)
            ReportProgress(0, "Beginning reg export process to " & targetPath, LogLvl.LOG_NOTICE, Me)
            OldParams.WinNUT_Params.ExportParams(targetPath)
            ReportProgress(100, "reg export process exited. Backup complete.", LogLvl.LOG_NOTICE, Me,
                String.Format(My.Resources.UpgradePrefsDialog_BackupProcedureCompleted, targetPath))
        End Sub

        Private Sub DoDeleteWork()
            ReportProgress(0, "Starting delete procedure.", LogLvl.LOG_NOTICE, Me)
            OldParams.WinNUT_Params.DeleteParams()
            ReportProgress(100, "Delete procedure completed successfully.", LogLvl.LOG_NOTICE, Me,
                    My.Resources.UpgradePrefsDialog_DeleteProcedureComplete)
        End Sub

#End Region

        Protected Class UpgradeWorkerProgressReport
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

        Private Class UpgradeWorkerArguments
            Public Property BackupPath As String
        End Class

    End Class

End Namespace
