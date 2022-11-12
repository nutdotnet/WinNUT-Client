' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports WinNUT_Client_Common

Namespace My
    ' Les événements suivants sont disponibles pour MyApplication :
    ' Startup : Déclenché au démarrage de l'application avant la création du formulaire de démarrage.
    ' Shutdown : Déclenché après la fermeture de tous les formulaires de l'application.  Cet événement n'est pas déclenché si l'application se termine de façon anormale.
    ' UnhandledException : Déclenché si l'application rencontre une exception non gérée.
    ' StartupNextInstance : Déclenché lors du lancement d'une application à instance unique et si cette application est déjà active. 
    ' NetworkAvailabilityChanged : Déclenché quand la connexion réseau est connectée ou déconnectée.
    Partial Friend Class MyApplication
        Private CrashBug_Form As New Form
        Private BtnClose As New Button
        Private BtnGenerate As New Button
        Private Msg_Crash As New Label
        Private Msg_Error As New TextBox

        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            'Init WinNUT Variables
            Init_Globals()
            LogFile.LogTracing("Init Globals Variables Complete", LogLvl.LOG_DEBUG, Me)
        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            e.ExitApplication = False

            Dim Frms As New FormCollection
            Frms = Application.OpenForms()

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

            Dim Exception_data = BuildExceptionString(e.Exception)

            If e.Exception.InnerException IsNot Nothing Then
                Exception_data &= vbNewLine & "InnerException present:" & vbNewLine
                Exception_data &= BuildExceptionString(e.Exception.InnerException)
            End If

            With Msg_Error
                .Location = New Point(6, 110)
                .Multiline = True
                .ScrollBars = ScrollBars.Vertical
                .ReadOnly = True
                .Text = Exception_data.ToString()
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
            AddHandler CrashBug_Form.FormClosing, AddressOf Application.CrashBug_FormClosing

            CrashBug_Form.Show()
            CrashBug_Form.BringToFront()
            WinNUT.HasCrashed = True
        End Sub

        ''' <summary>
        ''' Generate a friendly message describing an exception.
        ''' </summary>
        ''' <param name="ex">The exception that will be read for the message.</param>
        ''' <returns>The final string representation of the exception.</returns>
        Private Function BuildExceptionString(ex As Exception) As String
            Dim retStr = String.Empty

            retStr &= String.Format("Exception type: {0}" & vbNewLine, ex.GetType.ToString)
            retStr &= String.Format("Exception message: {0}" & vbNewLine, ex.Message)
            retStr &= "Exception stack trace:" & vbNewLine
            retStr &= ex.StackTrace & vbNewLine

            Return retStr
        End Function

        Private Sub CrashBug_FormClosing(sender As Object, e As FormClosingEventArgs)
            End
        End Sub
        Private Sub Close_Button_Click(sender As Object, e As EventArgs)
            End
        End Sub

        Private Sub Generate_Button_Click(sender As Object, e As EventArgs)
            'Generate a bug report with all essential datas 
            Dim Crash_Report As String = "WinNUT Bug Report" & vbNewLine
            Dim WinNUT_Config As New Dictionary(Of String, Object)
            Try
                WinNUT_Config = Arr_Reg_Key
            Catch ex As Exception
                Crash_Report &= "ALERT: Encountered exception while trying to access Arr_Reg_Key:" & vbNewLine
                Crash_Report &= BuildExceptionString(ex)
            End Try

            ' Initialize directory for data
            Dim CrashLog_Dir = ApplicationData & "\CrashLog"
            If Not Computer.FileSystem.DirectoryExists(CrashLog_Dir) Then
                Computer.FileSystem.CreateDirectory(CrashLog_Dir)
            End If

            Dim CrashLog_Filename As String = "Crash_Report_" & Format(Now, "dd-MM-yyyy") & "_" &
                String.Format("{0}-{1}-{2}.txt", Now.Hour.ToString("00"), Now.Minute.ToString("00"), Now.Second.ToString("00"))



            Crash_Report &= "Os Version : " & Computer.Info.OSVersion & vbNewLine
            Crash_Report &= "WinNUT Version : " & Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString & vbNewLine

            Crash_Report &= vbNewLine & "WinNUT Parameters : " & vbNewLine
            If WinNUT_Config.Count > 0 Then
                ' Prepare config values by removing sensitive information.
                For Each kvp As KeyValuePair(Of String, Object) In Arr_Reg_Key
                    Select Case kvp.Key
                        Case "ServerAddress", "Port", "UPSName", "NutLogin", "NutPassword"
                            WinNUT_Config.Remove(kvp.Key)
                    End Select
                Next
                Crash_Report &= Newtonsoft.Json.JsonConvert.SerializeObject(WinNUT_Config, Newtonsoft.Json.Formatting.Indented) & vbNewLine

            Else
                Crash_Report &= "[EMPTY]" & vbNewLine
            End If

            Crash_Report &= vbNewLine & "Error Message : " & vbNewLine
            Crash_Report &= Msg_Error.Text & vbNewLine & vbNewLine
            Crash_Report &= "Last Events :" & vbNewLine

            For Each WinNUT_Event In LogFile.LastEvents
                Crash_Report &= WinNUT_Event & vbNewLine
            Next

            Computer.Clipboard.SetText(Crash_Report)

            Dim CrashLog_Report As StreamWriter
            CrashLog_Report = Computer.FileSystem.OpenTextFileWriter(CrashLog_Dir & "\" & CrashLog_Filename, True)
            CrashLog_Report.WriteLine(Crash_Report)
            CrashLog_Report.Close()

            ' Open an Explorer window to the crash log.
            ' Dim fullFilepath As String = CrashLog_Dir & "\" & CrashLog_Filename
            ' If WinNUT IsNot Nothing Then
            Process.Start(CrashLog_Dir)
            ' End If
            End
        End Sub
    End Class
End Namespace
