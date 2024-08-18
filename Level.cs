using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using TiledCS;

namespace GMTK2024;

internal class Level
{
    static bool debugShowPath = false;
    static bool debugAimAtMouse = false;
    static bool debugTowerInfo = false;

    RaylibTilemap tilemap;
    Random rng = new Random();
    List<Tower> towers = new List<Tower>();
    List<Bullet> bullets = new List<Bullet>();
    List<Enemy> enemies = new List<Enemy>();
    Camera2D camera = new Camera2D();
    List<Vector2> enemyPath;
    Vector2 basePosition;
    Vector2 enemySpawn;

    AnimationState homeCrystalAnimation = new AnimationState();
    float homeCrystalRotation = 0;
    float enemySpawnRotation = 0;

    List<EnemyWave> waves = new List<EnemyWave>();
    int currentWaveIndex = 0;

    float waveSpawnTimer = 0f;
    Vector2? worldMouse = null;

    float maxHealth = Program.playerHealth;
    float health = Program.playerHealth;
    int gold = Program.startingGold;
    TowerType selectedTower = TowerType.Revolver;

    bool won = false;

    UI ui = new UI();

    public Level(RaylibTilemap tilemap)
    {
        this.tilemap = tilemap;

        var baseMarker = GetMarker("base");
        basePosition = new Vector2(baseMarker.x, baseMarker.y);
        basePosition.X = DivMultipleFloor(basePosition.X, Program.tileSize) + Program.tileSize / 2;
        basePosition.Y = DivMultipleFloor(basePosition.Y, Program.tileSize) + Program.tileSize / 2;
        homeCrystalRotation = baseMarker.rotation;

        var spawnMarker = GetMarker("spawn");
        enemySpawn = new Vector2(spawnMarker.x, spawnMarker.y);
        enemySpawn.X = DivMultipleFloor(enemySpawn.X, Program.tileSize) + Program.tileSize / 2;
        enemySpawn.Y = DivMultipleFloor(enemySpawn.Y, Program.tileSize) + Program.tileSize / 2;
        enemySpawnRotation = spawnMarker.rotation;

        camera.rotation = 0;
        camera.target = Program.canvasSize / 2;

        currentWaveIndex = 0;
        waves.Add(new EnemyWave([
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
        ]));
        waves.Add(new EnemyWave([
            new() { delay = 0.1f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
            new() { delay = 0.5f, type = EnemyType.Slime },
        ]));

        enemyPath = new List<Vector2>();

        var pathLayer = tilemap.GetLayer("path", TiledLayerType.ObjectLayer);
        Debug.Assert(pathLayer != null);
        Array.Sort(pathLayer.objects, Comparer<TiledObject>.Create((a, b) => int.Parse(a.name).CompareTo(int.Parse(b.name))));

        foreach (var obj in pathLayer.objects)
        {
            if (obj.point == null) continue;

            enemyPath.Add(new Vector2(obj.x, obj.y));
        }

        enemyPath.Insert(0, enemySpawn);
        enemyPath.Add(basePosition);
    }

    public Vector2 GetMarkerPosition(string name)
    {
        var marker = GetMarker(name);

        return new Vector2(marker.x, marker.y);
    }

    public TiledObject GetMarker(string name)
    {
        var markersLayer = tilemap.GetLayer("markers", TiledLayerType.ObjectLayer);
        Debug.Assert(markersLayer != null);
        var marker = RaylibTilemap.GetObject(markersLayer, name);
        Debug.Assert(marker != null);

        return marker;
    }

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

    public Rectangle GetVisibleRectInWorld(Camera2D camera)
    {
        return GetRectScreenToWorld(camera, GetOnscreenArea());
    }

    public Rectangle GetOnscreenArea()
    {
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var canvasSizeOnScreen = Program.canvasSize * camera.zoom;
        var unusedSpace = screenSize - canvasSizeOnScreen;

        return new Rectangle(
            unusedSpace.X / 2,
            unusedSpace.Y / 2,
            canvasSizeOnScreen.X,
            canvasSizeOnScreen.Y
        );
    }

    public void CoverOffscreenArea(Color color)
    {
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var onscreenArea = GetOnscreenArea();

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

    public Vector2? GetMouseInWorld(Camera2D camera)
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
            Raylib.DrawLineV(points[i], points[i + 1], color);
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

    void CheckMergingTowers(List<Tower> towers, Vector2 position)
    {
        var tileSize = Program.tileSize;

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

                    foundTowers.Add(tower);
                }
            }

            if (foundTowers.Count != 4) continue;

            var towerType = foundTowers[0].type;
            { // Check if all of the found towers are of the same type
                var towerTypeMatches = true;
                foreach (var tower in foundTowers)
                {
                    if (tower.type != towerType)
                    {
                        towerTypeMatches = false;
                        break;
                    }
                }
                if (!towerTypeMatches) continue;
            }

            var topLeft = foundTowers[0].position;
            var bottomRight = foundTowers[0].position + foundTowers[0].size;
            foreach (var tower in foundTowers)
            {
                var towerTopLeft = tower.position;
                var towerBottomRight = tower.position + tower.size;

                topLeft = Vector2.Min(topLeft, towerTopLeft);
                bottomRight = Vector2.Max(bottomRight, towerBottomRight);
            }

            var size = bottomRight - topLeft;

            // If the found towers don't create a square shape, skip.
            if (size.X != size.Y) continue;

            TowerType biggerTowerType;
            if (towerType == TowerType.Revolver)
            {
                biggerTowerType = TowerType.BigRevolver;
            } else
            {
                // Merging for this tower type is not supported
                continue;
            }

            foreach (var tower in foundTowers)
            {
                towers.Remove(tower);
            }

            var aim = rng.NextSingle() * 2 * (float)Math.PI;
            towers.Add(Tower.Create(biggerTowerType, topLeft, size, aim));
        }
    }

    public static Enemy? GetNearestEnemy(List<Enemy> enemies, Vector2 position, float minRange, float maxRange)
    {
        if (enemies.Count == 0) return null;

        Enemy? nearestEnemy = null;
        foreach (var enemy in enemies)
        {
            var distance = Vector2.Distance(enemy.position, position);
            if (distance <= minRange) continue;
            if (distance >= maxRange) continue;

            if (nearestEnemy == null || Vector2.Distance(nearestEnemy.position, position) > distance)
            {
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    public bool IsTowerPlaceable(Vector2 position)
    {
        var ground = tilemap.GetLayer("ground", TiledLayerType.TileLayer);
        Debug.Assert(ground != null);

        var tileX = (int)(position.X / Program.tileSize);
        var tileY = (int)(position.Y / Program.tileSize);
        var gid = RaylibTilemap.GetTileAt(ground, tileX, tileY);
        if (gid != 0)
        {
            var mapTileset = tilemap.map.GetTiledMapTileset(gid);
            var rlTileset = tilemap.rlTilesets[mapTileset.firstgid];
            var tile = rlTileset.GetTile(gid - mapTileset.firstgid);
            if (tile == null)
            {
                return false;
            }

            if (!Utils.GetBoolTiledProperty(tile.properties, "tower-placeable", false))
            {
                return false;
            }
        }

        var existingTower = GetTowerAt(
            towers,
            new Vector2(tileX + 0.5f, tileY + 0.5f) * Program.tileSize
        );
        if (existingTower != null)
        {
            return false;
        }

        return true;
    }

    public bool IsWaveFinished()
    {
        return waves[currentWaveIndex].spawns.Count == 0;
    }

    public void DrawTower(Tower tower)
    {
        var middle = tower.position + tower.size / 2;
        var towerRect = tower.GetRect();
        Program.towerPlatformMain.DrawRectangle(towerRect, Raylib.GetColor(0x5f5f5fFF));
        Program.towerPlatformFoliage.DrawRectangle(towerRect, Raylib.WHITE);

        var aim = tower.aim + (float)Math.PI / 2;
        var aimDegress = Utils.ToDegrees(aim);
        if (tower.type == TowerType.Revolver)
        {
            Program.revolver.DrawCentered(tower.animation, middle + tower.recoil, aimDegress, 1, Raylib.WHITE);
        }
        else if (tower.type == TowerType.BigRevolver)
        {
            Program.bigRevolverUnderbelly.DrawCentered(tower.animation, middle, aimDegress, 1, Raylib.WHITE);
            Program.bigRevolverLeftAmmo.DrawCentered(tower.leftGunAnimation, middle, aimDegress, 1, Raylib.WHITE);
            Program.bigRevolverRightAmmo.DrawCentered(tower.rightGunAnimation, middle, aimDegress, 1, Raylib.WHITE);
            Program.bigRevolverAmmoRack.DrawCentered(tower.animation, middle, aimDegress, 1, Raylib.WHITE);

            Program.bigRevolverRightGun.Draw(tower.rightGunAnimation, tower.GetRightGunCenter(), Program.bigRevolverRightPivot, Utils.ToDegrees(aim + tower.rightAim), 1, Raylib.WHITE);
            Program.bigRevolverLeftGun.Draw(tower.leftGunAnimation, tower.GetLeftGunCenter(), Program.bigRevolverLeftPivot, Utils.ToDegrees(aim + tower.leftAim), 1, Raylib.WHITE);

        }
        else if (tower.type == TowerType.Mortar)
        {
            Program.mortarReload.Draw(tower.animation, middle, Program.mortarPivot, aimDegress, 1, Raylib.WHITE);
        }
        
        if (debugTowerInfo)
        {
            Raylib.DrawCircleLines((int)middle.X, (int)middle.Y, tower.maxRange, Raylib.RED);
            Raylib.DrawCircleLines((int)middle.X, (int)middle.Y, tower.minRange, Raylib.BLUE);
            Raylib.DrawLineV(middle, middle + Utils.GetAngledVector2(tower.aim, 100), Raylib.GREEN);
        }
    }

    public void TryShootingBullet(Tower tower)
    {

        if (tower.type == TowerType.Revolver)
        {
            if (tower.shootCooldown == 0)
            {
                var aimDirection = Utils.GetAngledVector2(tower.aim);
                tower.reloaded = false;
                tower.shootCooldown = Program.revolver.GetDuration() * 1.1f;
                bullets.Add(new Bullet
                {
                    position = tower.Center(),
                    speed = Program.revolverBulletSpeed,
                    damage = Program.revolverBulletDamage,
                    direction = aimDirection
                });
                Raylib.PlaySoundMulti(Program.revolverGunshot);
                tower.recoil = -aimDirection * 8;
            }
        }
        else if (tower.type == TowerType.Mortar)
        {
            if (tower.shootCooldown == 0)
            {
                var aimDirection = Utils.GetAngledVector2(tower.aim);
                tower.fired = true;
                tower.reloaded = false;
                tower.shootCooldown = (Program.mortarReload.GetDuration() + Program.mortarFire.GetDuration()) * 1.1f;
                bullets.Add(new Bullet
                {
                    position = tower.Center(),
                    speed = Program.revolverBulletSpeed,
                    damage = Program.revolverBulletDamage,
                    direction = aimDirection
                });
                Raylib.PlaySoundMulti(Program.mortarGunshot);
                tower.recoil = -aimDirection;
            }
        }
        else if (tower.type == TowerType.BigRevolver)
        {
            var bigRevolverCooldown = Program.bigRevolverLeftGun.GetDuration() * 1.1f;

            if (Utils.IsAngleClose(tower.leftTargetAim, tower.leftAim) && tower.leftShootCooldown == 0)
            {
                tower.leftReloaded = false;
                tower.leftShootCooldown = bigRevolverCooldown;
                bullets.Add(new Bullet
                {
                    position = tower.GetLeftGunCenter(),
                    speed = Program.bigRevolverBulletSpeed,
                    damage = Program.bigRevolverBulletDamage,
                    direction = Utils.GetAngledVector2(tower.aim + tower.leftAim)
                });
                Raylib.PlaySoundMulti(Program.bigRevolverGunshot);
            }

            if (Utils.IsAngleClose(tower.leftTargetAim, tower.leftAim) && tower.rightShootCooldown == 0 && tower.leftShootCooldown < bigRevolverCooldown / 2)
            {
                tower.rightReloaded = false;
                tower.rightShootCooldown = bigRevolverCooldown;
                bullets.Add(new Bullet
                {
                    position = tower.GetRightGunCenter(),
                    speed = Program.bigRevolverBulletSpeed,
                    damage = Program.bigRevolverBulletDamage,
                    direction = Utils.GetAngledVector2(tower.aim + tower.rightAim)
                });
                Raylib.PlaySoundMulti(Program.bigRevolverGunshot);
            }
        }
    }

    public void UpdateTower(Tower tower, float dt)
    {
        // Update animations
        {
            if (tower.recoil.Length() > 0.01)
            {
                tower.recoil = Vector2.Normalize(tower.recoil) * tower.recoil.Length() * 0.8f;
            } else
            {
                tower.recoil = Vector2.Zero;
            }

            tower.shootCooldown = Math.Max(tower.shootCooldown - dt, 0);
            tower.leftShootCooldown = Math.Max(tower.leftShootCooldown - dt, 0);
            tower.rightShootCooldown = Math.Max(tower.rightShootCooldown - dt, 0);

            if (tower.type == TowerType.Revolver)
            {
                Program.revolver.PlayOnce(dt, ref tower.animation, ref tower.reloaded);
            }
            else if (tower.type == TowerType.BigRevolver)
            {
                Program.revolver.PlayOnce(dt, ref tower.leftGunAnimation, ref tower.leftReloaded);
                Program.revolver.PlayOnce(dt, ref tower.rightGunAnimation, ref tower.rightReloaded);
            }
            else if (tower.type == TowerType.Mortar)
            {
                if (tower.fired)
                {
                    if (Program.mortarFire.UpdateOnce(dt, ref tower.animation))
                    {
                        tower.fired = false;
                    }
                } else
                {
                    Program.mortarReload.PlayOnce(dt, ref tower.animation, ref tower.reloaded);
                }
            }
        }


        // Find target
        {
            tower.targetPosition = null;
            var nearestEnemy = GetNearestEnemy(enemies, tower.position, tower.minRange, tower.maxRange);
            if (nearestEnemy != null)
            {
                tower.targetPosition = nearestEnemy.position;
            }

            if (debugAimAtMouse && worldMouse != null)
            {
                tower.targetPosition = worldMouse.Value;
            }

            if (tower.targetPosition != null)
            {
                var targetPosition = tower.targetPosition.Value;
                tower.targetAim = Utils.GetAimAngle(tower.Center(), targetPosition);

                if (tower.type == TowerType.BigRevolver && Utils.IsAngleClose(tower.targetAim, tower.aim))
                {
                    tower.leftTargetAim = Utils.GetAimAngle(tower.GetLeftGunCenter(), targetPosition) - tower.aim;
                    tower.rightTargetAim = Utils.GetAimAngle(tower.GetRightGunCenter(), targetPosition) - tower.aim;
                }
            }
            else
            {
                var seed = Raylib.GetTime() + tower.createdAt;
                tower.targetAim += (float)Math.Sin(seed) / (2 * (float)Math.PI) / 10;

                if (tower.type == TowerType.BigRevolver)
                {
                    tower.leftTargetAim += (float)Math.Sin(seed + 100) / (2 * (float)Math.PI) / 20;
                    tower.rightTargetAim -= (float)Math.Sin(seed + 200) / (2 * (float)Math.PI) / 20;
                }
            }
        }

        if (tower.targetPosition != null && Utils.IsAngleClose(tower.targetAim, tower.aim))
        {
            TryShootingBullet(tower);
        }

        bool canAim = false;
        if (tower.type == TowerType.Revolver)
        {
            canAim = tower.shootCooldown == 0;
        }
        else if (tower.type == TowerType.BigRevolver)
        {
            canAim = tower.leftShootCooldown == 0 || tower.rightShootCooldown == 0;
        }
        else if (tower.type == TowerType.Mortar)
        {
            canAim = tower.reloaded;
        }

        if (canAim)
        {
            tower.aim = Utils.ApproachAngle(tower.aim, tower.targetAim, dt * tower.aimSpeed);
            if (tower.type == TowerType.BigRevolver)
            {
                tower.leftAim = Utils.ApproachAngle(tower.leftAim, tower.leftTargetAim, dt * tower.aimSpeed);
                tower.rightAim = Utils.ApproachAngle(tower.rightAim, tower.rightTargetAim, dt * tower.aimSpeed);
            }
        }
    }
    
    public void Tick()
    {
        var canvasSize = Program.canvasSize;
        var tileSize = Program.tileSize;

        var dt = Raylib.GetFrameTime();
        var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        camera.offset = screenSize / 2;
        camera.zoom = Math.Min(screenSize.X / canvasSize.X, screenSize.Y / canvasSize.Y);

        // UI
        {
            ui.Begin(GetOnscreenArea(), canvasSize);

            Raylib.DrawFPS(10, 10);

            if (!won)
            {
                Raylib.DrawText($"Enemies: {enemies.Count}", 10, 30, 10, Raylib.WHITE);
                Raylib.DrawText($"Wave: {currentWaveIndex + 1}/{waves.Count}", 10, 40, 10, Raylib.WHITE);

                Utils.DrawTextureCentered(Program.coin, new Vector2(20, 70), 0, 0.75f, Raylib.WHITE);
                Raylib.DrawText($"{gold}", 30, 53, 30, Raylib.GOLD);

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

                if (IsWaveFinished() && currentWaveIndex < waves.Count - 1 && ui.ShowButton(new Rectangle(10, canvasSize.Y - 20 - 10, 100, 20), "Next wave"))
                {
                    currentWaveIndex++;
                }
            }
            else
            {
                var center = canvasSize / 2;
                var font = Raylib.GetFontDefault();
                Utils.DrawTextCentered(font, "You win!", center, 50, 5, Raylib.GREEN);

                if (ui.ShowButton(new(center.X - 100, center.Y + 80, 200, 20), "Exit"))
                {
                    Program.running = false;
                }
            }

            ui.End();
        }

        worldMouse = null;
        if (!ui.hot)
        {
            worldMouse = GetMouseInWorld(camera);
        }

        // Towers
        {
            if (worldMouse != null)
            {
                var mouseTile = new Vector2(
                    DivMultipleFloor(worldMouse.Value.X, tileSize),
                    DivMultipleFloor(worldMouse.Value.Y, tileSize)
                );
                var towerCost = Tower.GetCost(selectedTower);

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && gold >= towerCost && IsTowerPlaceable(mouseTile))
                {
                    towers.Add(Tower.Create(
                        selectedTower,
                        mouseTile,
                        new Vector2(tileSize, tileSize),
                        rng.NextSingle() * 2 * (float)Math.PI
                    ));

                    CheckMergingTowers(towers, mouseTile);

                    gold -= towerCost;
                }
            }

            foreach (var tower in towers)
            {
                UpdateTower(tower, dt);
            }
        }

        // Bullets
        {
            foreach (var bullet in bullets)
            {
                bullet.position += bullet.direction * dt * bullet.speed;

                foreach (var enemy in enemies)
                {
                    if (enemy.dead) continue;

                    if (Raylib.CheckCollisionCircleRec(bullet.position, 3, enemy.GetRect()))
                    {
                        enemy.health = Math.Max(enemy.health -= bullet.damage, 0);
                        gold += enemy.goldValue;
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
            var currentWave = waves[currentWaveIndex];

            waveSpawnTimer += dt;
            while (currentWave.spawns.Count > 0 && (waveSpawnTimer > currentWave.spawns[0].delay || enemies.Count == 0))
            {
                var enemyHealth = 100;
                enemies.Add(new Enemy
                {
                    position = enemyPath[0] + new Vector2(rng.NextSingle(), rng.NextSingle()) * 4,
                    targetEndpoint = 1,
                    type = currentWave.spawns[0].type,
                    goldValue = Program.slimeGoldDrop,
                    state = EnemyState.SlimeCooldown,
                    size = Program.slimeJump.size,
                    maxHealth = enemyHealth,
                    health = enemyHealth,
                    collisionRadius = 10
                });
                waveSpawnTimer = Math.Max(waveSpawnTimer - currentWave.spawns[0].delay, 0);

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
                    var line = enemyPath[enemy.targetEndpoint] - enemyPath[enemy.targetEndpoint - 1];
                    var enemyProgress = enemy.position - enemyPath[enemy.targetEndpoint - 1];

                    var progress = Vector2.Dot(line, enemyProgress) / line.Length();
                    if (progress > line.Length())
                    {
                        gotoNextTarget = true;
                    }
                }
                else
                {
                    if (Vector2.Distance(enemyPath[enemy.targetEndpoint], enemy.position) < 0.01)
                    {
                        gotoNextTarget = true;
                    }
                }

                if (gotoNextTarget)
                {
                    if (enemy.targetEndpoint == enemyPath.Count - 1)
                    {
                        enemy.dead = true;
                        health = Math.Max(health - 10, 0);
                    }
                    else
                    {
                        enemy.targetEndpoint += 1;
                    }
                }

                var targetPosition = enemyPath[enemy.targetEndpoint];
                enemy.targetAim = Utils.GetAimAngle(enemy.position, targetPosition);
                enemy.aim = Utils.ApproachAngle(enemy.aim, enemy.targetAim, dt * 3);

                if (enemy.type == EnemyType.Slime)
                {
                    enemy.jumpCooldown = Math.Max(enemy.jumpCooldown - dt, 0);

                    if (enemy.state == EnemyState.SlimeCooldown)
                    {
                        if (enemy.jumpCooldown == 0)
                        {
                            enemy.state = EnemyState.SlimeWindup;
                        }
                    }
                    else if (enemy.state == EnemyState.SlimeJump)
                    {
                        if (Program.slimeJump.UpdateOnce(dt, ref enemy.animation))
                        {
                            enemy.animation.frame = 0;
                            enemy.state = EnemyState.SlimeCooldown;
                        }
                    }
                    else if (enemy.state == EnemyState.SlimeWindup)
                    {
                        if (Program.slimeWindup.UpdateOnce(dt, ref enemy.animation))
                        {
                            enemy.jumpCooldown = rng.NextSingle() * 0.5f + 0;

                            var jumpPower = rng.NextSingle() * 100 + 100;
                            enemy.velocity += Vector2.Normalize(targetPosition - enemy.position) * jumpPower;
                            enemy.animation.frame = 0;
                            enemy.state = EnemyState.SlimeJump;
                        }
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

                if (enemy.velocity.X != 0 || enemy.velocity.Y != 0)
                {
                    enemy.velocity = Vector2.Normalize(enemy.velocity) * enemy.velocity.Length() * (1 - enemy.friction);
                }
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

        if (IsWaveFinished() && currentWaveIndex == waves.Count - 1 && enemies.Count == 0)
        {
            won = true;
        }

        Program.homeCrystal.UpdateLooped(dt, ref homeCrystalAnimation);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.GetColor(0x232323ff));

        Raylib.BeginMode2D(camera);
        {
            //DrawGrid(GetScreenRectInWorld(camera), tileSize, Raylib.WHITE);
            tilemap.Draw();

            Utils.DrawTextureCentered(Program.enemySpawner, enemySpawn, enemySpawnRotation, 1, Raylib.WHITE);
            Program.homeCrystal.DrawCentered(homeCrystalAnimation.frame, basePosition, homeCrystalRotation, 1, Raylib.WHITE);

            foreach (var enemy in enemies)
            {
                if (enemy.type == EnemyType.Slime)
                {
                    var rotation = Utils.ToDegrees(enemy.aim) - 90;
                    if (enemy.state == EnemyState.SlimeWindup)
                    {
                        Program.slimeWindup.DrawCentered(enemy.animation.frame, enemy.position, rotation, 1, Raylib.WHITE);
                    }
                    else if (enemy.state == EnemyState.SlimeJump)
                    {
                        Program.slimeJump.DrawCentered(enemy.animation.frame, enemy.position, rotation, 1, Raylib.WHITE);
                    }
                    else if (enemy.state == EnemyState.SlimeCooldown)
                    {
                        Program.slimeWindup.DrawCentered(0, enemy.position, rotation, 1, Raylib.WHITE);
                    }
                }

                Raylib.DrawCircleLines((int)enemy.position.X, (int)enemy.position.Y, enemy.collisionRadius, Raylib.GREEN);
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
                DrawTower(tower);
            }

            if (worldMouse != null)
            {
                var tileX = DivMultipleFloor(worldMouse.Value.X, tileSize);
                var tileY = DivMultipleFloor(worldMouse.Value.Y, tileSize);
                var color = IsTowerPlaceable(worldMouse.Value) ? Raylib.GREEN : Raylib.RED;
                Raylib.DrawRectangleLinesEx(new Rectangle(tileX, tileY, tileSize, tileSize), 1, color);
            }

            if (debugShowPath)
            {
                DrawLine(enemyPath, Raylib.RED);
                foreach (var point in enemyPath)
                {
                    Raylib.DrawCircleV(point, 2, Raylib.RED);
                }

                Raylib.DrawCircleLines((int)basePosition.X, (int)basePosition.Y, tileSize / 3, Raylib.YELLOW);
            }
        }
        Raylib.EndMode2D();

        ui.Draw();

        CoverOffscreenArea(Raylib.GetColor(0x232323ff));

        Raylib.EndDrawing();
    }
}
