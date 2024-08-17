using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.IO;
using Raylib_CsLo;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.JavaScript;
using TiledCS;

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
        return GetRectScreenToWorld(camera, GetOnscreenArea(camera));
    }

    public static Rectangle GetOnscreenArea(Camera2D camera)
    {
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var canvasSizeOnScreen = canvasSize * camera.zoom;
        var unusedSpace = screenSize - canvasSizeOnScreen;

        return new Rectangle(
            unusedSpace.X / 2,
            unusedSpace.Y / 2,
            canvasSizeOnScreen.X,
            canvasSizeOnScreen.Y
        );
    }

    public static void CoverOffscreenArea(Camera2D camera, Color color)
    {
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var onscreenArea = GetOnscreenArea(camera);

        if (onscreenArea.X > 0)
        {
            var rightEdge = onscreenArea.X + onscreenArea.width;

            Raylib.DrawRectangleRec(
                new Rectangle(0, 0, onscreenArea.X, screenSize.X),
                color
            );
            Raylib.DrawRectangleRec(
                new Rectangle(rightEdge, 0, screenSize.X - rightEdge, screenSize.Y),
                color
            );
        }

        if (onscreenArea.Y > 0)
        {
            var bottomEdge = onscreenArea.Y + onscreenArea.height;

            Raylib.DrawRectangleRec(
                new Rectangle(0, 0, screenSize.X, onscreenArea.Y),
                color
            );
            Raylib.DrawRectangleRec(
                new Rectangle(0, bottomEdge, screenSize.X, screenSize.Y - bottomEdge),
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

    static void DrawLine(List<Vector2> points, Color color)
    {
        if (points.Count < 2) return;

        for (var i = 0; i < points.Count - 1; i++)
        {
            Raylib.DrawLineV(points[i], points[i+1], color);
        }
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

        var path = new List<Vector2>();

        var pathLayer = rlTilemap.GetLayer("path", TiledLayerType.ObjectLayer);
        Debug.Assert(pathLayer != null);
        Array.Sort(pathLayer.objects, Comparer<TiledObject>.Create((a, b) => a.name.CompareTo(b.name)));

        foreach (var obj in pathLayer.objects)
        {
            if (obj.point == null) continue;

            path.Add(new Vector2(obj.x, obj.y));
        }

        var markersLayer = rlTilemap.GetLayer("markers", TiledLayerType.ObjectLayer);
        Debug.Assert(markersLayer != null);
        var baseMarker = RaylibTilemap.GetObject(markersLayer, "base");
        Debug.Assert(baseMarker != null);

        var basePosition = new Vector2(baseMarker.x, baseMarker.y);

        path.Add(basePosition);

        var currentWave = new EnemyWave();
        var waveSpawnTimer = 0f;
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.1f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.5f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.5f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.5f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 1.0f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 1.0f, type = EnemyType.Slime });

        var enemies = new List<Enemy>();

        var maxHealth = 100f;
        var health = maxHealth;

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

            waveSpawnTimer += dt;
            while (currentWave.spawns.Count > 0 && waveSpawnTimer > currentWave.spawns[0].delay)
            {
                enemies.Add(new Enemy
                {
                    position = path[0],
                    type = currentWave.spawns[0].type
                });
                waveSpawnTimer -= currentWave.spawns[0].delay;

                currentWave.spawns.RemoveAt(0);
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(0x232323ff));
            
            Raylib.BeginMode2D(camera);
            {
                DrawGrid(GetScreenRectInWorld(camera), tileSize, Raylib.WHITE);
                rlTilemap.Draw();

                foreach (var enemy in enemies)
                {
                    if (Vector2.Distance(path[enemy.targetEndpoint], enemy.position) < 8)
                    {
                        if (enemy.targetEndpoint < path.Count - 1)
                        {
                            enemy.targetEndpoint += 1;
                        }
                        else
                        {
                            enemy.alive = false;
                            health = Math.Max(health - 10, 0);
                        }
                    }
                    
                    var enemySpeed = 32* 5;

                    var targetPosition = path[enemy.targetEndpoint];
                    var distanceToTarget = Vector2.Distance(targetPosition, enemy.position);
         
                    if (distanceToTarget > 0)
                    {
                        var velocity = Vector2.Normalize(targetPosition - enemy.position) * enemySpeed;
                        var step = velocity * dt;
                        if (step.Length() > distanceToTarget)
                        {
                            step = Vector2.Normalize(step) * distanceToTarget;
                        }
                        enemy.position += step;
                    }

                    var size = new Vector2(16, 16);
                    Raylib.DrawRectangleV(enemy.position - size / 2, size, Raylib.PURPLE);
                }

                for (int i = 0; i < enemies.Count; i++)
                {
                    if (!enemies[i].alive)
                    {
                        enemies.RemoveAt(i);
                        i--;
                    }
                }

                if (mouseTile != null)
                {
                    Raylib.DrawRectangleLinesEx(new Rectangle(mouseTile.Value.X * tileSize, mouseTile.Value.Y * tileSize, tileSize, tileSize), 1, Raylib.RED);
                }

                DrawLine(path, Raylib.RED);

                Raylib.DrawCircleLines((int)basePosition.X, (int)basePosition.Y, tileSize/3, Raylib.YELLOW);
            }

            Raylib.EndMode2D();

            { // UI
                RlGl.rlPushMatrix();
                var onscreenArea = GetOnscreenArea(camera);
                RlGl.rlTranslatef(onscreenArea.x, onscreenArea.y, 0);
                RlGl.rlScalef(camera.zoom, camera.zoom, 1);

                Raylib.DrawFPS(10, 10);

                var healthbarWidth = canvasSize.X * 0.75f;
                var healthbarContainer = new Rectangle(
                    (canvasSize.X - healthbarWidth) / 2,
                    10,
                    healthbarWidth,
                    32
                );
                Raylib.DrawRectangleRec(healthbarContainer, Raylib.GRAY);

                var maxHealthbarRect = Utils.ShrinkRect(healthbarContainer, 8);
                Raylib.DrawRectangleRec(maxHealthbarRect, Raylib.DARKGRAY);

                var healtbarRect = maxHealthbarRect;
                healtbarRect.width *= (health / maxHealth);
                Raylib.DrawRectangleRec(healtbarRect, Raylib.GREEN);

                RlGl.rlPopMatrix();
            }

            CoverOffscreenArea(camera, Raylib.GetColor(0x232323ff));

            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}