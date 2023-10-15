' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
''' <summary>
''' Represet a UPS Variable from the WinNUT Client Common library.
''' </summary>
Public Class CommUPSVariable
    Implements INotifyPropertyChanged

    Private _Name As String
    Private _Value As String
    Private _Description As String

    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

    ''' <summary>
    ''' The name of the UPS varialbe, often represented in a hierarchical node format with periods.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property

    Public Property Value As String
        Get
            Return _Value
        End Get
        Set(value As String)
            If Not _Value.Equals(value) Then
                _Value = value
                NotifyPropertyChanged()
            End If
        End Set
    End Property

    Public Property Description As String
        Get
            Return _Description
        End Get
        Set(value As String)
            If Not _Description.Equals(value) Then
                _Description = value
                NotifyPropertyChanged()
            End If
        End Set
    End Property

    Public Sub New(name As String)
        _Name = name
    End Sub

    Private Sub NotifyPropertyChanged(<CallerMemberName()> Optional propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
