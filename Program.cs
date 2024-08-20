using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

internal class Program
{
    public static bool debugSkipLevel = false; // Press T

    public static bool running = true;
    public static bool gotoNextLevel = false;

    public static float tileSize = 32;
    public static Vector2 canvasSize = new Vector2(32 * 32, 32 * 18);

    public static int   level1StartingGold = 1000;
    public static int   level2StartingGold = 1000;
    public static int   level3StartingGold = 1000;

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
    public static float slimeKnockbackResistance = 0;
    public static int   slimeGoldDrop = 5;
    public static int   slimeDamage = 10;
    public static Func<Random, float> slimeJumpStrength = rng => Utils.RandRange(rng, 100, 200);
    public static Func<Random, float> slimeJumpCooldown = rng => Utils.RandRange(rng, 0, 0.5f);

    public static int   bigSlimeHealth = 500;
    public static float bigSlimeCollisionRadius = 25;
    public static float bigSlimeKnockbackResistance = 0.95f;
    public static int   bigSlimeGoldDrop = 50;
    public static int   bigSlimeDamage = 100;
    public static Func<Random, float> bigSlimeJumpStrength = rng => Utils.RandRange(rng, 200, 300);
    public static Func<Random, float> bigSlimeJumpCooldown = rng => Utils.RandRange(rng, 3f, 4f);

    public static int   smallSlimeHealth = 10;
    public static float smallSlimeCollisionRadius = 5;
    public static float smallSlimeKnockbackResistance = 0;
    public static int   smallSlimeGoldDrop = 1;
    public static int   smallSlimeDamage = 5;
    public static Func<Random, float> smallSlimeJumpStrength = rng => Utils.RandRange(rng, 300, 400);
    public static Func<Random, float> smallSlimeJumpCooldown = rng => Utils.RandRange(rng, 0, 0.5f);

    public static Assets assets;
    public static Dictionary<string, RaylibTileset> tilesets;
    public static DualGridTileset towerPlatformMain;
    public static DualGridTileset towerPlatformFoliage;
    public static Texture enemySpawner;
    public static Texture coin;
    public static Font font;
    public static RaylibAnimation homeCrystal;
    public static List<Sound> voice;
    public static Texture heart;
    public static byte[] musicBytes;
    public static Music music;
    public static Sound bigGunDrop;
    public static Sound smallGunDrop;
    public static Texture heartSign;
    public static Texture coinSign;
    public static Texture waveSign;
    public static Texture nextWaveSign;
    public static Rectangle heartSignTextBounds;
    public static Rectangle coinSignTextBounds;
    public static Rectangle waveSignTextBounds;
    public static Rectangle nextWaveSignTextBounds;
    public static Rectangle totalWaveSignTextBounds;

    public static List<Texture> mainMenuBackground;
    public static Rectangle mainMenuSlider;
    public static Texture mainMenuSliderKnob;
    public static Rectangle mainMenuStartButton;

    public static List<Texture> signPlaque;
    public static Texture signLeftChain;
    public static Texture signRightChain;

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
    public static RaylibAnimation bigSlimeJump;
    public static RaylibAnimation bigSlimeWindup;
    public static RaylibAnimation smallSlimeJump;
    public static RaylibAnimation smallSlimeWindup;
    public static Sound           slimeJumpSound;

    public static Texture         hansFace;
    public static RaylibAnimation hansMouth;
    public static Texture         hansHat;
    public static Vector2         hansMouthPivot;

    public static Texture         privateFace;
    public static RaylibAnimation privateMouth;
    public static Texture         privateHat;
    public static Vector2         privateMouthPivot;

    public static Texture mortarButtonNormal;
    public static Texture mortarButtonHover;
    public static Texture mortarButtonActive;

    public static Texture revolverButtonNormal;
    public static Texture revolverButtonHover;
    public static Texture revolverButtonActive;

    public static List<DialogItem> dialog1 = new List<DialogItem>
    {
        new(PersonName.Private, "Hans we need better transmission"),
        new(PersonName.Hans, "More armor you say?"),
        new(PersonName.Private, "Nein!"),
        new(PersonName.Private, "Better transmission"),
        new(PersonName.Hans, "Bigger Kannon, you say?"),
        new(PersonName.Private, "God for damn Hans"),
        new(PersonName.Hans, "Oh!"),
        new(PersonName.Hans, "Battleship kannon!"),
        new(PersonName.Private, "..."),
        new(PersonName.Private, "Ja Hans"),
        new(PersonName.Hans, "Ja"),
    };

    public static void Main(string[] args)
    {
        assets = new Assets();

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1920, 1080, "GMTK2024");
        Raylib.InitAudioDevice();
        Raylib.SetWindowMinSize((int)canvasSize.X, (int)canvasSize.Y);
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(0);

        // Load assets
        {
            tilesets = RaylibTileset.LoadAll(assets);
        
            var towerBaseTileset = assets.LoadAseprite("grass_tower_base_tileset.aseprite");

            towerPlatformMain = new DualGridTileset(
                Utils.FlattenLayersToTexture(towerBaseTileset.Frames[0], "tower_base"),
                new Vector2(tileSize, tileSize)
            );

            towerPlatformFoliage = new DualGridTileset(
                Utils.FlattenLayersToTexture(towerBaseTileset.Frames[0], "foliage"),
                new Vector2(tileSize, tileSize)
            );

            var revolverAse = assets.LoadAseprite("revolver.aseprite");
            revolver = Utils.FlattenToAnimation(revolverAse);
            revolverNozzle = Utils.GetSlicePivot(revolverAse, "nozzle");

            var slimeAse = assets.LoadAseprite("slime2.aseprite");
            slimeWindup = Utils.FlattenTagToAnimation(slimeAse, "windup");
            slimeJump = Utils.FlattenTagToAnimation(slimeAse, "jump");

            var bigSlimeAse = assets.LoadAseprite("chonk slime.aseprite");
            bigSlimeWindup = Utils.FlattenTagToAnimation(bigSlimeAse, "windup");
            bigSlimeJump = Utils.FlattenTagToAnimation(bigSlimeAse, "jump");

            var smallSlimeAse = assets.LoadAseprite("mini slime.aseprite");
            smallSlimeWindup = Utils.FlattenTagToAnimation(smallSlimeAse, "windup");
            smallSlimeJump = Utils.FlattenTagToAnimation(smallSlimeAse, "jump");

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

            var mortarButtonAse = assets.LoadAseprite("mortar_button.aseprite");
            mortarButtonNormal = Utils.FlattenTagToTexture(mortarButtonAse, "normal");
            mortarButtonHover = Utils.FlattenTagToTexture(mortarButtonAse, "hover");
            mortarButtonActive = Utils.FlattenTagToTexture(mortarButtonAse, "active");

            var revolverButtonAse = assets.LoadAseprite("revolving_button.aseprite");
            revolverButtonNormal = Utils.FlattenTagToTexture(revolverButtonAse, "normal");
            revolverButtonHover = Utils.FlattenTagToTexture(revolverButtonAse, "hover");
            revolverButtonActive = Utils.FlattenTagToTexture(revolverButtonAse, "active");

            var signAse = assets.LoadAseprite("sign.aseprite");
            signPlaque = new List<Texture>();
            signPlaque.Add(Utils.FlattenLayersToTexture(signAse.Frames[0], "sign"));
            signPlaque.Add(Utils.FlattenLayersToTexture(signAse.Frames[1], "sign"));
            signPlaque.Add(Utils.FlattenLayersToTexture(signAse.Frames[2], "sign"));
            signLeftChain = Utils.FlattenLayersToTexture(signAse.Frames[0], "left chain");
            signRightChain = Utils.FlattenLayersToTexture(signAse.Frames[0], "right chain");

            var hansAse = assets.LoadAseprite("hans.aseprite");
            hansFace  = Utils.FlattenLayersToTexture(hansAse.Frames[0], "face");
            hansHat   = Utils.FlattenLayersToTexture(hansAse.Frames[0], "hat");
            hansMouth = Utils.FlattenLayerToAnimation(hansAse, "mouth");
            hansMouthPivot = Utils.GetSlicePivot(hansAse, "mouth");

            var privateAse = assets.LoadAseprite("private.aseprite");
            privateFace = Utils.FlattenLayersToTexture(privateAse.Frames[0], "face");
            privateHat = Utils.FlattenLayersToTexture(privateAse.Frames[0], "helm");
            privateMouth = Utils.FlattenLayerToAnimation(privateAse, "mouth");
            privateMouthPivot = Utils.GetSlicePivot(privateAse, "mouth");

            font = assets.LoadFont("font.ttf", 30);

            voice = [
                assets.LoadSound("voice/v1.wav"),
                assets.LoadSound("voice/v2.wav"),
                assets.LoadSound("voice/v3.wav"),
                assets.LoadSound("voice/v4.wav"),
                assets.LoadSound("voice/v5.wav"),
                assets.LoadSound("voice/v6.wav"),
            ];

            heart = assets.LoadAsepriteTexture("heart.aseprite");

            musicBytes = assets.LoadBytes("bgm.wav");
            unsafe
            {
                fixed (byte* dataPtr = musicBytes)
                {
                    music = Raylib.LoadMusicStreamFromMemory(".wav", dataPtr, musicBytes.Length);
                }
            }

            Raylib.SetMusicVolume(music, 0.1f);

            smallGunDrop = assets.LoadSound("small_gun_drop.wav");
            bigGunDrop = assets.LoadSound("big_gun_drop.wav");

            var gameInfoAse = assets.LoadAseprite("game info.aseprite");
            heartSign    = Utils.FlattenLayersToTexture(gameInfoAse.Frames[0], "heart sign", "heart chain");
            coinSign     = Utils.FlattenLayersToTexture(gameInfoAse.Frames[0], "coin sign", "coin chain");
            waveSign     = Utils.FlattenLayersToTexture(gameInfoAse.Frames[0], "wave sign", "wave chain");
            nextWaveSign = Utils.FlattenLayersToTexture(gameInfoAse.Frames[0], "next sign", "next chain");
            heartSignTextBounds    = Utils.GetSliceBounds(gameInfoAse, "life text");
            coinSignTextBounds     = Utils.GetSliceBounds(gameInfoAse, "coin text");
            waveSignTextBounds     = Utils.GetSliceBounds(gameInfoAse, "wave text");
            totalWaveSignTextBounds = Utils.GetSliceBounds(gameInfoAse, "total wave text");
            nextWaveSignTextBounds = Utils.GetSliceBounds(gameInfoAse, "next wave text");

            var menuAse = assets.LoadAseprite("menu.aseprite");
            mainMenuBackground = new List<Texture>
            {
                Utils.FlattenLayersToTexture(menuAse.Frames[0], "background"),
                Utils.FlattenLayersToTexture(menuAse.Frames[1], "background")
            };
            mainMenuStartButton = Utils.GetSliceBounds(menuAse, "start button");
            mainMenuSlider = Utils.GetSliceBounds(menuAse, "slider");
        }

        var currentLevel = 1;
        var level = GetLevel(currentLevel);

        var playSceneTransition = false;
        var loadingAnimation = 0f;
        var mainmenu = true;
        var transitionToLevel1 = false;
        float audioVolume = 0.6f;
        var ui = new UI();

        Raylib.SetMasterVolume(audioVolume);

        Raylib.PlayMusicStream(music);
        while (!Raylib.WindowShouldClose() && running) 
        {
            Raylib.UpdateMusicStream(music);

            var dt = Raylib.GetFrameTime();

            if (transitionToLevel1 || gotoNextLevel)
            {
                playSceneTransition = true;
            }

            float loadingDirection = playSceneTransition ? 1 : -1;
            loadingAnimation = (float)Math.Clamp(loadingAnimation + dt * loadingDirection, 0, 1);
            if (loadingAnimation == 1)
            {
                playSceneTransition = false;
                if (transitionToLevel1)
                {
                    mainmenu = false;
                }
                else if (gotoNextLevel)
                {
                    currentLevel += 1;
                    level = GetLevel(currentLevel);
                }
                transitionToLevel1 = false;
                gotoNextLevel = false;
            }

            if (debugSkipLevel && Raylib.IsKeyPressed(KeyboardKey.KEY_T))
            {
                gotoNextLevel = true;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.GetColor(0x232323ff));

            var screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            if (mainmenu)
            {
                var maimMenuSize = Utils.TextureSize(mainMenuBackground[0]);
                ui.Begin(Utils.GetMaxRectInContainer(screenSize, maimMenuSize), maimMenuSize);
                
                var startButtonResult = ui.ButtonLogic(mainMenuStartButton);
                if (startButtonResult.hover)
                {
                    Raylib.DrawTexture(mainMenuBackground[1], 0, 0, Raylib.WHITE);
                }
                else
                {
                    Raylib.DrawTexture(mainMenuBackground[0], 0, 0, Raylib.WHITE);
                }

                if (startButtonResult.pressed)
                {
                    transitionToLevel1 = true;
                }

                var knobSize = 1;
                if (ui.SliderLogic(0, ref audioVolume, new Vector2(mainMenuSlider.x, mainMenuSlider.y + mainMenuSlider.height / 2), mainMenuSlider.width, knobSize))
                {
                    Raylib.SetMasterVolume(audioVolume);
                }
                Raylib.DrawRectangleV(
                    new Vector2(mainMenuSlider.x - knobSize + mainMenuSlider.width * audioVolume, mainMenuSlider.y + mainMenuSlider.height / 2 - knobSize),
                    new Vector2(2* knobSize, 2* knobSize),
                    Raylib.BLACK
                );

                ui.End();
                ui.Draw();
            } else
            {
                level.Update(dt);
                level.Draw();
            }

            Raylib.DrawCircleV(screenSize/2, (screenSize/2).Length() * loadingAnimation * 1.1f, Raylib.GetColor(0x121212ff));

            Raylib.EndDrawing();
        }

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    public static Level GetLevel(int level)
    {
        if (level == 1)
        {
            return CreateLevel1();
        } else if (level == 2)
        {
            return CreateLevel2();
        } else if (level == 3)
        {
            return CreateLevel3();
        } else
        {
            throw new NotImplementedException();
        }
    }

    public static Level CreateLevel1()
    {
        var waves = new List<EnemyWave>
        {
            new([
                new() { delay = 0.1f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
            ])
        };

        var startDialog = new List<DialogItem> {
            new(PersonName.Private, "Start"),
        };

        var endDialog = new List<DialogItem> {
            new(PersonName.Private, "end"),
        };

        var tilemap = new RaylibTilemap(tilesets, assets.LoadStream("level1.tmx"));
        return new Level(1, tilemap, waves, false, false, level1StartingGold, startDialog, endDialog);
    }

    public static Level CreateLevel2()
    {
        var waves = new List<EnemyWave>
        {
            new([
                new() { delay = 0.1f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
            ]),
        };

        var startDialog = new List<DialogItem> {
            new(PersonName.Private, "Start"),
        };

        var endDialog = new List<DialogItem> {
            new(PersonName.Private, "end"),
        };

        var tilemap = new RaylibTilemap(tilesets, assets.LoadStream("level1.tmx"));
        return new Level(2, tilemap, waves, false, true, level2StartingGold, startDialog, endDialog);
    }

    public static Level CreateLevel3()
    {
        var waves = new List<EnemyWave>
        {
            new([
                new() { delay = 0.1f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
                new() { delay = 0.5f, type = EnemyType.Slime },
            ]),
        };

        var startDialog = new List<DialogItem> {
            new(PersonName.Private, "Start"),
        };

        var endDialog = new List<DialogItem> {
            new(PersonName.Private, "end"),
        };

        var tilemap = new RaylibTilemap(tilesets, assets.LoadStream("level1.tmx"));
        return new Level(3, tilemap, waves, true, true, level3StartingGold, startDialog, endDialog);
    }
}