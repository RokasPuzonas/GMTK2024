using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;

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

    public static Texture FlattenLayerToTexture(AsepriteFrame frame, string name)
    {
        var rgba = FlattenLayer(frame, name);
        var image = LoadImageFromRgba(rgba, frame.Size.Width, frame.Size.Height);

        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        return texture;
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
}
