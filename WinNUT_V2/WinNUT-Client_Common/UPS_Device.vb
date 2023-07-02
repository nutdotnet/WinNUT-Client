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
            Return Nut_Socket.IsLoggedIn
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

    Public Property IsUpdatingData As Boolean
        Get
            Return Update_Data.Enabled
        End Get
        Set(value As Boolean)
            LogFile.LogTracing("UPS device updating status is now [" & value & "]", LogLvl.LOG_NOTICE, Me)
            Update_Data.Enabled = value
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

#Region "Events"

    ' Public Event Unknown_UPS()
    Public Event DataUpdated()
    Public Event Connected(sender As UPS_Device)
    Public Event ReConnected(sender As UPS_Device)
    ' Notify that the connection was closed gracefully.
    Public Event Disconnected()
    ' Notify of an unexpectedly lost connection (??)
    Public Event Lost_Connect()
    ' Error encountered when trying to connect.
    Public Event ConnectionError(sender As UPS_Device, innerException As Exception)
    Public Event EncounteredNUTException(ex As NutException, sender As Object)
    Public Event New_Retry()
    ' Public Event Shutdown_Condition()
    ' Public Event Stop_Shutdown()

#End Region

    Private Const CosPhi As Double = 0.6
    ' How many milliseconds to wait before the Reconnect routine tries again.
    Private Const DEFAULT_RECONNECT_WAIT_MS As Double = 5000

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




    Private LogFile As Logger

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

    Public Sub Connect_UPS(Optional retryOnConnFailure = False)
        LogFile.LogTracing("Beginning connection: " & Nut_Config.ToString(), LogLvl.LOG_DEBUG, Me)

        Try
            Nut_Socket.Connect()
            ' If Nut_Socket.ExistsOnServer(Nut_Config.UPSName) Then
            UPS_Datas = GetUPSProductInfo()
            Update_Data.Start()
            RaiseEvent Connected(Me)
        Catch ex As NutException
            ' This is how we determine if we have a valid UPS name entered, among other errors.
            RaiseEvent EncounteredNUTException(ex, Me)

        Catch ex As Exception
            RaiseEvent ConnectionError(Me, ex)

            If retryOnConnFailure Then
                LogFile.LogTracing("Reconnection Process Started", LogLvl.LOG_NOTICE, Me)
                Reconnect_Nut.Start()
            End If
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

            If IsConnected Then
                With UPS_Datas.UPS_Value
                    .Batt_Charge = Double.Parse(GetUPSVar("battery.charge", 255), ciClone)
                    .Batt_Voltage = Double.Parse(GetUPSVar("battery.voltage", 12), ciClone)
                    .Batt_Runtime = Double.Parse(GetUPSVar("battery.runtime", 86400), ciClone)
                    .Power_Frequency = Double.Parse(GetUPSVar("input.frequency", Double.Parse(GetUPSVar("output.frequency", Freq_Fallback), ciClone)), ciClone)
                    .Input_Voltage = Double.Parse(GetUPSVar("input.voltage", 220), ciClone)
                    .Output_Voltage = Double.Parse(GetUPSVar("output.voltage", .Input_Voltage), ciClone)
                    .Load = Double.Parse(GetUPSVar("ups.load", 100), ciClone)
                    UPS_rt_Status = GetUPSVar("ups.status", UPS_States.None)
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
                        LogFile.LogTracing("UPS statuses have CHANGED...", LogLvl.LOG_NOTICE, Me)
                        LogFile.LogTracing("Current statuses: " & UPS_rt_Status, LogLvl.LOG_NOTICE, Me)
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
    Public Function GetUPSVar(varName As String, Optional Fallback_value As Object = Nothing, Optional recursing As Boolean = False) As String
        If Not IsConnected Then
            Throw New InvalidOperationException("Tried to GetUPSVar while disconnected.")
        Else
            Dim Nut_Query As Transaction

            Try
                Nut_Query = Nut_Socket.Query_Data("GET VAR " & Name & " " & varName)

                If Nut_Query.ResponseType = NUTResponse.OK Then
                    Return ExtractData(Nut_Query.RawResponse)
                Else
                    Throw New NutException(Nut_Query)
                End If

            Catch ex As NutException
                Select Case ex.LastTransaction.ResponseType
                    Case NUTResponse.VARNOTSUPPORTED
                        LogFile.LogTracing(varName & " is not supported by server.", LogLvl.LOG_WARNING, Me)

                    Case NUTResponse.DATASTALE
                        LogFile.LogTracing("DATA-STALE Error Result On Retrieving  " & varName & " : " & ex.LastTransaction.RawResponse, LogLvl.LOG_ERROR, Me)

                        If recursing Then
                            Return Nothing
                        Else
                            Dim retryNum = 1
                            Dim returnString As String = Nothing

                            While returnString Is Nothing AndAlso retryNum <= MAX_VAR_RETRIES
                                LogFile.LogTracing("Attempting retry " & retryNum & " to get variable.", LogLvl.LOG_NOTICE, Me)
                                returnString = GetUPSVar(varName, Fallback_value, True)
                                retryNum += 1
                            End While

                            If returnString IsNot Nothing Then
                                Return returnString
                            End If
                        End If
                End Select

                If Not String.IsNullOrEmpty(Fallback_value) Then
                    LogFile.LogTracing("Apply Fallback Value when retrieving " & varName, LogLvl.LOG_WARNING, Me)
                    Return Fallback_value
                Else
                    LogFile.LogTracing("Unhandled error while getting " & varName, LogLvl.LOG_ERROR, Me)
                    Throw
                End If
            End Try
        End If

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

    Private Function ExtractData(Var_Data As String) As String
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
