using AsepriteDotNet.IO;
using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using TiledCS;

namespace GMTK2024;

internal class Program
{
    static float tileSize = 32;
    static Vector2 canvasSize = new Vector2(320 * 3, 180 * 3);
    static Random rng = new Random();

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

    public static Vector2? GetMouseInWorld(Camera2D camera, Vector2 canvasSize)
    {
        var mouse = Raylib.GetMousePosition();
        var worldMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        if (!Utils.IsInsideRect(worldMouse, GetVisibleRectInWorld(camera))) return null;

        return worldMouse;
    }

    static void DrawLine(List<Vector2> points, Color color)
    {
        if (points.Count < 2) return;

        for (var i = 0; i < points.Count - 1; i++)
        {
            Raylib.DrawLineV(points[i], points[i+1], color);
        }
    }

    static Tower? GetTowerAt(List<Tower> towers, Vector2 position)
    {
        foreach (var tower in towers)
        {
            if (Utils.IsInsideRect(position, tower.GetRect()))
            {
                return tower;
            }
        }

        return null;
    }

    static void CheckMergingTowers(List<Tower> towers, Vector2 position)
    {
        var offsets = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, -1),
            new Vector2(-1, 0),
            new Vector2(-1, -1),
        };

        foreach (var offset in offsets)
        {
            var foundTowers = new List<Tower>();
            for (int oy = 0; oy < 2; oy++)
            {
                for (int ox = 0; ox < 2; ox++)
                {
                    var tower = GetTowerAt(towers, position + (new Vector2(ox + 0.5f, oy + 0.5f) + offset) * tileSize);
                    if (tower == null) continue;
                    if (tower.size.X > tileSize || tower.size.Y > tileSize) continue;

                    foundTowers.Add(tower);
                }
            }

            if (foundTowers.Count == 4)
            {
                var topLeft     = foundTowers[0].position;
                var bottomRight = foundTowers[0].position + foundTowers[0].size;
                foreach (var tower in foundTowers)
                {
                    var towerTopLeft = tower.position;
                    var towerBottomRight = tower.position + tower.size;

                    topLeft = Vector2.Min(topLeft, towerTopLeft);
                    bottomRight = Vector2.Max(bottomRight, towerBottomRight);
                }

                var size = bottomRight - topLeft;

                if (size.X == size.Y)
                {
                    foreach (var tower in foundTowers)
                    {
                        towers.Remove(tower);
                    }

                    var aim = rng.NextSingle() * 2 * (float)Math.PI;
                    towers.Add(new Tower
                    {
                        position = topLeft,
                        size = size,
                        type = TowerType.Revolver,
                        createdAt = (float)Raylib.GetTime(),
                        targetAim = aim,
                        aim = aim
                    });
                }
            }
        }
    }

    public static Enemy? GetNearestEnemy(List<Enemy> enemies, Vector2 position)
    {
        if (enemies.Count == 0) return null;

        var nearestEnemy = enemies[0];
        foreach (var enemy in enemies)
        {
            if (Vector2.DistanceSquared(nearestEnemy.position, position) > Vector2.DistanceSquared(enemy.position, position))
            {
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    public static void Main(string[] args)
    {
        var assets = new Assets();

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1920, 1080, "GMTK2024");
        Raylib.SetWindowMinSize((int)canvasSize.X, (int)canvasSize.Y);
        Raylib.SetTargetFPS(60);

        var rlTilesets = RaylibTileset.LoadAll(assets);
        var rlTilemap = new RaylibTilemap(rlTilesets, assets.LoadStream("main.tmx"));
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
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.1f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.1f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 0.1f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 2.0f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 2.0f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 2.0f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 2.0f, type = EnemyType.Slime });
        currentWave.spawns.Add(new EnemyWaveSpawn { delay = 3.0f, type = EnemyType.Slime });

        var enemies = new List<Enemy>();

        var maxHealth = 100f;
        var health = maxHealth;

        var towerBaseTileset = assets.LoadAseprite("grass_tower_base_tileset.aseprite");

        var dualGridTowerBase = new DualGridTileset(
            Utils.FlattenLayerToTexture(towerBaseTileset.Frames[0], "tower_base"),
            new Vector2(tileSize, tileSize)
        );

        var dualGridTowerFoliage = new DualGridTileset(
            Utils.FlattenLayerToTexture(towerBaseTileset.Frames[0], "foliage"),
            new Vector2(tileSize, tileSize)
        );

        var revolverAse = assets.LoadAseprite("revolver.aseprite");
        var revolver = Utils.FlattenToAnimation(revolverAse);

        var slimeAse = assets.LoadAseprite("slime2.aseprite");
        var slime = Utils.FlattenTagToAnimation(slimeAse, "jump");

        var towers = new List<Tower>();
        var bullets = new List<Bullet>();

        // Main game loop
        while (!Raylib.WindowShouldClose()) // Detect window close button or ESC key
        {
            var dt = Raylib.GetFrameTime();
            var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            camera.offset = screenSize / 2;
            camera.zoom = Math.Min(screenSize.X / canvasSize.X, screenSize.Y / canvasSize.Y);

            var mouse = GetMouseInWorld(camera, canvasSize);

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

            // Towers
            {
                if (mouse != null)
                {
                    var mouseTile = new Vector2(
                        DivMultipleFloor(mouse.Value.X, tileSize),
                        DivMultipleFloor(mouse.Value.Y, tileSize)
                    );

                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        var existingTower = GetTowerAt(towers, mouseTile + new Vector2(tileSize, tileSize) / 2);
                        if (existingTower == null)
                        {
                            var aim = rng.NextSingle() * 2 * (float)Math.PI;
                            towers.Add(new Tower
                            {
                                position = mouseTile,
                                size = new Vector2(tileSize, tileSize),
                                createdAt = (float)Raylib.GetTime(),
                                targetAim = aim,
                                aim = aim
                            });

                            CheckMergingTowers(towers, mouseTile);
                        }
                    }
                }

                foreach (var tower in towers)
                {
                    tower.shootCooldown = Math.Max(tower.shootCooldown - dt, 0);
                
                    if (tower.shootCooldown == 0)
                    {
                        tower.state = TowerState.Idle;
                    }

                    if (tower.state == TowerState.Shoot)
                    {
                        revolver.UpdateAnimation(dt, ref tower.animationTimer, ref tower.animationIndex);
                    } else {
                        tower.animationIndex = 0;
                    }

                    if (tower.state == TowerState.Idle)
                    {
                        var nearestEnemy = GetNearestEnemy(enemies, tower.position);
                        if (nearestEnemy != null && Vector2.Distance(nearestEnemy.position, tower.position) < tower.range)
                        {
                            tower.targetAim = Utils.GetAimAngle(tower.Center(), nearestEnemy.position);
                            tower.target = nearestEnemy;
                        } else
                        {
                            tower.target = null;
                        }

                        if (tower.target == null)
                        {
                            tower.targetAim += (float)Math.Sin(Raylib.GetTime() + tower.createdAt) / (2*(float)Math.PI) / 10;
                        }

                        if (tower.target != null && Math.Abs(Utils.AngleDifference(tower.targetAim, tower.aim)) < 0.01)
                        {
                            tower.state = TowerState.Shoot;
                            tower.shootCooldown = revolver.GetDuration();
                            bullets.Add(new Bullet
                            {
                                position = tower.Center(),
                                speed = 200,
                                direction = new Vector2((float)Math.Cos(tower.aim), (float)Math.Sin(tower.aim))
                            });
                        }
                    }

                    tower.aim = Utils.ApproachAngle(tower.aim, tower.targetAim, dt * tower.aimSpeed);
                }
            }

            // Bullets
            {
                foreach (var bullet in bullets)
                {
                    bullet.position += bullet.direction * dt * bullet.speed;

                    foreach (var enemy in enemies)
                    {
                        if (Raylib.CheckCollisionCircleRec(bullet.position, 3, enemy.GetRect())) {
                            enemy.health = Math.Max(enemy.health -= bullet.damage, 0);
                            bullet.dead = true;
                        }
                    }

                    if (Vector2.Distance(bullet.position, camera.target) > 10_000)
                    {
                        bullet.dead = true;
                    }
                }

                for (int i = 0; i < bullets.Count; i++)
                {
                    if (bullets[i].dead)
                    {
                        bullets.RemoveAt(i);
                        i--;
                    }
                }
            }

            // Enemies
            {
                waveSpawnTimer += dt;
                while (currentWave.spawns.Count > 0 && waveSpawnTimer > currentWave.spawns[0].delay)
                {
                    var enemyHealth = 100;
                    enemies.Add(new Enemy
                    {
                        position = path[0],
                        type = currentWave.spawns[0].type,
                        size = new Vector2(slimeAse.CanvasWidth, slimeAse.CanvasHeight),
                        maxHealth = enemyHealth,
                        health = enemyHealth,
                        collisionRadius = 10
                    });
                    waveSpawnTimer -= currentWave.spawns[0].delay;

                    currentWave.spawns.RemoveAt(0);
                }

                foreach (var enemy in enemies)
                {
                    if (enemy.health == 0)
                    {
                        enemy.dead = true;
                    }

                    bool gotoNextTarget = false;
                    if (enemy.targetEndpoint > 0)
                    {
                        var line = path[enemy.targetEndpoint] - path[enemy.targetEndpoint - 1];
                        var enemyProgress = enemy.position - path[enemy.targetEndpoint - 1];

                        var progress = Vector2.Dot(line, enemyProgress) / line.Length();
                        if (progress > line.Length())
                        {
                            gotoNextTarget = true;
                        }
                    }
                    else
                    {
                        if (Vector2.Distance(path[enemy.targetEndpoint], enemy.position) < 0.01)
                        {
                            gotoNextTarget = true;
                        }
                    }

                    if (gotoNextTarget)
                    {
                        if (enemy.targetEndpoint == path.Count - 1)
                        {
                            enemy.dead = true;
                            health = Math.Max(health - 10, 0);
                        } else
                        {
                            enemy.targetEndpoint += 1;
                        }
                    }

                    var targetPosition = path[enemy.targetEndpoint];
                    enemy.targetAim = Utils.GetAimAngle(enemy.position, targetPosition);
                    enemy.aim = Utils.ApproachAngle(enemy.aim, enemy.targetAim, dt * 3);

                    if (enemy.type == EnemyType.Slime)
                    {
                        enemy.jumpCooldown = Math.Max(enemy.jumpCooldown - dt, 0);
                        slime.UpdateAnimation(dt, ref enemy.animationTimer, ref enemy.animationIndex, false);

                        if (enemy.jumpCooldown == 0)
                        {
                            enemy.jumpCooldown = rng.NextSingle() * 0.5f + 0.75f;

                            var jumpPower = rng.NextSingle() * 100 + 100;
                            enemy.velocity += Vector2.Normalize(targetPosition - enemy.position) * jumpPower;
                            enemy.animationIndex = 0;
                        }
                    }

                    foreach (var otherEnemy in enemies)
                    {
                        if (otherEnemy == enemy) continue;

                        if (Vector2.Distance(otherEnemy.position, enemy.position) < otherEnemy.collisionRadius + enemy.collisionRadius)
                        {
                            enemy.velocity += (enemy.position - otherEnemy.position);
                        }
                    }

                    enemy.velocity = Vector2.Normalize(enemy.velocity) * enemy.velocity.Length() * (1 - enemy.friction);
                    enemy.position += enemy.velocity * dt;
                }

                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].dead)
                    {
                        enemies.RemoveAt(i);
                        i--;
                    }
                }
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(0x232323ff));
            
            Raylib.BeginMode2D(camera);
            {
                DrawGrid(GetScreenRectInWorld(camera), tileSize, Raylib.WHITE);
                rlTilemap.Draw();

                foreach (var enemy in enemies)
                {
                    var rotation = Utils.ToDegrees(enemy.aim) - 90;
                    Utils.DrawTextureCentered(slime.frames[enemy.animationIndex].texture, enemy.position, rotation, 1, Raylib.WHITE);
                    Raylib.DrawCircleLines((int)enemy.position.X, (int)enemy.position.Y, enemy.collisionRadius, Raylib.RED);
                    //Raylib.DrawRectangleRec(enemy.GetRect(), Raylib.RED);
                    //Raylib.DrawCircleV(enemy.position, 1, Raylib.BLUE);
                    //Raylib.DrawLineV(enemy.position, enemy.position + new Vector2((float)Math.Cos(enemy.aim), (float)Math.Sin(enemy.aim)) * 100, Raylib.GREEN);
                }

                foreach (var bullet in bullets)
                {
                    Raylib.DrawCircleV(bullet.position, 3, Raylib.RED);
                }

                foreach (var tower in towers)
                {
                    var middle = tower.position + tower.size / 2;
                    var towerRect = tower.GetRect();
                    dualGridTowerBase.DrawRectangle(towerRect, Raylib.GetColor(0x5f5f5fFF));
                    dualGridTowerFoliage.DrawRectangle(towerRect, Raylib.WHITE);

                    if (tower.type == TowerType.Revolver)
                    {
                        var rotation = Utils.ToDegrees(tower.aim) + 90;
                        Utils.DrawTextureCentered(revolver.frames[tower.animationIndex].texture, middle, rotation, 1, Raylib.WHITE);
                        Raylib.DrawCircleLines((int)middle.X, (int)middle.Y, tower.range, Raylib.RED);
                        Raylib.DrawLineV(middle, middle + new Vector2((float)Math.Cos(tower.aim), (float)Math.Sin(tower.aim)) * 100, Raylib.GREEN);
                    }
                }

                if (mouse != null)
                {
                    var tileX = DivMultipleFloor(mouse.Value.X, tileSize);
                    var tileY = DivMultipleFloor(mouse.Value.Y, tileSize);
                    Raylib.DrawRectangleLinesEx(new Rectangle(tileX, tileY, tileSize, tileSize), 1, Raylib.RED);
                }

                DrawLine(path, Raylib.RED);
                foreach (var point in path)
                {
                    Raylib.DrawCircleV(point, 2, Raylib.RED);
                }

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