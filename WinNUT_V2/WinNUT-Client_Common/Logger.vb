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
Imports System.Text
Imports Microsoft.VisualBasic.Logging

Public Class Logger
#Region "Constants/Shared"
    Private Const LOG_FILE_CREATION_SCHEDULE = LogFileCreationScheduleOption.Daily

    ' Set TEST_RELEASE_DIRS in the custom compiler constants dialog for file storage to behave like release.
#If DEBUG And Not TEST_RELEASE_DIRS Then
    Private Shared ReadOnly DEFAULT_DATETIMEFORMAT = DateTimeFormatInfo.InvariantInfo
    Private Shared ReadOnly DEFAULT_LOCATION = LogFileLocation.ExecutableDirectory
#Else
    Private Shared ReadOnly DEFAULT_DATETIMEFORMAT = DateTimeFormatInfo.CurrentInfo
    ' Actually goes to the Roaming folder.
    Private Shared ReadOnly DEFAULT_LOCATION = LogFileLocation.LocalUserApplicationDirectory
#End If

    Private ReadOnly TEventCache As New TraceEventCache()
#End Region

#Region "Private/backing values"

    Private LogFile As FileLogTraceListener
    Private L_CurrentLogData As String
    Private LastEventsList As New List(Of Object)
    Private _DateTimeFormatInfo As DateTimeFormatInfo = DEFAULT_DATETIMEFORMAT

#End Region

    Public LogLevelValue As LogLvl

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
    Public ReadOnly Property LogFilePath() As String
        Get
            Return LogFile.FullLogFileName
        End Get
    End Property

    Public Property DateTimeFormatInfo As DateTimeFormatInfo
        Get
            Return _DateTimeFormatInfo
        End Get
        Set(value As DateTimeFormatInfo)
            _DateTimeFormatInfo = value
        End Set
    End Property


#End Region

    Public Sub New(LogLevel As LogLvl)
        LogLevelValue = LogLevel
    End Sub

    ''' <summary>
    ''' Instantiates a new <see cref="FileLogTraceListener"/> at a the desired location, and outputs the
    ''' <see cref="LastEvents"/> buffer into the file before synchronizing with write calls.
    ''' </summary>
    ''' <param name="baseDataFolder">Desired location to initiate the log file. If unspecified,
    ''' then a default location is used.</param>
    Public Sub InitializeLogFile(Optional baseDataFolder As String = Nothing)
        LogFile = New FileLogTraceListener() With {
            .TraceOutputOptions = TraceOptions.DateTime Or TraceOptions.ProcessId,
            .Append = True,
            .AutoFlush = True,
            .LogFileCreationSchedule = LOG_FILE_CREATION_SCHEDULE
        }

        If baseDataFolder Is Nothing Then
            LogFile.Location = DEFAULT_LOCATION
        Else
            LogFile.Location = LogFileLocation.Custom
            LogFile.CustomLocation = baseDataFolder
        End If

        LogTracing(String.Format("{0} {1} Log file init", ProgramName, ProgramVersion), LogLvl.LOG_NOTICE, Me)

        If LastEventsList.Count > 0 Then
            ' Fill new file with the LastEventsList buffer
            LogFile.WriteLine("==== History of " & LastEventsList.Count & " previous events ====")

            For index As Integer = 0 To LastEventsList.Count - 1
                LogFile.WriteLine(String.Format("[{0}] {1}", index + 1, LastEventsList(index)))
            Next
        End If

        LogFile.WriteLine("==== Begin Live Log ====")
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
        Dim FinalMsg = FormatLogLine(message, LvlError, sender)

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

    Public Sub LogException(ex As Exception, sender As Object)
        Dim sb As New StringBuilder
        sb.AppendLine(ex.GetType().ToString() & " thrown in " & ex.Source)
        sb.AppendLine("Message: " & ex.Message)
        sb.AppendLine(ex.StackTrace)

        LogTracing(sb.ToString(), LogLvl.LOG_ERROR, sender)

        If ex.InnerException IsNot Nothing Then
            LogTracing("Inner exception present:", LogLvl.LOG_ERROR, sender)
            LogException(ex.InnerException, ex)
        End If

        LogTracing("Exception report complete.", LogLvl.LOG_NOTICE, Me)
    End Sub

    Private Function FormatLogLine(message As String, logLvl As LogLvl, Optional sender As Object = Nothing)
        Dim Pid = TEventCache.ProcessId
        Dim SenderName = "Nothing"

        If sender IsNot Nothing Then
            SenderName = sender.GetType.Name
        End If

        Return String.Format("{0} [{1}, {2}]: {3}", Date.Now.ToString(_DateTimeFormatInfo), Pid, SenderName, message)
    End Function
End Class
