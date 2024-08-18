using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

internal class Program
{
    public static float tileSize = 32;
    public static Vector2 canvasSize = new Vector2(320 * 3, 180 * 3);

    public static Assets assets;
    public static Dictionary<string, RaylibTileset> tilesets;
    public static DualGridTileset towerPlatformMain;
    public static DualGridTileset towerPlatformFoliage;
    public static RaylibAnimation revolver;
    public static RaylibAnimation slime;

    public static void Main(string[] args)
    {
        assets = new Assets();

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1920, 1080, "GMTK2024");
        Raylib.SetWindowMinSize((int)canvasSize.X, (int)canvasSize.Y);
        Raylib.SetTargetFPS(60);

        tilesets = RaylibTileset.LoadAll(assets);
        
        var towerBaseTileset = assets.LoadAseprite("grass_tower_base_tileset.aseprite");

        towerPlatformMain = new DualGridTileset(
            Utils.FlattenLayerToTexture(towerBaseTileset.Frames[0], "tower_base"),
            new Vector2(tileSize, tileSize)
        );

        towerPlatformFoliage = new DualGridTileset(
            Utils.FlattenLayerToTexture(towerBaseTileset.Frames[0], "foliage"),
            new Vector2(tileSize, tileSize)
        );

        var revolverAse = assets.LoadAseprite("revolver.aseprite");
        revolver = Utils.FlattenToAnimation(revolverAse);

        var slimeAse = assets.LoadAseprite("slime2.aseprite");
        slime = Utils.FlattenTagToAnimation(slimeAse, "jump");

        var tilemap = new RaylibTilemap(tilesets, assets.LoadStream("main.tmx"));
        var currentLevel = new Level(tilemap);

        while (!Raylib.WindowShouldClose()) 
        {
            currentLevel.Tick();
        }

        Raylib.CloseWindow();
    }
}