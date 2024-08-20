using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GMTK2024;

struct ButtonResult
{
    public bool hover;
    public bool active;
    public bool pressed;
};

internal class UI
{
    public Rectangle rect;
    public RenderTexture? renderTexture;
    public Vector2 mouse;
    
    public bool hot = false;
    public int? activeWidget = null;

    public ButtonResult ButtonLogic(Rectangle rect)
    {
        var result = new ButtonResult();

        if (Utils.IsInsideRect(mouse, rect))
        {
            this.hot = true;
            result.hover = true;
            result.active = Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                result.pressed = true;
            }
        }

        return result;
    }

    public bool ShowButton(Rectangle rect, string text, float fontSize = 10)
    {
        var result = ButtonLogic(rect);

        Raylib.DrawRectangleRec(rect, result.hover ? Raylib.GRAY : Raylib.DARKGRAY);
        Utils.DrawTextCentered(Program.font, text, Utils.GetRectCenter(rect), fontSize, fontSize / 10, Raylib.WHITE);

        return result.pressed;
    }

    public bool ShowImageToggleButton(bool enabled, Vector2 center, Texture normal, Texture hover, Texture active)
    {
        var rect = Utils.GetCenteredRect(center, new Vector2(normal.width, normal.height));
        var result = ButtonLogic(rect);

        Texture texture;
        if (result.active || enabled)
        {
            texture = active;
        }
        else if (result.hover)
        {
            texture = hover;
        }
        else
        {
            texture = normal;
        }

        Utils.DrawTextureCentered(texture, center, 0, 1, Raylib.WHITE);

        return result.pressed;
    }

    public bool ShowSlider(int id, ref float value, Vector2 position, float width)
    {
        var knobSize = 5;
        var knobPosition = position + new Vector2(width * value, 0);

        var changed = false;

        Raylib.DrawLineEx(position, position + new Vector2(width, 0), 2, Raylib.BLACK);
        Raylib.DrawCircleV(position + new Vector2(width * value, 0), knobSize, Raylib.DARKGRAY);

        if (Vector2.Distance(mouse, knobPosition) < knobSize && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            this.activeWidget = id;
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && this.activeWidget == id)
        {
            this.activeWidget = null;
        }

        if (this.activeWidget == id)
        {
            this.hot = true;
            value = Math.Clamp((mouse.X - position.X) / width, 0, 1);
            changed = true;
        }

        return changed;
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
        if (renderTexture == null) return;
        var texture = renderTexture.Value.texture;

        var source = new Rectangle(0, 0, texture.width, -texture.height);
        Raylib.DrawTexturePro(texture, source, rect, Vector2.Zero, 0, Raylib.WHITE);
    }
}
