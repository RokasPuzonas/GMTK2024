using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024;

internal class UI
{
    public Rectangle rect;
    public RenderTexture? renderTexture;
    public Vector2 mouse;
    public bool hot = false;

    public bool ShowButton(Rectangle rect, string text, float fontSize = 10)
    {
        var font = Raylib.GetFontDefault();

        var hover = false;
        var pressed = false;

        if (Utils.IsInsideRect(mouse, rect))
        {
            this.hot = true;
            hover = true;
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                pressed = true;
            }
        }

        Raylib.DrawRectangleRec(rect, hover ? Raylib.GRAY : Raylib.DARKGRAY);

        Utils.DrawTextCentered(font, text, Utils.GetRectCenter(rect), fontSize, fontSize / 10, Raylib.WHITE);

        return pressed;
    }

    public void Begin(Rectangle rect, Vector2 uiSize)
    {
        this.hot = false;
        this.rect = rect;
        mouse = Raylib.GetMousePosition();
        mouse.X = (mouse.X - rect.x) / rect.width * uiSize.X;
        mouse.Y = (mouse.Y - rect.y) / rect.height * uiSize.Y;

        if (renderTexture != null && (renderTexture.Value.texture.width != (int)rect.width || renderTexture.Value.texture.height != (int)rect.height))
        {
            Raylib.UnloadRenderTexture(renderTexture.Value);
            renderTexture = null;
        }

        if (renderTexture == null)
        {
            renderTexture = Raylib.LoadRenderTexture((int)rect.width, (int)rect.height);
        }

        Debug.Assert(renderTexture != null);
        Raylib.BeginTextureMode(renderTexture.Value);
        Raylib.ClearBackground(Raylib.GetColor(0x000000));
        RlGl.rlPushMatrix();
        RlGl.rlScalef(rect.width / uiSize.X, rect.height / uiSize.Y, 1);
    }

    public void End()
    {
        RlGl.rlPopMatrix();
        Raylib.EndTextureMode();
    }

    public void Draw()
    {
        Debug.Assert(renderTexture != null);
        var texture = renderTexture.Value.texture;

        var source = new Rectangle(0, 0, texture.width, -texture.height);
        Raylib.DrawTexturePro(texture, source, rect, Vector2.Zero, 0, Raylib.WHITE);
    }
}
