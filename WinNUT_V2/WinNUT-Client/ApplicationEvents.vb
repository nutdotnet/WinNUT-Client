Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices
Imports Newtonsoft.Json
Imports WinNUT_Client_Common

Namespace My
    Partial Friend Class MyApplication
        ' Default culture for output so logs can be shared with the project.
        Private Shared ReadOnly DEF_CULTURE_INFO As CultureInfo = CultureInfo.InvariantCulture

        Private CrashBug_Form As New Form
        Private BtnClose As New Button
        Private BtnGenerate As New Button
        Private Msg_Crash As New Label
        Private Msg_Error As New TextBox

        Private SensitiveProperties As List(Of String) = New List(Of String)({"NUT_ServerAddress", "NUT_ServerPort", "NUT_UPSName",
                                                           "NUT_Username", "NUT_Password"})

        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            LogFile.LogTracing(String.Format("{0} v{1} starting up.", ProgramName, ProgramVersion),
                           LogLvl.LOG_NOTICE, Me)
            LogFile.LogTracing("Data storage path: " & DataDirectory, LogLvl.LOG_NOTICE, Me)

            AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppDomainUnhandledException
            AddHandler Settings.SettingsLoaded, AddressOf OnSettingsFirstLoaded
            AddHandler Settings.PropertyChanged, AddressOf OnPropertyChanged

            LogFile.LogTracing("Event handlers configured.", LogLvl.LOG_DEBUG, Me)

            ApplyLoggingSettings()

            ' Starting without previous settings. May be new installation or MSI upgrade.
            If Settings.IsFirstRun Then
                Try
                    ' Handle MSI upgrade scenario.
                    Settings.Upgrade()
                    LogFile.LogTracing("Settings upgrade completed without exception.", LogLvl.LOG_NOTICE, Me)
                Catch ex As ConfigurationErrorsException
                    LogFile.LogTracing("Error encountered while trying to upgrade Settings:", LogLvl.LOG_ERROR, Me)
                    LogFile.LogException(ex, Me)
                End Try

                ' If Settings still appear new, check if old Registry preferences are leftover.
                If Settings.IsFirstRun AndAlso OldParams.WinNUT_Params.ParamsExist Then
                    LogFile.LogTracing("Previous preferences data detected in the Registry.", LogLvl.LOG_NOTICE, Me,
                               Resources.DetectedPreviousPrefsData)

                    Forms.UpgradePrefsDialog.ShowDialog()
                End If

                Settings.IsFirstRun = False
                Settings.Save()
            End If

            LogFile.LogTracing("MyApplication_Startup complete.", LogLvl.LOG_DEBUG, Me)
        End Sub

        Private Sub AppDomainUnhandledException(sender As Object, e As System.UnhandledExceptionEventArgs)
            LogFile.LogTracing("AppDomainUnhandledException", LogLvl.LOG_ERROR, Me)
            MyApplication_UnhandledException(sender, New UnhandledExceptionEventArgs(False, e.ExceptionObject))
        End Sub

        Private caughtException As Exception
        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            LogFile.LogTracing("UnhandledException", LogLvl.LOG_ERROR, Me)
            WinNUT.HasCrashed = True
            WinNUT.Hide()
            e.ExitApplication = False
            caughtException = e.Exception

            With Msg_Crash
                .Location = New Point(6, 6)
                .Text = "WinNUT has encountered a critical error and will close soon." & vbNewLine &
                    "You can :" & vbNewLine &
                    "- generate a crash report which will contain most of the configured parameters (without sensitive" & vbNewLine &
                    "  information such as your connection information to your NUT server), the last 50 events logged" & vbNewLine &
                    "  and the error message displayed below." & vbNewLine &
                    "  This information will Then be copied To your clipboard For easy reporting." & vbNewLine &
                    "- simply close WinNUT without generating a report."
                .Size = New Point(470, 100)
            End With

            With Msg_Error
                .Location = New Point(6, 110)
                .Multiline = True
                .ScrollBars = ScrollBars.Vertical
                .ReadOnly = True
                .Text = e.Exception.ToString()
                .Size = New Point(470, 300)
            End With

            With BtnClose
                .Location = New Point(370, 425)
                .TextAlign = ContentAlignment.MiddleCenter
                .Text = "Close WinNUT"
                .Size = New Point(100, 25)
            End With

            With BtnGenerate
                .Location = New Point(160, 425)
                .TextAlign = ContentAlignment.MiddleCenter
                .Text = "Generate Report and Close WinNUT"
                .Size = New Point(200, 25)
            End With

            With CrashBug_Form
                .Icon = Resources.WinNut
                .Size = New Point(500, 500)
                .FormBorderStyle = FormBorderStyle.Sizable
                .MaximizeBox = False
                .MinimizeBox = False
                .StartPosition = FormStartPosition.CenterParent
                .Text = "Critical Error Occurred in WinNUT"
                .Controls.Add(Msg_Crash)
                .Controls.Add(Msg_Error)
                .Controls.Add(BtnClose)
                .Controls.Add(BtnGenerate)
            End With

            AddHandler BtnClose.Click, AddressOf Application.Close_Button_Click
            AddHandler BtnGenerate.Click, AddressOf Application.Generate_Button_Click

            CrashBug_Form.Show()
            CrashBug_Form.BringToFront()
        End Sub

        Private Function GenerateCrashReport() As String
            Dim jsonSerializerSettings As New JsonSerializerSettings()
            jsonSerializerSettings.Culture = DEF_CULTURE_INFO
            jsonSerializerSettings.Formatting = Formatting.Indented

            Dim reportStream As New StringWriter(DEF_CULTURE_INFO)
            reportStream.WriteLine("WinNUT Bug Report")
            reportStream.WriteLine("Generated at " + Date.UtcNow.ToString("F", DEF_CULTURE_INFO))
            reportStream.WriteLine()
            reportStream.WriteLine("OS Version: " & Computer.Info.OSVersion)
            reportStream.WriteLine("WinNUT Version: " & ProgramVersion)

#Region "Config output"
            reportStream.WriteLine()
            reportStream.WriteLine("==== Settings ====")
            reportStream.WriteLine()

            For Each setProp As SettingsProperty In Settings.Properties
                Dim setVal As String

                If SensitiveProperties.Contains(setProp.Name) Then
                    setVal = "{Removed}"
                    SensitiveProperties.Remove(setProp.Name)
                Else
                    setVal = Settings.Item(setProp.Name)
                End If

                reportStream.WriteLine(setProp.Name & ": " & setVal & " (" & setProp.DefaultValue & ")")
            Next
#End Region

#Region "Exceptions"
            reportStream.WriteLine("==== Exception ====")
            reportStream.WriteLine()
            reportStream.WriteLine(Regex.Unescape(JsonConvert.SerializeObject(caughtException, jsonSerializerSettings)))
            reportStream.WriteLine()
#End Region

            reportStream.WriteLine("==== Last Events ====")

            LogFile.LastEvents.Reverse()
            reportStream.WriteLine()
            reportStream.WriteLine(Regex.Unescape(JsonConvert.SerializeObject(LogFile.LastEvents, jsonSerializerSettings)))

            Return reportStream.ToString()
        End Function

        Private Sub Generate_Button_Click(sender As Object, e As EventArgs)
            Dim logFileName = "CrashReport_" + Date.Now.ToString("s").Replace(":", ".") + ".txt"
            Dim generatedReport = GenerateCrashReport()

            Computer.Clipboard.SetText(generatedReport)

            Dim CrashLog_Report = New StreamWriter(Path.Combine(DataDirectory, logFileName))
            CrashLog_Report.WriteLine(generatedReport)
            CrashLog_Report.Close()

            ' Open an Explorer window to the crash log.
            Process.Start(DataDirectory)
            End
        End Sub

        Private Sub Close_Button_Click(sender As Object, e As EventArgs)
            CrashBug_Form.Close()
        End Sub

        ''' <summary>
        ''' Handles validation of Settings when loaded.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub OnSettingsFirstLoaded(sender As Object, e As SettingsLoadedEventArgs)
            LogFile.LogTracing("OnSettingsFirstLoaded event raised.", LogLvl.LOG_DEBUG, Me)

            ' Verify that encrypted data can be decrypted
            Try
                Settings.NUT_Username?.ToString()
            Catch ex As Exception
                LogFile.LogTracing("Error attempting to decrypt encrypted data. Resetting to defaults.",
                                   LogLvl.LOG_ERROR, Me, Resources.Log_Str_ErrorDecrypting)
                LogFile.LogException(ex, Me)

                Settings.NUT_Username = New SerializedProtectedString()
                Settings.NUT_Password = New SerializedProtectedString()
            End Try

            If Not Settings.NUT_PollIntervalMsec > 0 Then
                LogFile.LogTracing("Incorrect value of " & Settings.NUT_PollIntervalMsec &
                               " for Poll Delay/Interval, resetting to default.", LogLvl.LOG_ERROR, Me)
                Settings.NUT_PollIntervalMsec = MySettings.Default.NUT_PollIntervalMsec
            End If
        End Sub

        ''' <summary>
        ''' Raised when any Settings property is changed through a set accessor, or when reloaded/reset.
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub OnPropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs)
            LogFile.LogTracing("Handling OnPropertyChanged for " & e.PropertyName, LogLvl.LOG_DEBUG, Me)
            If e.PropertyName = "LG_LogToFile" OrElse e.PropertyName = "LG_LogLevel" Then
                LogFile.LogTracing("Settings property changed for logging subsystem, updating...", LogLvl.LOG_DEBUG, Me)
                ApplyLoggingSettings()
            End If
        End Sub

        Private Sub ApplyLoggingSettings()
            LogFile.IsWritingToFile = Settings.LG_LogToFile
            LogFile.LogLevelValue = Settings.LG_LogLevel
        End Sub
    End Class
End Namespace
