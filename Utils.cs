using AsepriteDotNet;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using GMTK2024.Aseprite;
using Raylib_CsLo;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Xml.Linq;
using TiledCS;
using static System.Net.Mime.MediaTypeNames;

namespace GMTK2024;

static class Utils
{
    public static bool IsInside(Vector2 point, Vector2 size)
    {
        return (0 <= point.X && point.X < size.X) && (0 <= point.Y && point.Y < size.Y);
    }

    public static bool IsInsideRect(Vector2 point, Raylib_CsLo.Rectangle rect)
    {
        return (rect.x <= point.X && point.X <= rect.x + rect.width) && (rect.y <= point.Y && point.Y <= rect.y + rect.height);
    }

    public static Vector2 GetRectCenter(Raylib_CsLo.Rectangle rect)
    {
        return new Vector2(rect.x + rect.width/2, rect.y + rect.height/2);
    }

    public static Raylib_CsLo.Rectangle ShrinkRect(Raylib_CsLo.Rectangle rect, float amount)
    {
        return new Raylib_CsLo.Rectangle(
            rect.x + amount,
            rect.y + amount,
            rect.width - 2*amount,
            rect.height - 2*amount
        );
    }

    public static AsepriteCel? GetCelByLayerName(AsepriteFrame frame, string name)
    {
        foreach (var cel in frame.Cels)
        {
            if (cel.Layer.Name == name)
            {
                return cel;
            }
        }

        return null;
    }

    
    public static Rgba32[] FlattenLayer(AsepriteFrame frame, string name, bool onlyVisibleLayers = true, bool includeBackgroundLayer = false, bool includeTilemapCels = true)
    {
        ArgumentNullException.ThrowIfNull(frame, "frame");
        Rgba32[] array = new Rgba32[frame.Size.Width * frame.Size.Height];
        ReadOnlySpan<AsepriteCel> cels = frame.Cels;
        for (int i = 0; i < cels.Length; i++)
        {
            AsepriteCel asepriteCel = cels[i];
            if (asepriteCel is AsepriteLinkedCel asepriteLinkedCel)
            {
                asepriteCel = asepriteLinkedCel.Cel;
            }

            if (asepriteCel.Layer.Name != name) continue;

            if ((!onlyVisibleLayers || asepriteCel.Layer.IsVisible) && (!asepriteCel.Layer.IsBackgroundLayer || includeBackgroundLayer))
            {
                if (asepriteCel is AsepriteImageCel asepriteImageCel)
                {
                    BlendCel(array, asepriteImageCel.Pixels, asepriteImageCel.Layer.BlendMode, new AsepriteDotNet.Common.Rectangle(asepriteImageCel.Location, asepriteImageCel.Size), frame.Size.Width, asepriteImageCel.Opacity, asepriteImageCel.Layer.Opacity);
                }
                else if (includeTilemapCels && asepriteCel is AsepriteTilemapCel cel)
                {
                    throw new NotSupportedException();
                }
            }
        }

        return array;
    }

    public static Raylib_CsLo.Image LoadImageFromRgba(Rgba32[] rgba, int width, int height)
    {
        Debug.Assert(rgba.Length == width * height);
        var image = Raylib.GenImageColor(width, height, Raylib.GetColor(0));

        unsafe
        {
            var image_data = (byte*)image.data;
            for (int i = 0; i < rgba.Length; i++)
            {
                image_data[4 * i + 0] = rgba[i].R;
                image_data[4 * i + 1] = rgba[i].G;
                image_data[4 * i + 2] = rgba[i].B;
                image_data[4 * i + 3] = rgba[i].A;
            }
        }

        return image;
    }

    public static Raylib_CsLo.Texture RGBAToTexture(Rgba32 []rgba, int width, int height)
    {
        var image = LoadImageFromRgba(rgba, width, height);

        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        return texture;
    }

    public static Raylib_CsLo.Texture FlattenLayerToTexture(AsepriteFrame frame, string layer)
    {
        var rgba = FlattenLayer(frame, layer);

        return RGBAToTexture(rgba, frame.Size.Width, frame.Size.Height);
    }

    public static Raylib_CsLo.Texture FrameToTexture(AsepriteFrame frame)
    {
        var rgba = frame.FlattenFrame();

        return RGBAToTexture(rgba, frame.Size.Width, frame.Size.Height);
    }

    public static RaylibAnimation FlattenLayerToAnimation(AsepriteFile ase, string layerName)
    {
        var frames = new List<RaylibAnimationFrame>();

        foreach (var frame in ase.Frames)
        {
            var rgba = FlattenLayer(frame, layerName);
            frames.Add(new RaylibAnimationFrame
            {
                texture = RGBAToTexture(rgba, frame.Size.Width, frame.Size.Height),
                duration = (float)frame.Duration.TotalSeconds
            });
        }

        return new RaylibAnimation
        {
            size = new Vector2(ase.CanvasWidth, ase.CanvasHeight),
            frames = frames
        };
    }

    public static RaylibAnimation FlattenRangeToAnimation(AsepriteFile ase, int from, int to)
    {
        var frames = new List<RaylibAnimationFrame>();
        
        for (int i = from; i <= to; i++)
        {
            var frame = ase.Frames[i];
            var rgba = frame.FlattenFrame();
            frames.Add(new RaylibAnimationFrame
            {
                texture = RGBAToTexture(rgba, frame.Size.Width, frame.Size.Height),
                duration = (float)frame.Duration.TotalSeconds
            });
        }

        return new RaylibAnimation
        {
            size = new Vector2(ase.CanvasWidth, ase.CanvasHeight),
            frames = frames
        };
    }

    public static RaylibAnimation FlattenToAnimation(AsepriteFile ase)
    {
        return FlattenRangeToAnimation(ase, 0, ase.FrameCount - 1);
    }

    public static RaylibAnimation FlattenTagToAnimation(AsepriteFile ase, string tagName)
    {
        AsepriteTag? foundTag = null;
        foreach (var tag in ase.Tags)
        {
            if (tag.Name == tagName)
            {
                foundTag = tag;
            }
        }

        Debug.Assert(foundTag != null);

        return FlattenRangeToAnimation(ase, foundTag.From, foundTag.To);
    }

    private static void BlendCel(Span<Rgba32> backdrop, ReadOnlySpan<Rgba32> source, AsepriteBlendMode blendMode, AsepriteDotNet.Common.Rectangle bounds, int frameWidth, int celOpacity, int layerOpacity)
    {
        byte opacity = Calc.MultiplyUnsigned8Bit(celOpacity, layerOpacity);
        int num = Math.Max(0, -bounds.X);
        int num2 = Math.Max(0, -bounds.Y);
        int num3 = Math.Min(bounds.Width, frameWidth - bounds.X);
        int num4 = Math.Min(bounds.Height, backdrop.Length / frameWidth - bounds.Y);
        for (int i = num2; i < num4; i++)
        {
            for (int j = num; j < num3; j++)
            {
                int index = (i + bounds.Y) * frameWidth + (j + bounds.X);
                Rgba32 backdrop2 = backdrop[index];
                Rgba32 source2 = source[i * bounds.Width + j];
                backdrop[index] = AsepriteColorUtilities.Blend(backdrop2, source2, opacity, blendMode);
            }
        }
    }

    public static void DrawTextureCentered(Raylib_CsLo.Texture texture, Vector2 position, float rotation, float scale, Raylib_CsLo.Color tint)
    {
        var source = new Raylib_CsLo.Rectangle(0, 0, texture.width, texture.height);
        var dest = new Raylib_CsLo.Rectangle(position.X, position.Y, texture.width * scale, texture.height * scale);
        Raylib.DrawTexturePro(texture, source, dest, new Vector2(texture.width, texture.height) / 2, rotation, tint);
    }

    public static float LineAngle(Vector2 start, Vector2 end)
    {
        return (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
    }

    public static float ToDegrees(float radians)
    {
        return (float)(radians * 180 / Math.PI);
    }

    public static float GetAimAngle(Vector2 from, Vector2 to)
    {
        var dir = Vector2.Normalize(to - from);
        if (Math.Abs(dir.X - 1) < 1e-5)
        {
            return 0;
        }

        var lineAngle = LineAngle(dir, new Vector2(1, 0));
        return (2*lineAngle + (float)Math.PI) % (float)(2*Math.PI);
    }

    public static float AngleDifference(float a, float b)
    {
        var diff = a % (2 * Math.PI) - b % (2 * Math.PI);
        if (diff >  Math.PI) diff -= 2*Math.PI;
        if (diff < -Math.PI) diff += 2*Math.PI;
        return (float)diff;
    }

    public static float ApproachAngle(float angle, float targetAnle, float stepSize)
    {
        var angleDiff = AngleDifference(targetAnle, angle);
        if (Math.Abs(angleDiff) > 0.01)
        {
            angle += Math.Sign(angleDiff) * Math.Min(stepSize, Math.Abs(angleDiff));
        }

        return angle;
    }

    public static bool GetBoolTiledProperty(TiledProperty[] props, string name, bool fallback)
    {
        foreach (var prop in props)
        {
            if (prop.type == TiledPropertyType.Bool)
            {
                return prop.value == "true";
            }
        }

        return fallback;
    }
    public static void DrawTextCentered(Raylib_CsLo.Font font, string text, Vector2 position, float fontSize, float spacing, Raylib_CsLo.Color tint)
    {
        var size = Raylib.MeasureTextEx(font, text, fontSize, spacing);

        Raylib.DrawTextEx(font, text, position - size / 2, fontSize, spacing, tint);
    }
}
