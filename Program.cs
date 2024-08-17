using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

internal class Program
{
    static float tileSize = 32;
    static Vector2 canvasSize = new Vector2(320 * 3, 180 * 3);

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

    public static Rectangle GetVisibleRectInWorld(Camera2D camera)
    {
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var canvasSizeOnScreen = canvasSize * camera.zoom;
        var unusedSpace = screenSize - canvasSizeOnScreen;

        return GetRectScreenToWorld(camera, new Rectangle(
            unusedSpace.X/2,
            unusedSpace.Y/2,
            canvasSizeOnScreen.X,
            canvasSizeOnScreen.Y
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

    public static Vector2? GetTileUnderMouse(Camera2D camera, Vector2 canvasSize)
    {
        var mouse = Raylib.GetMousePosition();
        var worldMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        if (!Utils.IsInsideRect(worldMouse, GetVisibleRectInWorld(camera))) return null;

        return new Vector2(
            (float)Math.Floor(worldMouse.X / tileSize),
            (float)Math.Floor(worldMouse.Y / tileSize)
        );
    }

    public static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1920, 1080, "GMTK2024");
        Raylib.SetWindowMinSize((int)canvasSize.X, (int)canvasSize.Y);
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

            var mouseTile = GetTileUnderMouse(camera, canvasSize);

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

                camera.target += new Vector2(dx, dy) * dt * tileSize * 2;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(0x232323ff));
            
            Raylib.BeginMode2D(camera);
            {
                DrawGrid(GetScreenRectInWorld(camera), tileSize, Raylib.WHITE);
                rlTilemap.Draw();

                if (mouseTile != null)
                {
                    Raylib.DrawRectangleLinesEx(new Rectangle(mouseTile.Value.X * tileSize, mouseTile.Value.Y * tileSize, tileSize, tileSize), 1, Raylib.RED);
                }
            }
            Raylib.EndMode2D();

            CoverOffscreenArea(camera, canvasSize, Raylib.GetColor(0x232323ff));

            Raylib.DrawFPS(10, 10);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}