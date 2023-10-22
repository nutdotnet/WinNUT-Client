' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.Security.Cryptography
Imports System.Text

Namespace Utils

    <Serializable()>
    Public Class SerializedProtectedString

        Private encryptedString As String

        Public Property Value As String
            Get
                Return Encoding.Unicode.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encryptedString),
                                       Nothing, DataProtectionScope.CurrentUser))
            End Get
            Set(value As String)
                encryptedString = Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(value),
                                                            Nothing, DataProtectionScope.CurrentUser))
            End Set
        End Property

        ''' <summary>
        ''' Create a new object starting with unencrypted data by default.
        ''' </summary>
        ''' <param name="data">Data that needs to be encrypted.</param>
        ''' <param name="alreadyEncrypted">The provided data is already encrypted and the first encryption should
        ''' be skipped. Default to false (data is starting as unencrypted.)</param>
        Public Sub New(data As String, Optional alreadyEncrypted As Boolean = False)
            If alreadyEncrypted Then
                encryptedString = data
            Else
                Value = data
            End If
        End Sub

        ''' <summary>
        ''' Provide the decrypted string.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class

End Namespace
