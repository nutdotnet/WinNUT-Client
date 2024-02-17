' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports WinNUT_Client_Common

Public Class About_Gui
    Private Sub About_Gui_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Lbl_ProgNameVersion.Text = ProgramName & vbNewLine & "Version " & ProgramVersion
        Lbl_Copyright_2019.Text = Replace(Copyright, "©", vbNewLine & "©")
        LkLbl_Github.Text = GitHubURL
        Icon = WinNUT.Icon
        ' Me.LogFile = WinNUT.LogFile
        LogFile.LogTracing("Load About Gui", LogLvl.LOG_DEBUG, Me)
    End Sub

    Private Sub Btn_OK_Click(sender As Object, e As EventArgs) Handles Btn_OK.Click
        LogFile.LogTracing("Close About Gui", LogLvl.LOG_DEBUG, Me)
        Close()
    End Sub

    Private Sub LkLbl_Github_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LkLbl_Github.LinkClicked
        Process.Start(sender.Text)
    End Sub
End Class
