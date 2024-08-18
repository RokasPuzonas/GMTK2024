using Raylib_CsLo;
using TiledCS;

namespace GMTK2024;

class RaylibTileset
{
    public string source;
    public TiledTileset tileset;
    public Texture texture;

    public RaylibTileset(Assets assets, string name)
    {
        this.source = name;
        tileset = new TiledTileset(assets.LoadStream(name));

        texture = assets.LoadTexture(tileset.Image.source);
    }

    public static Dictionary<string, RaylibTileset> LoadAll(Assets assets)
    {
        var tilesets = new Dictionary<string, RaylibTileset>();
        foreach (var filename in assets.ListNames(".tsx"))
        {
            var rlTileset = new RaylibTileset(assets, Path.GetFileName(filename));
            tilesets.Add(rlTileset.source, rlTileset);
        }

        return tilesets;
    }

    public TiledTile? GetTile(int id)
    {
        foreach (var tile in tileset.Tiles)
        {
            if (tile.id == id)
            {
                return tile;
            }
        }

        return null;
    }

    public bool GetBoolProperty(string name, bool fallback)
    {
        return Utils.GetBoolTiledProperty(tileset.Properties, name, fallback);
    }
}