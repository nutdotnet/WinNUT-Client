' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AGaugeTest
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txt_minValue = New System.Windows.Forms.TextBox()
        Me.txt_maxValue = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.AGauge1 = New System.Windows.Forms.AGauge()
        Me.btn_update = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 150)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(51, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "MinValue"
        '
        'txt_minValue
        '
        Me.txt_minValue.Location = New System.Drawing.Point(99, 150)
        Me.txt_minValue.Name = "txt_minValue"
        Me.txt_minValue.Size = New System.Drawing.Size(62, 20)
        Me.txt_minValue.TabIndex = 2
        '
        'txt_maxValue
        '
        Me.txt_maxValue.Location = New System.Drawing.Point(99, 176)
        Me.txt_maxValue.Name = "txt_maxValue"
        Me.txt_maxValue.Size = New System.Drawing.Size(62, 20)
        Me.txt_maxValue.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 176)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(54, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "MaxValue"
        '
        'AGauge1
        '
        Me.AGauge1.BaseArcColor = System.Drawing.Color.Gray
        Me.AGauge1.BaseArcRadius = 45
        Me.AGauge1.BaseArcStart = 135
        Me.AGauge1.BaseArcSweep = 270
        Me.AGauge1.BaseArcWidth = 5
        Me.AGauge1.Center = New System.Drawing.Point(74, 70)
        Me.AGauge1.GaugeAutoSize = False
        Me.AGauge1.GradientColor = System.Windows.Forms.AGauge.GradientType.RedGreen
        Me.AGauge1.GradientColorOrientation = System.Windows.Forms.AGauge.GradientOrientation.BottomToUp
        Me.AGauge1.Location = New System.Drawing.Point(13, 13)
        Me.AGauge1.MaxValue = 100.0!
        Me.AGauge1.MinValue = 0!
        Me.AGauge1.Name = "AGauge1"
        Me.AGauge1.NeedleColor1 = System.Windows.Forms.AGaugeNeedleColor.Gray
        Me.AGauge1.NeedleColor2 = System.Drawing.Color.DimGray
        Me.AGauge1.NeedleRadius = 32
        Me.AGauge1.NeedleType = System.Windows.Forms.NeedleType.Advance
        Me.AGauge1.NeedleWidth = 2
        Me.AGauge1.ScaleLinesInterColor = System.Drawing.Color.Black
        Me.AGauge1.ScaleLinesInterInnerRadius = 40
        Me.AGauge1.ScaleLinesInterOuterRadius = 48
        Me.AGauge1.ScaleLinesInterWidth = 1
        Me.AGauge1.ScaleLinesMajorColor = System.Drawing.Color.Black
        Me.AGauge1.ScaleLinesMajorInnerRadius = 40
        Me.AGauge1.ScaleLinesMajorOuterRadius = 48
        Me.AGauge1.ScaleLinesMajorStepValue = 50.0!
        Me.AGauge1.ScaleLinesMajorWidth = 2
        Me.AGauge1.ScaleLinesMinorColor = System.Drawing.Color.Gray
        Me.AGauge1.ScaleLinesMinorInnerRadius = 42
        Me.AGauge1.ScaleLinesMinorOuterRadius = 48
        Me.AGauge1.ScaleLinesMinorTicks = 9
        Me.AGauge1.ScaleLinesMinorWidth = 1
        Me.AGauge1.ScaleNumbersColor = System.Drawing.Color.Black
        Me.AGauge1.ScaleNumbersFormat = Nothing
        Me.AGauge1.ScaleNumbersRadius = 60
        Me.AGauge1.ScaleNumbersRotation = 0
        Me.AGauge1.ScaleNumbersStartScaleLine = 0
        Me.AGauge1.ScaleNumbersStepScaleLines = 1
        Me.AGauge1.Size = New System.Drawing.Size(148, 130)
        Me.AGauge1.TabIndex = 0
        Me.AGauge1.Text = "AGauge1"
        Me.AGauge1.UnitValue1 = System.Windows.Forms.AGauge.UnitValue.Volts
        Me.AGauge1.UnitValue2 = System.Windows.Forms.AGauge.UnitValue.None
        Me.AGauge1.Value1 = 0!
        Me.AGauge1.Value2 = 0!
        '
        'btn_update
        '
        Me.btn_update.Location = New System.Drawing.Point(52, 246)
        Me.btn_update.Name = "btn_update"
        Me.btn_update.Size = New System.Drawing.Size(75, 23)
        Me.btn_update.TabIndex = 5
        Me.btn_update.Text = "Update"
        Me.btn_update.UseVisualStyleBackColor = True
        '
        'AGaugeTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(180, 281)
        Me.Controls.Add(Me.btn_update)
        Me.Controls.Add(Me.txt_maxValue)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txt_minValue)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.AGauge1)
        Me.Name = "AGaugeTest"
        Me.Text = "AGaugeTest"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents AGauge1 As AGauge
    Friend WithEvents Label1 As Label
    Friend WithEvents txt_minValue As TextBox
    Friend WithEvents txt_maxValue As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents btn_update As Button
End Class
