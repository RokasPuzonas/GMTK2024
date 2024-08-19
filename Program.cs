using Raylib_CsLo;
using System.Numerics;
using System.Reflection;

namespace GMTK2024;

internal class Program
{
    public static bool running = true;

    public static float tileSize = 32;
    public static Vector2 canvasSize = new Vector2(320 * 3, 180 * 3);

    public static int   startingGold = 1000;
    public static float playerHealth = 100;
    public static float bulletColliderRadius = 3;
    public static Func<float, float> bulletShellScale = p => (1 - Math.Max(p - 0.75f, 0));
    public static float bulletShellDespawnStart    = 120f;
    public static float bulletShellDespawnDuration = 3f;

    public static int   revolverCost = 25;
    public static float revolverAimSpeed = (float)Math.PI;
    public static float revolverBulletSpeed = 200;
    public static int   revolverBulletDamage = 50;
    public static int   revolverBulletPierce = 2;
    public static int   revolverBulletSmear = 5;
    public static float revolverBulletKnockback = 150f;
    public static float revolverMinRange = 10f;
    public static float revolverMaxRange = 200f;
    public static Func<Random, float> revolverShellLaunchPower = rng => Utils.RandRange(rng, 25, 75);
    public static Func<Random, float> revolverShellAngle = rng => Utils.RandRange(rng, -1, 1) * (float)Math.PI / 6;

    public static float bigRevolverAimSpeed = (float)Math.PI / 2;
    public static float bigRevolverBulletSpeed = 400;
    public static int   bigRevolverBulletDamage = 100;
    public static int   bigRevolverBulletPierce = 10;
    public static float bigRevolverBulletKnockback = 50f;
    public static int   bigRevolverBulletSmear = 10;
    public static float bigRevolverMinRange = 50f;
    public static float bigRevolverMaxRange = 350f;
    public static Func<Random, float> bigRevolverShellLaunchPower = rng => Utils.RandRange(rng, 50, 150);
    public static Func<Random, float> bigRevolverShellAngle = rng => Utils.RandRange(rng, -1, 1) * (float)Math.PI / 12;

    public static int   mortarCost = 50;
    public static float mortarAimSpeed = (float)Math.PI / 3;
    public static float mortarBulletSpeed = 100;
    public static int   mortarBulletDamage = 100;
    public static float mortarBulletKnockback = 300f;
    public static float mortarBulletRadius = 60;
    public static int   mortarBulletSmear = 10;
    public static float mortarBulletMinHeightScale = 1f;
    public static float mortarBulletMaxHeightScale = 2.5f;
    public static float mortarMinRange = 75f;
    public static float mortarMaxRange = 400f;
    public static Func<Random, float> mortarShellLaunchPower = rng => Utils.RandRange(rng, 25, 50);
    public static Func<Random, float> mortarShellAngle = rng => Utils.RandRange(rng, -1, 1) * (float)Math.PI / 48;

    public static int   slimeHealth = 100;
    public static float slimeCollisionRadius = 10;
    public static int   slimeGoldDrop = 5;
    public static float slimeDamage = 10;
    public static Func<Random, float> slimeJumpStrength = rng => Utils.RandRange(rng, 100, 200);
    public static Func<Random, float> slimeJumpCooldown = rng => Utils.RandRange(rng, 0, 0.5f);


    public static Assets assets;
    public static Dictionary<string, RaylibTileset> tilesets;
    public static DualGridTileset towerPlatformMain;
    public static DualGridTileset towerPlatformFoliage;
    public static Texture enemySpawner;
    public static Texture coin;
    public static RaylibAnimation homeCrystal;
    
    public static RaylibAnimation revolver;
    public static Texture         revolverBullet;
    public static Texture         revolverShell;
    public static Vector2         revolverNozzle;
    public static Sound           revolverGunshot;

    public static RaylibAnimation bigRevolverUnderbelly;
    public static RaylibAnimation bigRevolverAmmoRack;
    public static RaylibAnimation bigRevolverLeftGun;
    public static RaylibAnimation bigRevolverRightGun;
    public static RaylibAnimation bigRevolverLeftAmmo;
    public static RaylibAnimation bigRevolverRightAmmo;
    public static Vector2         bigRevolverLeftPivot;
    public static Vector2         bigRevolverRightPivot;
    public static Vector2         bigRevolverLeftNozzle;
    public static Vector2         bigRevolverRightNozzle;
    public static Texture         bigRevolverBullet;
    public static Texture         bigRevolverShell;
    public static Sound           bigRevolverGunshot;
    
    public static RaylibAnimation mortarReload;
    public static RaylibAnimation mortarFire;
    public static Vector2         mortarPivot;
    public static Texture         mortarBullet;
    public static Texture         mortarShell;
    public static Vector2         mortarNozzle;
    public static Sound           mortarGunshot;
    
    public static RaylibAnimation slimeJump;
    public static RaylibAnimation slimeWindup;
    public static Sound           slimeJumpSound;

    public static void Main(string[] args)
    {
        assets = new Assets();

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1920, 1080, "GMTK2024");
        Raylib.InitAudioDevice();
        Raylib.SetWindowMinSize((int)canvasSize.X, (int)canvasSize.Y);
        Raylib.SetTargetFPS(60);

        // Load assets
        {
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
            revolverNozzle = Utils.GetSlicePivot(revolverAse, "nozzle");

            var slimeAse = assets.LoadAseprite("slime2.aseprite");
            slimeWindup = Utils.FlattenTagToAnimation(slimeAse, "windup");
            slimeJump = Utils.FlattenTagToAnimation(slimeAse, "jump");

            coin = assets.LoadAsepriteTexture("coin.aseprite");
            
            revolverGunshot = assets.LoadSound("hard_gunshot.wav");
            Raylib.SetSoundVolume(revolverGunshot, 0.15f);

            bigRevolverGunshot = assets.LoadSound("big_gunshot.wav");
            Raylib.SetSoundVolume(bigRevolverGunshot, 0.15f);

            mortarGunshot = assets.LoadSound("mortar_gunshot.wav");
            Raylib.SetSoundVolume(mortarGunshot, 0.25f);

            enemySpawner = assets.LoadAsepriteTexture("spawner.aseprite");
            
            var homeCrystalAse = assets.LoadAseprite("end.aseprite");
            homeCrystal = Utils.FlattenToAnimation(homeCrystalAse);

            var bigRevolverAse = assets.LoadAseprite("big_revolver.aseprite");
            bigRevolverLeftGun     = Utils.FlattenLayerToAnimation(bigRevolverAse, "left gub");
            bigRevolverRightGun    = Utils.FlattenLayerToAnimation(bigRevolverAse, "right gub");
            bigRevolverLeftAmmo    = Utils.FlattenLayerToAnimation(bigRevolverAse, "ammo left");
            bigRevolverRightAmmo   = Utils.FlattenLayerToAnimation(bigRevolverAse, "ammo right");
            bigRevolverAmmoRack    = Utils.FlattenLayerToAnimation(bigRevolverAse, "ammo rack");
            bigRevolverUnderbelly  = Utils.FlattenLayerToAnimation(bigRevolverAse, "underbelly");
            bigRevolverLeftPivot   = Utils.GetSlicePivot(bigRevolverAse, "left gub pivot");
            bigRevolverRightPivot  = Utils.GetSlicePivot(bigRevolverAse, "right gun pivot");
            bigRevolverLeftNozzle  = Utils.GetSlicePivot(bigRevolverAse, "left nozzle");
            bigRevolverRightNozzle = Utils.GetSlicePivot(bigRevolverAse, "right nozzle");

            var mortarAse = assets.LoadAseprite("mortar.aseprite");
            mortarFire   = Utils.FlattenTagToAnimation(mortarAse, "fire");
            mortarReload = Utils.FlattenTagToAnimation(mortarAse, "reload");
            mortarPivot = Utils.GetSlicePivot(mortarAse, "gun pivot point");
            mortarNozzle = Utils.GetSlicePivot(mortarAse, "nozzle");

            revolverBullet = assets.LoadAsepriteTexture("revolver_bullet.aseprite");
            
            bigRevolverBullet = assets.LoadAsepriteTexture("big_revolver_bullet.aseprite");
            
            mortarBullet = assets.LoadAsepriteTexture("mortar_bullet.aseprite");
            
            slimeJumpSound = assets.LoadSound("uhwra.wav");
            Raylib.SetSoundVolume(slimeJumpSound, 0.5f);

            revolverShell = assets.LoadAsepriteTexture("revolver_shell.aseprite");
            
            bigRevolverShell = assets.LoadAsepriteTexture("big_revolver_shell.aseprite");

            mortarShell = assets.LoadAsepriteTexture("mortar_shell.aseprite");
        }

        var tilemap = new RaylibTilemap(tilesets, assets.LoadStream("main.tmx"));
        var currentLevel = new Level(tilemap);

        var mainmenu = false;
        var ui = new UI();

        while (!Raylib.WindowShouldClose() && running) 
        {
            if (mainmenu)
            {
                var font = Raylib.GetFontDefault();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.GetColor(0x232323ff));

                var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                {
                    ui.Begin(Utils.GetMaxRectInContainer(screenSize, canvasSize), canvasSize);

                    var center = canvasSize / 2;
                    Utils.DrawTextCentered(font, "GMTK2024", center, 20, 1, Raylib.WHITE);
                
                    if (ui.ShowButton(Utils.GetCenteredRect(center + new Vector2(0, 50), new(100, 20)), "Play"))
                    {
                        mainmenu = false;
                    }

                    ui.End();
                }

                ui.Draw();


                Raylib.EndDrawing();
            } else
            {
                currentLevel.Tick();
            }
        }

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}