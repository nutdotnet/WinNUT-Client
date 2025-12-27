Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Logging

Public Class Logger
#Region "Constants/Shared"
    Private Const LOG_FILE_CREATION_SCHEDULE = LogFileCreationScheduleOption.Daily
    Private Const SUBDIRECTORY = "Logs"
    Public Const MAX_DISPLAYED_LOGS = 50

#If DEBUG Then
    Private Shared ReadOnly DEFAULT_DATETIMEFORMAT = DateTimeFormatInfo.InvariantInfo
#Else
    Private Shared ReadOnly DEFAULT_DATETIMEFORMAT = DateTimeFormatInfo.CurrentInfo
#End If

    Private ReadOnly TEventCache As New TraceEventCache()
#End Region

#Region "Private/backing values"

    Private LogFile As FileLogTraceListener
    Private LastEventsList As New List(Of Object)
    Private _displayedLogs As New Queue(Of String)(MAX_DISPLAYED_LOGS)
    Private _displayedLogsCounter As Integer = 0 ' As incrementing when a new displayed log is added.
    Private _DateTimeFormatInfo As DateTimeFormatInfo = DEFAULT_DATETIMEFORMAT

#End Region

    Public LogLevelValue As LogLvl

    Public Event DisplayedLogsLineAdded(newLine As String)
    Public Event DisplayedLogsTrimmed(removedLine As String)

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

    ''' <summary>
    ''' Friendly log messages that are intended to be displayed to the user.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DisplayedLogs As Queue(Of String)
        Get
            Return _displayedLogs
        End Get
    End Property

    Public ReadOnly Property LastEvents() As List(Of Object)
        Get
            Return LastEventsList
        End Get
    End Property

    ''' <summary>
    ''' Check status of the log file writer, as well as start or stop logging to a file.
    ''' Events continue to be recorded to memory regardless.
    ''' </summary>
    ''' <returns>True when the <see cref="LogFile"/> object is instantiated, false if not.</returns>
    Public Property IsWritingToFile As Boolean
        Get
            Return LogFile IsNot Nothing
        End Get
        Set(value As Boolean)
            If value <> IsWritingToFile Then
                If value Then
                    InitializeLogFile()
                Else
                    TerminateLogFile()
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Get the full path of the <see cref="LogFile"/> object's log file.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">
    Public ReadOnly Property LogFilePath As String
        Get
            If IsWritingToFile Then
                Return LogFile.FullLogFileName
            Else
                Throw New InvalidOperationException("Log file has not been created.")
            End If
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

#Region "Log file management"

    ''' <summary>
    ''' Instantiates a new <see cref="FileLogTraceListener"/> with a location that's determined by a pre-defined
    ''' algorithm. The <see cref="LastEvents"/> buffer is written into the file before synchronizing with write calls.
    ''' </summary>
    Private Sub InitializeLogFile()
        LogTracing("InitializeLogFile called...", LogLvl.LOG_DEBUG, Me)

        Try
            LogFile = New FileLogTraceListener() With {
                .TraceOutputOptions = TraceOptions.DateTime Or TraceOptions.ProcessId,
                .Append = True,
                .AutoFlush = True,
                .LogFileCreationSchedule = LOG_FILE_CREATION_SCHEDULE,
                .CustomLocation = Path.Combine({DataDirectory, SUBDIRECTORY}),
                .Location = LogFileLocation.Custom
            }

            ' Calling the trace sub will also cause it to try to write to the log file, validating it.
            LogTracing($"Attempt init log file: { LogFilePath }", LogLvl.LOG_NOTICE, Me)
        Catch ex As Exception
            TerminateLogFile()
            LogException(ex, Me)
        End Try

        If LastEventsList.Count > 0 Then
            ' Fill new file with the LastEventsList buffer
            LogFile.WriteLine("==== History of " & LastEventsList.Count & " previous events ====")

            For index As Integer = 0 To LastEventsList.Count - 1
                LogFile.WriteLine(String.Format("[{0}] {1}", index + 1, LastEventsList(index)))
            Next
        End If

        LogFile.WriteLine("==== Begin Live Log ====" & Environment.NewLine)
    End Sub

    ''' <summary>
    ''' End logging to the <see cref="LogFile"/> by writing a terminating line to it, then closing and dereferencing it.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">The LogFile object is already Nothing and file logging is disabled.</exception>
    ''' 
    Public Sub TerminateLogFile()
        If IsWritingToFile Then
            LogTracing("Terminating log file.", LogLvl.LOG_NOTICE, Me)
            LogFile.Close()
            LogFile.Dispose()
            LogFile = Nothing
        Else
            Dim invOpExcp As New InvalidOperationException("Unable to terminate log file - already disabled.")
            LogException(invOpExcp, Me)
            Throw invOpExcp
        End If
    End Sub

    ''' <summary>
    ''' Disable logging and delete the current file. May throw an exception while deleting the file,
    ''' even if file logging was enabled.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">LogFile object is Nothing.</exception>
    Public Sub DeleteLogFile()
        If IsWritingToFile Then
            Dim fileLocation = LogFile.FullLogFileName
            TerminateLogFile()
            File.Delete(fileLocation)
            LogTracing("Log file has been deleted.", LogLvl.LOG_NOTICE, Me)
        Else
            Dim invOpExcp As New InvalidOperationException("File logging is disabled, unable to delete log file.")
            LogException(invOpExcp, Me)
            Throw invOpExcp
        End If
    End Sub

#End Region

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
        Trace.WriteLine(FinalMsg)

        'Create Event in EventList in case of crash for generate Report
        If LastEventsList.Count >= MaxEvents Then
            LastEventsList.RemoveAt(0)
        End If
        LastEventsList.Add(FinalMsg)

        ' Send message to log file if enabled
        If IsWritingToFile AndAlso LogLevelValue >= LvlError Then
            LogFile.WriteLine(FinalMsg)
        End If

        ' Insert new log message for display to the user, and prune old ones.
        If LogToDisplay IsNot Nothing Then
            If _displayedLogs.Count >= MAX_DISPLAYED_LOGS Then
                Dim removedLog = _displayedLogs.Dequeue()
                LogTracing($"Removed log from displayed logs collection: { removedLog }", LogLvl.LOG_DEBUG, Me)
                RaiseEvent DisplayedLogsTrimmed(removedLog)
            End If

            _displayedLogsCounter += 1
            Dim newLogLine = String.Format("[{0}][{1}] {2}", _displayedLogsCounter,
                                           String.Format(Now, "General Date"), LogToDisplay)
            _displayedLogs.Enqueue(newLogLine)
            LogTracing("Added new line to displayed logs collection: " & newLogLine, LogLvl.LOG_DEBUG, Me)
            RaiseEvent DisplayedLogsLineAdded(newLogLine)
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
