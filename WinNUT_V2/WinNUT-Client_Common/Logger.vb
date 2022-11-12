' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.IO

Public Class Logger
#Region "Constants/Shared"
    Private Shared ReadOnly BASE_FILE_NAME = ProgramName ' "WinNUT-CLient"
    Private Const LOG_FILE_CREATION_SCHEDULE = Logging.LogFileCreationScheduleOption.Daily
    ' The LogFileCreationScheduleOption doesn't present the string format of what it uses
    Private Const LOG_FILE_DATESTRING = "yyyy-MM-dd"
    ' The subfolder that will contain logs.
    Public Const LOG_SUBFOLDER = "\Logs\"
#End Region

    Private LogFile As Logging.FileLogTraceListener
    Private ReadOnly TEventCache As New TraceEventCache()
    Public LogLevelValue As LogLvl
    Private L_CurrentLogData As String
    Private LastEventsList As New List(Of Object)
    Public Event NewData(sender As Object)

#Region "Properties"

    Private _MaxEvents As Integer = 200
    Public Property MaxEvents As Integer
        Get
            Return _MaxEvents
        End Get
        Set(value As Integer)
            If value < 0 Then
                Throw New ArgumentOutOfRangeException("MaxInteger", "Maximum number of events cannot be negative.")
            End If
        End Set
    End Property

    Public Property CurrentLogData() As String
        Get
            Dim Tmp_Data = L_CurrentLogData
            L_CurrentLogData = Nothing
            Return Tmp_Data
        End Get
        Set(Value As String)
            L_CurrentLogData = Value
        End Set
    End Property

    Public ReadOnly Property LastEvents() As List(Of Object)
        Get
            Return LastEventsList
        End Get
    End Property

    ''' <summary>
    ''' Returns if data is being written to a file. Also allows for file logging to be setup or stopped.
    ''' </summary>
    ''' <returns>True when the <see cref="LogFile"/> object is instantiated, false if not.</returns>
    Public ReadOnly Property IsWritingToFile() As Boolean
        Get
            Return LogFile IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Get the log file location from the <see cref="LogFile"/> object.
    ''' </summary>
    ''' <returns>The possible path to the log file. Note that this does not gaurantee it exists.</returns>
    Public ReadOnly Property LogFileLocation() As String
        Get
            Return LogFile.FullLogFileName
        End Get
    End Property

#End Region

    Public Sub New(LogLevel As LogLvl)
        LogLevelValue = LogLevel
    End Sub

    Public Sub InitializeLogFile(baseDataFolder As String)
        LogFile = New Logging.FileLogTraceListener(BASE_FILE_NAME) With {
            .TraceOutputOptions = TraceOptions.DateTime Or TraceOptions.ProcessId,
            .Append = True,
            .AutoFlush = True,
            .LogFileCreationSchedule = LOG_FILE_CREATION_SCHEDULE,
            .CustomLocation = baseDataFolder & LOG_SUBFOLDER,
            .Location = Logging.LogFileLocation.Custom
        }

        LogTracing("Log file is initialized at " & LogFile.FullLogFileName, LogLvl.LOG_NOTICE, Me)
    End Sub

    ''' <summary>
    ''' Disable logging and delete the current file.
    ''' </summary>
    ''' <returns>True if file was successfully deleted. False if an exception was encountered.</returns>
    Public Function DeleteLogFile() As Boolean
        Dim fileLocation = LogFile.FullLogFileName

        ' Disable logging first.
        If LogFile IsNot Nothing Then
            LogFile.Close()
            LogFile.Dispose()
            ' For some reason, the object needs to be dereferenced to actually get it to close the handle.
            LogFile = Nothing
            LogTracing("Logging to file has been disabled.", LogLvl.LOG_NOTICE, Me)
        End If

        Try
            ' IsWritingToFile = False
            File.Delete(fileLocation)
            Return True
        Catch ex As Exception
            LogTracing("Error when deleteing log file: " & ex.ToString(), LogLvl.LOG_ERROR, Me)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Write the <paramref name="message"/> to the Debug tracer is debugging, into the <see cref="LastEventsList" />
    ''' for report generating, to the <see cref="LogFile"/> if appropriate, and notify any listeners if
    ''' <paramref name="LogToDisplay"/> is specified.
    ''' </summary>
    ''' <param name="message">The raw information that needs to be recorded.</param>
    ''' <param name="LvlError">The severity of the message.</param>
    ''' <param name="sender">What generated this message.</param>
    ''' <param name="LogToDisplay">A user-friendly, translated string to be shown.</param>
    Public Sub LogTracing(message As String, LvlError As LogLvl, sender As Object, Optional LogToDisplay As String = Nothing)
        Dim Pid = TEventCache.ProcessId
        Dim SenderName
        ' Handle a null sender
        If sender Is Nothing Then
            SenderName = "Nothing"
        Else
            SenderName = sender.GetType.Name
        End If

        Dim EventTime = Now.ToLocalTime
        Dim FinalMsg = EventTime & " Pid: " & Pid & " " & SenderName & " : " & message

        ' Always write log messages to the attached debug messages window.
#If DEBUG Then
        Debug.WriteLine(FinalMsg)
#End If

        'Create Event in EventList in case of crash for generate Report
        If LastEventsList.Count >= MaxEvents Then
            LastEventsList.RemoveAt(0)
        End If
        LastEventsList.Add(FinalMsg)

        ' Send message to log file if enabled
        If IsWritingToFile AndAlso LogLevelValue >= LvlError Then
            LogFile.WriteLine(FinalMsg)
        End If

        'If LvlError = LogLvl.LOG_NOTICE Then
        If LogToDisplay IsNot Nothing Then
            L_CurrentLogData = LogToDisplay
            RaiseEvent NewData(sender)
        End If
    End Sub
End Class
