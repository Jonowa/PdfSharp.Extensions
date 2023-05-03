using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PdfSharp.Extensions
{
    using Pdf;
    using Pdf.Content;
    using Pdf.Content.Objects;
    using Pdf.IO;
    using Pdf.Advanced;

    /// <summary>
    /// Single Threaded pdf text extractor, use multiple instances for multi-threaded app
    /// </summary>
    public class TextExtractor : IDisposable
    {
        public static Task<string> GetTextAsync(string file)
        {
            return Task.Factory.StartNew(() => {
                return GetText(file);
            });
        }

        public static string GetText(string file)
        {
            using (var _document = PdfReader.Open(file, PdfDocumentOpenMode.ReadOnly))
            {
                TextExtractor ext = new TextExtractor(_document);
                return ext.GetText();
            }
        }

        public string GetText()
        {
            StringBuilder result = new StringBuilder();

            foreach (var page in document.Pages)
            {
                this.page = page;
                ExtractText(page, result);
                result.AppendLine();
            }

            return result.ToString();
        }

        public string GetText(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > document.PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));

            StringBuilder result = new StringBuilder();

            this.page = document.Pages[pageNumber - 1];
            ExtractText(page, result);
            result.AppendLine();

            return result.ToString();
        }


        internal PdfPage page { get; set; }
        PdfDocument document { get; }

        public TextExtractor(PdfDocument doc)
        {
            document = doc;
        }

        public TextExtractor(string filename)
        {
            document = PdfReader.Open(filename, PdfDocumentOpenMode.ReadOnly);
        }

        internal class Font
        {
            PdfDictionary font;
            public enum font_type
            {
                Type0,
                Type1,
                Type3,
                TrueType,
                Other
            };
            Dictionary<ushort, string> toUnicode = new Dictionary<ushort, string>();
            public string ToUnicode(ushort val)
            {
                if (toUnicode != null)
                {
                    string str;
                    if (toUnicode.TryGetValue(val, out str)) return str;
                    else
                    {
                        Console.WriteLine($"Warning! No unicode symbol for {val}!");
                    }
                }
                return "";
            }
            public font_type Type { get; set; }
            public string Encoding { get; set; }

            const int Flag_Symbolic = 4;
            public int Flags { get; set; }
            public bool IsTwoByte { get; }

            public Font(PdfDictionary dictionary)
            {
                font = dictionary;

                // font.Elements["SubType"]
                // font.Elements["Encoding"]
                if (font.Elements.Keys.Contains("/Encoding"))
                {
                    PdfItem item = font.Elements["/Encoding"];
                    if (item is PdfReference)
                    {
                        var dict = (((item as PdfReference).Value) as PdfDictionary);
                        if (dict.Elements.ContainsKey("/BaseEncoding"))
                            Encoding = dict.Elements["/BaseEncoding"].ToString();
                    }
                    else Encoding = item.ToString();
                }

                if (Encoding != null && Encoding.StartsWith("/Identity-")) IsTwoByte = true;

                if (font.Elements.Keys.Contains("/Subtype"))
                {
                    switch (font.Elements["/Subtype"].ToString())
                    {
                        case "/TrueType":
                            Type = font_type.TrueType;
                            break;
                        case "/Type0":
                            Type = font_type.Type0;
                            break;
                        case "/Type1":
                            Type = font_type.Type1;
                            break;
                        case "/Type3":
                            Type = font_type.Type3;
                            break;
                        default:
                            Type = font_type.Other;
                            break;
                    }
                }

                if (font.Elements.ContainsKey("/FontDescriptor"))
                {
                    var obj = font.Elements["/FontDescriptor"];
                    if (obj is PdfReference) obj = (obj as PdfReference).Value;
                    obj = (obj as PdfDictionary).Elements["/Flags"];
                    if (obj != null)
                    {
                        if (obj is PdfReference) obj = (obj as PdfReference).Value;
                        if (obj is PdfInteger) Flags = (obj as PdfInteger).Value;
                    }
                }

                if (font.Elements.Keys.Contains("/ToUnicode"))
                { // parse to unicode
                    PdfItem item = font.Elements["/ToUnicode"];
                    if (item is PdfReference) item = (item as PdfReference).Value;
                    if (item is PdfDictionary)
                    {
                        string map = (item as PdfDictionary).Stream.ToString();
                        toUnicode = ParseCMap(map);
                    }
                }
                else
                {

                    if (Encoding != null)
                    {
                        switch (Encoding)
                        {
                            case "/MacRomanEncoding":
                                toUnicode = EncodingTables.MacRoman;
                                break;
                            case "/WinAnsiEncoding":
                                toUnicode = EncodingTables.WinAnsi;
                                break;
                            case "/MacExpertEncoding":
                                toUnicode = EncodingTables.MacExpert;
                                break;
                            case "/Standard":
                                toUnicode = EncodingTables.Standard;
                                break;
                            case "/Symbol":
                                toUnicode = EncodingTables.Symbol;
                                break;
                        }
                    }
                    else
                    {
                        if ((Flags & Flag_Symbolic) != 0)
                            toUnicode = EncodingTables.Symbol;
                        else
                            toUnicode = EncodingTables.Standard;
                    }
                }



            }


            public char nodef_char { get; set; } = '\xE202';
            Dictionary<ushort, string> ParseCMap(string map)
            {
                Dictionary<ushort, string> cmap = new Dictionary<ushort, string>();
                try
                {
                    map = map.ToLower();
                    int bf = map.IndexOf("beginbfrange");
                    while (bf >= 0)
                    {
                        int ef = map.IndexOf("endbfrange", bf);
                        if (ef < 0) ef = map.Length;

                        // parsing ranges
                        string[] Ranges = map.Substring(bf + 13, ef - (bf + 13)).Split('\n', '\r');
                        foreach (string range in Ranges)
                        {
                            Match m = Regex.Match(range, "<([0-9abcdef]+)> ?<([0-9abcdef]+)> ?<([0-9abcdef]+)>");
                            if (m.Success)
                            {
                                int st = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                                int end = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                                char uni = (char)int.Parse(m.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);
                                end = Math.Min(ushort.MaxValue - 1, end);
                                st = Math.Min(st, end);
                                for (ushort q = (ushort)st; q <= end; q++)
                                    cmap[q] = "" + uni++;
                                continue;
                            }
                            m = Regex.Match(range, @"<([0-9abcdef]+)> ?<([0-9abcdef]+)> ?\[(.+)\]");
                            if (m.Success)
                            {
                                int st = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                                int end = int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                                end = Math.Min(ushort.MaxValue - 1, end);
                                st = Math.Min(st, end);
                                foreach (Match mm in Regex.Matches(m.Groups[3].Value, "<([0-9abcdef]+)>"))
                                {
                                    if (mm.Groups.Count > 1)
                                    {
                                        cmap[(ushort)st++] = new string(mm.Groups[1].Value.Select((x, i) => new { x, i }).GroupBy(o => o.i / 4).Select(g => new string(g.Select(o => o.x).ToArray()))
                                            .Select(s => (char)int.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray());
                                        if (st >= end) break;
                                    }

                                }
                            }
                        }
                        bf = map.IndexOf("beginbfrange", ef);

                    }
                    bf = map.IndexOf("beginbfchar");
                    while (bf >= 0)
                    {
                        int ef = map.IndexOf("endbfchar", bf);
                        if (ef < 0) ef = map.Length;

                        // parsing ranges
                        string[] Ranges = map.Substring(bf + 11, ef - (bf + 11)).Split('\n', '\r');
                        foreach (string range in Ranges)
                        {
                            Match m = Regex.Match(range, "<([0-9abcdef]+)> ?<([0-9abcdef]+)>");
                            if (m.Success)
                            {
                                int st = int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                                st = Math.Min(st, ushort.MaxValue - 1);
                                cmap[(ushort)st] = new string(m.Groups[2].Value.Select((x, i) => new { x, i }).GroupBy(o => o.i / 4).Select(g => new string(g.Select(o => o.x).ToArray()))
                                    .Select(s => (char)int.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray());
                                continue;
                            }
                        }
                        bf = map.IndexOf("beginbfchar", ef);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing cmap range: " + e.Message);
                }

                return cmap;
            }


            public string ProcessString(string str)
            {
                return "";
            }

        }

        Font curr_font;

        Dictionary<string, Font> fonts = new Dictionary<string, Font>();

        public bool ExtractText(PdfPage page, StringBuilder res)
        {
            // create font table
            curr_font = null; // no default font
            this.page = page;
            fonts.Clear();

            if ((page.Resources != null) && (page.Resources.Elements["/Font"] != null))
            {
                var obj = page.Resources.Elements["/Font"];
                if (obj is PdfReference) obj = (obj as PdfReference).Value;
                if (obj is PdfDictionary)
                    foreach (var kp in (obj as PdfDictionary).Elements)
                    {

                        PdfItem fobj = kp.Value;
                        if (fobj is PdfReference)
                        {
                            fobj = ((PdfReference)fobj).Value;
                        }
                        // now we make font
                        if (fobj is PdfDictionary)
                        {
                            fonts.Add(kp.Key, new Font((PdfDictionary)fobj));
                        }


                    }
            }
            try
            {
                ExtractText(ContentReader.ReadContent(page), res);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return false;
            }
            return true;
        }

        void ExtractText(CObject obj, StringBuilder target)
        {
            if (obj is CArray)
                ExtractText((CArray)obj, target);
            /*            else if (obj is CComment)
                            ExtractText((CComment)obj, target);
                        else if (obj is CInteger)
                            ExtractText((CInteger)obj, target);
                        else if (obj is CName)
                            ExtractText((CName)obj, target);
                        else if (obj is CNumber)
                            ExtractText((CNumber)obj, target);
                            */
            else if (obj is COperator)
                ExtractText((COperator)obj, target);
            /*
            else if (obj is CReal)
                ExtractText((CReal)obj, target);
                */
            else if (obj is CSequence)
                ExtractText((CSequence)obj, target);
            else if (obj is CString)
                ExtractText((CString)obj, target);
            /*else
                throw new NotImplementedException(obj.GetType().AssemblyQualifiedName);
                */
        }

        private void ExtractText(CArray obj, StringBuilder target)
        {
            foreach (var element in obj)
            {
                ExtractText(element, target);
            }
        }
        private void ExtractText(COperator obj, StringBuilder target)
        {

            if (obj.OpCode.OpCodeName == OpCodeName.QuoteSingle || obj.OpCode.OpCodeName == OpCodeName.QuoteDbl || obj.OpCode.OpCodeName == OpCodeName.Tj || obj.OpCode.OpCodeName == OpCodeName.TJ)
            {
                if (obj.OpCode.OpCodeName == OpCodeName.QuoteSingle || obj.OpCode.OpCodeName == OpCodeName.QuoteDbl) target.Append("\n");

                if (obj.Operands.Count == 1)
                {
                    if (obj.Operands[0] is CArray)
                    {
                        foreach (var elem in ((CArray)(obj.Operands[0])))
                        {

                            if (elem is CString)
                            {
                                ExtractText(elem as CString, target);
                            }
                            else
                            {
                                if ((elem is CNumber) && (obj.OpCode.OpCodeName == OpCodeName.Tj))
                                    if (GetNumberValue((CNumber)elem) > 750)
                                    {
                                        target.Append(" ");
                                    }
                            }
                        }
                    }
                    else ExtractText(obj.Operands[0], target);
                }
                else
                    Console.WriteLine("Error TJ!");
            }
            else
            if ((obj.OpCode.OpCodeName == OpCodeName.Tx) || (obj.OpCode.OpCodeName == OpCodeName.TD) || (obj.OpCode.OpCodeName == OpCodeName.Td))
            {
                //target.Append("\n");

            }
            else
            if (obj.OpCode.OpCodeName == OpCodeName.Tm)
            {
                // TODO: check if position shifts enough (sometimes Tm is used in word parts)
                target.Append(" ");
            }
            else
            if (obj.OpCode.OpCodeName == OpCodeName.Tf)
            {
                if (obj.Operands.Count == 2)
                {
                    //if (obj.Operands[0] is CString)
                    {
                        string nF = obj.Operands[0].ToString();

                        curr_font = fonts[nF];

                        //font = page.Resources.Elements["/Font"];
                    }
                }
                else
                {
                    Console.WriteLine("Error in Tf operator");
                }
            }

        }

        double GetNumberValue(CNumber numb)
        {
            if (numb is CReal) return ((CReal)numb).Value;
            else
            if (numb is CInteger) return ((CInteger)numb).Value;
            else return double.NaN;
        }

        private void ExtractText(CString elem, StringBuilder target)
        {
            if (curr_font.IsTwoByte)
            {
                foreach (var s in elem.Value.Select((c, i) => new { c = c << (1 - i % 2) * 8, i }).GroupBy(o => o.i / 2).Select(g => (char)g.Sum(o => o.c))
                    .Select(c => curr_font.ToUnicode(c)))
                    target.Append(s);
            }
            else
                foreach (var s in elem.Value.Select(c => curr_font.ToUnicode(c))) target.Append(s);
        }

        private void ExtractText(CSequence obj, StringBuilder target)
        {
            foreach (var element in obj)
            {
                ExtractText(element, target);
            }
        }

        public virtual void Dispose()
        {
            fonts.Clear();
            curr_font = null;
        }


    }

}