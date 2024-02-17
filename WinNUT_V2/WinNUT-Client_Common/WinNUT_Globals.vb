' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Public Module WinNUT_Globals

#Region "Constants/Shareds"

    Public ReadOnly ProgramName = My.Application.Info.ProductName
    Public ReadOnly ProgramVersion = My.Application.Info.Version.ToString()
    Public ReadOnly ShortProgramVersion = ProgramVersion.Substring(0, ProgramVersion.IndexOf(".", ProgramVersion.IndexOf(".") + 1))
    Public ReadOnly GitHubURL = My.Application.Info.Trademark
    Public ReadOnly Copyright = My.Application.Info.Copyright

    Public WithEvents LogFile As Logger = New Logger(LogLvl.LOG_DEBUG)
    Public StrLog As New List(Of String)

#End Region

    Public Sub Init_Globals()

    End Sub
End Module
