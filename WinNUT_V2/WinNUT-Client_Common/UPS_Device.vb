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

    Public ReadOnly Property Name As String
        Get
            Return Nut_Config.UPSName
        End Get
    End Property

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return (Nut_Socket.IsConnected) ' And Me.Socket_Status
        End Get
    End Property

    Public ReadOnly Property IsAuthenticated As Boolean
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

    Private upsData As UPSData
    Public Property UPS_Datas As UPSData
        Get
            Return upsData
        End Get
        Private Set(value As UPSData)
            upsData = value
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

    Private WithEvents Update_Data As New Timer
    'Private Nut_Conn As Nut_Comm
    ' Private LogFile As Logger
    Private Freq_Fallback As Double
    Private ciClone As CultureInfo

    Public Nut_Config As Nut_Parameter

    ' Public UPSData As New UPSData


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
    'Private Follow_FSD As Boolean = False
    'Private Unknown_UPS_Name As Boolean = False
    'Private Invalid_Data As Boolean = False
    'Private Invalid_Auth_Data As Boolean = False

#Region "Properties"

#End Region

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
    Public Event EncounteredNUTException(ex As NutException, sender As Object)
    Public Event New_Retry()
    ' Public Event Shutdown_Condition()
    ' Public Event Stop_Shutdown()

    ''' <summary>
    ''' Raise an event when a status code is added to the UPS that wasn't there before.
    ''' </summary>
    ''' <param name="newStatuses">The bitmask of status flags that are currently set on the UPS.</param>
    Public Event StatusesChanged(sender As UPS_Device, newStatuses As UPS_States)

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
            Update_Data.Start()
            RaiseEvent Connected(Me)
        Catch ex As NutException
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
            If Nut_Query.ResponseType = NUTResponse.NORESPONSE Then
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

#End Region

    ''' <summary>
    ''' Convenient function to get data that never changes from the UPS.
    ''' </summary>
    ''' <returns></returns>
    Private Function GetUPSProductInfo() As UPSData
        Dim freshData = New UPSData(
            Trim(GetUPSVar("ups.mfr", "Unknown")),
            Trim(GetUPSVar("ups.model", "Unknown")),
            Trim(GetUPSVar("ups.serial", "Unknown")),
            Trim(GetUPSVar("ups.firmware", "Unknown")))

        ' Other constant values for UPS calibration.
        freshData.UPS_Value.Batt_Capacity = Double.Parse(GetUPSVar("battery.capacity", 7), ciClone)
        Freq_Fallback = Double.Parse(GetUPSVar("output.frequency.nominal", (50 + CInt(WinNUT_Params.Arr_Reg_Key.Item("FrequencySupply")) * 10)), ciClone)

        Return freshData
    End Function

    Private oldStatusBitmask As Integer

    Public Sub Retrieve_UPS_Datas() Handles Update_Data.Tick ' As UPSData
        LogFile.LogTracing("Enter Retrieve_UPS_Datas", LogLvl.LOG_DEBUG, Me)
        Try
            Dim UPS_rt_Status As String
            Dim InputA As Double
            ' LogFile.LogTracing("Enter Retrieve_UPS_Data", LogLvl.LOG_DEBUG, Me)
            If IsConnected Then
                'With UPS_Datas
                '    Select Case "Unknown"
                '        Case .Mfr, .Model, .Serial, .Firmware
                '            UPS_Datas = GetUPSProductInfo()
                '    End Select
                'End With

                With UPS_Datas.UPS_Value
                    .Batt_Charge = Double.Parse(GetUPSVar("battery.charge", 255), ciClone)
                    .Batt_Voltage = Double.Parse(GetUPSVar("battery.voltage", 12), ciClone)
                    .Batt_Runtime = Double.Parse(GetUPSVar("battery.runtime", 86400), ciClone)
                    .Power_Frequency = Double.Parse(GetUPSVar("input.frequency", Double.Parse(GetUPSVar("output.frequency", Freq_Fallback), ciClone)), ciClone)
                    .Input_Voltage = Double.Parse(GetUPSVar("input.voltage", 220), ciClone)
                    .Output_Voltage = Double.Parse(GetUPSVar("output.voltage", .Input_Voltage), ciClone)
                    .Load = Double.Parse(GetUPSVar("ups.load", 100), ciClone)
                    UPS_rt_Status = GetUPSVar("ups.status")
                    .Output_Power = Double.Parse((GetUPSVar("ups.realpower.nominal", 0)), ciClone)
                    If .Output_Power = 0 Then
                        .Output_Power = Double.Parse((GetUPSVar("ups.power.nominal", 0)), ciClone)
                        If .Output_Power = 0 Then
                            InputA = Double.Parse(GetUPSVar("ups.current.nominal", 1), ciClone)
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

                    ' Prepare the status string for Enum parsing by replacing spaces with commas.
                    UPS_rt_Status = UPS_rt_Status.Replace(" ", ",")
                    Try
                        .UPS_Status = [Enum].Parse(GetType(UPS_States), UPS_rt_Status)
                    Catch ex As ArgumentException
                        LogFile.LogTracing("Likely encountered an unknown/invalid UPS status. Using previous status." &
                                           vbNewLine & ex.Message, LogLvl.LOG_ERROR, Me)
                    End Try

                    ' Get the difference between the old and new statuses, and filter only for active ones.
                    Dim statusDiff = (oldStatusBitmask Xor .UPS_Status) And .UPS_Status

                    If statusDiff = 0 Then
                        LogFile.LogTracing("UPS statuses have not changed since last update, skipping.", LogLvl.LOG_DEBUG, Me)
                    Else
                        LogFile.LogTracing("UPS statuses have CHANGED, updating...", LogLvl.LOG_NOTICE, Me)
                        oldStatusBitmask = .UPS_Status
                        RaiseEvent StatusesChanged(Me, statusDiff)
                    End If
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
    End Sub

    Private Const MAX_VAR_RETRIES = 3
    Public Function GetUPSVar(ByVal varName As String, Optional ByVal Fallback_value As Object = Nothing, Optional recursing As Boolean = False) As String
        ' Try
        ' LogFile.LogTracing("Enter GetUPSVar", LogLvl.LOG_DEBUG, Me)
        'If Not Me.ConnectionStatus Then
        If Not IsConnected Then
            ' Throw New NutException(Nut_Exception_Value.SOCKET_BROKEN, varName)
            Throw New InvalidOperationException("Tried to GetUPSVar while disconnected.")
            ' Return Nothing
        Else
            Dim Nut_Query As Transaction
            Try
                Nut_Query = Nut_Socket.Query_Data("GET VAR " & Name & " " & varName)

                Select Case Nut_Query.ResponseType
                    Case NUTResponse.OK
                        ' LogFile.LogTracing("Process Result With " & varName & " : " & Nut_Query.Data, LogLvl.LOG_DEBUG, Me)
                        Return ExtractData(Nut_Query.RawResponse)
                ' Case NUTResponse.UNKNOWNUPS
                    'Me.Invalid_Data = False
                    'Me.Unknown_UPS_Name = True
                    ' RaiseEvent Unknown_UPS()
                    ' Throw New Nut_Exception(Nut_Exception_Value.UNKNOWN_UPS, "The UPS does not exist on the server.")
                ' upsd won't provide any value for the var, in case of false reading.
                    Case NUTResponse.DATASTALE
                        LogFile.LogTracing("DATA-STALE Error Result On Retrieving  " & varName & " : " & Nut_Query.RawResponse, LogLvl.LOG_ERROR, Me)

                        If recursing Then
                            Return Nothing
                        Else
                            Dim retryNum = 1
                            Dim returnString As String = Nothing

                            While returnString Is Nothing AndAlso retryNum <= MAX_VAR_RETRIES
                                LogFile.LogTracing("Attempting retry " & retryNum & " to get variable.", LogLvl.LOG_NOTICE, Me)
                                returnString = GetUPSVar(varName, Fallback_value, True)
                                Return returnString
                            End While

                            RaiseEvent EncounteredNUTException(New NutException(Nut_Query), Me)
                            Return Fallback_value
                        End If

                        ' Throw New System.Exception(varName & " : " & Nut_Query.Data)
                        Throw New NutException(Nut_Query)

                        ' Return NUTResponse.DATASTALE
                    Case Else
                        Throw New NutException(Nut_Query)
                        ' Return Nothing
                End Select
            Catch ex As NutException
                If ex.LastTransaction.ResponseType = NUTResponse.VARNOTSUPPORTED Then
                    'Me.Unknown_UPS_Name = False
                    'Me.Invalid_Data = False

                    If Not String.IsNullOrEmpty(Fallback_value) Then
                        LogFile.LogTracing("Apply Fallback Value when retrieving " & varName, LogLvl.LOG_WARNING, Me)
                        'Dim FakeData = "VAR " & Name & " " & varName & " " & """" & Fallback_value & """"
                        'Return ExtractData(FakeData)
                        Return Fallback_value
                    Else
                        ' Var is unknown and caller was not prepared to handle it, pass exception along.
                        Throw
                    End If
                End If
            End Try
        End If
        'Catch Excep As Exception
        '    'RaiseEvent OnError(Excep, LogLvl.LOG_ERROR, Me)
        '    Return Nothing
        'End Try
        Return Nothing
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
