using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;
using TiledCS;

namespace GMTK2024;

class RaylibTilemap
{
    TiledMap map;
    Dictionary<int, RaylibTileset> rlTilesets;

    static Vector2[] dualGridOffsets =
    {
        new Vector2(0, 0), // Top left
        new Vector2(1, 0), // Top right
        new Vector2(0, 1), // Bottom left
        new Vector2(1, 1)  // Bottom right
    };

    public RaylibTilemap(Dictionary<string, RaylibTileset> tilesets, string tilemap)
    {
        map = new TiledMap(tilemap);

        rlTilesets = new Dictionary<int, RaylibTileset>();
        foreach (var mapTileset in map.Tilesets)
        {
            rlTilesets.Add(mapTileset.firstgid, tilesets[mapTileset.source]);
        }
    }

    static int GetTileAt(TiledLayer layer, int x, int y)
    {
        if (Utils.IsInside(new Vector2(x, y), new Vector2(layer.width, layer.height)))
        {
            var index = (y * layer.width) + x;
            return layer.data[index];
        }

        return 0;
    }

    static void DrawDualLayerTile(RaylibTileset rlTileset, TiledLayer layer, TiledMap map, int x, int y)
    {
        Debug.Assert(rlTileset.tileset.TileCount == 16);

        var neighbourLookup = new Vector2[16];

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

        var gid = 0;
        foreach (var mapTileset in map.Tilesets)
        {
            if (mapTileset.source == rlTileset.source)
            {
                gid = mapTileset.firstgid;
                break;
            }
        }
        Debug.Assert(gid != 0);
        gid += 6;

        var neighbourIndex = 0;
        for (var i = 0; i < dualGridOffsets.Length; i++)
        {
            var tile_id = GetTileAt(layer, x + (int)dualGridOffsets[i].X, y + (int)dualGridOffsets[i].Y);

            if (tile_id == gid || tile_id == 0)
            {
                neighbourIndex += (1 << i);
            }
        }

        var rect = new Rectangle(
            neighbourLookup[neighbourIndex].X * rlTileset.tileset.TileWidth,
            neighbourLookup[neighbourIndex].Y * rlTileset.tileset.TileHeight,
            rlTileset.tileset.TileWidth,
            rlTileset.tileset.TileHeight
        );

        var tileX = (x + 0.5f) * map.TileWidth;
        var tileY = (y + 0.5f) * map.TileHeight;
        Raylib.DrawTexturePro(rlTileset.texture, rect, new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight), Vector2.Zero, 0, Raylib.WHITE);
    }

    public void Draw()
    {
        foreach (var layer in map.Layers)
        {
            if (layer.type != TiledLayerType.TileLayer) continue;
            for (var y = -1; y < layer.height + 1; y++)
            {
                for (var x = -1; x < layer.width + 1; x++)
                {
                    var gid = GetTileAt(layer, x, y);
                    if (gid == 0)
                    {
                        for (var i = 0; i < dualGridOffsets.Length; i++)
                        {
                            var nx = x + (int)dualGridOffsets[i].X;
                            var ny = y + (int)dualGridOffsets[i].Y;
                            var ngid = GetTileAt(layer, nx, ny);
                            if (ngid == 0) continue;

                            var mapTileset = map.GetTiledMapTileset(ngid);
                            var rlTileset = rlTilesets[mapTileset.firstgid];
                            if (rlTileset.GetBoolProperty("dual-grid", false))
                            {
                                DrawDualLayerTile(rlTileset, layer, map, x, y);
                            }
                        }
                    }
                    else
                    {
                        var mapTileset = map.GetTiledMapTileset(gid);
                        var rlTileset = rlTilesets[mapTileset.firstgid];

                        if (rlTileset.GetBoolProperty("dual-grid", false))
                        {
                            DrawDualLayerTile(rlTileset, layer, map, x, y);
                        }
                        else
                        {
                            var tileX = x * map.TileWidth;
                            var tileY = y * map.TileHeight;
                            var rect = map.GetSourceRect(mapTileset, rlTileset.tileset, gid);
                            var rlRect = new Rectangle(rect.x, rect.y, rect.width, rect.height);

                            Raylib.DrawTexturePro(rlTileset.texture, rlRect, new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight), Vector2.Zero, 0, Raylib.WHITE);
                        }
                    }

                }
            }
        }
    }
}
