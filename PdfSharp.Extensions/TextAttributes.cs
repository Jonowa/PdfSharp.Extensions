using System;
using System.Drawing;

namespace PdfSharp.Extensions
{
    using Drawing;

    public enum TextAlign
    {
        Near,
        Left,
        Center,
        Far,
        Right,
        Justify
    }

    public class TextAttributes
    {
        /// <summary>
        /// X-coordinate
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Y-coordinate
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Line height
        /// </summary>
        public double LineHeight { get; set; }

        /// <summary>
        /// Kerning (letter spacing)
        /// </summary>
        public int Kerning { get; set; }

        /// <summary>
        /// Opacity (0 to 100)
        /// </summary>
        public int Opacity { get; set; }

        /// <summary>
        /// Brush color
        /// </summary>
        public XColor Color { get; set; }

        /// <summary>
        /// Text alignment
        /// </summary>
        public TextAlign Align { get; set; }

        /// <summary>
        /// Rotation angle (-90 to 90)
        /// </summary>
        public double Angle { get; set; }

        internal XBrush Brush => new XSolidBrush(GetColor());

        public TextAttributes()
        {
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
            LineHeight = 0;
            Kerning = 0;
            Opacity = 100;
            Color = XColors.Black;
            Align = TextAlign.Near;
        }

        public TextAttributes(XBrush brush, XPoint point) : base()
        {
            Left = point.X;
            Top = point.Y;

            XBrushToColor(brush);
        }

        public TextAttributes(XBrush brush, XPoint point, XStringFormat format) : base()
        {
            Left = point.X;
            Top = point.Y;

            XBrushToColor(brush);
            FormatToAlign(format);
        }

        public TextAttributes(XBrush brush, XRect rectangle) : base()
        {
            Left = rectangle.X;
            Top = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;

            XBrushToColor(brush);
        }

        public TextAttributes(XBrush brush, XRect rectangle, XStringFormat format) : base()
        {
            Left = rectangle.X;
            Top = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;

            XBrushToColor(brush);
            FormatToAlign(format);
        }

        private XColor GetColor()
        {
            return XColor.FromArgb((int)(Opacity * 2.55), Color.R, Color.G, Color.B);
        }

        private void XBrushToColor(XBrush brush)
        {
            if (brush.GetType() == typeof(XSolidBrush))
            {
                Color = (brush as XSolidBrush).Color;
                Opacity = (int)(Color.A * 100);
            }
            else
            {
                Color = XColors.Black;
                Opacity = 100;
            }
        }

        private void FormatToAlign(XStringFormat format)
        {
            switch (format.Alignment)
            {
                case XStringAlignment.Center:
                    Align = TextAlign.Center;
                    break;

                case XStringAlignment.Far:
                    Align = TextAlign.Far;
                    break;

                default:
                    Align = TextAlign.Near;
                    break;
            }
        }
    }
}
