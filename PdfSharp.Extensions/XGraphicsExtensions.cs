using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;

namespace PdfSharp.Extensions
{
    using Drawing;

    /// <summary>
    /// 
    /// </summary>
    public static class XGraphicsExtensions
    {
        /// <summary>
        /// This alternative to <see cref="XGraphics.DrawString(string, XFont, XBrush, XPoint)"/>
        /// supports some HTML tags line breaks, line height, font kerning, text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, XBrush brush, XPoint point)
        {
            return g.DrawText(text, font, new TextAttributes(brush, point));
        }

        /// <summary>
        /// This alternative to <see cref="XGraphics.DrawString(string, XFont, XBrush, XPoint, XStringFormat)"/>
        /// supports some HTML tags line breaks, line height, font kerning, text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, XBrush brush, XPoint point, XStringFormat format)
        {
            return g.DrawText(text, font, new TextAttributes(brush, point, format));
        }

        /// <summary>
        /// This alternative to <see cref="XGraphics.DrawString(string, XFont, XBrush, XRect)"/>
        /// supports some HTML tags line breaks, line height, font kerning, text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, XBrush brush, XRect rect)
        {
            return g.DrawText(text, font, new TextAttributes(brush, rect));
        }

        /// <summary>
        /// This alternative to <see cref="XGraphics.DrawString(string, XFont, XBrush, XRect, XStringFormat)"/>
        /// supports some HTML tags line breaks, line height, font kerning, text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, XBrush brush, XRect rect, XStringFormat format)
        {
            return g.DrawText(text, font, new TextAttributes(brush, rect, format));
        }

        /// <summary>
        /// This alternative to <see cref="XGraphics.DrawString(string, XFont, XBrush, double, double)"/>
        /// supports some HTML tags line breaks, line height, font kerning, text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, XBrush brush, double x, double y)
        {
            return g.DrawText(text, font, new TextAttributes(brush, new XPoint(x, y)));
        }

        /// <summary>
        /// This alternative to <see cref="XGraphics.DrawString(string, XFont, XBrush, double, double, XStringFormat)"/>
        /// supports some HTML tags line breaks, line height, font kerning, text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, XBrush brush, double x, double y, XStringFormat format)
        {
            return g.DrawText(text, font, new TextAttributes(brush, new XPoint(x, y), format));
        }

        /// <summary>
        /// This function supports some HTML tags line breaks, line height, font kerning,
        /// text alignment and opacity.
        /// </summary>
        public static double DrawText(this XGraphics g, string text, XFont font, TextAttributes attributes)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            _ = font ?? throw new ArgumentNullException(nameof(font));
            _ = attributes ?? throw new ArgumentNullException(nameof(attributes));

            text = text.Replace("<br>", "\n");

            double baseline = 0;
            string htmlTag = "";
            bool addToTag = false;
            List<Letter> word = new List<Letter>();
            List<Letter> letters = new List<Letter>();
            List<Letter[]> lines = new List<Letter[]>();
            string[] tag;

            bool subText = false;
            bool supText = false;
            double top = attributes.Top;

            XFont resetFont = font;
            XColor resetColor = attributes.Color;

            if (attributes.Width == 0)
            {
                attributes.Width = g.PageSize.Width - attributes.Left;
            }

            if (attributes.LineHeight == 0)
            {
                attributes.LineHeight = font.GetHeight();
            }

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '<':
                        addToTag = true;
                        htmlTag = "";
                        break;

                    case '>':
                        addToTag = false;
                        tag = htmlTag.Split('=');
                        int idx = _supportedHtmlTags.ToList().IndexOf(tag[0].ToLower());
                        switch (idx)
                        {
                            case 0: // b = bold ON
                                font = new XFont(font.Name, font.Size, font.Style | XFontStyle.Bold);
                                break;

                            case 1: // i = italic ON
                                font = new XFont(font.Name, font.Size, font.Style | XFontStyle.Italic);
                                break;

                            case 2: // u = underline ON
                                font = new XFont(font.Name, font.Size, font.Style | XFontStyle.Underline);
                                break;

                            case 3: // s = strikeout ON
                                font = new XFont(font.Name, font.Size, font.Style | XFontStyle.Strikeout);
                                break;

                            case 4: // color = change color ON
                                if (tag.Length != 2) break;

                                var color = tag[1].Trim('\'', '#');
                                if (System.Text.RegularExpressions.Regex.IsMatch(color, "^[a-fA-F0-9]{6}$"))
                                {
                                    int red = int.Parse($"{color[0]}{color[1]}", NumberStyles.AllowHexSpecifier);
                                    int green = int.Parse($"{color[2]}{color[3]}", NumberStyles.AllowHexSpecifier);
                                    int blue = int.Parse($"{color[4]}{color[5]}", NumberStyles.AllowHexSpecifier);
                                    attributes.Color = XColor.FromArgb(red, green, blue);
                                }
                                else if (System.Text.RegularExpressions.Regex.IsMatch(color, "^[a-fA-F0-9]{3}$"))
                                {
                                    int red = int.Parse($"{color[0]}{color[0]}", NumberStyles.AllowHexSpecifier);
                                    int green = int.Parse($"{color[1]}{color[1]}", NumberStyles.AllowHexSpecifier);
                                    int blue = int.Parse($"{color[2]}{color[2]}", NumberStyles.AllowHexSpecifier);
                                    attributes.Color = XColor.FromArgb(red, green, blue);
                                }
                                else
                                {
                                    var systemColor = Color.FromName(color);
                                    attributes.Color = XColor.FromArgb(systemColor.R, systemColor.G, systemColor.B);
                                }
                                break;

                            case 5: // size = change font size ON
                                if (tag.Length != 2) break;

                                var size = tag[1].Trim('\'');
                                XUnit s = new XUnit(double.Parse(size));
                                s.ConvertType(g.PageUnit);
                                font = new XFont(font.Name, s.Value, font.Style);
                                break;

                            case 6: // sub = sub-script ON
                                if (!subText)
                                {
                                    font = new XFont(font.Name, font.Size * .8);
                                    baseline = font.Size * .175;
                                }
                                subText = true;
                                break;

                            case 7: // sup = superscript ON
                                if (!supText)
                                {
                                    font = new XFont(font.Name, font.Size * .8);
                                    baseline = -(font.Size * .33);
                                }
                                supText = true;
                                break;

                            case 8: // /b = bold OFF
                                font = new XFont(font.Name, font.Size, font.Style & ~XFontStyle.Bold);
                                break;

                            case 9: // /i = italic OFF
                                font = new XFont(font.Name, font.Size, font.Style & ~XFontStyle.Italic);
                                break;

                            case 10: // /u = underline OFF
                                font = new XFont(font.Name, font.Size, font.Style & ~XFontStyle.Underline);
                                break;

                            case 11: // /s = strikeout OFF
                                font = new XFont(font.Name, font.Size, font.Style & ~XFontStyle.Strikeout);
                                break;

                            case 12: // /color = reset color
                                attributes.Color = resetColor;
                                break;

                            case 13: // /size = reset font size
                                font = resetFont;
                                break;

                            case 14: // /sub = sub-script OFF
                                if (subText)
                                {
                                    font = new XFont(font.Name, font.Size / .8);
                                    baseline = 0;
                                }
                                subText = false;
                                break;

                            case 15: // /sup = superscript OFF
                                if (supText)
                                {
                                    font = new XFont(font.Name, font.Size / .8);
                                    baseline = 0;
                                }
                                supText = false;
                                break;

                            default:
                                htmlTag += c;
                                foreach (char t in htmlTag)
                                {
                                    word.Add(new Letter(g, t, font, attributes, baseline));
                                }
                                break;
                        }
                        htmlTag = "";
                        break;

                    case ' ':
                    case '-':
                        if (addToTag)
                        {
                            htmlTag += c;
                            break;
                        }

                        word.Add(new Letter(g, c, font, attributes, baseline));

                        if (GetTextWidth(letters) + GetTextWidth(word) >= attributes.Width)
                        {
                            lines.Add(letters.ToArray());
                            letters.Clear();
                        }

                        letters.AddRange(word);
                        word.Clear();
                        break;

                    case '\r':
                    case '\n':
                        if (c == '\r')
                        {
                            if (i < text.Length - 1 && text[i + 1] == '\n')
                            {
                                i++;
                            }
                        }

                        if (word.Count == 0)
                        {
                            word.Add(new Letter(g, ' ', font, attributes, baseline));
                        }
                        
                        word[word.Count - 1].AddLineBreak();

                        if (GetTextWidth(letters) + GetTextWidth(word) >= attributes.Width)
                        {
                            lines.Add(letters.ToArray());
                            letters.Clear();
                        }

                        letters.AddRange(word);
                        lines.Add(letters.ToArray());
                        letters.Clear();
                        word.Clear();
                        break;

                    default:
                        if (addToTag)
                        {
                            tag = (htmlTag + c).ToLower().Split('=');
                            if (_supportedHtmlTags.Any(a => a.Length >= tag[0].Length && a.StartsWith(tag[0])))
                            {
                                htmlTag += c;
                                break;
                            }
                            addToTag = false;
                            htmlTag = "<" + htmlTag;
                            foreach (char t in htmlTag)
                            {
                                word.Add(new Letter(g, t, font, attributes, baseline));
                            }
                            htmlTag = "";
                        }

                        word.Add(new Letter(g, c, font, attributes, baseline));
                        break;
                }
            }

            if (word.Count != 0)
            {
                if (GetTextWidth(letters) + GetTextWidth(word) >= attributes.Width)
                {
                    lines.Add(letters.ToArray());
                    letters.Clear();
                }
                letters.AddRange(word);
            }

            if (letters.Count != 0)
            {
                lines.Add(letters.ToArray());
            }

            if (attributes.Angle != 0)
            {
                g.Save();
                g.RotateAtTransform(attributes.Angle, new XPoint(attributes.Left, top));
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i][lines[i].Length - 1].Value == " ")
                {
                    lines[i] = lines[i].ToList().Take(lines[i].Length - 1).ToArray();
                }

                Letter[] line = lines[i];
                double left = attributes.Left;
                double textWidth = GetTextWidth(line);

                switch (attributes.Align)
                {
                    case TextAlign.Center:
                        left += (attributes.Width - textWidth) / 2;
                        break;

                    case TextAlign.Far:
                    case TextAlign.Right:
                        left += attributes.Width - textWidth;
                        break;

                    case TextAlign.Justify:
                        if (!line[line.Length - 1].LineBreak && i < lines.Count - 1)
                        {
                            int spaces = line.Where(l => l.Value == " ").Count();
                            double offset = (attributes.Width - textWidth) / spaces;
                            line.ToList().ForEach(l => l.AddOffset(offset));
                        }
                        break;

                    default:
                        break;
                }

                foreach (Letter letter in line)
                {
                    g.DrawString(letter.Value, letter.Font, letter.Brush, left, top + letter.Baseline);
                    left += letter.Width;
                }

                top += attributes.LineHeight;
            }

            if (attributes.Angle != 0)
            {
                g.Restore();
                return 0;
            }

            return lines.Count * attributes.LineHeight;
        }

        /// <summary>
        /// Draws a text as watermark across the current page.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="attributes"></param>
        public static void DrawWatermark(this XGraphics g, string text, XFont font, TextAttributes attributes)
        {
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            XUnit w = new XUnit(g.PdfPage.Width);
            w.ConvertType(g.PageUnit);
            double width = w.Value;

            XUnit h = new XUnit(g.PdfPage.Height);
            h.ConvertType(g.PageUnit);
            double height = h.Value;

            g.Save();
            g.RotateAtTransform((-Math.Atan(height / width) * 180 / Math.PI), new XPoint(width / 2, height / 2));

            for (int i = 0; i < lines.Length; i++)
            {
                var x = width / 2;
                var y = (height / 2) - (((double)lines.Length / 2 - .5) * font.Size) + (i * font.Size);
                g.DrawString(lines[i], font, attributes.Brush, new XPoint(x, y), XStringFormats.Center);
            }

            g.Restore();
        }

        /// <summary>
        /// Calculates the width of a text line.
        /// </summary>
        /// <param name="letters"></param>
        /// <returns></returns>
        private static double GetTextWidth(IEnumerable<Letter> letters)
        {
            double width = 0;
            foreach (Letter letter in letters)
            {
                width += letter.Width;
            }
            return width;
        }

        /// <summary>
        /// Supported HTML tags
        /// </summary>
        private static string[] _supportedHtmlTags = {
             "b",  "i",  "u",  "s",  "color",  "size",  "sub",  "sup",
            "/b", "/i", "/u", "/s", "/color", "/size", "/sub", "/sup"
        };
    }
}
