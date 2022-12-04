' WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
' Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
'
' This program is free software: you can redistribute it and/or modify it under the terms of the
' GNU General Public License as published by the Free Software Foundation, either version 3 of the
' License, or any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

Imports System.ComponentModel

Namespace Controls
    Public Class UPSVarGauge
        Inherits AGauge

#Region "Private Fields"

        Private m_value1 As Single
        Private m_value2 As Single

        Private m_Center = New Point(74, 70)
        Private m_MinValue As Single = 0
        Private m_MaxValue As Single = 100

        Private m_BaseArcRadius = 45
        Private m_BaseArcStart = 135
        Private m_BaseArcSweep = 270
        Private m_BaseArcWidth = 5

        Private m_ScaleLinesInterInnerRadius = 40
        Private m_ScaleLinesInterOuterRadius = 48
        Private m_ScaleLinesInterWidth = 1

        Private m_ScaleLinesMinorTicks = 9
        Private m_ScaleLinesMinorInnerRadius = 42
        Private m_ScaleLinesMinorOuterRadius = 48
        Private m_ScaleLinesMinorWidth = 1

        Private m_ScaleLinesMajorStepValue = 50.0F
        Private m_ScaleLinesMajorInnerRadius = 40
        Private m_ScaleLinesMajorOuterRadius = 48
        Private m_ScaleLinesMajorWidth = 2

        Private m_ScaleNumbersRadius = 60
        Private m_ScaleNumbersFormat As String
        Private m_ScaleNumbersStartScaleLine As Integer
        Private m_ScaleNumbersStepScaleLines = 1
        Private m_ScaleNumbersRotation As Integer

        Private m_NeedleType As NeedleType
        Private m_NeedleRadius = 32
        Private m_NeedleColor1 = AGaugeNeedleColor.Gray
        Private m_NeedleColor2 = Color.DimGray
        Private m_NeedleWidth = 2

        Private m_gradientType = GradientType.RedGreen
        Private m_gradientOrientation = GradientOrientation.BottomToUp
        Private m_unitvalue1 = UnitValue.Volts
        Private m_unitvalue2 = UnitValue.None

#End Region

#Region "Properties"

        ' Hide the old Value property
        Private Overloads Property Value As Single
            Get
                Return Nothing
            End Get
            Set(value As Single)

            End Set
        End Property

        <Browsable(True),
    Category("UPSVarGauge"),
    Description("First gauge value.")>
        Public Property Value1 As Single
            Get
                Return m_value1
            End Get
            Set(value As Single)
                value = Math.Min(Math.Max(value, m_MinValue), m_MaxValue)
                If m_value1 <> value Then

                End If
            End Set

        End Property


#End Region

        Public Enum GradientType
            None
            RedGreen
        End Enum

        Public Enum GradientOrientation
            UpToBottom
            BottomToUp
            RightToLeft
            LeftToRight
        End Enum

        Public Enum UnitValue
            None
            Hertz
            Percent
            Volts
            Watts
        End Enum

        Public Sub New()
            MyBase.New()

            Size = New Size(148, 130)
        End Sub

        'Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        '    MyBase.OnPaint(e)

        '    'Add your custom paint code here
        'End Sub

    End Class
End Namespace
