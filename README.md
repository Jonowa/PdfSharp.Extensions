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
```C#
using PdfSharp.Extensions;

...

TextAttributes attributes = new TextAttributes(XBrushes.Black, new XPoint(20, 20));
double height = graphics.DrawText("Hello World.", new XFont("Helvetica", 10), attributes);
// The function returns the height of the text element.

attributes.Top += height;
attributes.Opacity = 50;
height = graphics.DrawText("Half transparent text.", new XFont("Helvetica", 10), attributes);

attributes.Top += height;
attributes.Opacity = 100;
attributes.Kerning = 20;
height = graphics.DrawText("Condensed text with a <b>bold formatted</b> part.", new XFont("Helvetica", 10), attributes);

attributes.Top += height;
attributes.Kerning = -50;
height = graphics.DrawText("Expanded text with an <i>italic formatted</i> part.", new XFont("Helvetica", 10), attributes);

attributes.Top += height;
attributes.Kerning = 0;
attributes.Angle = -45;
height = graphics.DrawText("Text from bottom left to top right.", new XFont("Helvetica", 10), attributes);
// Note: If angle is not equal to 0 the return value of DrawText is 0!

attributes = new TextAttributes(XBrushes.Black, new XPoint(20, 100)) {
    Width = 100,
    Align = TextAlign.Justify
};
height = graphics.DrawText("Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.", new XFont("Helvetica", 10), attributes);
// The text will have a width of 100 point with automatic line breaks. It also is formatted in justification.
```
