Imports System.IO
Imports System.Net.Sockets

Public Class Nut_Socket

    Private Const TIMEOUT_MS = 5000
#Region "Properties"
    Public ReadOnly Property ConnectionStatus As Boolean
        Get
            Return If(client IsNot Nothing, client.Connected, False)
        End Get
    End Property

    Private _isLoggedIn As Boolean = False
    Public ReadOnly Property IsLoggedIn() As Boolean
        Get
            Return _isLoggedIn
        End Get
    End Property

    Public ReadOnly Property NUTVersion As String
    Public ReadOnly Property NetVersion As String
#End Region

    Private LogFile As Logger
    Private NutConfig As Nut_Parameter

    'Socket Variables
    Private client As TcpClient
    Private NutStream As NetworkStream
    Private ReaderStream As StreamReader
    Private WriterStream As StreamWriter

    ''' <summary>
    ''' Possibly a race condition going on where a query is sent while reading a response from another.
    ''' </summary>
    Private streamInUse As Boolean

    Public Event Socket_Broken()

    Public Sub New(Nut_Config As Nut_Parameter, ByRef logger As Logger)
        LogFile = logger
        NutConfig = Nut_Config
    End Sub

    Public Sub Connect()
        'TODO: Use LIST UPS protocol command to get valid UPSs.
        Dim Host = NutConfig.Host
        Dim Port = NutConfig.Port
        Dim Login = NutConfig.Login
        Dim Password = NutConfig.Password

        If String.IsNullOrEmpty(Host) Or IsNothing(Port) Then
            Throw New InvalidOperationException("Host and Port must be specified to connect.")
        End If

        Try
            LogFile.LogTracing(String.Format("Attempting TCP socket connection to {0}:{1}...", Host, Port), LogLvl.LOG_NOTICE, Me)

            client = New TcpClient(Host, Port) With
            {
                .SendTimeout = TIMEOUT_MS,
                .ReceiveTimeout = TIMEOUT_MS
            }

            NutStream = client.GetStream()
            ReaderStream = New StreamReader(NutStream)
            WriterStream = New StreamWriter(NutStream)

            LogFile.LogTracing("Connection established and streams ready.", LogLvl.LOG_NOTICE, Me)

            LogFile.LogTracing("Gathering basic info about the NUT server...", LogLvl.LOG_DEBUG, Me)

            Try
                Dim Nut_Query = Query_Data("VER")

                If Nut_Query.ResponseType = NUTResponse.OK Then
                    _NUTVersion = (Nut_Query.RawResponse.Split(" "c))(4)
                    LogFile.LogTracing("Server version: " & NUTVersion, LogLvl.LOG_NOTICE, Me)
                End If
            Catch nutEx As NutException
                LogFile.LogTracing("Error retrieving server version.", LogLvl.LOG_WARNING, Me)
                LogFile.LogException(nutEx, Me)
            End Try

            Try
                Dim Nut_Query = Query_Data("NETVER")

                If Nut_Query.ResponseType = NUTResponse.OK Then
                    _NetVersion = Nut_Query.RawResponse
                    LogFile.LogTracing("Protocol version: " & NetVersion, LogLvl.LOG_NOTICE, Me)
                End If
            Catch nutEx As NutException
                LogFile.LogTracing("Error retrieving protocol version.", LogLvl.LOG_WARNING, Me)
                LogFile.LogException(nutEx, Me)
            End Try

            LogFile.LogTracing("Completed gathering basic info about NUT server.", LogLvl.LOG_DEBUG, Me)
        Catch Excep As Exception
            Disconnect(True)
            Throw ' Pass exception on up to UPS
        End Try
    End Sub

    Public Sub Login()
        If _isLoggedIn Then
            Throw New InvalidOperationException("Attempted to login when already logged in.")
        End If

        LogFile.LogTracing(String.Format("Logging in to UPS [{0}] as user [{1}] ({2})...",
                            NutConfig.UPSName, NutConfig.Login,
                            If(String.IsNullOrEmpty(NutConfig.Password),
                                "NO Password", "Password provided")), LogLvl.LOG_NOTICE, Me)

        If Not String.IsNullOrEmpty(NutConfig.Login) Then
            Query_Data("USERNAME " & NutConfig.Login)

            If Not String.IsNullOrEmpty(NutConfig.Password) Then
                Query_Data("PASSWORD " & NutConfig.Password)
            End If
        End If

        Query_Data("LOGIN " & NutConfig.UPSName)
        _isLoggedIn = True
        LogFile.LogTracing("Authenticated successfully.", LogLvl.LOG_NOTICE, Me)
    End Sub

    ''' <summary>
    ''' Perform various functions necessary to disconnect the socket from the NUT server.
    ''' </summary>
    ''' <param name="skipLogout">Do not send the LOGOUT command to the NUT server. Unknown effects.</param>
    Public Sub Disconnect(Optional skipLogout = False)
        If IsLoggedIn AndAlso Not skipLogout Then
            ' TODO: Move to new subroutine.
            Query_Data("LOGOUT")
        End If

        _isLoggedIn = False

        If WriterStream IsNot Nothing Then
            WriterStream.Dispose()
        End If

        If ReaderStream IsNot Nothing Then
            ReaderStream.Dispose()
        End If

        If client IsNot Nothing Then
            client.Close()
        End If
    End Sub

    ''' <summary>
    ''' Attempt to send a query to the NUT server, and do some basic parsing.
    ''' </summary>
    ''' <param name="Query_Msg">The query to be sent to the server, within specifications of the NUT protocol.</param>
    ''' <returns>The full <see cref="Transaction"/> of this function call.</returns>
    ''' <exception cref="InvalidOperationException">Thrown when calling this function while disconnected, or another
    ''' call is in progress.</exception>
    ''' <exception cref="NutException">Thrown when the NUT server returns an error or unexpected response.</exception>
    Function Query_Data(Query_Msg As String) As Transaction
        If Not ConnectionStatus Then
            Throw New InvalidOperationException("Attempted to send query " & Query_Msg & " while disconnected.")
        End If

        If streamInUse Then
            Throw New InvalidOperationException("Attempted to send query " & Query_Msg & " while stream is in use.")
        End If

        Try
            streamInUse = True
            WriterStream.WriteLine(Query_Msg)
            WriterStream.Flush()
        Catch
            Throw
        Finally
            streamInUse = False
        End Try

        Dim responseEnum = NUTResponse.EMPTY
        Dim response = ReaderStream.ReadLine()

        If String.IsNullOrEmpty(response) Then
            ' End of stream reached, likely server terminated connection.
            Disconnect(True)
            RaiseEvent Socket_Broken()
        Else
            Dim parseResponse = response.Trim().ToUpper().Split(" "c) ' TODO: Is Trim unnecessary?

            Select Case parseResponse(0)
                Case "OK", "VAR", "DESC", "UPS"
                    responseEnum = NUTResponse.OK
                Case "BEGIN"
                    responseEnum = NUTResponse.BEGINLIST
                Case "END"
                    responseEnum = NUTResponse.ENDLIST
                Case "NETWORK", "1.0", "1.1", "1.2", "1.3"
                    'In case of "VER" or "NETVER" Query
                    responseEnum = NUTResponse.OK
                Case "ERR"
                    responseEnum = DirectCast([Enum].Parse(GetType(NUTResponse),
                                                parseResponse(1).Replace("-", String.Empty)), NUTResponse)
                Case Else
                    responseEnum = NUTResponse.UNRECOGNIZED
            End Select
        End If

        Dim transaction = New Transaction(Query_Msg, response, responseEnum)

        If responseEnum = NUTResponse.OK OrElse responseEnum = NUTResponse.BEGINLIST OrElse responseEnum = NUTResponse.ENDLIST Then
            Return transaction
        End If

        Throw New NutException(transaction)
    End Function

    Public Function Query_List_Datas(Query_Msg As String) As List(Of UPS_List_Datas)
        Dim List_Datas As New List(Of String)
        Dim List_Result As New List(Of UPS_List_Datas)
        Dim start As Date = Date.Now

        ' Read in first line to get initial response.
        ' LogFile.LogTracing("Sending LIST query " & Query_Msg, LogLvl.LOG_DEBUG, Me)
        Dim response = Query_Data(Query_Msg)
        streamInUse = True
        Dim readLine As String

        While True
            readLine = ReaderStream.ReadLine()

            If Not readLine.StartsWith("END") Then
                List_Datas.Add(readLine)
            Else
                Exit While
            End If
        End While

        streamInUse = False
        ' LogFile.LogTracing("Done processing LIST response for query " & Query_Msg, LogLvl.LOG_DEBUG, Me)

        Dim Key As String
        Dim Value As String
        For Each Line In List_Datas
            Dim SplitString = Split(Line, " ", 4)

            Select Case SplitString(0)
                Case "BEGIN"
                Case "VAR"
                    'Query 
                    'LIST VAR <upsname>
                    'Response List of var
                    'VAR <upsname><varname> "<value>"
                    Key = Replace(SplitString(2), """", "")
                    Value = Replace(SplitString(3), """", "")
                    Dim UPSName = SplitString(1)
                    Dim VarDESC = GetVarDescription(Key)
                    List_Result.Add(New UPS_List_Datas With {
                            .VarKey = Key,
                            .VarValue = Trim(Value),
                            .VarDesc = If(Not IsNothing(VarDESC), Split(Replace(VarDESC, """", ""), " ", 4)(3), String.Empty)}
                        )

                Case "UPS"
                    'Query 
                    'LIST UPS
                    'List of ups
                    'UPS <upsname> "<description>"
                    List_Result.Add(New UPS_List_Datas With {
                            .VarKey = "UPSNAME",
                            .VarValue = SplitString(1),
                            .VarDesc = Replace(SplitString(2), """", "")}
                        )
                Case "RW"
                    'Query 
                    'LIST RW <upsname>
                    'List of RW var
                    'RW <upsname><varname> "<value>"
                    Key = Replace(SplitString(2), """", "")
                    Value = Replace(SplitString(3), """", "")
                    Dim UPSName = SplitString(1)
                    Dim VarDESC = GetVarDescription(Key)
                    If Not IsNothing(VarDESC) Then
                        List_Result.Add(New UPS_List_Datas With {
                            .VarKey = Key,
                            .VarValue = Trim(Value),
                            .VarDesc = If(Not IsNothing(VarDESC), Split(Replace(VarDESC, """", ""), " ", 4)(3), String.Empty)}
                        )
                    Else
                        'TODO: Convert to nut_exception error
                        Throw New Exception("error")
                    End If
                Case "CMD"
                            'Query 
                            'LIST CMD <upsname>
                            'List of CMD
                            'CMD <upsname><cmdname>
                Case "ENUM"
                    'Query 
                    'LIST ENUM <upsname>
                    'List of Enum ??
                    'ENUM <upsname><varname> "<value>"
                    Key = Replace(SplitString(2), """", "")
                    Value = Replace(SplitString(3), """", "")
                    Dim UPSName = SplitString(1)
                    Dim VarDESC = Query_Data("GET DESC " & UPSName & " " & Key)
                    If VarDESC.ResponseType = NUTResponse.OK Then
                        List_Result.Add(New UPS_List_Datas With {
                            .VarKey = Key,
                            .VarValue = Value,
                            .VarDesc = Split(Replace(VarDESC.RawResponse, """", ""), " ", 4)(3)}
                        )
                    Else
                        'TODO: Convert to nut_exception error
                        Throw New Exception("error")
                    End If
                Case "RANGE"
                            'Query 
                            'LIST RANGE <upsname><varname>
                            'List of Range
                            'RANGE <upsname><varname> "<min>" "<max>"
                Case "CLIENT"
                    'Query 
                    'LIST CLIENT <upsname>
                    'List of Range
                    'CLIENT <device name><client IP address>
            End Select
        Next

        Return List_Result
    End Function

    Public Function GetVarDescription(VarName As String) As String
        Dim Nut_Query = Query_Data("GET DESC " & NutConfig.UPSName & " " & VarName)

        If Nut_Query.ResponseType = NUTResponse.OK Then
            Return Nut_Query.RawResponse
        Else
            Throw New NutException(Nut_Query)
        End If
    End Function
End Class
