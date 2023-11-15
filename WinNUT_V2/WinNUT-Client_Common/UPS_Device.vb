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
            If Nut_Config IsNot Nothing Then
                Return Nut_Config.UPSName
            Else
                Return "null"
            End If
        End Get
    End Property

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return (Nut_Socket.IsConnected)
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

    Private _PowerCalculationMethod As PowerMethod

    Public ReadOnly Property PowerCalculationMethod As PowerMethod
        Get
            Return _PowerCalculationMethod
        End Get
    End Property

#End Region

#Region "Events"
    Public Event DataUpdated()
    Public Event Connected(sender As UPS_Device)
    Public Event ReConnected(sender As UPS_Device)
    ' Notify that the connection was closed gracefully.
    Public Event Disconnected()
    ' Notify of an unexpectedly lost connection (??)
    Public Event Lost_Connect()
    ' Error encountered when trying to connect.
    Public Event ConnectionError(sender As UPS_Device, innerException As Exception)
    Public Event New_Retry()

    ''' <summary>
    ''' Raised when the NUT server returns an error during normal communication and is deemed important for the client
    ''' application to know.
    ''' </summary>
    ''' <param name="sender">The device object that has received the error.</param>
    ''' <param name="nutEx">An exception detailing the error and cirucmstances surrounding it.</param>
    Public Event EncounteredNUTException(sender As UPS_Device, nutEx As NutException)

    ''' <summary>
    ''' Raise an event when a status code is added to the UPS that wasn't there before.
    ''' </summary>
    ''' <param name="newStatuses">The bitmask of status flags that are currently set on the UPS.</param>
    Public Event StatusesChanged(sender As UPS_Device, newStatuses As UPS_States)

#End Region

    Private Const CosPhi As Double = 0.6
    ' How many milliseconds to wait before the Reconnect routine tries again.
    Private Const DEFAULT_RECONNECT_WAIT_MS As Double = 5000

    Private WithEvents Update_Data As New Timer
    Private WithEvents Reconnect_Nut As New Timer
    Private WithEvents Nut_Socket As Nut_Socket

    Private Freq_Fallback As Double
    Private ciClone As CultureInfo
    Public Nut_Config As Nut_Parameter
    Public Retry As Integer = 0
    Public MaxRetry As Integer = 30
    Private LogFile As Logger

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
        End With
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
            RaiseEvent EncounteredNUTException(Me, ex)

        Catch ex As Exception
            RaiseEvent ConnectionError(Me, ex)

            If retryOnConnFailure Then
                LogFile.LogTracing("Reconnection Process Started", LogLvl.LOG_NOTICE, Me)
                Reconnect_Nut.Start()
            End If
        End Try
    End Sub

    Public Sub Disconnect(Optional cancelReconnect As Boolean = True, Optional forceful As Boolean = False)
        LogFile.LogTracing("Processing request to disconnect...", LogLvl.LOG_DEBUG, Me)

        Update_Data.Stop()
        If cancelReconnect And Reconnect_Nut.Enabled Then
            LogFile.LogTracing("Stopping Reconnect timer.", LogLvl.LOG_DEBUG, Me)
            Reconnect_Nut.Stop()
        End If

        Retry = 0
        Try
            Nut_Socket.Disconnect(forceful)
        Catch nutEx As NutException
            RaiseEvent EncounteredNUTException(Me, nutEx)
        Finally
            RaiseEvent Disconnected()
        End Try
    End Sub

#Region "Socket Interaction"

    Private Sub SocketDisconnected() Handles Nut_Socket.SocketDisconnected
        LogFile.LogTracing("NutSocket raised Disconnected event.", LogLvl.LOG_DEBUG, Me)

        RaiseEvent Disconnected()
    End Sub

    Private Sub Socket_Broken() Handles Nut_Socket.Socket_Broken
        LogFile.LogTracing("Socket has reported a Broken event.", LogLvl.LOG_WARNING, Me)
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
            LogFile.LogTracing(String.Format("Try Reconnect {0} / {1}", Retry, MaxRetry), LogLvl.LOG_NOTICE, Me, String.Format(StrLog.Item(AppResxStr.STR_LOG_NEW_RETRY), Retry, MaxRetry))
            Connect_UPS()
            If IsConnected Then
                LogFile.LogTracing("Nut Host Reconnected", LogLvl.LOG_DEBUG, Me)
                Reconnect_Nut.Stop()
                Retry = 0
                RaiseEvent ReConnected(Me)
            End If
        Else
            LogFile.LogTracing("Max Retry reached. Stop Process Autoreconnect and wait for manual Reconnection", LogLvl.LOG_ERROR, Me, StrLog.Item(AppResxStr.STR_LOG_STOP_RETRY))
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

        ' Determine available power & load data
        Try
            GetUPSVar("ups.realpower")
            _PowerCalculationMethod = PowerMethod.RealPower
            LogFile.LogTracing("Using RealPower method to calculate power usage.", LogLvl.LOG_NOTICE, Me)
        Catch
            Try
                GetUPSVar("ups.realpower.nominal")
                GetUPSVar("ups.load")
                _PowerCalculationMethod = PowerMethod.NominalPowerCalc
                LogFile.LogTracing("Using NominalPowerCalc method to calculate power usage.", LogLvl.LOG_NOTICE, Me)
            Catch
                Try
                    GetUPSVar("input.current.nominal")
                    GetUPSVar("input.voltage.nominal")
                    GetUPSVar("ups.load")
                    _PowerCalculationMethod = PowerMethod.VoltAmpCalc
                    LogFile.LogTracing("Using VoltAmpCalc method to calculate power usage.", LogLvl.LOG_NOTICE, Me)
                Catch
                    _PowerCalculationMethod = PowerMethod.Unavailable
                    LogFile.LogTracing("Unable to find a suitable method to calculate power usage.", LogLvl.LOG_WARNING, Me)
                End Try
            End Try
        End Try

        ' Other constant values for UPS calibration.
        freshData.UPS_Value.Batt_Capacity = Double.Parse(GetUPSVar("battery.capacity", 7), ciClone)
        Freq_Fallback = Double.Parse(GetUPSVar("output.frequency.nominal", (50 + CInt(Arr_Reg_Key.Item("FrequencySupply")) * 10)), ciClone)

        Return freshData
    End Function

    Private oldStatusBitmask As Integer

    Public Sub Retrieve_UPS_Datas() Handles Update_Data.Tick ' As UPSData
        LogFile.LogTracing("Enter Retrieve_UPS_Datas", LogLvl.LOG_DEBUG, Me)
        Try
            Dim UPS_rt_Status As String

            If IsConnected Then
                With UPS_Datas.UPS_Value
                    .Batt_Charge = Double.Parse(GetUPSVar("battery.charge", 255), ciClone)
                    .Batt_Voltage = Double.Parse(GetUPSVar("battery.voltage", 12), ciClone)
                    .Batt_Runtime = Double.Parse(GetUPSVar("battery.runtime", 86400), ciClone)
                    .Power_Frequency = Double.Parse(GetUPSVar("input.frequency", Double.Parse(GetUPSVar("output.frequency", Freq_Fallback), ciClone)), ciClone)
                    .Input_Voltage = Double.Parse(GetUPSVar("input.voltage", 220), ciClone)
                    .Output_Voltage = Double.Parse(GetUPSVar("output.voltage", .Input_Voltage), ciClone)
                    .Load = Double.Parse(GetUPSVar("ups.load", 0), ciClone)
                    .Output_Power = If(_PowerCalculationMethod <> PowerMethod.Unavailable, GetPowerUsage(), 0)

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

                    UPS_rt_Status = GetUPSVar("ups.status", UPS_States.None)
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
            Disconnect(False, True)
            Socket_Broken()
        End Try
    End Sub

    ''' <summary>
    ''' Attempts to get the power usage of this UPS.
    ''' </summary>
    ''' <returns></returns>
    ''' <throws><see cref="NutException"/></throws>
    Private Function GetPowerUsage() As Double
        If _PowerCalculationMethod = PowerMethod.RealPower Then
            Return Integer.Parse(GetUPSVar("ups.realpower"))
        ElseIf _PowerCalculationMethod = PowerMethod.NominalPowerCalc Then
            Return Integer.Parse(GetUPSVar("ups.realpower.nominal")) *
                (UPS_Datas.UPS_Value.Load / 100.0)
        ElseIf _PowerCalculationMethod = PowerMethod.VoltAmpCalc Then
            Dim nomCurrent = Double.Parse(GetUPSVar("input.current.nominal"))
            Dim nomVoltage = Double.Parse(GetUPSVar("input.voltage.nominal"))

            Return (nomCurrent * nomVoltage * 0.8) * (UPS_Datas.UPS_Value.Load / 100.0)
        Else
            Throw New InvalidOperationException("Insufficient variables to calculate power.")
        End If
    End Function

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

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
