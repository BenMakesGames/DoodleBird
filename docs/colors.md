# Colors

This project uses DawnBringer's 16-color palette, a limited palette created by online pixel artist "DawnBringer".

**Important constraints:**

- All art and UI in the game uses only these 16 colors. Never pick a color outside this palette for a sprite, UI element, or shader parameter.

## Color Reference

| Palette Index | Color Name | Hex Code |
| ------------- | ---------- | -------- |
| 0             | Black      | #140c1c  |
| 1             | DarkPurple | #442434  |
| 2             | DarkBlue   | #30346d  |
| 3             | DarkGray   | #4e4a4e  |
| 4             | Brown      | #854c30  |
| 5             | DarkGreen  | #346524  |
| 6             | Red        | #d04648  |
| 7             | Gray       | #757161  |
| 8             | Blue       | #597dce  |
| 9             | Orange     | #d27d2c  |
| 10            | LightGray  | #8595a1  |
| 11            | Green      | #6daa2c  |
| 12            | Pink       | #d2aa99  |
| 13            | LightBlue  | #6dc2ca  |
| 14            | Yellow     | #dad45e  |
| 15            | White      | #deeed6  |

Example usage:

```c#
var green = DawnBringers16.Green;
var alsoGreen = DawnBringers16.Palette[11];
```

