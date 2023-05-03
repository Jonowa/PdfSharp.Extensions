# PdfSharp.Extensions
This is an extension for PdfSharp. It allows you to draw text with attributes like formatting (bold, italic etc.), line breaks, font kerning and opacity.

The following text formatting are supported:
<b>bold</b>
<i>italic</i>
<u>underlined</u>
<s>strikeout</s>
<color='#f00'>red text</color> (Short HTML color)
<color='#ff0000'>red text</color> (Regular HTML color)
<color='Red'>red text</color> (Named color of 216 basic colors)
<size=12>change font size</size>
<sub>sub-script</sub>
<sup>superscript</sup>
<br> (line break as alternative to \n or \r\n)

The function DrawText() uses the class TextAttributes which has these public properties:

Left (X-coordinate)

Top (Y-coordinate)

Width

Height
LineHeight
Kerning (letter spacing)
Opacity (0 to 100)
Color (XColor)
Align (enum TextAlign)
Angle (rotation angle -90 to 90 deg.)
