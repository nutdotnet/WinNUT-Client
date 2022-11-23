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
    ' What path we'd like to keep our data in.

#If DEBUG Then
    ' If debugging, keep any generated data next to the debug executable.
    Private ReadOnly DESIRED_DATA_PATH As String = Path.Combine(Environment.CurrentDirectory, "Data")
#Else
        Private ReadOnly DESIRED_DATA_PATH As String = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "\WinNUT-Client")
#End If

    Private ReadOnly FALLBACK_DATA_PATH = Path.GetTempPath() & ProgramName
#End Region

    Public LongProgramName As String
    Public ProgramName As String
    Public ProgramVersion As String
    Public ShortProgramVersion As String
    Public GitHubURL As String
    Public Copyright As String
    Public IsConnected As Boolean
    Public ApplicationData As String
    Public WithEvents LogFile As Logger
    Public AppIcon As Dictionary(Of Integer, Drawing.Icon)
    Public StrLog As New List(Of String)

    Public Sub Init_Globals()
        ApplicationData = GetAppDirectory(DESIRED_DATA_PATH)
        LogFile = New Logger(LogLvl.LOG_DEBUG)

        LongProgramName = My.Application.Info.Description
        ProgramName = My.Application.Info.ProductName
        ProgramVersion = Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString
        ShortProgramVersion = ProgramVersion.Substring(0, ProgramVersion.IndexOf(".", ProgramVersion.IndexOf(".") + 1))
        GitHubURL = My.Application.Info.Trademark
        Copyright = My.Application.Info.Copyright
        IsConnected = False
    End Sub

    ''' <summary>
    ''' Do everything possible to find a safe place to write to. If the requested option is unavailable, we fall back
    ''' to the temporary directory for the current user.
    ''' </summary>
    ''' <param name="requestedDir">The requested directory.</param>
    ''' <returns>The best possible option available as a writable data directory.</returns>
    Private Function GetAppDirectory(requestedDir As String) As String
        Try
            Directory.CreateDirectory(requestedDir)
            Return requestedDir

        Catch ex As Exception
            LogFile.LogTracing(ex.ToString & " encountered trying to create app data directory. Falling back to temp.",
                               LogLvl.LOG_ERROR, Nothing)

            Directory.CreateDirectory(FALLBACK_DATA_PATH)
            Return FALLBACK_DATA_PATH
        End Try
    End Function
End Module
