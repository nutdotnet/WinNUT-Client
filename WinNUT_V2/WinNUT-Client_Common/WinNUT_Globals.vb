Public Module WinNUT_Globals

#Region "Constants/Shareds"

    Private Const PARAM_PERSIST_DATA_IN_STARTUP_PATH = "-PersistDataInStartupPath"
    Private ReadOnly PREFERRED_DATA_DIRECTORY = Windows.Forms.Application.LocalUserAppDataPath

    Public ReadOnly ProgramName As String = My.Application.Info.ProductName
    Public ReadOnly ProgramVersion As String = My.Application.Info.Version.ToString()
    Public ReadOnly ShortProgramVersion As String =
        ProgramVersion.Substring(0, ProgramVersion.IndexOf(".", ProgramVersion.IndexOf(".") + 1))
    Public ReadOnly GitHubURL As String = My.Application.Info.Trademark
    Public ReadOnly Copyright As String = My.Application.Info.Copyright
    Public ReadOnly DataDirectory As String

    Public WithEvents LogFile As Logger = New Logger(LogLvl.LOG_DEBUG)
    Public WithEvents UpdateController As New Updater.UpdateUtil
    Public StrLog As New List(Of String)

#End Region

    Sub New()
        If Environment.GetCommandLineArgs().Contains(PARAM_PERSIST_DATA_IN_STARTUP_PATH) Then
            LogFile.LogTracing("Detected CommandLineArg to store persistent data to StartupPath.", LogLvl.LOG_DEBUG, Nothing)

            If IsPathWritable(Windows.Forms.Application.StartupPath) Then
                LogFile.LogTracing("Confirmed StartupPath as chosen path.", LogLvl.LOG_DEBUG, Nothing)
                DataDirectory = Windows.Forms.Application.StartupPath
                Return
            Else
                LogFile.LogTracing("Log to StartupPath requested, but path is not writable.", LogLvl.LOG_ERROR, Nothing)
            End If
        End If

        LogFile.LogTracing("Setting DataDirectory to preferred location.", LogLvl.LOG_DEBUG, Nothing)
        DataDirectory = PREFERRED_DATA_DIRECTORY
    End Sub

    Private Function IsPathWritable(_path As String, Optional throwExceptions As Boolean = False) As Boolean
        LogFile.LogTracing($"Checking path { _path } for writability...", LogLvl.LOG_DEBUG, Nothing)
        Try
            Using fs = IO.File.Create(IO.Path.Combine(_path, IO.Path.GetRandomFileName()), 1, IO.FileOptions.DeleteOnClose)
            End Using
            LogFile.LogTracing("Path is writable.", LogLvl.LOG_DEBUG, Nothing)
            Return True
        Catch
            LogFile.LogTracing("Path is not writable.", LogLvl.LOG_DEBUG, Nothing)
            If throwExceptions Then
                Throw
            Else
                Return False
            End If
        End Try
    End Function
End Module
