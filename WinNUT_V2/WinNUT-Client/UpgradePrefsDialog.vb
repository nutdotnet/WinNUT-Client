' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel
Imports WinNUT_Client.Models
Imports WinNUT_Client_Common

Namespace Forms
    Friend Class UpgradePrefsDialog
        Private WithEvents backingDataModel As UpgradePrefsDialogModel

        Private Sub UpgradePrefsDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            If OldParams.WinNUT_Params.RegistryKeyRoot Is Nothing Then
                MessageBox.Show(My.Resources.UpgradePrefsDialog_NoPrefsExistError,
                                My.Resources.UpgradePrefsDialog_NoPrefsExistCaption)
                Close()
            End If

            ' Prepare the backing model for this form, with a reference back to it.
            backingDataModel = New UpgradePrefsDialogModel(Me)
            ' Connect the BindingSource object to an instance of the data source (our Model class)
            UpgradePrefsDialogModelBindingSource.DataSource = backingDataModel
            backingDataModel.InitializeProperties()
        End Sub

        Private Sub OK_Button_Click(sender As Object, e As EventArgs) Handles OK_Button.Click
            backingDataModel.BeginUpgradeWorkAsync()
        End Sub

        Private Sub Cancel_Button_Click(sender As Object, e As EventArgs) Handles Cancel_Button.Click
            backingDataModel.CancelButtonClicked()
        End Sub
    End Class

End Namespace
