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
''' Represent important details resulting from a completed async request to check for updates.
''' </summary>
Public Class CheckForUpdateCompletedEventArgs
    Inherits AsyncCompletedEventArgs

    Private ReadOnly _LatestRelease As Octokit.Release

    ReadOnly Property LatestRelease As Octokit.Release
        Get
            Return _LatestRelease
        End Get
    End Property

    Public Sub New(latestRelease As Octokit.Release, Optional [error] As Exception = Nothing,
                   Optional cancelled As Boolean = False)

        MyBase.New([error], cancelled, Nothing)
        _LatestRelease = latestRelease
    End Sub

End Class
