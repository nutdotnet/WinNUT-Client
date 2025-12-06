Public Class UPS_Var_Node
    Public VarKey As String
    Public VarValue As String
    Public VarDesc As String
End Class

Public Class UPS_List_Datas
    Public VarKey As String
    Public VarValue As String
    Public VarDesc As String
End Class

Public Class UPS_Values
    Public Batt_Charge As Double = Nothing
    Public Batt_Voltage As Double = Nothing
    Public Batt_Runtime As Double = Nothing
    Public Input_Voltage As Double = Nothing
    Public Output_Voltage As Double = Nothing
    Public Output_Current As Single?
    Public Power_Frequency As Double = Nothing
    Public Load As Double = Nothing
    Public Output_Power As Double = Nothing
    Public Batt_Capacity As Double = Nothing
    Public UPS_Status As UPS_States = Nothing
End Class

Public Class UPSData
    Public ReadOnly Mfr As String
    Public ReadOnly Model As String
    Public ReadOnly Serial As String
    Public ReadOnly Firmware As String
    Public UPS_Value As New UPS_Values

    Public Sub New(Mfr As String, Model As String, Serial As String, Firmware As String)
        Me.Mfr = Mfr
        Me.Model = Model
        Me.Serial = Serial
        Me.Firmware = Firmware
    End Sub
End Class

''' <summary>
''' Encapsulates a query and response between a NUT client and server.
''' </summary>
Public Class Transaction
    ''' <summary>
    ''' The original query sent by the client, to the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Query As String
    ''' <summary>
    ''' See <see cref="NUTResponse"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ResponseType As NUTResponse
    ''' <summary>
    ''' The full, unaltered response from the server.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property RawResponse As String

    ''' <summary>
    ''' A <see cref="RawResponse"/> that has been split around the delimeter character (space)
    ''' </summary>
    Public ReadOnly Property SplitResponse As String()

    Public Sub New(query As String, response As String, responseType As NUTResponse, Optional splitResponse As String() = Nothing)
        Me.Query = query
        RawResponse = response
        Me.ResponseType = responseType
        Me.SplitResponse = splitResponse
    End Sub
End Class

Public Class NutException
    Inherits ApplicationException

    Public ReadOnly Property LastTransaction As Transaction

    ''' <summary>
    ''' Raise an exception that resulted from a defined error in the NUT protocol.
    ''' </summary>
    Public Sub New(transaction As Transaction)
        MyBase.New(String.Format("{0} ({1})" & vbNewLine & "Query: {2}", transaction.ResponseType,
                                   transaction.RawResponse, transaction.Query))
        LastTransaction = transaction
    End Sub
End Class

Public Class Nut_Parameter
    Public Host As String = ""
    Public Port As Integer = Nothing
    Public Login As String = ""
    Public Password As String = ""
    Public UPSName As String = ""
    Public AutoReconnect As Boolean = False

    Public Sub New(Host As String, Port As Integer, Login As String, Password As String, UPSName As String,
                   Optional AutoReconnect As Boolean = False)
        Me.Host = Host
        Me.Port = Port
        Me.Login = Login
        Me.Password = Password
        Me.UPSName = UPSName
        Me.AutoReconnect = AutoReconnect
    End Sub

    ''' <summary>
    ''' Generate an informative String representing this Parameter object. Note password is not printed.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return String.Format("{0}@{1}:{2}, Name: {3}" & If(AutoReconnect, " [AutoReconnect]", Nothing),
                             Login, Host, Port, UPSName, AutoReconnect)
        ' Return MyBase.ToString())
    End Function
End Class
