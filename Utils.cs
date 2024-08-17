using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;

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

    public static Image LoadImageFromRgba(Rgba32[] rgba, int width, int height)
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

    public static Texture RGBAToTexture(Rgba32 []rgba, int width, int height)
    {
        var image = LoadImageFromRgba(rgba, width, height);

        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        return texture;
    }

    public static Texture FlattenLayerToTexture(AsepriteFrame frame, string layer)
    {
        var rgba = FlattenLayer(frame, layer);

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
            frames = frames
        };
    }

    public static RaylibAnimation FlattenToAnimation(AsepriteFile ase)
    {
        var frames = new List<RaylibAnimationFrame>();

        foreach (var frame in ase.Frames)
        {
            var rgba = frame.FlattenFrame();
            frames.Add(new RaylibAnimationFrame
            {
                texture = RGBAToTexture(rgba, frame.Size.Width, frame.Size.Height),
                duration = (float)frame.Duration.TotalSeconds
            });
        }

        return new RaylibAnimation
        {
            frames = frames
        };
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

    public static void DrawTextureCentered(Texture texture, Vector2 position, float rotation, float scale, Color tint)
    {
        var source = new Raylib_CsLo.Rectangle(0, 0, texture.width, texture.height);
        var dest = new Raylib_CsLo.Rectangle(position.X, position.Y, texture.width, texture.height);
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
        var diff = a % (2*Math.PI) - b % (2 * Math.PI);
        if (diff > Math.PI) diff -= 2*Math.PI;
        if (diff < -Math.PI) diff += 2*Math.PI;
        return (float)diff;
    }
}
