// WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.
// Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
//
// This program is free software: you can redistribute it and/or modify it under the terms of the
// GNU General Public License as published by the Free Software Foundation, either version 3 of the
// License, or any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY

using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    public class AGaugeRange
    {
        public AGaugeRange() { }

        public AGaugeRange(Color color, Single startValue, Single endValue)
        {
            Color = color;
            _StartValue = startValue;
            _EndValue = endValue;
        }

        public AGaugeRange(Color color, Single startValue, Single endValue, Int32 innerRadius, Int32 outerRadius)
        {
            Color = color;
            _StartValue = startValue;
            _EndValue = endValue;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
        }

        [Browsable(true),
        Category("Design"),
        DisplayName("(Name)"),
        Description("Instance Name.")]
        public string Name { get; set; }

        [Browsable(false)]
        public Boolean InRange { get; set; }

        private AGauge Owner;
        [Browsable(false)]
        public void SetOwner(AGauge value) { Owner = value; }
        private void NotifyOwner() { if (Owner != null) Owner.RepaintControl(); }

        [Browsable(true),
        Category("Appearance"),
        Description("The color of the range.")]
        public Color Color { get { return _Color; } set { _Color = value; NotifyOwner(); } }
        private Color _Color;

        [Browsable(true),
        Category("Limits"),
        Description("The start value of the range, must be less than RangeEndValue.")]
        public Single StartValue
        {
            get { return _StartValue; }
            set
            {
                if (Owner != null)
                {
                    if (value < Owner.MinValue) value = Owner.MinValue;
                    if (value > Owner.MaxValue) value = Owner.MaxValue;
                }
                _StartValue = value; NotifyOwner();
            }
        }
        private Single _StartValue;

        [Browsable(true),
        Category("Limits"),
        Description("The end value of the range. Must be greater than RangeStartValue.")]
        public Single EndValue
        {
            get { return _EndValue; }
            set
            {
                if (Owner != null)
                {
                    if (value < Owner.MinValue) value = Owner.MinValue;
                    if (value > Owner.MaxValue) value = Owner.MaxValue;
                }
                _EndValue = value; NotifyOwner();
            }
        }
        private Single _EndValue;

        [Browsable(true),
        Category("Appearance"),
        Description("The inner radius of the range.")]
        public Int32 InnerRadius
        {
            get { return _InnerRadius; }
            set { if (value > 0) { _InnerRadius = value; NotifyOwner(); } }
        }
        private Int32 _InnerRadius = 70;

        [Browsable(true),
        Category("Appearance"),
        Description("The outer radius of the range.")]
        public Int32 OuterRadius
        {
            get { return _OuterRadius; }
            set { if (value > 0) { _OuterRadius = value; NotifyOwner(); } }
        }
        private Int32 _OuterRadius = 80;
    }

    public class AGaugeRangeCollection : CollectionBase
    {
        private AGauge Owner;
        public AGaugeRangeCollection(AGauge sender) { Owner = sender; }

        public AGaugeRange this[int index] { get { return (AGaugeRange)List[index]; } }
        public bool Contains(AGaugeRange itemType) { return List.Contains(itemType); }
        public int Add(AGaugeRange itemType)
        {
            itemType.SetOwner(Owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            var ret = List.Add(itemType);
            if (Owner != null) Owner.RepaintControl();
            return ret;
        }
        public void Remove(AGaugeRange itemType)
        {
            List.Remove(itemType);
            if (Owner != null) Owner.RepaintControl();
        }
        public void Insert(int index, AGaugeRange itemType)
        {
            itemType.SetOwner(Owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            List.Insert(index, itemType);
            if (Owner != null) Owner.RepaintControl();
        }
        public int IndexOf(AGaugeRange itemType) { return List.IndexOf(itemType); }
        public AGaugeRange FindByName(string name)
        {
            foreach (AGaugeRange ptrRange in List)
            {
                if (ptrRange.Name == name) return ptrRange;
            }
            return null;
        }

        protected override void OnInsert(int index, object value)
        {
            if (string.IsNullOrEmpty(((AGaugeRange)value).Name)) ((AGaugeRange)value).Name = GetUniqueName();
            base.OnInsert(index, value);
            ((AGaugeRange)value).SetOwner(Owner);
        }
        protected override void OnRemove(int index, object value)
        {
            if (Owner != null) Owner.RepaintControl();
        }
        protected override void OnClear()
        {
            if (Owner != null) Owner.RepaintControl();
        }

        private string GetUniqueName()
        {
            const string Prefix = "GaugeRange";
            int index = 1;
            bool valid;
            while (this.Count != 0)
            {
                valid = true;
                for (int x = 0; x < this.Count; x++)
                {
                    if (this[x].Name == (Prefix + index.ToString()))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid) break;
                index++;
            };
            return Prefix + index.ToString();
        }
    }
}
