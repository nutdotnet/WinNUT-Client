' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.IO

Public Module WinNUT_Globals

#Region "Constants/Shareds"
    Private DEFAULT_DATA_PATH As String
#End Region

    Public LongProgramName As String
    Public ProgramName As String
    Public ProgramVersion As String
    Public ShortProgramVersion As String
    Public GitHubURL As String
    Public Copyright As String
    Public IsConnected As Boolean
    Public ApplicationData As String
    ' Public LogFile As String
    ' Handle application messages and debug events.
    ' Public WithEvents LogFile As Logger '  As New Logger(False, 0)
    ' Logging
    Public WithEvents LogFile As Logger
    Public AppIcon As Dictionary(Of Integer, Drawing.Icon)
    Public StrLog As New List(Of String)
    ' Public LogFilePath As String

    Public Sub Init_Globals()
#If DEBUG Then
        ' If debugging, keep any generated data next to the debug executable.
        DEFAULT_DATA_PATH = Path.Combine(Environment.CurrentDirectory, "Data")
#Else
        DEFAULT_DATA_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\WinNUT-Client")
#End If

        LongProgramName = My.Application.Info.Description
        ProgramName = My.Application.Info.ProductName
        ProgramVersion = Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString
        ShortProgramVersion = ProgramVersion.Substring(0, ProgramVersion.IndexOf(".", ProgramVersion.IndexOf(".") + 1))
        GitHubURL = My.Application.Info.Trademark
        Copyright = My.Application.Info.Copyright
        IsConnected = False
        LogFile = New Logger(LogLvl.LOG_DEBUG)

        SetupAppDirectory()
    End Sub

    Sub SetupAppDirectory()
        If Not Directory.Exists(DEFAULT_DATA_PATH) Then
            Try
                Directory.CreateDirectory(DEFAULT_DATA_PATH)
                ApplicationData = DEFAULT_DATA_PATH
            Catch ex As Exception
                LogFile.LogTracing(ex.ToString & " encountered trying to create app data directory. Falling back to temp.",
                                   LogLvl.LOG_ERROR, Nothing)
                ApplicationData = Path.GetTempPath() & "\WinNUT_Data\"
                Directory.CreateDirectory(ApplicationData)
            End Try
        End If
    End Sub
End Module
