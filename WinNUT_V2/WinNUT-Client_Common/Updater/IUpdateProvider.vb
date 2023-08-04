' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel
Imports Octokit

Namespace Updater

    ''' <summary>
    ''' Defines an object that handles checking and retrieving updates for WinNUT.
    ''' </summary>
    Public Interface IUpdateProvider
        Inherits IComponent

        Function CheckForUpdate() As Release
        Sub CheckForUpdateWorker()
        Sub CheckForUpdateAsync()
        Sub CheckForUpdateAsyncCancel()
        Sub CheckForUpdateAsyncCompleted(eventArguments As CheckForUpdateCompletedEventArgs)
        Event OnCheckForUpdateCompleted(sender As IUpdateProvider, eventArguments As CheckForUpdateCompletedEventArgs)

        Function UpdateWorker() As Boolean
        Function Update() As Boolean
        Sub UpdateAsync()
        Sub UpdateAsyncCancel()

    End Interface

End Namespace
