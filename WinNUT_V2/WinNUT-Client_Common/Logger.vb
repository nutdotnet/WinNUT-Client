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
    Private Const BaseFileName = "WinNUT-CLient"
    ' Logs will be stored in the program's appdata folder, in a Logs subdirectory.
    Public Shared ReadOnly LogFolder = Path.Combine(ApplicationData, "Logs")

    Private LogFile As Logging.FileLogTraceListener
    Private ReadOnly TEventCache As New TraceEventCache()
    Public LogLevelValue As LogLvl
    Private L_CurrentLogData As String
    Private LastEventsList As New List(Of Object)
    Public Event NewData(ByVal sender As Object)

#Region "Properties"

    Public Property CurrentLogData() As String
        Get
            Dim Tmp_Data = Me.L_CurrentLogData
            Me.L_CurrentLogData = Nothing
            Return Tmp_Data
        End Get
        Set(ByVal Value As String)
            Me.L_CurrentLogData = Value
        End Set
    End Property
    Public ReadOnly Property LastEvents() As List(Of Object)
        Get
            Return Me.LastEventsList
        End Get
    End Property

    ''' <summary>
    ''' Returns if data is being written to a file. Also allows for file logging to be setup or stopped.
    ''' </summary>
    ''' <returns>True when the <see cref="LogFile"/> object is instantiated, false if not.</returns>
    Public Property IsWritingToFile() As Boolean
        Get
            Return Not (LogFile Is Nothing)
        End Get

        Set(Value As Boolean)
            If Value = False And LogFile IsNot Nothing Then
                LogFile.Close()
                LogFile.Dispose()
                LogFile = Nothing
                LogTracing("Logging to file has been disabled.", LogLvl.LOG_NOTICE, Me)
            ElseIf Value Then
                SetupLogfile()
            End If
        End Set
    End Property

    Public ReadOnly Property LogFileLocation() As String
        Get
            If IsWritingToFile Then
                Return LogFile.FullLogFileName
            Else
                Return String.Empty
            End If
        End Get
    End Property

    Public Property LogLevel() As LogLvl
        Get
            Return Me.LogLevelValue
        End Get
        Set(ByVal Value As LogLvl)
            Me.LogLevelValue = Value
        End Set
    End Property

#End Region

    Public Sub New(WriteLog As Boolean, LogLevel As LogLvl)
        IsWritingToFile = WriteLog
        LogLevelValue = LogLevel
        LastEventsList.Capacity = 50
    End Sub

    Public Sub SetupLogfile()
        LogFile = New Logging.FileLogTraceListener(BaseFileName) With {
            .TraceOutputOptions = TraceOptions.DateTime Or TraceOptions.ProcessId,
            .Append = True,
            .AutoFlush = True,
            .LogFileCreationSchedule = Logging.LogFileCreationScheduleOption.Daily,
            .CustomLocation = LogFolder,
            .Location = Logging.LogFileLocation.Custom
        }

        LogTracing("Log file is initialized at " & LogFile.FullLogFileName, LogLvl.LOG_NOTICE, Me)
    End Sub

    Public Function DeleteLogFile() As Boolean
        Try
            Dim fileLocation = LogFile.FullLogFileName
            IsWritingToFile = False
            File.Delete(fileLocation)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Write the <paramref name="message"/> to the Debug tracer is debugging, into the <see cref="LastEventsList" />
    ''' for report generating, to the <see cref="LogFile"/> if appropriate, and notify any listeners if
    ''' <paramref name="LogToDisplay"/> is specified.
    ''' </summary>
    ''' <param name="message">The raw information that needs to be recorded.</param>
    ''' <param name="LvlError">How important the information is.</param>
    ''' <param name="sender"></param>
    ''' <param name="LogToDisplay">A user-friendly, translated string to be shown.</param>
    Public Sub LogTracing(ByVal message As String, ByVal LvlError As Int16, sender As Object, Optional ByVal LogToDisplay As String = Nothing)
        Dim Pid = TEventCache.ProcessId
        Dim SenderName = sender.GetType.Name
        Dim EventTime = Now.ToLocalTime
        Dim FinalMsg = EventTime & " Pid: " & Pid & " " & SenderName & " : " & message

        'Update LogFilePath to make sure it's still the correct path
        ' gbakeman 31/7/2022: Disabling since the LogFilePath should never change throughout the lifetime of this
        '   object, unless proper initialization has occured.

        ' WinNUT_Globals.LogFilePath = Me.LogFile.FullLogFileName

        ' Always write log messages to the attached debug messages window.
#If DEBUG Then
        Debug.WriteLine(FinalMsg)
#End If

        'Create Event in EventList in case of crash for generate Report
        If Me.LastEventsList.Count = Me.LastEventsList.Capacity Then
            Me.LastEventsList.RemoveAt(0)
        End If

        Me.LastEventsList.Add(FinalMsg)

        If IsWritingToFile AndAlso Me.LogLevel >= LvlError Then
            LogFile.WriteLine(FinalMsg)
        End If
        'If LvlError = LogLvl.LOG_NOTICE Then
        If LogToDisplay IsNot Nothing Then
            Me.L_CurrentLogData = LogToDisplay
            RaiseEvent NewData(sender)
        End If
    End Sub
End Class
