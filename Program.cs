using Raylib_CsLo;
using TiledCS;
using System.Numerics;

namespace GMTK2024;

class RaylibTileset
{
    public TiledTileset tileset;
    public Texture texture;

    public RaylibTileset(string path)
    {
        tileset = new TiledTileset(path);

        var imagePath = Path.Join(Path.GetDirectoryName(path), tileset.Image.source);
        texture = Raylib.LoadTexture(imagePath);
    }

    public static Dictionary<string, RaylibTileset> LoadAllInFolder(string dir) {
        var tilesets = new Dictionary<string, RaylibTileset>();
        foreach (var filename in Directory.GetFiles(dir))
        {
            if (!filename.EndsWith(".tsx")) continue;

            var rlTileset = new RaylibTileset(filename);
            tilesets.Add(Path.GetFileName(filename), rlTileset);
        }

        return tilesets;
    }
}

class RaylibTilemap
{
    TiledMap map;
    Dictionary<int, RaylibTileset> rlTilesets;

    public RaylibTilemap(Dictionary<string, RaylibTileset> tilesets, string tilemap)
    {
        map = new TiledMap(tilemap);

        rlTilesets = new Dictionary<int, RaylibTileset>();
        foreach (var mapTileset in map.Tilesets)
        {
            rlTilesets.Add(mapTileset.firstgid, tilesets[mapTileset.source]);
        }
    }

    public void Draw()
    {
        foreach (var layer in map.Layers)
        {
            if (layer.type != TiledLayerType.TileLayer) continue;
            for (var y = 0; y < layer.height; y++)
            {
                for (var x = 0; x < layer.width; x++)
                {
                    var index = (y * layer.width) + x;
                    var gid = layer.data[index];
                    var tileX = x * map.TileWidth;
                    var tileY = y * map.TileHeight;

                    if (gid == 0)
                    {
                        continue;
                    }

                    var mapTileset = map.GetTiledMapTileset(gid);
                    var rlTileset = rlTilesets[mapTileset.firstgid];
                    var rect = map.GetSourceRect(mapTileset, rlTileset.tileset, gid);
                    var rlRect = new Rectangle(rect.x, rect.y, rect.width, rect.height);

                    Raylib.DrawTexturePro(rlTileset.texture, rlRect, new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight), Vector2.Zero, 0, Raylib.WHITE);
                }
            }
        }
    }
}

internal class Program
{
    public static float DivMultipleFloor(float a, float b)
    {
        return (float)Math.Floor(a / b) * b;
    }
    public static float DivMultipleCeil(float a, float b)
    {
        return (float)Math.Ceiling(a / b) * b;
    }

    public static void DrawGrid(Rectangle rect, float tileSize, Color color)
    {
        for (float y = 0; y <= Math.Ceiling(rect.height / tileSize); y++)
        {
            var oy = DivMultipleFloor(rect.y, tileSize);
            Raylib.DrawLineV(
                new Vector2(DivMultipleFloor(rect.x, tileSize), oy + y * tileSize),
                new Vector2(DivMultipleCeil(rect.x + rect.width, tileSize), oy + y * tileSize),
                color
            );
        }

        for (float x = 0; x <= Math.Ceiling(rect.width / tileSize); x++)
        {
            var ox = DivMultipleFloor(rect.x, tileSize);
            Raylib.DrawLineV(
                new Vector2(ox + x * tileSize, DivMultipleFloor(rect.y, tileSize)),
                new Vector2(ox + x * tileSize, DivMultipleCeil(rect.y + rect.height, tileSize)),
                color
            );
        }
    }

    public static Rectangle GetRectWorldToScreen(Camera2D camera, Rectangle rect)
    {
        var topLeft = Raylib.GetWorldToScreen2D(new Vector2(rect.x, rect.y), camera);
        var bottomRight = Raylib.GetWorldToScreen2D(new Vector2(rect.x + rect.width, rect.y + rect.height), camera);

        var size = bottomRight - topLeft;
        return new Rectangle(
            topLeft.X,
            topLeft.Y,
            size.X,
            size.Y
        );
    }

    public static Rectangle GetRectScreenToWorld(Camera2D camera, Rectangle rect)
    {
        var topLeft = Raylib.GetScreenToWorld2D(new Vector2(rect.x, rect.y), camera);
        var bottomRight = Raylib.GetScreenToWorld2D(new Vector2(rect.x + rect.width, rect.y + rect.height), camera);

        var size = bottomRight - topLeft;
        return new Rectangle(
            topLeft.X,
            topLeft.Y,
            size.X,
            size.Y
        );
    }

    public static Rectangle GetScreenRectInWorld(Camera2D camera)
    {
        return GetRectScreenToWorld(camera, new Rectangle(
            0,
            0,
            Raylib.GetScreenWidth(),
            Raylib.GetScreenHeight()
        ));
    }

    public static void CoverOffscreenArea(Camera2D camera, Vector2 canvasSize, Color color)
    {
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var canvasSizeOnScreen = canvasSize * camera.zoom;
        var unusedSpace = screenSize - canvasSizeOnScreen;

        if (unusedSpace.X > 0)
        {
            Raylib.DrawRectangleRec(
                new Rectangle(0, 0, unusedSpace.X / 2, screenSize.Y),
                color
            );
            Raylib.DrawRectangleRec(
                new Rectangle(canvasSizeOnScreen.X + unusedSpace.X / 2, 0, unusedSpace.X / 2, screenSize.Y),
                color
            );
        }

        if (unusedSpace.Y > 0)
        {
            Raylib.DrawRectangleRec(
                new Rectangle(0, 0, screenSize.X, unusedSpace.Y / 2),
                color
            );
            Raylib.DrawRectangleRec(
                new Rectangle(0, canvasSizeOnScreen.Y + unusedSpace.Y / 2, screenSize.X, unusedSpace.Y / 2),
                color
            );
        }
    }

    public static void Main(string[] args)
    {
        var tileSize = 10;
        var canvasSize = new Vector2(320, 180);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1920, 1080, "GMTK2024");
        Raylib.SetTargetFPS(60);

        var rlTilesets = RaylibTileset.LoadAllInFolder("./assets");
        var rlTilemap = new RaylibTilemap(rlTilesets, "./assets/main.tmx");
        var camera = new Camera2D();
        camera.rotation = 0;
        camera.target = canvasSize / 2;

        // Main game loop
        while (!Raylib.WindowShouldClose()) // Detect window close button or ESC key
        {
            var dt = Raylib.GetFrameTime();
            var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            camera.offset = screenSize / 2;
            camera.zoom = Math.Min(screenSize.X / canvasSize.X, screenSize.Y / canvasSize.Y);

            { // Camera controls
                var dx = 0;
                var dy = 0;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                {
                    dx += 1;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                {
                    dx -= 1;
                }

                if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
                {
                    dy += 1;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
                {
                    dy -= 1;
                }

                camera.target += new Vector2(dx, dy) * dt * tileSize * camera.zoom;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(0x232323ff));
            
            Raylib.BeginMode2D(camera);
            {
                DrawGrid(GetScreenRectInWorld(camera), tileSize, Raylib.WHITE);
                rlTilemap.Draw();
            }
            Raylib.EndMode2D();

            CoverOffscreenArea(camera, canvasSize, Raylib.GetColor(0x232323ff));

            Raylib.DrawFPS(10, 10);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}