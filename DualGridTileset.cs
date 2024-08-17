using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024;

internal class DualGridTileset
{
    Texture texture;
    Vector2 tileSize;

    static Vector2[] offsets =
    {
        new Vector2(0, 0), // Top left
        new Vector2(1, 0), // Top right
        new Vector2(0, 1), // Bottom left
        new Vector2(1, 1)  // Bottom right
    };

    public DualGridTileset(Texture texture, Vector2 tileSize)
    {
        Debug.Assert(texture.width  == tileSize.X * 4);
        Debug.Assert(texture.height == tileSize.Y * 4);
        this.texture = texture;
        this.tileSize = tileSize;
    }

    public void DrawTile(Rectangle rect, Color color, bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
    {
        var bitset = 0;
        bitset += topLeft     ? 1 : 0;
        bitset += topRight    ? 2 : 0;
        bitset += bottomLeft  ? 4 : 0;
        bitset += bottomRight ? 8 : 0;

        DrawTile(rect, color, bitset);
    }

    public void DrawTile(Rectangle rect, Color color, int neighbourBitset)
    {
        var neighbourLookup = new Vector2[16];
        {
            neighbourLookup[0b0100] = new Vector2(0, 0);
            neighbourLookup[0b1001] = new Vector2(0, 1);
            neighbourLookup[0b0010] = new Vector2(0, 2);
            neighbourLookup[0b0000] = new Vector2(0, 3);

            neighbourLookup[0b1010] = new Vector2(1, 0);
            neighbourLookup[0b1110] = new Vector2(1, 1);
            neighbourLookup[0b0011] = new Vector2(1, 2);
            neighbourLookup[0b1000] = new Vector2(1, 3);

            neighbourLookup[0b1101] = new Vector2(2, 0);
            neighbourLookup[0b1111] = new Vector2(2, 1);
            neighbourLookup[0b1011] = new Vector2(2, 2);
            neighbourLookup[0b0110] = new Vector2(2, 3);

            neighbourLookup[0b1100] = new Vector2(3, 0);
            neighbourLookup[0b0111] = new Vector2(3, 1);
            neighbourLookup[0b0101] = new Vector2(3, 2);
            neighbourLookup[0b0001] = new Vector2(3, 3);
        }

        var source = new Rectangle(
            neighbourLookup[neighbourBitset].X * tileSize.X,
            neighbourLookup[neighbourBitset].Y * tileSize.Y,
            tileSize.X,
            tileSize.Y
        );

        rect.x += rect.width / 2;
        rect.y += rect.height / 2;
        Raylib.DrawTexturePro(texture, source, rect, Vector2.Zero, 0, color);
    }

    public void DrawRectangle(Rectangle rect, Color color)
    {
        Debug.Assert(rect.width  % tileSize.X == 0);
        Debug.Assert(rect.height % tileSize.Y == 0);

        int widthInTiles = (int)(rect.width / tileSize.X);
        int heightInTiles = (int)(rect.height / tileSize.Y);

        for (int y = -1; y < heightInTiles; y++)
        {
            for (int x = -1; x < widthInTiles; x++)
            {
                var tileRect = new Rectangle(
                    rect.x + x * tileSize.X,
                    rect.y + y * tileSize.Y,
                    tileSize.X,
                    tileSize.Y
                );
                bool topLeft = y >= 0 && x >= 0;
                bool topRight = y >= 0 && x < widthInTiles-1;
                bool bottomLeft = x >= 0 && y < heightInTiles - 1;
                bool bottomRight = x < widthInTiles - 1 && y < heightInTiles - 1;
                DrawTile(tileRect, color, topLeft, topRight, bottomLeft, bottomRight);
            }
        }
    }
}
