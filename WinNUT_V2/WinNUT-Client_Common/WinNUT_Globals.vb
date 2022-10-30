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
#Region "Properties"
    ' The directory where volatile appdata is stored.
    ReadOnly Property ApplicationData() As String
        Get
#If DEBUG Then
            ' If debugging, keep any generated data next to the debug executable.
            Return Path.Combine(Environment.CurrentDirectory, "Data")
#Else
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProgramName)
#End If
        End Get
    End Property
#End Region

    Public LongProgramName As String
    Public ProgramName As String
    Public ProgramVersion As String
    Public ShortProgramVersion As String
    Public GitHubURL As String
    Public Copyright As String
    Public IsConnected As Boolean
    ' Public LogFile As String
    ' Handle application messages and debug events.
    ' Public WithEvents LogFile As Logger '  As New Logger(False, 0)
    ' Logging
    Public WithEvents LogFile As Logger
    Public AppIcon As Dictionary(Of Integer, Drawing.Icon)
    Public StrLog As New List(Of String)
    ' Public LogFilePath As String

    Public Sub Init_Globals()
        LongProgramName = My.Application.Info.Description
        ProgramName = My.Application.Info.ProductName
        ProgramVersion = Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString
        ShortProgramVersion = ProgramVersion.Substring(0, ProgramVersion.IndexOf(".", ProgramVersion.IndexOf(".") + 1))
        GitHubURL = My.Application.Info.Trademark
        Copyright = My.Application.Info.Copyright
        IsConnected = False
        LogFile = New Logger(False, LogLvl.LOG_DEBUG)
    End Sub

    'Sub SetupAppDirectory()
    '    If Not Directory.Exists(ApplicationData) Then
    '        Try
    '            Directory.CreateDirectory(ApplicationData)
    '        Catch ex As Exception
    '            Logger.LogTracing("Could not create application directory! Operating with reduced functionality.\n\n" & ex.ToString(),
    '                                   LogLvl.LOG_ERROR, Nothing)
    '        End Try
    '    End If
    'End Sub
End Module
