using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if GDI
using System.Drawing;
#endif

namespace PdfSharp.Extensions
{
    using Drawing;

    internal class Letter
    {
        public string Value { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public bool LineBreak { get; private set; }
        public XFont Font { get; private set; }
        public double Baseline { get; private set; }
        public XBrush Brush { get; private set; }

        public Letter(XGraphics graphics, char letter, XFont font, TextAttributes attributes, double baseline = 0)
        {
            Value = letter.ToString();
            Height = font.GetHeight();
            LineBreak = false;
            Font = font;
            Brush = attributes.Brush;
            Baseline = baseline;

            Width = graphics.MeasureString(Value, font).Width;

            if (attributes.Kerning != 0)
            {
                double geviert = graphics.MeasureString('\u2014'.ToString(), font).Width;
                Width -= (double)attributes.Kerning / 1000 * geviert;
            }
        }

        public void AddOffset(double offset)
        {
            if (Value == " ")
                Width += offset;
        }

        public void AddLineBreak()
        {
            LineBreak = true;
        }
    }
}
