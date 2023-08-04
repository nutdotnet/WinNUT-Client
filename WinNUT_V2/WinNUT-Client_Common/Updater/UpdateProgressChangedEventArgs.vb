' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel

''' <summary>
''' Notify a listener that an async method for either checking or installing an update has progress to report.
''' NOTE: Currently just a wrapper for <see cref="ProgressChangedEventArgs"/>
''' </summary>
Public Class UpdateProgressChangedEventArgs
    Inherits ProgressChangedEventArgs

    Public Sub New(progressPercentage As Integer, userState As Object)
        MyBase.New(progressPercentage, userState)
    End Sub
End Class
