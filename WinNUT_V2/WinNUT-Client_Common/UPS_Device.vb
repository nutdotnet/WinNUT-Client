' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.Globalization
Imports System.Windows.Forms

Public Class UPS_Device
#Region "Properties"

    Public ReadOnly Property IsConnected() As Boolean
        Get
            Return (Nut_Socket.IsConnected) ' And Me.Socket_Status
        End Get
    End Property

    Public ReadOnly Property IsAuthenticated() As Boolean
        Get
            Return Nut_Socket.Auth_Success
        End Get
    End Property

    Public Property PollingInterval As Integer
        Get
            Return Update_Data.Interval
        End Get
        Set(value As Integer)
            Update_Data.Interval = value
        End Set
    End Property

#End Region
    Private Const CosPhi As Double = 0.6
    ' How many milliseconds to wait before the Reconnect routine tries again.
#If DEBUG Then
    Private Const DEFAULT_RECONNECT_WAIT_MS As Double = 5000
#Else
    Private Const DEFAULT_RECONNECT_WAIT_MS As Double = 30000
#End If


    'Private Nut_Conn As Nut_Comm
    ' Private LogFile As Logger
    Private Freq_Fallback As Double
    Private ciClone As CultureInfo

    Public Nut_Config As Nut_Parameter

    Public UPS_Datas As New UPS_Datas
    Public WithEvents Nut_Socket As Nut_Socket

    Public Retry As Integer = 0
    Public MaxRetry As Integer = 30
    Private WithEvents Reconnect_Nut As New System.Windows.Forms.Timer
    ' Private ReadOnly WatchDog As New System.Windows.Forms.Timer
    ' Private Socket_Status As Boolean = False




    Private LogFile As Logger
    'Private ConnectionStatus As Boolean = False
    'Private Server As String
    'Private Port As Integer
    'Private UPSName As String
    'Private Delay As Integer
    'Private Login As String
    'Private Password As String
    'Private Mfr As String
    'Private Model As String
    'Private Serial As String
    'Private Firmware As String
    'Private BattCh As Double
    'Private BattV As Double
    'Private BattRuntime As Double
    'Private BattCapacity As Double
    'Private PowerFreq As Double
    'Private InputV As Double
    'Private OutputV As Double
    'Private Load As Double
    'Private Status As String
    'Private OutPower As Double
    'Private InputA As Double
    'Private Low_Batt As Integer
    'Private Low_Backup As Integer
    'Private LConnect As Boolean = False
    'Private AReconnect As Boolean = False
    'Private MaxRetry As Integer = 30
    'Private Retry As Integer = 0
    'Private ErrorStatus As Boolean = False
    'Private ErrorMsg As String = ""
    'Private Update_Nut As New System.Windows.Forms.Timer
    'Private Reconnect_Nut As New System.Windows.Forms.Timer
    'Private NutSocket As System.Net.Sockets.Socket
    'Private NutTCP As System.Net.Sockets.TcpClient
    'Private NutStream As System.Net.Sockets.NetworkStream
    'Private ReaderStream As System.IO.StreamReader
    'Private WriterStream As System.IO.StreamWriter
    'Private ShutdownStatus As Boolean = False
    'Private Follow_FSD As Boolean = False
    'Private Unknown_UPS_Name As Boolean = False
    'Private Invalid_Data As Boolean = False
    'Private Invalid_Auth_Data As Boolean = False
    'Private Const CosPhi As Double = 0.6

    ' Public Event Unknown_UPS()
    Public Event DataUpdated()
    Public Event Connected(sender As UPS_Device)
    Public Event ReConnected(sender As UPS_Device)
    ' Notify that the connection was closed gracefully.
    Public Event Disconnected()
    ' Notify of an unexpectedly lost connection (??)
    Public Event Lost_Connect()
    ' Error encountered when trying to connect.
    Public Event ConnectionError(innerException As Exception)
    Public Event EncounteredNUTException(ex As Nut_Exception, sender As Object)
    Public Event New_Retry()
    Public Event Shutdown_Condition()
    Public Event Stop_Shutdown()

    Private WithEvents Update_Data As New Timer



    Public Sub New(ByRef Nut_Config As Nut_Parameter, ByRef LogFile As Logger, pollInterval As Integer)
        Me.LogFile = LogFile
        Me.Nut_Config = Nut_Config
        PollingInterval = pollInterval
        ciClone = CType(CultureInfo.InvariantCulture.Clone(), CultureInfo)
        ciClone.NumberFormat.NumberDecimalSeparator = "."
        Nut_Socket = New Nut_Socket(Me.Nut_Config, LogFile)

        With Reconnect_Nut
            .Interval = DEFAULT_RECONNECT_WAIT_MS
            .Enabled = False
            ' AddHandler .Tick, AddressOf Reconnect_Socket
        End With
        'With WatchDog
        '    .Interval = 1000
        '    .Enabled = False
        '    AddHandler .Tick, AddressOf Event_WatchDog
        'End With
    End Sub

    Public Sub Connect_UPS()
        LogFile.LogTracing("Beginning connection: " & Nut_Config.ToString(), LogLvl.LOG_DEBUG, Me)

        Try
            Nut_Socket.Connect()
            LogFile.LogTracing("TCP Socket Created", LogLvl.LOG_NOTICE, Me)

            ' If Nut_Socket.ExistsOnServer(Nut_Config.UPSName) Then
            UPS_Datas = GetUPSProductInfo()
            Init_Constant(Nut_Socket)
            Update_Data.Start()
            RaiseEvent Connected(Me)
            ' Else
            ' LogFile.LogTracing("Given UPS Name is unknown", LogLvl.LOG_ERROR, Me)
            ' RaiseEvent Unknown_UPS()
            ' Disconnect(True)
            ' End If
        Catch ex As Nut_Exception
            ' This is how we determine if we have a valid UPS name entered, among other errors.
            RaiseEvent EncounteredNUTException(ex, Me)

        Catch ex As Exception
            RaiseEvent ConnectionError(ex)

        End Try
    End Sub

    'Private Sub HandleDisconnectRequest(sender As Object, Optional cancelAutoReconnect As Boolean = True) Handles Me.RequestDisconnect
    '    If disconnectInProgress Then
    '        Throw New InvalidOperationException("Disconnection already in progress.")
    '    End If

    '    ' WatchDog.Stop()

    '    If cancelAutoReconnect And Reconnect_Nut.Enabled = True Then
    '        Debug.WriteLine("Cancelling ")
    '    End If
    'End Sub

    Public Sub Disconnect(Optional cancelReconnect As Boolean = True, Optional silent As Boolean = False, Optional forceful As Boolean = False)
        LogFile.LogTracing("Processing request to disconnect...", LogLvl.LOG_DEBUG, Me)
        ' WatchDog.Stop()
        Update_Data.Stop()
        If cancelReconnect And Reconnect_Nut.Enabled Then
            LogFile.LogTracing("Stopping Reconnect timer.", LogLvl.LOG_DEBUG, Me)
            Reconnect_Nut.Stop()
        End If

        Retry = 0
        Nut_Socket.Disconnect(silent, forceful)
        ' Confirmation of disconnection will come from raised Disconnected event.

        'LogFile.LogTracing("Completed disconnecting UPS, notifying listeners.", LogLvl.LOG_DEBUG, Me)
        'RaiseEvent Disconnected()
    End Sub

#Region "Socket Interaction"

    Private Sub SocketDisconnected() Handles Nut_Socket.SocketDisconnected
        LogFile.LogTracing("NutSocket raised Disconnected event.", LogLvl.LOG_DEBUG, Me)

        RaiseEvent Disconnected()
    End Sub

    ''' <summary>
    ''' Check underlying connection for an error state by sending an empty query to the server.
    ''' A watchdog may not actually be necessary under normal circumstances, since queries are regularly being sent to
    ''' the NUT server and will catch a broken socket that way.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Event_WatchDog(sender As Object, e As EventArgs)
        If IsConnected Then
            Dim Nut_Query = Nut_Socket.Query_Data("")
            If Nut_Query.Response = NUTResponse.NORESPONSE Then
                LogFile.LogTracing("WatchDog Socket report a Broken State", LogLvl.LOG_WARNING, Me)
                Nut_Socket.Disconnect(True)
                RaiseEvent Lost_Connect()
                ' Me.Socket_Status = False
            End If
        End If
    End Sub

    Private Sub Socket_Broken() Handles Nut_Socket.Socket_Broken
        ' LogFile.LogTracing("TCP Socket seems Broken", LogLvl.LOG_WARNING, Me)
        LogFile.LogTracing("Socket has reported a Broken event.", LogLvl.LOG_WARNING, Me)
        ' SocketDisconnected()
        RaiseEvent Lost_Connect()

        If Nut_Config.AutoReconnect Then
            LogFile.LogTracing("Reconnection Process Started", LogLvl.LOG_NOTICE, Me)
            Reconnect_Nut.Start()
        End If
    End Sub

    Private Sub Reconnect_Socket(sender As Object, e As EventArgs) Handles Reconnect_Nut.Tick
        Retry += 1
        If Retry <= MaxRetry Then
            RaiseEvent New_Retry()
            LogFile.LogTracing(String.Format("Try Reconnect {0} / {1}", Retry, MaxRetry), LogLvl.LOG_NOTICE, Me, String.Format(WinNUT_Globals.StrLog.Item(AppResxStr.STR_LOG_NEW_RETRY), Retry, MaxRetry))
            Connect_UPS()
            If IsConnected Then
                LogFile.LogTracing("Nut Host Reconnected", LogLvl.LOG_DEBUG, Me)
                Reconnect_Nut.Stop()
                Retry = 0
                RaiseEvent ReConnected(Me)
            End If
        Else
            LogFile.LogTracing("Max Retry reached. Stop Process Autoreconnect and wait for manual Reconnection", LogLvl.LOG_ERROR, Me, WinNUT_Globals.StrLog.Item(AppResxStr.STR_LOG_STOP_RETRY))
            'Reconnect_Nut.Stop()
            'RaiseEvent Disconnected()
            Disconnect(True)
        End If
    End Sub

    'Private Sub NUTSocketError(nutEx As Nut_Exception, NoticeLvl As LogLvl, sender As Object) Handles Nut_Socket.OnNUTException
    '    If nutEx.ExceptionValue = Nut_Exception_Value.SOCKET_BROKEN AndAlso nutEx.InnerException IsNot Nothing Then
    '        LogFile.LogTracing("Socket_Broken event InnerException: " & nutEx.InnerException.ToString(), LogLvl.LOG_DEBUG, Me)
    '    End If

    '    LogFile.LogTracing("[" & Nut_Config.UPSName & "] " & nutEx.ToString(), LogLvl.LOG_WARNING, Nut_Socket)
    'End Sub

#End Region

    'Public Sub ReConnect()
    '    If Not Me.IsConnected Then
    '        Nut_Socket.Connect()
    '    End If
    'End Sub

    Private Function GetUPSProductInfo() As UPS_Datas
        Dim UDatas As New UPS_Datas
        Dim UPSName = Nut_Config.UPSName
        UDatas.Mfr = Trim(GetUPSVar("ups.mfr", UPSName, "Unknown"))
        UDatas.Model = Trim(GetUPSVar("ups.model", UPSName, "Unknown"))
        UDatas.Serial = Trim(GetUPSVar("ups.serial", UPSName, "Unknown"))
        UDatas.Firmware = Trim(GetUPSVar("ups.firmware", UPSName, "Unknown"))
        Return UDatas
    End Function

    Private Sub Init_Constant(ByRef Nut_Socket As Nut_Socket)
        Dim UPSName = Nut_Config.UPSName
        UPS_Datas.UPS_Value.Batt_Capacity = Double.Parse(GetUPSVar("battery.capacity", UPSName, 7), ciClone)
        Freq_Fallback = Double.Parse(GetUPSVar("output.frequency.nominal", UPSName, (50 + CInt(WinNUT_Params.Arr_Reg_Key.Item("FrequencySupply")) * 10)), ciClone)
    End Sub

    Public Sub Retrieve_UPS_Datas() Handles Update_Data.Tick ' As UPS_Datas
        Dim UPSName = Nut_Config.UPSName
        LogFile.LogTracing("Enter Retrieve_UPS_Datas", LogLvl.LOG_DEBUG, Me)
        Try
            Dim UPS_rt_Status As String
            Dim InputA As Double
            ' LogFile.LogTracing("Enter Retrieve_UPS_Data", LogLvl.LOG_DEBUG, Me)
            If IsConnected Then
                With UPS_Datas
                    Select Case "Unknown"
                        Case .Mfr, .Model, .Serial, .Firmware
                            UPS_Datas = GetUPSProductInfo()
                    End Select
                End With
                With UPS_Datas.UPS_Value
                    .Batt_Charge = Double.Parse(GetUPSVar("battery.charge", UPSName, 255), ciClone)
                    .Batt_Voltage = Double.Parse(GetUPSVar("battery.voltage", UPSName, 12), ciClone)
                    .Batt_Runtime = Double.Parse(GetUPSVar("battery.runtime", UPSName, 86400), ciClone)
                    .Power_Frequency = Double.Parse(GetUPSVar("input.frequency", UPSName, Double.Parse(GetUPSVar("output.frequency", UPSName, Freq_Fallback), ciClone)), ciClone)
                    .Input_Voltage = Double.Parse(GetUPSVar("input.voltage", UPSName, 220), ciClone)
                    .Output_Voltage = Double.Parse(GetUPSVar("output.voltage", UPSName, .Input_Voltage), ciClone)
                    .Load = Double.Parse(GetUPSVar("ups.load", UPSName, 100), ciClone)
                    UPS_rt_Status = GetUPSVar("ups.status", UPSName, "OL")
                    .Output_Power = Double.Parse((GetUPSVar("ups.realpower.nominal", UPSName, 0)), ciClone)
                    If .Output_Power = 0 Then
                        .Output_Power = Double.Parse((GetUPSVar("ups.power.nominal", UPSName, 0)), ciClone)
                        If .Output_Power = 0 Then
                            InputA = Double.Parse(GetUPSVar("ups.current.nominal", UPSName, 1), ciClone)
                            .Output_Power = Math.Round(.Input_Voltage * 0.95 * InputA * CosPhi)
                        Else
                            .Output_Power = Math.Round(.Output_Power * (.Load / 100) * CosPhi)
                        End If
                    Else
                        .Output_Power = Math.Round(.Output_Power * (.Load / 100))
                    End If
                    Dim PowerDivider As Double = 0.5
                    Select Case .Load
                        Case 76 To 100
                            PowerDivider = 0.4
                        Case 51 To 75
                            PowerDivider = 0.3
                    End Select
                    If .Batt_Charge = 255 Then
                        Dim nBatt = Math.Floor(.Batt_Voltage / 12)
                        .Batt_Charge = Math.Floor((.Batt_Voltage - (11.6 * nBatt)) / (0.02 * nBatt))
                    End If
                    If .Batt_Runtime >= 86400 Then
                        'If Load is 0, the calculation results in infinity. This causes an exception in DataUpdated(), causing Me.Disconnect to run in the exception handler below.
                        'Thus a connection is established, but is forcefully disconneced almost immediately. This cycle repeats on each connect until load is <> 0
                        '(Example: I have a 0% load if only Pi, Microtik Router, Wifi AP and switches are running)
                        .Load = If(.Load <> 0, .Load, 0.1)
                        Dim BattInstantCurrent = (.Output_Voltage * .Load) / (.Batt_Voltage * 100)
                        .Batt_Runtime = Math.Floor(.Batt_Capacity * 0.6 * .Batt_Charge * (1 - PowerDivider) * 3600 / (BattInstantCurrent * 100))
                    End If
                    Dim StatusArr = UPS_rt_Status.Trim().Split(" ")
                    .UPS_Status = 0
                    For Each State In StatusArr
                        Select Case State
                            Case "OL"
                                LogFile.LogTracing("UPS is On Line", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.OL
                            'If Not Update_Nut.Interval = Me.Delay Then
                            '    Update_Nut.Stop()
                            '    Update_Nut.Interval = Me.Delay
                            '    Update_Nut.Start()
                            'End If
                            'If ShutdownStatus Then
                            '    LogFile.LogTracing("Stop condition Canceled", LogLvl.LOG_NOTICE, Me, WinNUT_Globals.StrLog.Item(AppResxStr.STR_LOG_SHUT_STOP))
                            '    ShutdownStatus = False
                            '    RaiseEvent Stop_Shutdown()
                            'End If
                            Case "OB"
                                LogFile.LogTracing("UPS is On Battery", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.OB
                            'If Update_Nut.Interval = Me.Delay Then
                            '    Update_Nut.Stop()
                            '    Update_Nut.Interval = If((Math.Floor(Me.Delay / 5) < 1000), 1000, Math.Floor(Me.Delay / 5))
                            '    Update_Nut.Start()
                            'End If
                            'If ((Me.BattCh <= Me.Low_Batt Or Me.BattRuntime <= Me.Backup_Limit) And Not ShutdownStatus) Then
                            '    LogFile.LogTracing("Stop condition reached", LogLvl.LOG_NOTICE, Me, WinNUT_Globals.StrLog.Item(AppResxStr.STR_LOG_SHUT_START))
                            '    RaiseEvent Shutdown_Condition()
                            '    ShutdownStatus = True
                            'End If
                            Case "LB", "HB"
                                LogFile.LogTracing("High/Low Battery on UPS", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.LBHB
                            Case "CHRG"
                                LogFile.LogTracing("Battery is Charging on UPS", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.CHRG
                            Case "DISCHRG"
                                LogFile.LogTracing("Battery is Discharging on UPS", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.DISCHRG
                            Case "FSD"
                                LogFile.LogTracing("Stop condition imposed by the NUT server", LogLvl.LOG_NOTICE, Me, WinNUT_Globals.StrLog.Item(AppResxStr.STR_LOG_NUT_FSD))
                                .UPS_Status = .UPS_Status Or UPS_States.FSD
                                RaiseEvent Shutdown_Condition()
                            'ShutdownStatus = True
                            Case "BYPASS"
                                LogFile.LogTracing("UPS bypass circuit is active - no battery protection is available", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.BYPASS
                            Case "CAL"
                                LogFile.LogTracing("UPS is currently performing runtime calibration (on battery)", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.CAL
                            Case "OFF"
                                LogFile.LogTracing("UPS is offline and is not supplying power to the load", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.OFF
                            Case "OVER"
                                LogFile.LogTracing("UPS is overloaded", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.OVER
                            Case "TRIM"
                                LogFile.LogTracing("UPS is trimming incoming voltage", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.TRIM
                            Case "BOOST"
                                LogFile.LogTracing("UPS is boosting incoming voltage", LogLvl.LOG_NOTICE, Me)
                                .UPS_Status = .UPS_Status Or UPS_States.BOOST
                        End Select
                    Next
                End With
                RaiseEvent DataUpdated()
            End If
        Catch Excep As Exception
            ' Something went wrong while trying to read the data... Consider the socket broken and proceed from here.
            LogFile.LogTracing("Something went wrong in Retrieve_UPS_Datas: " & Excep.ToString(), LogLvl.LOG_ERROR, Me)
            Disconnect(False, True, True)
            Socket_Broken()
            'Me.Disconnect(True)
            'Enter_Reconnect_Process(Excep, "Error When Retrieve_UPS_Data : ")
        End Try
        ' Return Me.UPS_Datas
    End Sub

    Private Const MAX_VAR_RETRIES = 3
    Public Function GetUPSVar(ByVal varName As String, ByVal UPSName As String, Optional ByVal Fallback_value As Object = Nothing, Optional recursing As Boolean = False) As String
        ' Try
        ' LogFile.LogTracing("Enter GetUPSVar", LogLvl.LOG_DEBUG, Me)
        'If Not Me.ConnectionStatus Then
        If Not IsConnected Then
            Throw New Nut_Exception(Nut_Exception_Value.SOCKET_BROKEN, varName)
            Return Nothing
        Else
            Dim Nut_Query = Nut_Socket.Query_Data("GET VAR " & UPSName & " " & varName)

            Select Case Nut_Query.Response
                Case NUTResponse.OK
                    ' LogFile.LogTracing("Process Result With " & varName & " : " & Nut_Query.Data, LogLvl.LOG_DEBUG, Me)
                    Return ExtractData(Nut_Query.Data)
                Case NUTResponse.UNKNOWNUPS
                    'Me.Invalid_Data = False
                    'Me.Unknown_UPS_Name = True
                    ' RaiseEvent Unknown_UPS()
                    Throw New Nut_Exception(Nut_Exception_Value.UNKNOWN_UPS, "The UPS does not exist on the server.")
                Case NUTResponse.VARNOTSUPPORTED
                    'Me.Unknown_UPS_Name = False
                    'Me.Invalid_Data = False

                    If Not String.IsNullOrEmpty(Fallback_value) Then
                        LogFile.LogTracing("Apply Fallback Value when retrieving " & varName, LogLvl.LOG_WARNING, Me)
                        Dim FakeData = "VAR " & UPSName & " " & varName & " " & """" & Fallback_value & """"
                        Return ExtractData(FakeData)
                    Else
                        LogFile.LogTracing("Error Result On Retrieving  " & varName & " : " & Nut_Query.Data, LogLvl.LOG_ERROR, Me)
                        Return Nothing
                    End If
                ' upsd won't provide any value for the var, in case of false reading.
                Case NUTResponse.DATASTALE
                    LogFile.LogTracing("DATA-STALE Error Result On Retrieving  " & varName & " : " & Nut_Query.Data, LogLvl.LOG_ERROR, Me)

                    If recursing Then
                        Return Nothing
                    Else
                        Dim retryNum = 1
                        Dim returnString As String = Nothing

                        While returnString Is Nothing AndAlso retryNum <= MAX_VAR_RETRIES
                            LogFile.LogTracing("Attempting retry " & retryNum & " to get variable.", LogLvl.LOG_NOTICE, Me)
                            returnString = GetUPSVar(varName, UPSName, Fallback_value, True)
                        End While

                        If returnString Is Nothing Then
                            returnString = Fallback_value
                            RaiseEvent EncounteredNUTException(New Nut_Exception(NUTResponse.DATASTALE,
                                        "Server reported that the data is stale after " & retryNum & " retries."), Me)
                        End If

                        Return returnString
                    End If

                    ' Throw New System.Exception(varName & " : " & Nut_Query.Data)
                    Throw New Nut_Exception(NUTResponse.DATASTALE, "Server reported that the data is stale.")

                    ' Return NUTResponse.DATASTALE
                Case Else
                    Return Nothing
            End Select
        End If
        'Catch Excep As Exception
        '    'RaiseEvent OnError(Excep, LogLvl.LOG_ERROR, Me)
        '    Return Nothing
        'End Try
    End Function

    Public Function GetUPS_ListVar() As List(Of UPS_List_Datas)
        Dim Response = New List(Of UPS_List_Datas)
        Dim Query = "LIST VAR " & Nut_Config.UPSName
        ' Try
        LogFile.LogTracing("Enter GetUPS_ListVar", LogLvl.LOG_DEBUG, Me)
        'If Not Me.ConnectionStatus Then
        If Not IsConnected Then
            Throw New InvalidOperationException("Attempted to list vars while disconnected.")
        Else
            Dim List_Var = Nut_Socket.Query_List_Datas(Query)
            If Not IsNothing(List_Var) Then
                Response = List_Var
            End If
        End If

        'Catch Excep As Exception
        '    'RaiseEvent OnError(Excep, LogLvl.LOG_ERROR, Me)
        'End Try
        Return Response
    End Function

    Private Function ExtractData(ByVal Var_Data As String) As String
        Dim SanitisedVar As String
        Dim StringArray(Nothing) As String
        Try
            SanitisedVar = Var_Data.Replace("""", String.Empty)
            StringArray = Split(SanitisedVar, " ", 4)
        Catch e As Exception
            MsgBox(e.Message)
        End Try
        Return StringArray(StringArray.Length - 1)
    End Function
End Class
