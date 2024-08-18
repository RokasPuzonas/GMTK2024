﻿using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

internal class Program
{
    public static bool running = true;

    public static float tileSize = 32;
    public static Vector2 canvasSize = new Vector2(320 * 3, 180 * 3);

    public static int startingGold = 1000;
    public static float playerHealth = 100;

    public static int revolverCost = 25;
    public static int slimeGoldDrop = 5;

    public static Assets assets;
    public static Dictionary<string, RaylibTileset> tilesets;
    public static DualGridTileset towerPlatformMain;
    public static DualGridTileset towerPlatformFoliage;
    public static RaylibAnimation revolver;
    public static RaylibAnimation bigRevolverUnderbelly;
    public static RaylibAnimation bigRevolverAmmoRack;
    public static RaylibAnimation bigRevolverLeftGun;
    public static RaylibAnimation bigRevolverRightGun;
    public static RaylibAnimation bigRevolverLeftAmmo;
    public static RaylibAnimation bigRevolverRightAmmo;
    public static Vector2 bigRevolverLeftPivot;
    public static Vector2 bigRevolverRightPivot;
    public static RaylibAnimation slimeJump;
    public static RaylibAnimation slimeWindup;
    public static RaylibAnimation homeCrystal;
    public static Texture enemySpawner;
    public static Texture coin;
    public static Sound gunshot;

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

            var slimeAse = assets.LoadAseprite("slime2.aseprite");
            slimeWindup = Utils.FlattenTagToAnimation(slimeAse, "windup");
            slimeJump = Utils.FlattenTagToAnimation(slimeAse, "jump");

            var coinAse = assets.LoadAseprite("coin.aseprite");
            coin = Utils.FrameToTexture(coinAse.Frames[0]);

            gunshot = assets.LoadSound("hard_gunshot.wav");
            Raylib.SetSoundVolume(gunshot, 0.45f);

            var spawnerAse = assets.LoadAseprite("spawner.aseprite");
            enemySpawner = Utils.FrameToTexture(spawnerAse.Frames[0]);

            var homeCrystalAse = assets.LoadAseprite("end.aseprite");
            homeCrystal = Utils.FlattenToAnimation(homeCrystalAse);

            var bigRevolverAse = assets.LoadAseprite("big_revolver.aseprite");
            bigRevolverLeftGun    = Utils.FlattenLayerToAnimation(bigRevolverAse, "left gub");
            bigRevolverRightGun   = Utils.FlattenLayerToAnimation(bigRevolverAse, "right gub");
            bigRevolverLeftAmmo   = Utils.FlattenLayerToAnimation(bigRevolverAse, "ammo left");
            bigRevolverRightAmmo  = Utils.FlattenLayerToAnimation(bigRevolverAse, "ammo right");
            bigRevolverAmmoRack   = Utils.FlattenLayerToAnimation(bigRevolverAse, "ammo rack");
            bigRevolverUnderbelly = Utils.FlattenLayerToAnimation(bigRevolverAse, "underbelly");
            bigRevolverLeftPivot  = Utils.GetSlicePivot(bigRevolverAse, "left gub pivot");
            bigRevolverRightPivot = Utils.GetSlicePivot(bigRevolverAse, "right gun pivot");
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

        Raylib.CloseWindow();
    }
}