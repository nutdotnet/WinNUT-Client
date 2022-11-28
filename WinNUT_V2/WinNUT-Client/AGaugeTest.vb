' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Public Class AGaugeTest
    Private Sub btn_update_Click(sender As Object, e As EventArgs) Handles btn_update.Click
        AGauge1.MinValue = txt_minValue.Text
        AGauge1.MaxValue = txt_maxValue.Text
    End Sub
End Class
