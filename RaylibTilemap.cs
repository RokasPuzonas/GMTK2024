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

        var dualGrid = new DualGridTileset(rlTileset.texture, new Vector2(rlTileset.tileset.TileWidth, rlTileset.tileset.TileHeight));

        var tileX = x * map.TileWidth;
        var tileY = y * map.TileHeight;
        dualGrid.DrawTile(new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight), Raylib.WHITE, neighbourIndex);
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

    public TiledLayer? GetLayer(string name, TiledLayerType? type = null)
    {
        foreach (var layer in map.Layers)
        {
            if (type != null)
            {
                if (layer.type != type) continue;
            }

            if (layer.name == name)
            {
                return layer;
            }
        }

        return null;
    }

    public static TiledObject? GetObject(TiledLayer layer, string name)
    {
        foreach (var obj in layer.objects)
        {
            if (obj.name == name)
            {
                return obj;
            }
        }

        return null;
    }
}
