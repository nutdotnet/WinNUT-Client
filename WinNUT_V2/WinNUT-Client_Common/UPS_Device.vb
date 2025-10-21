Imports System.Globalization
Imports System.Windows.Forms

Public Class UPS_Device
#Region "Statics/Defaults"
    Private ReadOnly INVARIANT_CULTURE = CultureInfo.InvariantCulture
    Private Const POWER_FACTOR = 0.8

    ' How many milliseconds to wait before the Reconnect routine tries again.
    Private Const DEFAULT_RECONNECT_WAIT_MS As Double = 5000
    Private Const DEFAULT_UPDATE_INTERVAL_MS As Double = 1000
#End Region

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
            Return (Nut_Socket.ConnectionStatus)
        End Get
    End Property

    Public ReadOnly Property IsReconnecting As Boolean
        Get
            Return Reconnect_Nut.Enabled
        End Get
    End Property

    Public ReadOnly Property IsLoggedIn As Boolean
        Get
            Return Nut_Socket.IsLoggedIn
        End Get
    End Property

    ''' <summary>
    ''' How often UPS data is updated, in milliseconds.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property PollingInterval As Integer
        Get
            Return Update_Data.Interval
        End Get
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
    ' Notify that the connection was closed gracefully.
    Public Event Disconnected()
    ' Notify of an unexpectedly lost connection.
    Public Event Lost_Connect()
    ' Error encountered when trying to connect.
    Public Event ConnectionError(sender As UPS_Device, innerException As Exception)

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

    Private WithEvents Update_Data As New Timer
    Private WithEvents Reconnect_Nut As New Timer
    Private WithEvents Nut_Socket As Nut_Socket

    Private Freq_Fallback As Double
    Public Nut_Config As Nut_Parameter
    Private ReadOnly LogFile As Logger

    Public Sub New(ByRef Nut_Config As Nut_Parameter, ByRef LogFile As Logger, pollInterval As Integer, defaultFrequency As Integer)
        Me.LogFile = LogFile
        Me.Nut_Config = Nut_Config
        Freq_Fallback = defaultFrequency
        Nut_Socket = New Nut_Socket(Me.Nut_Config, LogFile)

        With Reconnect_Nut
            .Interval = DEFAULT_RECONNECT_WAIT_MS
            .Enabled = False
            AddHandler .Tick, AddressOf AttemptReconnect
        End With

        With Update_Data
            .Interval = pollInterval
            .Enabled = False
            AddHandler .Tick, AddressOf Retrieve_UPS_Datas
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

            If Not String.IsNullOrEmpty(Nut_Config.Login) Then
                Login()
            End If

        Catch ex As NutException
            ' This is how we determine if we have a valid UPS name entered, among other errors.
            RaiseEvent EncounteredNUTException(Me, ex)

        Catch ex As Exception
            RaiseEvent ConnectionError(Me, ex)

            If retryOnConnFailure AndAlso IsReconnecting = False Then
                LogFile.LogTracing("Reconnection Process Started", LogLvl.LOG_NOTICE, Me)
                Reconnect_Nut.Start()
            End If
        End Try
    End Sub

    Public Sub Login()
        If Not IsConnected OrElse IsLoggedIn Then
            Throw New InvalidOperationException("UPS is in an invalid state to login.")
        End If

        If Not String.IsNullOrEmpty(Nut_Config.Login) Then
            Try
                Nut_Socket.Login()
            Catch ex As NutException
                LogFile.LogTracing("Error while attempting to log in.", LogLvl.LOG_ERROR, Me)
                RaiseEvent EncounteredNUTException(Me, ex)
            End Try
        End If
    End Sub

    Public Sub Disconnect(Optional cancelReconnect As Boolean = True, Optional forceful As Boolean = False)
        LogFile.LogTracing("Processing request to disconnect...", LogLvl.LOG_DEBUG, Me)

        Update_Data.Stop()
        If cancelReconnect And Reconnect_Nut.Enabled Then
            LogFile.LogTracing("Stopping Reconnect timer.", LogLvl.LOG_DEBUG, Me)
            Reconnect_Nut.Stop()
        End If

        Try
            Nut_Socket.Disconnect(forceful)
        Catch nutEx As NutException
            RaiseEvent EncounteredNUTException(Me, nutEx)
        Catch ex As Exception
            LogFile.LogTracing("Unexpected exception while Disconnecting.", LogLvl.LOG_ERROR, Me)
            LogFile.LogException(ex, Me)
        Finally
            RaiseEvent Disconnected()
        End Try
    End Sub

#Region "Socket Interaction"

    Private Sub Socket_Broken() Handles Nut_Socket.Socket_Broken
        LogFile.LogTracing("Socket has reported a Broken event.", LogLvl.LOG_WARNING, Me)
        Update_Data.Stop()
        RaiseEvent Lost_Connect()

        If Nut_Config.AutoReconnect Then
            LogFile.LogTracing("Reconnection Process Started", LogLvl.LOG_NOTICE, Me)
            Reconnect_Nut.Start()
        End If
    End Sub

    Private Sub AttemptReconnect(sender As Object, e As EventArgs)
        LogFile.LogTracing("Attempting reconnection...", LogLvl.LOG_NOTICE, Me)
        Connect_UPS()
        If IsConnected Then
            LogFile.LogTracing("Nut Host Reconnected", LogLvl.LOG_NOTICE, Me)
            Reconnect_Nut.Stop()
        End If
    End Sub

#End Region

    ''' <summary>
    ''' Convenient function to get data that never changes from the UPS.
    ''' </summary>
    ''' <returns></returns>
    Private Function GetUPSProductInfo() As UPSData
        LogFile.LogTracing("Retrieving basic UPS product information...", LogLvl.LOG_NOTICE, Me)

        Dim freshData = New UPSData(
            Trim(GetUPSVar({"ups.mfr", "device.mfr"}, "Unknown")),
            Trim(GetUPSVar({"ups.model", "device.model"}, "Unknown")),
            Trim(GetUPSVar({"ups.serial", "device.serial"}, "Unknown")),
            Trim(GetUPSVar("ups.firmware", "Unknown")))

        With freshData.UPS_Value
            LogFile.LogTracing("Initializing other well-known UPS variables...", LogLvl.LOG_DEBUG, Me)
            Try
                Dim value = Single.Parse(GetUPSVar("output.current"), INVARIANT_CULTURE)
                .Output_Current = value
                LogFile.LogTracing("output.current: " & value, LogLvl.LOG_DEBUG, Me)
            Catch ex As Exception
                If ex.GetType() <> GetType(NutException) Then
                    LogFile.LogException(ex, Me)
                End If
            End Try
            Try
                Dim value = Single.Parse(GetUPSVar("output.voltage"), INVARIANT_CULTURE)
                .Output_Voltage = value
                LogFile.LogTracing("output.voltage: " & value, LogLvl.LOG_DEBUG, Me)
            Catch ex As Exception
                If ex.GetType() <> GetType(NutException) Then
                    LogFile.LogException(ex, Me)
                End If
            End Try
            Try
                Dim value = Single.Parse(GetUPSVar("output.realpower"), INVARIANT_CULTURE)
                .Output_Power = value
                LogFile.LogTracing("output.power: " & value, LogLvl.LOG_DEBUG, Me)
            Catch ex As Exception

            End Try
        End With

        ' Determine optimal method for measuring power output from the UPS.
        LogFile.LogTracing("Determining best method to calculate power usage...", LogLvl.LOG_NOTICE, Me)
        ' Start with directly reading a variable from the UPS.
        Try
            If freshData.UPS_Value.Output_Power <> Nothing Then
                _PowerCalculationMethod = PowerMethod.RealOutputPower
                LogFile.LogTracing("Using RealOutputPower method.", LogLvl.LOG_NOTICE, Me)
            Else
                GetUPSVar("ups.realpower")
                _PowerCalculationMethod = PowerMethod.RealPower
                LogFile.LogTracing("Using RealPower method.", LogLvl.LOG_NOTICE, Me)
            End If
        Catch
            Try
                GetUPSVar("ups.realpower.nominal")
                GetUPSVar("ups.load")
                _PowerCalculationMethod = PowerMethod.RPNomLoadPct
                LogFile.LogTracing("Using RPNomLoadPct method.", LogLvl.LOG_NOTICE, Me)
            Catch
                Try
                    GetUPSVar("input.current.nominal")
                    GetUPSVar("input.voltage.nominal")
                    GetUPSVar("ups.load")
                    _PowerCalculationMethod = PowerMethod.InputNomVALoadPct
                    LogFile.LogTracing("Using InputNomVALoadPct method.", LogLvl.LOG_NOTICE, Me)
                Catch
                    If freshData.UPS_Value.Output_Current IsNot Nothing AndAlso
                            freshData.UPS_Value.Output_Voltage <> Nothing Then
                        _PowerCalculationMethod = PowerMethod.OutputVACalc
                        LogFile.LogTracing("Using OutputVACalc method.", LogLvl.LOG_NOTICE, Me)
                    Else
                        _PowerCalculationMethod = PowerMethod.Unavailable
                        LogFile.LogTracing("Unable to find a suitable method to calculate power usage.", LogLvl.LOG_WARNING, Me)
                    End If
                End Try
            End Try
        End Try

        ' Other constant values for UPS calibration.
        freshData.UPS_Value.Batt_Capacity = Double.Parse(GetUPSVar("battery.capacity", -1), INVARIANT_CULTURE)
        Freq_Fallback = Double.Parse(GetUPSVar("output.frequency.nominal", Freq_Fallback), INVARIANT_CULTURE)

        LogFile.LogTracing("Completed retrieval of basic UPS product information.", LogLvl.LOG_NOTICE, Me)
        Return freshData
    End Function

    Private oldStatusBitmask As Integer
    Private Sub Retrieve_UPS_Datas(sender As Object, e As EventArgs)
        LogFile.LogTracing("Enter Retrieve_UPS_Datas", LogLvl.LOG_DEBUG, Me)

        Try
            Dim UPS_rt_Status As String

            If IsConnected Then
                With UPS_Datas.UPS_Value
                    .Batt_Charge = Double.Parse(GetUPSVar("battery.charge", -1), INVARIANT_CULTURE)
                    .Batt_Voltage = Double.Parse(GetUPSVar("battery.voltage", -1), INVARIANT_CULTURE)
                    .Batt_Runtime = Double.Parse(GetUPSVar("battery.runtime", -1), INVARIANT_CULTURE)
                    .Power_Frequency = Double.Parse(GetUPSVar("input.frequency", Freq_Fallback), INVARIANT_CULTURE)
                    .Input_Voltage = Double.Parse(GetUPSVar("input.voltage", -1), INVARIANT_CULTURE)
                    .Output_Voltage = Double.Parse(GetUPSVar("output.voltage", -1), INVARIANT_CULTURE)
                    .Load = Double.Parse(GetUPSVar("ups.load", 0), INVARIANT_CULTURE)

                    ' Retrieve and/or calculate output power if possible.
                    If _PowerCalculationMethod <> PowerMethod.Unavailable Then
                        Dim parsedValue As Double

                        Try
                            Select Case _PowerCalculationMethod
                                Case PowerMethod.RealPower
                                    parsedValue = Double.Parse(GetUPSVar("ups.realpower"), INVARIANT_CULTURE)

                                Case PowerMethod.RealOutputPower
                                    parsedValue = Single.Parse(GetUPSVar("output.realpower"), INVARIANT_CULTURE)

                                Case PowerMethod.RPNomLoadPct
                                    parsedValue = Double.Parse(GetUPSVar("ups.realpower.nominal"), INVARIANT_CULTURE)
                                    parsedValue *= UPS_Datas.UPS_Value.Load / 100.0

                                Case PowerMethod.InputNomVALoadPct
                                    Dim nomCurrent = Double.Parse(GetUPSVar("input.current.nominal"), INVARIANT_CULTURE)
                                    Dim nomVoltage = Double.Parse(GetUPSVar("input.voltage.nominal"), INVARIANT_CULTURE)

                                    parsedValue = nomCurrent * nomVoltage * POWER_FACTOR
                                    parsedValue *= UPS_Datas.UPS_Value.Load / 100.0
                                Case PowerMethod.OutputVACalc
                                    .Output_Current = Single.Parse(GetUPSVar("output.current"), INVARIANT_CULTURE)
                                    parsedValue = .Output_Current * .Output_Voltage * POWER_FACTOR
                                Case Else
                                    ' Should not trigger - something has gone wrong.
                                    Throw New InvalidOperationException("Reached Else case when attempting to get power output for method " & _PowerCalculationMethod)
                            End Select
                        Catch ex As FormatException
                            LogFile.LogTracing("Unexpected format trying to parse value from UPS. Exception:", LogLvl.LOG_ERROR, Me)
                            LogFile.LogTracing(ex.ToString(), LogLvl.LOG_ERROR, Me)
                            LogFile.LogTracing("parsedValue: " & parsedValue, LogLvl.LOG_ERROR, Me)
                        Catch ex As Exception
                            LogFile.LogException(ex, Me)
                        End Try

                        ' Apply rounding to this number since calculations have extended to three decimal places.
                        ' TODO: Remove this round function once gauges can handle decimal places better.
                        .Output_Power = Math.Round(parsedValue, 1)
                    End If

                    ' Handle out-of-range battery charge
                    If .Batt_Charge < 0 OrElse .Batt_Charge > 100 Then
                        If .Batt_Voltage > 0 Then
                            Dim nBatt = Math.Floor(.Batt_Voltage / 12)
                            .Batt_Charge = Math.Floor((.Batt_Voltage - (11.6 * nBatt)) / (0.02 * nBatt))
                        Else
                            LogFile.LogTracing("Unable to calculate UPS Batt_Charge: Batt_Voltage (" & .Batt_Voltage & ") out of range.", LogLvl.LOG_WARNING, Me)
                        End If
                    End If

                    ' Attempt to calculate battery runtime if not given by the UPS.
                    If .Batt_Runtime = -1 Then
                        If .Output_Voltage = -1 OrElse .Batt_Voltage = -1 OrElse .Batt_Capacity = -1 OrElse .Batt_Charge = -1 Then
                            LogFile.LogTracing("Unable to calculate battery runtime, missing UPS variables.", LogLvl.LOG_WARNING, Me)
                            LogFile.LogTracing(String.Format("Output_Voltage: {0}, Batt_Voltage: {1}, Batt_Capacity: {2}, Batt_Charge: {3}",
                                .Output_Voltage, .Batt_Voltage, .Batt_Capacity, .Batt_Charge), LogLvl.LOG_WARNING, Me)

                        Else
                            Dim PowerDivider As Double = 0.5
                            Select Case .Load
                                Case 76 To 100
                                    PowerDivider = 0.4
                                Case 51 To 75
                                    PowerDivider = 0.3
                            End Select

                            .Load = If(.Load <> 0, .Load, 0.1)
                            Dim BattInstantCurrent = (.Output_Voltage * .Load) / (.Batt_Voltage * 100)
                            .Batt_Runtime = Math.Floor(.Batt_Capacity * 0.6 * .Batt_Charge * (1 - PowerDivider) * 3600 / (BattInstantCurrent * 100))
                        End If
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
            ' Something went wrong while trying to read the data... Consider the socket broken and proceed from here.
        Catch Excep As Exception
            LogFile.LogTracing("Something went wrong in Retrieve_UPS_Datas:", LogLvl.LOG_ERROR, Me)
            LogFile.LogException(Excep, Me)
            Socket_Broken()
        End Try
    End Sub

    Private Const MAX_VAR_RETRIES = 3
    Public Function GetUPSVar(varNames As String(), Optional Fallback_value As Object = Nothing, Optional recursing As Boolean = False) As String
        If Not IsConnected Then
            Throw New InvalidOperationException("Tried to GetUPSVar while disconnected.")
        End If

        ' Try each variable in the array sequentially
        For Each varName As String In varNames
            Try
                LogFile.LogTracing("Trying variable: " & varName, LogLvl.LOG_DEBUG, Me)

                Dim Nut_Query As Transaction
                Nut_Query = Nut_Socket.Query_Data("GET VAR " & Name & " " & varName)

                If Nut_Query.ResponseType = NUTResponse.OK Then
                    LogFile.LogTracing("Success with " & varName, LogLvl.LOG_DEBUG, Me)
                    Return ExtractData(Nut_Query.RawResponse)
                Else
                    Throw New NutException(Nut_Query)
                End If

            Catch ex As NutException
                Select Case ex.LastTransaction.ResponseType
                    Case NUTResponse.VARNOTSUPPORTED
                        LogFile.LogTracing(varName & " is not supported by server, trying next", LogLvl.LOG_WARNING, Me)
                        ' Continue to next variable
                        Continue For

                    Case NUTResponse.DATASTALE
                        LogFile.LogTracing("DATA-STALE Error Result On Retrieving " & varName & " : " & ex.LastTransaction.RawResponse, LogLvl.LOG_ERROR, Me)
                        If recursing Then
                            ' Continue to next variable instead of returning Nothing
                            Continue For
                        Else
                            Dim retryNum = 1
                            Dim returnString As String = Nothing
                            While returnString Is Nothing AndAlso retryNum <= MAX_VAR_RETRIES
                                LogFile.LogTracing("Attempting retry " & retryNum & " to get variable " & varName, LogLvl.LOG_NOTICE, Me)
                                returnString = GetUPSVar({varName}, Fallback_value, True)
                                retryNum += 1
                            End While
                            If returnString IsNot Nothing Then
                                Return returnString
                            Else
                                ' Retry failed, continue to next variable
                                Continue For
                            End If
                        End If

                    Case Else
                        LogFile.LogTracing("Error with " & varName & ", trying next", LogLvl.LOG_WARNING, Me)
                        ' Continue to next variable
                        Continue For
                End Select
            Catch ex As Exception
                LogFile.LogTracing("Exception for variable " & varName & ": " & ex.Message & ", trying next", LogLvl.LOG_WARNING, Me)
                ' Continue to next variable
                Continue For
            End Try
        Next

        ' If we reach here, all variables failed
        If Not String.IsNullOrEmpty(Fallback_value) Then
            LogFile.LogTracing("All variables failed, applying fallback value", LogLvl.LOG_WARNING, Me)
            Return Fallback_value
        Else
            LogFile.LogTracing("All variables failed and no fallback provided", LogLvl.LOG_ERROR, Me)
            Throw New NutException("All variables failed and no fallback provided", NUTResponse.VARNOTSUPPORTED, Nothing)
        End If
    End Function

    ' Overload for backward compatibility with existing code
    Public Function GetUPSVar(varName As String, Optional Fallback_value As Object = Nothing, Optional recursing As Boolean = False) As String
        Return GetUPSVar({varName}, Fallback_value, recursing)
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
