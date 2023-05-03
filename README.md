# PdfSharp.Extensions
This is an extension for PdfSharp. It allows you to draw text with attributes like formatting (bold, italic etc.), line breaks, font kerning and opacity.

The following text formatting are supported:

<b>bold</b><br>
<i>italic</i><br>
<u>underlined</u><br>
<s>strikeout</s><br>
<color='#f00'>red text</color> (Short HTML color)<br>
<color='#ff0000'>red text</color> (Regular HTML color)<br>
<color='Red'>red text</color> (Named color of 216 basic colors)<br>
<size=12>change font size</size><br>
<sub>sub-script</sub><br>
<sup>superscript</sup><br>
&lt;br&gt; (line break as alternative to \n or \r\n)

The function DrawText() uses the class TextAttributes which has these public properties:

Left (X-coordinate)<br>
Top (Y-coordinate)<br>
Width<br>
Height<br>
LineHeight<br>
Kerning (letter spacing)<br>
Opacity (0 to 100)<br>
Color (XColor)<br>
Align (enum TextAlign)<br>
Angle (rotation angle -90 to 90 deg.)
