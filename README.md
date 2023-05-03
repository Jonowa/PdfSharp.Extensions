# PdfSharp.Extensions
This is an extension for PdfSharp. It allows you to draw text with attributes like formatting (bold, italic etc.), line breaks, font kerning and opacity.

The following text formatting are supported:

- &lt;b>bold&lt;/b> = <b>bold</b>
- &lt;i>italic&lt;/i> = <i>italic</i>
- &lt;u>underlined&lt;/u> = <u>underlined</u>
- &lt;s>strikeout&lt;/s> = <s>strikeout</s>
- &lt;color='#f00'>red text&lt;/color> (Short HTML color)
- &lt;color='#ff0000'>red text&lt;/color> (Regular HTML color)
- &lt;color='Red'>red text&lt;/color> (Named color of 216 basic colors)
- &lt;size=12>change font size&lt;/size>
- &lt;sub>sub-script&lt;/sub> = <sub>sub-script</sub>
- &lt;sup>superscript&lt;/sup> = <sup>superscript</sup>
- &lt;br> (line break as alternative to \n or \r\n)

The function DrawText() uses the class TextAttributes which has these public properties:

- Left (X-coordinate)
- Top (Y-coordinate)
- Width
- Height
- LineHeight (line spacing)
- Kerning (letter spacing)
- Opacity (0 to 100)
- Color (XColor)
- Align (enum TextAlign)
- Angle (rotation angle -90 to 90 deg.)

Example:
´´´c-sharp
using PdfSharp.Extensions;

...

var attributes = new TextAttributes(XBrushes.Black, new XPoint(20, 20));
´´´
