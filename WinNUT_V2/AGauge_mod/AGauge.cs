﻿// Copyright (C) 2007 A.J.Bauer
//
//  This software is provided as-is, without any express or implied
//  warranty.  In no event will the authors be held liable for any damages
//  arising from the use of this software.

//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//  1. The origin of this software must not be misrepresented; you must not
//     claim that you wrote the original software. if you use this software
//     in a product, an acknowledgment in the product documentation would be
//     appreciated but is not required.
//  2. Altered source versions must be plainly marked as such, and must not be
//     misrepresented as being the original software.
//  3. This notice may not be removed or altered from any source distribution.
//
// -----------------------------------------------------------------------------------
// Copyright (C) 2012 Code Artist
// 
// Added several improvement to original code created by A.J.Bauer.
// Visit: http://codearteng.blogspot.com for more information on change history.
//
// -----------------------------------------------------------------------------------

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace System.Windows.Forms
{
    /// <summary>
    /// <para>AGauge - Copyright (C) 2007 A.J.Bauer</para>
    /// <link>http://www.codeproject.com/Articles/17559/A-fast-and-performing-gauge</link>
    /// </summary>
    [ToolboxBitmap(typeof(AGauge), "AGauge.AGauge.bmp"),
    DefaultEvent("ValueInRangeChanged"),
    Description("Displays a value on an analog gauge. Raises an event if the value enters one of the definable ranges.")]
    public partial class AGauge : Control
    {
        #region Private Fields

        private float fontBoundY1;
        private float fontBoundY2;
        private Bitmap gaugeBitmap;
        private bool drawGaugeBackground = true;

        //Modified by Gawindx
        //private Single m_value1;
        private float m_value1;
        //Added By Gawindx
        private float m_value2;
        //private Boolean m_AutoSize = false;

        //private Point m_Center = new Point(100, 100);
        //Modified by gawindx
        private Point m_Center = new Point(74, 70);
        private float m_MinValue = 0;
        private float m_MaxValue = 100;

        private Color m_BaseArcColor = Color.Gray;
        //Modified by Gawindx
        //private Int32 m_BaseArcRadius = 80;
        private int m_BaseArcRadius = 45;
        private int m_BaseArcStart = 135;
        private int m_BaseArcSweep = 270;
        //Modified by Gawindx
        //private Int32 m_BaseArcStart = -135;
        //private Int32 m_BaseArcSweep = -45;
        private int m_BaseArcWidth = 5;

        private Color m_ScaleLinesInterColor = Color.Black;
        //Modified by Gawindx
        //private Int32 m_ScaleLinesInterInnerRadius = 73;
        //private Int32 m_ScaleLinesInterOuterRadius = 80;
        private int m_ScaleLinesInterInnerRadius = 40;
        private int m_ScaleLinesInterOuterRadius = 48;
        private int m_ScaleLinesInterWidth = 1;


        private int m_ScaleLinesMinorTicks = 9;
        private Color m_ScaleLinesMinorColor = Color.Gray;
        //Modified by Gawindx
        //private Int32 m_ScaleLinesMinorInnerRadius = 75;
        //private Int32 m_ScaleLinesMinorOuterRadius = 80
        private int m_ScaleLinesMinorInnerRadius = 42;
        private int m_ScaleLinesMinorOuterRadius = 48;
        private int m_ScaleLinesMinorWidth = 1;

        private float m_ScaleLinesMajorStepValue = 50.0f;
        private Color m_ScaleLinesMajorColor = Color.Black;
        //Modified by Gawindx
        //private Int32 m_ScaleLinesMajorInnerRadius = 70;
        //private Int32 m_ScaleLinesMajorOuterRadius = 80;
        private int m_ScaleLinesMajorInnerRadius = 40;
        private int m_ScaleLinesMajorOuterRadius = 48;
        private int m_ScaleLinesMajorWidth = 2;

        //Modified by Gawindx
        //private Int32 m_ScaleNumbersRadius = 95;
        private int m_ScaleNumbersRadius = 60;
        private Color m_ScaleNumbersColor = Color.Black;
        private string m_ScaleNumbersFormat;
        private int m_ScaleNumbersStartScaleLine;
        private int m_ScaleNumbersStepScaleLines = 1;
        private int m_ScaleNumbersRotation;

        private NeedleType m_NeedleType;
        //Modified by Gawindx
        //private Int32 m_NeedleRadius = 80;
        private int m_NeedleRadius = 32;
        private AGaugeNeedleColor m_NeedleColor1 = AGaugeNeedleColor.Gray;
        private Color m_NeedleColor2 = Color.DimGray;
        private int m_NeedleWidth = 2;

        //Added By Gawindx
        public enum GradientType
        {
            None,
            RedGreen
        }
        public enum GradientOrientation
        {
            UpToBottom,
            BottomToUp,
            RightToLeft,
            LeftToRight
        }
        public enum UnitValue
        {
            None,
            Hertz,
            Percent,
            Volts,
            Watts
        }

        private GradientType m_gradientType = GradientType.RedGreen;
        private GradientOrientation m_gradientOrientation = GradientOrientation.BottomToUp;
        private UnitValue m_unitvalue1 = UnitValue.Volts;
        private UnitValue m_unitvalue2 = UnitValue.None;
        #endregion

        #region EventHandler

        [Description("This event is raised when gauge value changed.")]
        public event EventHandler ValueChanged;
        private void OnValueChanged()
        {
            EventHandler e = ValueChanged;
            if (e != null) e(this, null);
        }

        [Description("This event is raised if the value is entering or leaving defined range.")]
        public event EventHandler<ValueInRangeChangedEventArgs> ValueInRangeChanged;
        private void OnValueInRangeChanged(AGaugeRange range, float value)
        {
            EventHandler<ValueInRangeChangedEventArgs> e = ValueInRangeChanged;
            if (e != null) e(this, new ValueInRangeChangedEventArgs(range, value, range.InRange));
        }

        #endregion

        #region Hidden and overridden inherited properties

        public new bool AllowDrop { get { return false; } set { /*Do Nothing */ } }
        //public new Boolean AutoSize { get { return false; } set { /*Do Nothing */ } }
        public new bool ForeColor { get { return false; } set { /*Do Nothing */ } }
        public new bool ImeMode { get { return false; } set { /*Do Nothing */ } }
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                drawGaugeBackground = true;
                Refresh();
            }
        }
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                drawGaugeBackground = true;
                Refresh();
            }
        }
        public override ImageLayout BackgroundImageLayout
        {
            get { return base.BackgroundImageLayout; }
            set
            {
                base.BackgroundImageLayout = value;
                drawGaugeBackground = true;
                Refresh();
            }
        }
        #endregion

        public AGauge()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            _GaugeRanges = new AGaugeRangeCollection(this);
            _GaugeLabels = new AGaugeLabelCollection(this);

            //Default Values
            //Size = new Size(205, 180);
            //Modified by Gawindx
            Size = new Size(148, 130);
        }

        #region Properties  
        [Browsable(true),
        Category("AGauge"),
        //Modified By Gawindx
        //System.ComponentModel.Description("Gauge value.")]
        Description("Gauge value1.")]
        public float Value1
        {
            get { return m_value1; }
            set
            {
                value = Math.Min(Math.Max(value, m_MinValue), m_MaxValue);
                if (m_value1 != value)
                {
                    m_value1 = value;
                    OnValueChanged();

                    if (this.DesignMode) drawGaugeBackground = true;

                    foreach (AGaugeRange ptrRange in _GaugeRanges)
                    {
                        if ((m_value1 >= ptrRange.StartValue)
                            && (m_value1 <= ptrRange.EndValue))
                        {
                            //Entering Range
                            if (!ptrRange.InRange)
                            {
                                ptrRange.InRange = true;
                                OnValueInRangeChanged(ptrRange, m_value1);
                            }
                        }
                        else
                        {
                            //Leaving Range
                            if (ptrRange.InRange)
                            {
                                ptrRange.InRange = false;
                                OnValueInRangeChanged(ptrRange, m_value1);
                            }
                        }
                    }
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("Auto size Mode of the gauge.")]
        public bool GaugeAutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("Gauge Ranges.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public AGaugeRangeCollection GaugeRanges { get { return _GaugeRanges; } }
        private AGaugeRangeCollection _GaugeRanges;

        [Browsable(true),
        Category("AGauge"),
        Description("Gauge Labels.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public AGaugeLabelCollection GaugeLabels { get { return _GaugeLabels; } }
        private AGaugeLabelCollection _GaugeLabels;

        #region << Gauge Base >>
        [Browsable(true),
        Category("AGauge"),
        Description("The center of the gauge (in the control's client area)."),
        //Modified By Gawindx
        //System.ComponentModel.DefaultValue(typeof(Point), "100, 100")]
        DefaultValue(typeof(Point), "148, 130")]
        public Point Center
        {
            get { return m_Center; }
            set
            {
                if (m_Center != value)
                {
                    m_Center = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        //Added By Gawindx
        [Browsable(true),
        Category("AGauge"),
        Description("UseColor For Arc Base Color.")]
        public GradientType GradientColor
        {
            get { return m_gradientType; }
            set
            {
                if (m_gradientType != value)
                {
                    m_gradientType = value;
                    Refresh();
                }
            }
        }

        //Added By Gawindx
        [Browsable(true),
        Category("AGauge"),
        Description("Orientation Of Gradient Colors.")]
        public GradientOrientation GradientColorOrientation
        {
            get { return m_gradientOrientation; }
            set
            {
                if (m_gradientOrientation != value)
                {
                    m_gradientOrientation = value;
                    Refresh();
                }
            }
        }

        //Added By Gawindx
        [Browsable(true),
        Category("AGauge"),
        Description("Units For Value 1")]
        public UnitValue UnitValue1
        {
            get { return m_unitvalue1; }
            set
            {
                if (m_unitvalue1 != value)
                {
                    m_unitvalue1 = value;
                    Refresh();
                }
            }
        }

        //Added By Gawindx
        [Browsable(true),
        Category("AGauge"),
        Description("UseColor For Arc Base Color.")]
        public UnitValue UnitValue2
        {
            get { return m_unitvalue2; }
            set
            {
                if (m_unitvalue2 != value)
                {
                    m_unitvalue2 = value;
                    Refresh();
                }
            }
        }

        //Added By Gawindx
        [Browsable(true),
        Category("AGauge"),
        Description("Second Value To Display.")]
        public float Value2
        {
            get { return m_value2; }
            set
            {
                if (m_value2 != value)
                {
                    m_value2 = value;
                    OnValueChanged();
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The color of the base arc.")]
        public Color BaseArcColor
        {
            get { return m_BaseArcColor; }
            set
            {
                if (m_BaseArcColor != value)
                {
                    m_BaseArcColor = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The radius of the base arc.")]
        public int BaseArcRadius
        {
            get { return m_BaseArcRadius; }
            set
            {
                if (m_BaseArcRadius != value)
                {
                    m_BaseArcRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The start angle of the base arc.")]
        public int BaseArcStart
        {
            get { return m_BaseArcStart; }
            set
            {
                if (m_BaseArcStart != value)
                {
                    m_BaseArcStart = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The sweep angle of the base arc.")]
        public int BaseArcSweep
        {
            get { return m_BaseArcSweep; }
            set
            {
                if (m_BaseArcSweep != value)
                {
                    m_BaseArcSweep = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The width of the base arc.")]
        public int BaseArcWidth
        {
            get { return m_BaseArcWidth; }
            set
            {
                if (m_BaseArcWidth != value)
                {
                    m_BaseArcWidth = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #region << Gauge Scale >>

        [Browsable(true),
        Category("AGauge"),
        Description("The minimum value to show on the scale.")]
        public float MinValue
        {
            get { return m_MinValue; }
            set
            {
                if ((m_MinValue != value) && (value < m_MaxValue))
                {
                    m_MinValue = value;
                    m_value1 = Math.Min(Math.Max(m_value1, m_MinValue), m_MaxValue);
                    m_ScaleLinesMajorStepValue = Math.Min(m_ScaleLinesMajorStepValue, m_MaxValue - m_MinValue);
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The maximum value to show on the scale.")]
        public float MaxValue
        {
            get { return m_MaxValue; }
            set
            {
                if ((m_MaxValue != value) && (value > m_MinValue))
                {
                    m_MaxValue = value;
                    m_value1 = Math.Min(Math.Max(m_value1, m_MinValue), m_MaxValue);
                    m_ScaleLinesMajorStepValue = Math.Min(m_ScaleLinesMajorStepValue, m_MaxValue - m_MinValue);
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The color of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public Color ScaleLinesInterColor
        {
            get { return m_ScaleLinesInterColor; }
            set
            {
                if (m_ScaleLinesInterColor != value)
                {
                    m_ScaleLinesInterColor = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The inner radius of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public int ScaleLinesInterInnerRadius
        {
            get { return m_ScaleLinesInterInnerRadius; }
            set
            {
                if (m_ScaleLinesInterInnerRadius != value)
                {
                    m_ScaleLinesInterInnerRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The outer radius of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public int ScaleLinesInterOuterRadius
        {
            get { return m_ScaleLinesInterOuterRadius; }
            set
            {
                if (m_ScaleLinesInterOuterRadius != value)
                {
                    m_ScaleLinesInterOuterRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The width of the inter scale lines which are the middle scale lines for an uneven number of minor scale lines.")]
        public int ScaleLinesInterWidth
        {
            get { return m_ScaleLinesInterWidth; }
            set
            {
                if (m_ScaleLinesInterWidth != value)
                {
                    m_ScaleLinesInterWidth = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The number of minor scale lines.")]
        public int ScaleLinesMinorTicks
        {
            get { return m_ScaleLinesMinorTicks; }
            set
            {
                if (m_ScaleLinesMinorTicks != value)
                {
                    m_ScaleLinesMinorTicks = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The color of the minor scale lines.")]
        public Color ScaleLinesMinorColor
        {
            get { return m_ScaleLinesMinorColor; }
            set
            {
                if (m_ScaleLinesMinorColor != value)
                {
                    m_ScaleLinesMinorColor = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The inner radius of the minor scale lines.")]
        public int ScaleLinesMinorInnerRadius
        {
            get { return m_ScaleLinesMinorInnerRadius; }
            set
            {
                if (m_ScaleLinesMinorInnerRadius != value)
                {
                    m_ScaleLinesMinorInnerRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The outer radius of the minor scale lines.")]
        public int ScaleLinesMinorOuterRadius
        {
            get { return m_ScaleLinesMinorOuterRadius; }
            set
            {
                if (m_ScaleLinesMinorOuterRadius != value)
                {
                    m_ScaleLinesMinorOuterRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The width of the minor scale lines.")]
        public int ScaleLinesMinorWidth
        {
            get { return m_ScaleLinesMinorWidth; }
            set
            {
                if (m_ScaleLinesMinorWidth != value)
                {
                    m_ScaleLinesMinorWidth = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The step value of the major scale lines.")]
        public float ScaleLinesMajorStepValue
        {
            get { return m_ScaleLinesMajorStepValue; }
            set
            {
                if ((m_ScaleLinesMajorStepValue != value) && (value > 0))
                {
                    m_ScaleLinesMajorStepValue = Math.Min(value, m_MaxValue - m_MinValue);
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The color of the major scale lines.")]
        public Color ScaleLinesMajorColor
        {
            get { return m_ScaleLinesMajorColor; }
            set
            {
                if (m_ScaleLinesMajorColor != value)
                {
                    m_ScaleLinesMajorColor = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The inner radius of the major scale lines.")]
        public int ScaleLinesMajorInnerRadius
        {
            get { return m_ScaleLinesMajorInnerRadius; }
            set
            {
                if (m_ScaleLinesMajorInnerRadius != value)
                {
                    m_ScaleLinesMajorInnerRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The outer radius of the major scale lines.")]
        public int ScaleLinesMajorOuterRadius
        {
            get { return m_ScaleLinesMajorOuterRadius; }
            set
            {
                if (m_ScaleLinesMajorOuterRadius != value)
                {
                    m_ScaleLinesMajorOuterRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The width of the major scale lines.")]
        public int ScaleLinesMajorWidth
        {
            get { return m_ScaleLinesMajorWidth; }
            set
            {
                if (m_ScaleLinesMajorWidth != value)
                {
                    m_ScaleLinesMajorWidth = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #region << Gauge Scale Numbers >>

        [Browsable(true),
        Category("AGauge"),
        Description("The radius of the scale numbers.")]
        public int ScaleNumbersRadius
        {
            get { return m_ScaleNumbersRadius; }
            set
            {
                if (m_ScaleNumbersRadius != value)
                {
                    m_ScaleNumbersRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The color of the scale numbers.")]
        public Color ScaleNumbersColor
        {
            get { return m_ScaleNumbersColor; }
            set
            {
                if (m_ScaleNumbersColor != value)
                {
                    m_ScaleNumbersColor = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The format of the scale numbers.")]
        public string ScaleNumbersFormat
        {
            get { return m_ScaleNumbersFormat; }
            set
            {
                if (m_ScaleNumbersFormat != value)
                {
                    m_ScaleNumbersFormat = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The number of the scale line to start writing numbers next to.")]
        public int ScaleNumbersStartScaleLine
        {
            get { return m_ScaleNumbersStartScaleLine; }
            set
            {
                if (m_ScaleNumbersStartScaleLine != value)
                {
                    m_ScaleNumbersStartScaleLine = Math.Max(value, 1);
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The number of scale line steps for writing numbers.")]
        public int ScaleNumbersStepScaleLines
        {
            get { return m_ScaleNumbersStepScaleLines; }
            set
            {
                if (m_ScaleNumbersStepScaleLines != value)
                {
                    m_ScaleNumbersStepScaleLines = Math.Max(value, 1);
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The angle relative to the tangent of the base arc at a scale line that is used to rotate numbers. set to 0 for no rotation or e.g. set to 90.")]
        public int ScaleNumbersRotation
        {
            get { return m_ScaleNumbersRotation; }
            set
            {
                if (m_ScaleNumbersRotation != value)
                {
                    m_ScaleNumbersRotation = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #region << Gauge Needle >>

        [Browsable(true),
        Category("AGauge"),
        Description("The type of the needle, currently only type 0 and 1 are supported. Type 0 looks nicers but if you experience performance problems you might consider using type 1.")]
        public NeedleType NeedleType
        {
            get { return m_NeedleType; }
            set
            {
                if (m_NeedleType != value)
                {
                    m_NeedleType = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The radius of the needle.")]
        public int NeedleRadius
        {
            get { return m_NeedleRadius; }
            set
            {
                if (m_NeedleRadius != value)
                {
                    m_NeedleRadius = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The first color of the needle.")]
        public AGaugeNeedleColor NeedleColor1
        {
            get { return m_NeedleColor1; }
            set
            {
                if (m_NeedleColor1 != value)
                {
                    m_NeedleColor1 = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The second color of the needle.")]
        public Color NeedleColor2
        {
            get { return m_NeedleColor2; }
            set
            {
                if (m_NeedleColor2 != value)
                {
                    m_NeedleColor2 = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        [Browsable(true),
        Category("AGauge"),
        Description("The width of the needle.")]
        public int NeedleWidth
        {
            get { return m_NeedleWidth; }
            set
            {
                if (m_NeedleWidth != value)
                {
                    m_NeedleWidth = value;
                    drawGaugeBackground = true;
                    Refresh();
                }
            }
        }

        #endregion

        #endregion

        #region Helper

        private void FindFontBounds()
        {
            //find upper and lower bounds for numeric characters
            int c1;
            int c2;
            bool boundfound;
            using (SolidBrush backBrush = new SolidBrush(Color.White))
            using (SolidBrush foreBrush = new SolidBrush(Color.Black))
            {
                using (Bitmap bmpMeasure = new Bitmap(5, 5))
                using (Graphics gMeasure = Graphics.FromImage(bmpMeasure))
                {
                    SizeF boundingBox = gMeasure.MeasureString("0123456789", Font, -1, StringFormat.GenericTypographic);
                    using (var b = new Bitmap((int)(boundingBox.Width), (int)(boundingBox.Height)))
                    using (var g = Graphics.FromImage(b))
                    {
                        g.FillRectangle(backBrush, 0.0F, 0.0F, boundingBox.Width, boundingBox.Height);
                        g.DrawString("0123456789", Font, foreBrush, 0.0F, 0.0F, StringFormat.GenericTypographic);

                        fontBoundY1 = 0;
                        fontBoundY2 = 0;
                        c1 = 0;
                        boundfound = false;
                        while ((c1 < b.Height) && (!boundfound))
                        {
                            c2 = 0;
                            while ((c2 < b.Width) && (!boundfound))
                            {
                                if (b.GetPixel(c2, c1) != backBrush.Color)
                                {
                                    fontBoundY1 = c1;
                                    boundfound = true;
                                }
                                c2++;
                            }
                            c1++;
                        }

                        c1 = b.Height - 1;
                        boundfound = false;
                        while ((0 < c1) && (!boundfound))
                        {
                            c2 = 0;
                            while ((c2 < b.Width) && (!boundfound))
                            {
                                if (b.GetPixel(c2, c1) != backBrush.Color)
                                {
                                    fontBoundY2 = c1;
                                    boundfound = true;
                                }
                                c2++;
                            }
                            c1--;
                        }
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public void RepaintControl()
        {
            drawGaugeBackground = true;
            Refresh();
        }

        #endregion

        #region Base member overrides

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if ((Width < 10) || (Height < 10))
            {
                return;
            }

            #region AutoSize
            float centerFactor = 1;
            var center = Center;


            if (AutoSize)
            {
                double widthFactor = ((1.0 / (double)(2 * Center.X)) * (double)Size.Width);
                double heightFactor = ((1.0 / (double)(2 * Center.Y)) * (double)Size.Height);
                centerFactor = (float)Math.Min(widthFactor, heightFactor);
                center = new Point((int)(Center.X * widthFactor), (int)(Center.Y * heightFactor));
            }
            #endregion

            #region drawGaugeBackground
            if (drawGaugeBackground)
            {
                drawGaugeBackground = false;

                FindFontBounds();

                if (gaugeBitmap != null)
                {
                    gaugeBitmap.Dispose();
                    gaugeBitmap = null;
                }

                gaugeBitmap = new Bitmap(Width, Height, e.Graphics);
                using (Graphics ggr = Graphics.FromImage(gaugeBitmap))
                using (GraphicsPath gp = new GraphicsPath())
                {
                    using (var brBackground = new SolidBrush(BackColor))
                    {
                        ggr.FillRectangle(brBackground, ClientRectangle);
                    }

                    #region BackgroundImage
                    if (BackgroundImage != null)
                    {
                        switch (BackgroundImageLayout)
                        {
                            case ImageLayout.Center:
                                ggr.DrawImageUnscaled(BackgroundImage, Width / 2 - BackgroundImage.Width / 2, Height / 2 - BackgroundImage.Height / 2);
                                break;
                            case ImageLayout.None:
                                ggr.DrawImageUnscaled(BackgroundImage, 0, 0);
                                break;
                            case ImageLayout.Stretch:
                                ggr.DrawImage(BackgroundImage, 0, 0, Width, Height);
                                break;
                            case ImageLayout.Tile:
                                int pixelOffsetX = 0;
                                int pixelOffsetY = 0;
                                while (pixelOffsetX < Width)
                                {
                                    pixelOffsetY = 0;
                                    while (pixelOffsetY < Height)
                                    {
                                        ggr.DrawImageUnscaled(BackgroundImage, pixelOffsetX, pixelOffsetY);
                                        pixelOffsetY += BackgroundImage.Height;
                                    }
                                    pixelOffsetX += BackgroundImage.Width;
                                }
                                break;
                            case ImageLayout.Zoom:
                                if ((float)(BackgroundImage.Width / Width) < (float)(BackgroundImage.Height / Height))
                                {
                                    ggr.DrawImage(BackgroundImage, 0, 0, Height, Height);
                                }
                                else
                                {
                                    ggr.DrawImage(BackgroundImage, 0, 0, Width, Width);
                                }
                                break;
                        }
                    }
                    #endregion

                    ggr.SmoothingMode = SmoothingMode.HighQuality;
                    ggr.PixelOffsetMode = PixelOffsetMode.HighQuality;


                    #region _GaugeRanges
                    float rangeStartAngle;
                    float rangeSweepAngle;
                    foreach (AGaugeRange ptrRange in _GaugeRanges)
                    {
                        if (ptrRange.EndValue > ptrRange.StartValue)
                        {
                            rangeStartAngle = m_BaseArcStart + (ptrRange.StartValue - m_MinValue) * m_BaseArcSweep / (m_MaxValue - m_MinValue);
                            rangeSweepAngle = (ptrRange.EndValue - ptrRange.StartValue) * m_BaseArcSweep / (m_MaxValue - m_MinValue);
                            gp.Reset();
                            int outerRadius = (int)(ptrRange.OuterRadius * centerFactor);
                            gp.AddPie(new Rectangle(center.X - outerRadius, center.Y - outerRadius,
                                2 * outerRadius, 2 * outerRadius), rangeStartAngle, rangeSweepAngle);
                            gp.Reverse();
                            int innerRadius = (int)(ptrRange.InnerRadius * centerFactor);
                            gp.AddPie(new Rectangle(center.X - innerRadius, center.Y - innerRadius,
                                2 * innerRadius, 2 * innerRadius), rangeStartAngle, rangeSweepAngle);
                            gp.Reverse();
                            ggr.SetClip(gp);
                            using (var brRange = new SolidBrush(ptrRange.Color))
                            {
                                ggr.FillPie(brRange, new Rectangle(center.X - outerRadius, center.Y - outerRadius, 2 * outerRadius, 2 * outerRadius), rangeStartAngle, rangeSweepAngle);
                            }
                        }
                    }
                    #endregion

                    ggr.SetClip(ClientRectangle);
                    if (m_BaseArcRadius > 0)
                    {
                        int baseArcRadius = (int)(m_BaseArcRadius * centerFactor);
                        if (m_gradientType != GradientType.None)
                        {
                            Point GradientP1Brush = new Point(0, (Center.X + baseArcRadius + m_BaseArcWidth + 2));
                            Point GradientP2Brush = new Point(0, (Center.X - baseArcRadius - m_BaseArcWidth - 2));
                            switch (m_gradientOrientation)
                            {
                                case GradientOrientation.UpToBottom:
                                    GradientP1Brush = new Point(0, (Center.X - baseArcRadius - m_BaseArcWidth - 2));
                                    GradientP2Brush = new Point(0, (Center.X + baseArcRadius + m_BaseArcWidth + 2));
                                    break;
                                case GradientOrientation.BottomToUp:
                                    GradientP1Brush = new Point(0, (Center.X + baseArcRadius + m_BaseArcWidth + 2));
                                    GradientP2Brush = new Point(0, (Center.X - baseArcRadius - m_BaseArcWidth - 2));
                                    break;
                                case GradientOrientation.RightToLeft:
                                    GradientP1Brush = new Point((Center.Y + baseArcRadius + m_BaseArcWidth + 2), 0);
                                    GradientP2Brush = new Point((Center.Y - baseArcRadius - m_BaseArcWidth - 2), 0);
                                    break;
                                case GradientOrientation.LeftToRight:
                                    GradientP1Brush = new Point((Center.Y - baseArcRadius - m_BaseArcWidth - 2), 0);
                                    GradientP2Brush = new Point((Center.Y + baseArcRadius + m_BaseArcWidth + 2), 0);
                                    break;
                            }
                            LinearGradientBrush myArc1Gradient = new LinearGradientBrush(GradientP1Brush, GradientP2Brush, Color.Red, Color.Green);
                            using (var pnArc = new Pen(myArc1Gradient, (int)(m_BaseArcWidth * centerFactor)))
                            {
                                ggr.DrawArc(pnArc, new Rectangle(center.X - baseArcRadius, center.Y - baseArcRadius, 2 * baseArcRadius, 2 * baseArcRadius), m_BaseArcStart, m_BaseArcSweep);
                            }
                        }
                        else
                        {
                            using (var pnArc = new Pen(m_BaseArcColor, (int)(m_BaseArcWidth * centerFactor)))
                            {
                                ggr.DrawArc(pnArc, new Rectangle(center.X - baseArcRadius, center.Y - baseArcRadius, 2 * baseArcRadius, 2 * baseArcRadius), m_BaseArcStart, m_BaseArcSweep);
                            }
                        }
                    }

                    #region ScaleNumbers
                    string valueText = "";
                    SizeF boundingBox;
                    float countValue = 0;
                    int counter1 = 0;
                    var Format = StringFormat.GenericTypographic;
                    Format.Alignment = StringAlignment.Near;

                    using (var pnMajorScaleLines = new Pen(m_ScaleLinesMajorColor, (int)(m_ScaleLinesMajorWidth * centerFactor)))
                    using (var brScaleNumbers = new SolidBrush(m_ScaleNumbersColor))
                    {
                        while (countValue <= (m_MaxValue - m_MinValue))
                        {
                            valueText = (m_MinValue + countValue).ToString(m_ScaleNumbersFormat);
                            ggr.ResetTransform();
                            boundingBox = ggr.MeasureString(valueText, Font, -1, StringFormat.GenericTypographic);

                            gp.Reset();
                            int scaleLinesMajorOuterRadius = (int)(m_ScaleLinesMajorOuterRadius * centerFactor);
                            gp.AddEllipse(new Rectangle(center.X - scaleLinesMajorOuterRadius, center.Y - scaleLinesMajorOuterRadius, 2 * scaleLinesMajorOuterRadius, 2 * scaleLinesMajorOuterRadius));
                            gp.Reverse();
                            int scaleLinesMajorInnerRadius = (int)(m_ScaleLinesMajorInnerRadius * centerFactor);
                            gp.AddEllipse(new Rectangle(center.X - scaleLinesMajorInnerRadius, center.Y - scaleLinesMajorInnerRadius, 2 * scaleLinesMajorInnerRadius, 2 * scaleLinesMajorInnerRadius));
                            gp.Reverse();
                            ggr.SetClip(gp);

                            ggr.DrawLine(pnMajorScaleLines,
                            (float)(center.X),
                            (float)(center.Y),
                            (float)(center.X + 2 * scaleLinesMajorOuterRadius * Math.Cos((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0)),
                            (float)(center.Y + 2 * scaleLinesMajorOuterRadius * Math.Sin((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0)));

                            gp.Reset();
                            int scaleLinesMinorOuterRadius = (int)(m_ScaleLinesMinorOuterRadius * centerFactor);
                            gp.AddEllipse(new Rectangle(center.X - scaleLinesMinorOuterRadius, center.Y - scaleLinesMinorOuterRadius, 2 * scaleLinesMinorOuterRadius, 2 * scaleLinesMinorOuterRadius));
                            gp.Reverse();
                            int scaleLinesMinorInnerRadius = (int)(m_ScaleLinesMinorInnerRadius * centerFactor);
                            gp.AddEllipse(new Rectangle(center.X - scaleLinesMinorInnerRadius, center.Y - scaleLinesMinorInnerRadius, 2 * scaleLinesMinorInnerRadius, 2 * scaleLinesMinorInnerRadius));
                            gp.Reverse();
                            ggr.SetClip(gp);

                            if (countValue < (m_MaxValue - m_MinValue))
                            {
                                using (var pnScaleLinesInter = new Pen(m_ScaleLinesInterColor, (int)(m_ScaleLinesInterWidth * centerFactor)))
                                using (var pnScaleLinesMinorColor = new Pen(m_ScaleLinesMinorColor, (int)(m_ScaleLinesMinorWidth * centerFactor)))
                                {
                                    for (int counter2 = 1; counter2 <= m_ScaleLinesMinorTicks; counter2++)
                                    {
                                        if (((m_ScaleLinesMinorTicks % 2) == 1) && (m_ScaleLinesMinorTicks / 2) + 1 == counter2)
                                        {
                                            gp.Reset();
                                            int scaleLinesInterOuterRadius = (int)(m_ScaleLinesInterOuterRadius * centerFactor);
                                            gp.AddEllipse(new Rectangle(center.X - scaleLinesInterOuterRadius, center.Y - scaleLinesInterOuterRadius, 2 * scaleLinesInterOuterRadius, 2 * scaleLinesInterOuterRadius));
                                            gp.Reverse();
                                            int scaleLinesInterInnerRadius = (int)(m_ScaleLinesInterInnerRadius * centerFactor);
                                            gp.AddEllipse(new Rectangle(center.X - scaleLinesInterInnerRadius, center.Y - scaleLinesInterInnerRadius, 2 * scaleLinesInterInnerRadius, 2 * scaleLinesInterInnerRadius));
                                            gp.Reverse();
                                            ggr.SetClip(gp);

                                            ggr.DrawLine(pnScaleLinesInter,
                                            (float)(center.X),
                                            (float)(center.Y),
                                            (float)(center.X + 2 * scaleLinesInterOuterRadius * Math.Cos((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue) + counter2 * m_BaseArcSweep / (((float)((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue)) * (m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)),
                                            (float)(center.Y + 2 * scaleLinesInterOuterRadius * Math.Sin((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue) + counter2 * m_BaseArcSweep / (((float)((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue)) * (m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)));

                                            gp.Reset();
                                            gp.AddEllipse(new Rectangle(center.X - scaleLinesMinorOuterRadius, center.Y - scaleLinesMinorOuterRadius, 2 * scaleLinesMinorOuterRadius, 2 * scaleLinesMinorOuterRadius));
                                            gp.Reverse();
                                            gp.AddEllipse(new Rectangle(center.X - scaleLinesMinorInnerRadius, center.Y - scaleLinesMinorInnerRadius, 2 * scaleLinesMinorInnerRadius, 2 * scaleLinesMinorInnerRadius));
                                            gp.Reverse();
                                            ggr.SetClip(gp);
                                        }
                                        else
                                        {
                                            ggr.DrawLine(pnScaleLinesMinorColor,
                                            (float)(center.X),
                                            (float)(center.Y),
                                            (float)(center.X + 2 * scaleLinesMinorOuterRadius * Math.Cos((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue) + counter2 * m_BaseArcSweep / (((float)((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue)) * (m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)),
                                            (float)(center.Y + 2 * scaleLinesMinorOuterRadius * Math.Sin((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue) + counter2 * m_BaseArcSweep / (((float)((m_MaxValue - m_MinValue) / m_ScaleLinesMajorStepValue)) * (m_ScaleLinesMinorTicks + 1))) * Math.PI / 180.0)));
                                        }
                                    }
                                }
                            }

                            ggr.SetClip(ClientRectangle);

                            if (m_ScaleNumbersRotation != 0)
                            {
                                ggr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                ggr.RotateTransform(90.0F + m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue));
                            }

                            ggr.TranslateTransform((float)(center.X + (m_ScaleNumbersRadius * centerFactor) * Math.Cos((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0f)),
                                                   (float)(center.Y + (m_ScaleNumbersRadius * centerFactor) * Math.Sin((m_BaseArcStart + countValue * m_BaseArcSweep / (m_MaxValue - m_MinValue)) * Math.PI / 180.0f)),
                                                   System.Drawing.Drawing2D.MatrixOrder.Append);


                            if (counter1 >= ScaleNumbersStartScaleLine - 1)
                            {
                                var ptText = new PointF((-boundingBox.Width / 2f), (-fontBoundY1 - (fontBoundY2 - fontBoundY1 + 1f) / 2f));
                                ggr.DrawString(valueText, Font, brScaleNumbers, ptText.X, ptText.Y, Format);
                            }

                            countValue += m_ScaleLinesMajorStepValue;
                            counter1++;
                        }
                    }
                    #endregion

                    ggr.ResetTransform();
                    ggr.SetClip(ClientRectangle);

                    if (m_ScaleNumbersRotation != 0)
                    {
                        ggr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                    }

                    #region _GaugeLabels
                    Format = StringFormat.GenericTypographic;
                    Format.Alignment = StringAlignment.Center;
                    foreach (AGaugeLabel ptrGaugeLabel in _GaugeLabels)
                    {
                        if (!string.IsNullOrEmpty(ptrGaugeLabel.Text))
                        {
                            using (var brGaugeLabel = new SolidBrush(ptrGaugeLabel.Color))
                            {
                                ggr.DrawString(ptrGaugeLabel.Text, ptrGaugeLabel.Font, brGaugeLabel,
                                               (ptrGaugeLabel.Position.X) * centerFactor + center.X,
                                               (ptrGaugeLabel.Position.Y) * centerFactor + center.Y, Format);
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            e.Graphics.DrawImageUnscaled(gaugeBitmap, 0, 0);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            #region Needle
            float brushAngle = (int)(m_BaseArcStart + (m_value1 - m_MinValue) * m_BaseArcSweep / (m_MaxValue - m_MinValue)) % 360;
            if (brushAngle < 0) brushAngle += 360;
            double needleAngle = brushAngle * Math.PI / 180;

            int needleWidth = (int)(m_NeedleWidth * centerFactor);
            int needleRadius = (int)(m_NeedleRadius * centerFactor);
            switch (m_NeedleType)
            {
                case NeedleType.Advance:
                    PointF[] points = new PointF[3];

                    int subcol = (int)(((brushAngle + 225) % 180) * 100 / 180);
                    int subcol2 = (int)(((brushAngle + 135) % 180) * 100 / 180);

                    using (var brNeedle = new SolidBrush(m_NeedleColor2))
                    {
                        e.Graphics.FillEllipse(brNeedle, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                    }

                    Color clr1 = Color.White;
                    Color clr2 = Color.White;
                    Color clr3 = Color.White;
                    Color clr4 = Color.White;

                    switch (m_NeedleColor1)
                    {
                        case AGaugeNeedleColor.Gray:
                            clr1 = Color.FromArgb(80 + subcol, 80 + subcol, 80 + subcol);
                            clr2 = Color.FromArgb(180 - subcol, 180 - subcol, 180 - subcol);
                            clr3 = Color.FromArgb(80 + subcol2, 80 + subcol2, 80 + subcol2);
                            clr4 = Color.FromArgb(180 - subcol2, 180 - subcol2, 180 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Gray, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                        case AGaugeNeedleColor.Red:
                            clr1 = Color.FromArgb(145 + subcol, subcol, subcol);
                            clr2 = Color.FromArgb(245 - subcol, 100 - subcol, 100 - subcol);
                            clr3 = Color.FromArgb(145 + subcol2, subcol2, subcol2);
                            clr4 = Color.FromArgb(245 - subcol2, 100 - subcol2, 100 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Red, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                        case AGaugeNeedleColor.Green:
                            clr1 = Color.FromArgb(subcol, 145 + subcol, subcol);
                            clr2 = Color.FromArgb(100 - subcol, 245 - subcol, 100 - subcol);
                            clr3 = Color.FromArgb(subcol2, 145 + subcol2, subcol2);
                            clr4 = Color.FromArgb(100 - subcol2, 245 - subcol2, 100 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Green, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                        case AGaugeNeedleColor.Blue:
                            clr1 = Color.FromArgb(subcol, subcol, 145 + subcol);
                            clr2 = Color.FromArgb(100 - subcol, 100 - subcol, 245 - subcol);
                            clr3 = Color.FromArgb(subcol2, subcol2, 145 + subcol2);
                            clr4 = Color.FromArgb(100 - subcol2, 100 - subcol2, 245 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Blue, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                        case AGaugeNeedleColor.Magenta:
                            clr1 = Color.FromArgb(subcol, 145 + subcol, 145 + subcol);
                            clr2 = Color.FromArgb(100 - subcol, 245 - subcol, 245 - subcol);
                            clr3 = Color.FromArgb(subcol2, 145 + subcol2, 145 + subcol2);
                            clr4 = Color.FromArgb(100 - subcol2, 245 - subcol2, 245 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Magenta, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                        case AGaugeNeedleColor.Violet:
                            clr1 = Color.FromArgb(145 + subcol, subcol, 145 + subcol);
                            clr2 = Color.FromArgb(245 - subcol, 100 - subcol, 245 - subcol);
                            clr3 = Color.FromArgb(145 + subcol2, subcol2, 145 + subcol2);
                            clr4 = Color.FromArgb(245 - subcol2, 100 - subcol2, 245 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Violet, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                        case AGaugeNeedleColor.Yellow:
                            clr1 = Color.FromArgb(145 + subcol, 145 + subcol, subcol);
                            clr2 = Color.FromArgb(245 - subcol, 245 - subcol, 100 - subcol);
                            clr3 = Color.FromArgb(145 + subcol2, 145 + subcol2, subcol2);
                            clr4 = Color.FromArgb(245 - subcol2, 245 - subcol2, 100 - subcol2);
                            e.Graphics.DrawEllipse(Pens.Violet, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                            break;
                    }

                    if (Math.Floor((float)(((brushAngle + 225) % 360) / 180.0)) == 0)
                    {
                        Color clrTemp = clr1;
                        clr1 = clr2;
                        clr2 = clrTemp;
                    }

                    if (Math.Floor((float)(((brushAngle + 135) % 360) / 180.0)) == 0)
                    {
                        clr4 = clr3;
                    }

                    using (Brush brush1 = new SolidBrush(clr1))
                    using (Brush brush2 = new SolidBrush(clr2))
                    using (Brush brush3 = new SolidBrush(clr3))
                    using (Brush brush4 = new SolidBrush(clr4))
                    {
                        points[0].X = (float)(center.X + needleRadius * Math.Cos(needleAngle));
                        points[0].Y = (float)(center.Y + needleRadius * Math.Sin(needleAngle));
                        points[1].X = (float)(center.X - needleRadius / 20 * Math.Cos(needleAngle));
                        points[1].Y = (float)(center.Y - needleRadius / 20 * Math.Sin(needleAngle));
                        points[2].X = (float)(center.X - needleRadius / 5 * Math.Cos(needleAngle) + needleWidth * 2 * Math.Cos(needleAngle + Math.PI / 2));
                        points[2].Y = (float)(center.Y - needleRadius / 5 * Math.Sin(needleAngle) + needleWidth * 2 * Math.Sin(needleAngle + Math.PI / 2));
                        e.Graphics.FillPolygon(brush1, points);

                        points[2].X = (float)(center.X - needleRadius / 5 * Math.Cos(needleAngle) + needleWidth * 2 * Math.Cos(needleAngle - Math.PI / 2));
                        points[2].Y = (float)(center.Y - needleRadius / 5 * Math.Sin(needleAngle) + needleWidth * 2 * Math.Sin(needleAngle - Math.PI / 2));
                        e.Graphics.FillPolygon(brush2, points);

                        points[0].X = (float)(center.X - (needleRadius / 20 - 1) * Math.Cos(needleAngle));
                        points[0].Y = (float)(center.Y - (needleRadius / 20 - 1) * Math.Sin(needleAngle));
                        points[1].X = (float)(center.X - needleRadius / 5 * Math.Cos(needleAngle) + needleWidth * 2 * Math.Cos(needleAngle + Math.PI / 2));
                        points[1].Y = (float)(center.Y - needleRadius / 5 * Math.Sin(needleAngle) + needleWidth * 2 * Math.Sin(needleAngle + Math.PI / 2));
                        points[2].X = (float)(center.X - needleRadius / 5 * Math.Cos(needleAngle) + needleWidth * 2 * Math.Cos(needleAngle - Math.PI / 2));
                        points[2].Y = (float)(center.Y - needleRadius / 5 * Math.Sin(needleAngle) + needleWidth * 2 * Math.Sin(needleAngle - Math.PI / 2));
                        e.Graphics.FillPolygon(brush4, points);

                        points[0].X = (float)(center.X - needleRadius / 20 * Math.Cos(needleAngle));
                        points[0].Y = (float)(center.Y - needleRadius / 20 * Math.Sin(needleAngle));
                        points[1].X = (float)(center.X + needleRadius * Math.Cos(needleAngle));
                        points[1].Y = (float)(center.Y + needleRadius * Math.Sin(needleAngle));

                        using (var pnNeedle = new Pen(m_NeedleColor2))
                        {
                            e.Graphics.DrawLine(pnNeedle, center.X, center.Y, points[0].X, points[0].Y);
                            e.Graphics.DrawLine(pnNeedle, center.X, center.Y, points[1].X, points[1].Y);
                        }
                    }
                    break;
                case NeedleType.Simple:
                    Point startPoint = new Point((int)(center.X - needleRadius / 8 * Math.Cos(needleAngle)),
                            (int)(center.Y - needleRadius / 8 * Math.Sin(needleAngle)));
                    Point endPoint = new Point((int)(center.X + needleRadius * Math.Cos(needleAngle)),
                                             (int)(center.Y + needleRadius * Math.Sin(needleAngle)));

                    using (var brDisk = new SolidBrush(m_NeedleColor2))
                    {
                        e.Graphics.FillEllipse(brDisk, center.X - needleWidth * 3, center.Y - needleWidth * 3, needleWidth * 6, needleWidth * 6);
                    }

                    using (var pnLine = new Pen(GetColor(m_NeedleColor1), needleWidth))
                    {
                        e.Graphics.DrawLine(pnLine, center.X, center.Y, endPoint.X, endPoint.Y);
                        e.Graphics.DrawLine(pnLine, center.X, center.Y, startPoint.X, startPoint.Y);
                    }
                    break;
            }
            #endregion

            #region DisplayValue
            var PenString = new Pen(Color.Black);
            Font PenFontV1 = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
            Font PenFontV2 = new Font("Microsoft Sans Serif", 7, FontStyle.Bold);
            SolidBrush StringPen = new SolidBrush(Color.Black);
            int LineHeight = 15;
            PointF StrPos = Center;
            StrPos.Y = StrPos.Y + 5;
            if (UnitValue1 != UnitValue.None)
            {
                string StringToDraw = "";
                switch (UnitValue1)
                {
                    case UnitValue.Hertz:
                        StringToDraw = $"{Value1.ToString()} Hz";
                        break;
                    case UnitValue.Percent:
                        StringToDraw = $"{Value1.ToString()} %";
                        break;
                    case UnitValue.Volts:
                        StringToDraw = $"{Value1.ToString()} V";
                        break;
                    case UnitValue.Watts:
                        StringToDraw = $"{Value1.ToString()} W";
                        break;
                }
                Size StringSize = TextRenderer.MeasureText(StringToDraw, PenFontV1);
                StrPos.Y = StrPos.Y + LineHeight;
                e.Graphics.DrawString(StringToDraw, PenFontV1, StringPen, new PointF((StrPos.X - (StringSize.Width / 2) + 5), StrPos.Y));
            }
            if (UnitValue2 != UnitValue.None)
            {
                string StringToDraw = "";
                switch (UnitValue2)
                {
                    case UnitValue.Hertz:
                        StringToDraw = $"{Value2.ToString()} Hz";
                        break;
                    case UnitValue.Percent:
                        StringToDraw = $"{Value2.ToString()} %";
                        break;
                    case UnitValue.Volts:
                        StringToDraw = $"{Value2.ToString()} V";
                        break;
                    case UnitValue.Watts:
                        StringToDraw = $"{Value2.ToString()} W";
                        break;
                }
                Size StringSize = TextRenderer.MeasureText(StringToDraw, PenFontV2);
                StrPos.Y = StrPos.Y + LineHeight;
                e.Graphics.DrawString(StringToDraw, PenFontV2, StringPen, new PointF((StrPos.X - (StringSize.Width / 2) + 7), StrPos.Y));
            }

            #endregion
        }

        private Color GetColor(AGaugeNeedleColor clr)
        {
            switch (clr)
            {
                case AGaugeNeedleColor.Gray:
                    return Color.DarkGray;
                case AGaugeNeedleColor.Red:
                    return Color.Red;
                case AGaugeNeedleColor.Green:
                    return Color.Green;
                case AGaugeNeedleColor.Blue:
                    return Color.Blue;
                case AGaugeNeedleColor.Yellow:
                    return Color.Yellow;
                case AGaugeNeedleColor.Violet:
                    return Color.Violet;
                case AGaugeNeedleColor.Magenta:
                    return Color.Magenta;
                default:
                    Debug.Fail("Missing enumeration");
                    return Color.DarkGray;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            drawGaugeBackground = true;
            Refresh();
        }
        #endregion
    }

    #region [ Gauge Enum ]

    /// <summary>
    /// First needle color
    /// </summary>
    public enum AGaugeNeedleColor
    {
        Gray = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4,
        Violet = 5,
        Magenta = 6
    };

    public enum NeedleType
    {
        Advance,
        Simple
    }

    #endregion

    #region [ EventArgs ]
    /// <summary>
    /// Event argument for <see cref="ValueInRangeChanged"/> event.
    /// </summary>
    public class ValueInRangeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Affected GaugeRange
        /// </summary>
        public AGaugeRange Range { get; private set; }
        /// <summary>
        /// Gauge Value
        /// </summary>
        public float Value { get; private set; }
        /// <summary>
        /// True if value is within current range.
        /// </summary>
        public bool InRange { get; private set; }
        public ValueInRangeChangedEventArgs(AGaugeRange range, float value, bool inRange)
        {
            this.Range = range;
            this.Value = value;
            this.InRange = inRange;
        }
    }
    #endregion

    [Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc { } //Namespace Documentation
}
