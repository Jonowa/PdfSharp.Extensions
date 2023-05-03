# PdfSharp.Extensions
This is an extension for PdfSharp. It allows you to draw text with attributes like formatting (bold, italic etc.), line breaks, font kerning and opacity.

The following text formatting are supported:

- &lt;b>bold&lt;/b> = <b>bold</b><br>
- &lt;i>italic&lt;/i> = <i>italic</i><br>
- &lt;u>underlined&lt;/u> = <u>underlined</u><br>
- &lt;s>strikeout&lt;/s> = <s>strikeout</s><br>
- &lt;color='#f00'>red text&lt;/color> (Short HTML color)<br>
- &lt;color='#ff0000'>red text&lt;/color> (Regular HTML color)<br>
- &lt;color='Red'>red text&lt;/color> (Named color of 216 basic colors)<br>
- &lt;size=12>change font size&lt;/size><br>
- &lt;sub>sub-script&lt;/sub> = <sub>sub-script</sub><br>
- &lt;sup>superscript&lt;/sup> = <sup>superscript</sup><br>
- &lt;br> (line break as alternative to \n or \r\n)

The function DrawText() uses the class TextAttributes which has these public properties:

- Left (X-coordinate)<br>
- Top (Y-coordinate)<br>
- Width<br>
- Height<br>
- LineHeight<br>
- Kerning (letter spacing)<br>
- Opacity (0 to 100)<br>
- Color (XColor)<br>
- Align (enum TextAlign)<br>
- Angle (rotation angle -90 to 90 deg.)
