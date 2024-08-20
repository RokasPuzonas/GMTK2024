using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_CsLo;
using System.Reflection.Metadata.Ecma335;

namespace GMTK2024;

internal class DialogSystem
{
    List<DialogItem>? currentDialog = null;
    int dialogIndex = 0;
    float dialogTimer = 0;
    float textSpeed = 20; // Characters per second
    int textLength = 0;

    AvatarFace hansFace = AvatarFace.CreateHans();
    AvatarFace privateFace = AvatarFace.CreatePrivate();

    public void Play(List<DialogItem> dialog)
    {
        currentDialog = dialog;
        dialogIndex = 0;
        textLength = 0;
    }

    public bool PlayingDialog()
    {
        return currentDialog != null;
    }

    public static void DrawTextInRect(Font font, string text, Rectangle rect, float fontSize, float spacing, Color tint)
    {
        var words = text.Split(" ");
        var currentLine = "";
        var oy = 0f;

        foreach (var word in text.Split(" "))
        {
            var lineSize = Raylib.MeasureTextEx(font, currentLine + word, fontSize, spacing);
            if (lineSize.X > rect.width)
            {
                Raylib.DrawTextEx(font, currentLine, new Vector2(rect.x, rect.y + oy), fontSize, spacing, tint);
                oy += lineSize.Y * 1.2f;
                currentLine = "";
            }

            currentLine += word + " ";
        }

        if (currentLine.Length > 0)
        {
            Raylib.DrawTextEx(font, currentLine, new Vector2(rect.x, rect.y + oy), fontSize, spacing, tint);
        }
    }

    public bool Show()
    {
        if (currentDialog != null)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_E) || Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                textLength = 0;
                dialogIndex += 1;
                if (dialogIndex == currentDialog.Count)
                {
                    currentDialog = null;
                }
            }
        }

        if (currentDialog == null) return false;

        var dialog = currentDialog[dialogIndex];

        var dt = Raylib.GetFrameTime();
        hansFace.Update(dt);
        privateFace.Update(dt);

        var dialogBoxSize = new Vector2(400, 120);
        var hansDialogBox = new Rectangle(240, Program.canvasSize.Y - 160, dialogBoxSize.X, dialogBoxSize.Y);
        var privateDialogBox = new Rectangle(Program.canvasSize.X - dialogBoxSize.X - 250, Program.canvasSize.Y - 160, dialogBoxSize.X, dialogBoxSize.Y);

        var hansPosition = new Vector2(120, Program.canvasSize.Y - 140);
        var privatePosition = new Vector2(Program.canvasSize.X - 130, Program.canvasSize.Y - 120);

        Rectangle dialogBox;
        AvatarFace activeFace;
        AvatarFace otherFace;
        Vector2 activePosition;
        Vector2 otherPosition;
        if (dialog.person == PersonName.Hans)
        {
            dialogBox = hansDialogBox;
            activeFace = hansFace;
            otherFace = privateFace;
            dialogBox = hansDialogBox;
            activePosition = hansPosition;
            otherPosition = privatePosition;
        } else
        {
            dialogBox = privateDialogBox;
            activeFace = privateFace;
            otherFace = hansFace;
            dialogBox = privateDialogBox;
            activePosition = privatePosition;
            otherPosition = hansPosition;
        }

        Raylib.DrawCircleV(new Vector2(Program.canvasSize.X/2, 500), 200, Raylib.ColorAlpha(Raylib.BLACK, 0.7f));
        Raylib.DrawCircleV(new Vector2(Program.canvasSize.X/2, 500), 400, Raylib.ColorAlpha(Raylib.BLACK, 0.7f));
        Raylib.DrawCircleV(new Vector2(Program.canvasSize.X/2, 500), 600, Raylib.ColorAlpha(Raylib.BLACK, 0.7f));
        Raylib.DrawCircleV(new Vector2(Program.canvasSize.X/2, 500), 800, Raylib.ColorAlpha(Raylib.BLACK, 0.7f));
        
        Raylib.DrawRectangleRounded(dialogBox, 0.2f, 8, Raylib.ColorAlpha(Raylib.BLACK, 0.8f));
        DrawTextInRect(Program.font, dialog.text.Substring(0, textLength), Utils.ShrinkRect(dialogBox, 24), 24, 1, Raylib.WHITE);

        dialogTimer += dt;
        while (dialogTimer > 1f / textSpeed)
        {
            dialogTimer -= 1f / textSpeed;
            if (textLength < dialog.text.Length)
            {
                if (textLength % 5 == 0 && dialog.person != PersonName.Slime)
                {
                    activeFace.Talk();
                    var rng = new Random();
                    Utils.PlaySoundRandom(rng, Program.voice);
                }
                textLength++;
            }
        }

        var activeRotation = 5 * (float)Math.Sin(3 * Raylib.GetTime());
        var activeScale = 1.1f;
        if (dialog.person == PersonName.Slime)
        {
            Utils.DrawTextureCentered(Program.slimeKing, activePosition + new Vector2(50, -25), activeRotation, activeScale * 9, Raylib.WHITE);
        } else
        {
            activeFace.Draw(activePosition, rotation: activeRotation, scale: activeScale);
        }
        otherFace.Draw(otherPosition, color: Raylib.GetColor(0xABABABff));

        return true;
    }
}
