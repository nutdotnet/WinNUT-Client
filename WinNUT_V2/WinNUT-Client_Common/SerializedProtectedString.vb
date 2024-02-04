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

Public Class SerializedProtectedString

    Private protectedString As String

    ' Make the encrypted string available for serialization.
    Public Property ProtectedValue As String
        Get
            Return protectedString
        End Get
        Set(value As String)
            protectedString = value
        End Set
    End Property

    ' Do not save the unprotected string when serializing.
    Private Property UnprotectedValue As String
        Get
            Return Unprotect(protectedString)
        End Get
        Set(value As String)
            protectedString = Convert.ToBase64String(ProtectedData.Protect(Encoding.Unicode.GetBytes(value),
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
            protectedString = data
        Else
            UnprotectedValue = data
        End If
    End Sub

    ''' <summary>
    ''' Provide default constructor to create an empty object.
    ''' </summary>
    Public Sub New()
        Me.New(String.Empty)
    End Sub

    ''' <summary>
    ''' Attempts to unprotect an arbitrary string, suppressing any exceptions in the process.
    ''' Note: Use this function sparingly. Call the parametered constructor
    ''' <see cref="SerializedProtectedString.New(String, Boolean)"/> when the protection state is already known.
    ''' </summary>
    ''' <param name="unknownString"></param>
    ''' <returns>Either the unprotected value of the given parameter, or the parameter's value its self.</returns>
    Private Shared Function TryUnprotect(unknownString As String) As String
        Try
            Return Unprotect(unknownString)
        Catch ex As Exception
            Return unknownString
        End Try
    End Function

    Private Shared Function Unprotect(protectedString As String) As String
        Return Encoding.Unicode.GetString(ProtectedData.Unprotect(Convert.FromBase64String(protectedString),
                                   Nothing, DataProtectionScope.CurrentUser))
    End Function

    ''' <summary>
    ''' Provide the decrypted string.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return UnprotectedValue
    End Function

    ''' <summary>
    ''' Cast an arbitrary (unknown state) String into this object.
    ''' </summary>
    ''' <param name="unkStr"></param>
    ''' <returns></returns>
    Public Shared Narrowing Operator CType(unkStr As String) As SerializedProtectedString
        Return New SerializedProtectedString(TryUnprotect(unkStr))
    End Operator

    ''' <summary>
    ''' Support casting to a String, by providing the unprotected value (default ToString operation.)
    ''' </summary>
    ''' <param name="protStr"></param>
    ''' <returns></returns>
    Public Shared Widening Operator CType(protStr As SerializedProtectedString) As String
        If protStr IsNot Nothing Then
            Return protStr.ToString()
        Else
            Return Nothing
        End If
    End Operator
End Class
