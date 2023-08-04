' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel
Imports System.Threading
Imports Octokit

Namespace Updater

    Public Class GitHubUpdateProvider
        Implements IUpdateProvider

        Private Const REPOSITORY_OWNER = "nutdotnet"
        Private Const REPOSITORY_NAME = "WinNUT-Client"

        Private onCompletedDelegate As SendOrPostCallback
        Private Delegate Sub CheckForUpdateDelegate()
        ' Only one instance of a check or install procedure at a time.
        Private asyncOpInstance As AsyncOperation
        Private disposedValue As Boolean
        Private Shared _gitHubClient As GitHubClient

        Public Event OnCheckForUpdateCompleted(sender As IUpdateProvider, eventArgs As CheckForUpdateCompletedEventArgs) _
            Implements IUpdateProvider.OnCheckForUpdateCompleted

        Public Event Disposed As EventHandler Implements IComponent.Disposed

        Protected Overridable Sub InitializeDelegates()
            onCompletedDelegate = New SendOrPostCallback(AddressOf CheckForUpdateAsyncCompleted)
        End Sub

        Public Function CheckForUpdate() As Release Implements IUpdateProvider.CheckForUpdate
            Dim checkTask = _gitHubClient.Repository.Release.GetLatest(REPOSITORY_OWNER, REPOSITORY_NAME)

            checkTask.RunSynchronously()
            Return checkTask.Result
        End Function

        Public Sub CheckForUpdateAsync() Implements IUpdateProvider.CheckForUpdateAsync
            If asyncOpInstance IsNot Nothing Then
                Throw New InvalidOperationException("Instance of AsyncOperation already exists.")
            End If

            asyncOpInstance = AsyncOperationManager.CreateOperation(Nothing)
            Dim checkDelegate As New CheckForUpdateDelegate(AddressOf CheckForUpdateWorker)
            checkDelegate.BeginInvoke(Nothing, Nothing)
        End Sub

        Private Sub CheckForUpdateWorker() Implements IUpdateProvider.CheckForUpdateWorker
            Dim checkCompleteEvent = New CheckForUpdateCompletedEventArgs(CheckForUpdate())
            CheckForUpdateAsyncCompleted(checkCompleteEvent)
        End Sub

        ' Doesn't really do anything. May need to implement real cancellation logic.
        Public Sub CheckForUpdateAsyncCancel() Implements IUpdateProvider.CheckForUpdateAsyncCancel
            If asyncOpInstance IsNot Nothing Then
                asyncOpInstance = Nothing
            End If
        End Sub

        Protected Sub CheckForUpdateAsyncCompleted(eventArguments As CheckForUpdateCompletedEventArgs) _
            Implements IUpdateProvider.CheckForUpdateAsyncCompleted

            RaiseEvent OnCheckForUpdateCompleted(Me, eventArguments)
        End Sub

        Public Function UpdateWorker() As Boolean Implements IUpdateProvider.UpdateWorker
            Throw New NotImplementedException()
        End Function

        Public Function Update() As Boolean Implements IUpdateProvider.Update
            Throw New NotImplementedException()
        End Function

        Public Sub UpdateAsync() Implements IUpdateProvider.UpdateAsync
            Throw New NotImplementedException()
        End Sub

        Public Sub UpdateAsyncCancel() Implements IUpdateProvider.UpdateAsyncCancel
            Throw New NotImplementedException()
        End Sub

        Sub New()
            InitializeDelegates()

            _gitHubClient = New GitHubClient(New ProductHeaderValue(REPOSITORY_NAME))
        End Sub

        Public Property Site As ISite Implements IComponent.Site
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As ISite)
                Throw New NotImplementedException()
            End Set
        End Property

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

End Namespace
