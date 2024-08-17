using Raylib_CsLo;
using TiledCS;

namespace GMTK2024;

class RaylibTileset
{
    public string source;
    public TiledTileset tileset;
    public Texture texture;

    public RaylibTileset(string dir, string source)
    {
        var path = Path.Join(dir, source);
        this.source = source;
        tileset = new TiledTileset(path);

        var imagePath = Path.Join(Path.GetDirectoryName(path), tileset.Image.source);
        texture = Raylib.LoadTexture(imagePath);
    }

    public static Dictionary<string, RaylibTileset> LoadAllInFolder(string dir)
    {
        var tilesets = new Dictionary<string, RaylibTileset>();
        foreach (var filename in Directory.GetFiles(dir))
        {
            if (!filename.EndsWith(".tsx")) continue;

            var rlTileset = new RaylibTileset(dir, Path.GetFileName(filename));
            tilesets.Add(rlTileset.source, rlTileset);
        }

        return tilesets;
    }

    public bool GetBoolProperty(string name, bool fallback)
    {
        foreach (var prop in tileset.Properties)
        {
            if (prop.type == TiledPropertyType.Bool)
            {
                return prop.value == "true";
            }
        }

        return fallback;
    }
}