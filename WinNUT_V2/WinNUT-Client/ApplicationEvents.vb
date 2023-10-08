' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices
Imports Newtonsoft.Json
Imports WinNUT_Client_Common

Namespace My
    ' Les événements suivants sont disponibles pour MyApplication :
    ' Startup : Déclenché au démarrage de l'application avant la création du formulaire de démarrage.
    ' Shutdown : Déclenché après la fermeture de tous les formulaires de l'application.  Cet événement n'est pas déclenché si l'application se termine de façon anormale.
    ' UnhandledException : Déclenché si l'application rencontre une exception non gérée.
    ' StartupNextInstance : Déclenché lors du lancement d'une application à instance unique et si cette application est déjà active. 
    ' NetworkAvailabilityChanged : Déclenché quand la connexion réseau est connectée ou déconnectée.
    Partial Friend Class MyApplication
        ' Default culture for output so logs can be shared with the project.
        Private Shared ReadOnly DEF_CULTURE_INFO As CultureInfo = CultureInfo.InvariantCulture
        Private Shared ReadOnly CRASHBUG_OUTPUT_PATH = System.Windows.Forms.Application.LocalUserAppDataPath

        Private CrashBug_Form As New Form
        Private BtnClose As New Button
        Private BtnGenerate As New Button
        Private Msg_Crash As New Label
        Private Msg_Error As New TextBox

        Private crashReportData As String

        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            ' Uncomment below and comment out Handles line for _UnhandledException sub when debugging unhandled exceptions.
            ' AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppDomainUnhandledException
            Init_Globals()
            LogFile.LogTracing("MyApplication_Startup complete.", LogLvl.LOG_DEBUG, Me)
        End Sub

        Private Sub AppDomainUnhandledException(sender As Object, e As System.UnhandledExceptionEventArgs)
            MyApplication_UnhandledException(sender, New UnhandledExceptionEventArgs(False, e.ExceptionObject))
        End Sub

        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            e.ExitApplication = False

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

            crashReportData = GenerateCrashReport(e.Exception)

            AddHandler BtnClose.Click, AddressOf Application.Close_Button_Click
            AddHandler BtnGenerate.Click, AddressOf Application.Generate_Button_Click

            CrashBug_Form.Show()
            CrashBug_Form.BringToFront()
            WinNUT.HasCrashed = True
        End Sub

        Private Shared Function GenerateCrashReport(ex As Exception) As String
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
            Dim confCopy = New Dictionary(Of String, Object)

            reportStream.WriteLine()
            reportStream.WriteLine("==== Parameters ====")
            reportStream.WriteLine()

            ' Censor any identifying information
            If Arr_Reg_Key IsNot Nothing AndAlso Arr_Reg_Key.Count > 0 Then
                For Each kvp As KeyValuePair(Of String, Object) In Arr_Reg_Key
                    Dim newVal As String
                    Select Case kvp.Key
                        Case "ServerAddress", "Port", "UPSName", "NutLogin", "NutPassword"
                            newVal = "*****"
                        Case Else
                            newVal = kvp.Value
                    End Select

                    confCopy.Add(kvp.Key, newVal)
                Next

                reportStream.WriteLine(JsonConvert.SerializeObject(confCopy, jsonSerializerSettings))
                reportStream.WriteLine()
            Else
                reportStream.WriteLine("[EMPTY]")
            End If
#End Region

#Region "Exceptions"
            reportStream.WriteLine("==== Exception ====")
            reportStream.WriteLine()
            reportStream.WriteLine(Regex.Unescape(JsonConvert.SerializeObject(ex, jsonSerializerSettings)))
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

            Computer.Clipboard.SetText(crashReportData)

            Directory.CreateDirectory(CRASHBUG_OUTPUT_PATH)
            Dim CrashLog_Report = New StreamWriter(Path.Combine(CRASHBUG_OUTPUT_PATH, logFileName))
            CrashLog_Report.WriteLine(crashReportData)
            CrashLog_Report.Close()

            ' Open an Explorer window to the crash log.
            Process.Start(CRASHBUG_OUTPUT_PATH)
            End
        End Sub

        Private Sub Close_Button_Click(sender As Object, e As EventArgs)
            CrashBug_Form.Close()
        End Sub
    End Class
End Namespace
