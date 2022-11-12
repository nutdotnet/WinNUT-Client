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
    public class AGaugeLabel
    {
        [Browsable(true),
        Category("Design"),
        DisplayName("(Name)"),
        Description("Instance Name.")]
        public string Name { get; set; }

        private AGauge Owner;
        [Browsable(false)]
        public void SetOwner(AGauge value) { Owner = value; }
        private void NotifyOwner() { if (Owner != null) Owner.RepaintControl(); }

        [Browsable(true),
        Category("Appearance"),
        Description("The color of the caption text.")]
        public Color Color { get { return _Color; } set { _Color = value; NotifyOwner(); } }
        private Color _Color = Color.FromKnownColor(KnownColor.WindowText);

        [Browsable(true),
        Category("Appearance"),
        Description("The text of the caption.")]
        public string Text { get { return _Text; } set { _Text = value; NotifyOwner(); } }
        private string _Text;

        [Browsable(true),
        Category("Appearance"),
        Description("The position of the caption.")]
        public Point Position { get { return _Position; } set { _Position = value; NotifyOwner(); } }
        private Point _Position;

        [Browsable(true),
        Category("Appearance"),
        Description("Font of Text.")]
        public Font Font { get { return _Font; } set { _Font = value; NotifyOwner(); } }
        private Font _Font = DefaultFont;

        public void ResetFont() { _Font = DefaultFont; }
        private bool ShouldSerializeFont() { return (_Font != DefaultFont); }
        private static Font DefaultFont = System.Windows.Forms.Control.DefaultFont;
    }

    public class AGaugeLabelCollection : CollectionBase
    {
        private AGauge Owner;
        public AGaugeLabelCollection(AGauge sender) { Owner = sender; }

        public AGaugeLabel this[int index] { get { return (AGaugeLabel)List[index]; } }
        public bool Contains(AGaugeLabel itemType) { return List.Contains(itemType); }
        public int Add(AGaugeLabel itemType)
        {
            itemType.SetOwner(Owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            var ret = List.Add(itemType);
            if (Owner != null) Owner.RepaintControl();
            return ret;
        }
        public void Remove(AGaugeLabel itemType)
        {
            List.Remove(itemType);
            if (Owner != null) Owner.RepaintControl();
        }
        public void Insert(int index, AGaugeLabel itemType)
        {
            itemType.SetOwner(Owner);
            if (string.IsNullOrEmpty(itemType.Name)) itemType.Name = GetUniqueName();
            List.Insert(index, itemType);
            if (Owner != null) Owner.RepaintControl();
        }
        public int IndexOf(AGaugeLabel itemType) { return List.IndexOf(itemType); }
        public AGaugeLabel FindByName(string name)
        {
            foreach (AGaugeLabel ptrRange in List)
            {
                if (ptrRange.Name == name) return ptrRange;
            }
            return null;
        }

        protected override void OnInsert(int index, object value)
        {
            if (string.IsNullOrEmpty(((AGaugeLabel)value).Name)) ((AGaugeLabel)value).Name = GetUniqueName();
            base.OnInsert(index, value);
            ((AGaugeLabel)value).SetOwner(Owner);
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
            const string Prefix = "GaugeLabel";
            int index = 1;
            while (this.Count != 0)
            {
                for (int x = 0; x < this.Count; x++)
                {
                    if (this[x].Name == (Prefix + index.ToString()))
                        continue;
                    else
                        return Prefix + index.ToString();
                }
                index++;
            };
            return Prefix + index.ToString();
        }
    }
}
