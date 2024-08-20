using AsepriteDotNet;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using GMTK2024.Aseprite;
using Raylib_CsLo;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
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
        return new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
    }

    public static Raylib_CsLo.Rectangle GetCenteredRect(Vector2 position, Vector2 size)
    {
        return new Raylib_CsLo.Rectangle(
            position.X - size.X / 2,
            position.Y - size.Y / 2,
            size.X,
            size.Y
        );
    }

    public static Raylib_CsLo.Rectangle ShrinkRect(Raylib_CsLo.Rectangle rect, float amount)
    {
        return new Raylib_CsLo.Rectangle(
            rect.x + amount,
            rect.y + amount,
            rect.width - 2 * amount,
            rect.height - 2 * amount
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


    public static Rgba32[] FlattenLayers(AsepriteFrame frame, string[] layers, bool onlyVisibleLayers = true, bool includeBackgroundLayer = false, bool includeTilemapCels = true)
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

            if (!layers.Contains(asepriteCel.Layer.Name)) continue;

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

    public static Rgba32[] FlattenLayer(AsepriteFrame frame, string layer)
    {
        return FlattenLayers(frame, new string[] { layer });
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

    public static Raylib_CsLo.Texture RGBAToTexture(Rgba32[] rgba, int width, int height)
    {
        var image = LoadImageFromRgba(rgba, width, height);

        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        return texture;
    }

    public static Raylib_CsLo.Texture FlattenLayersToTexture(AsepriteFrame frame, params string[] layers)
    {
        var rgba = FlattenLayers(frame, layers);

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

    public static Raylib_CsLo.Texture FlattenTagToTexture(AsepriteFile ase, string tagName)
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

        return FrameToTexture(ase.Frames[foundTag.From]);
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
        Raylib.DrawTexturePro(texture, source, dest, new Vector2(dest.width, dest.height) / 2, rotation, tint);
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
        return (2 * lineAngle + (float)Math.PI) % (float)(2 * Math.PI);
    }

    public static float AngleDifference(float a, float b)
    {
        var diff = a % (2 * Math.PI) - b % (2 * Math.PI);
        if (diff > Math.PI) diff -= 2 * Math.PI;
        if (diff < -Math.PI) diff += 2 * Math.PI;
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

    public static void DrawTextVerticallyCentered(Raylib_CsLo.Font font, string text, Vector2 position, float fontSize, float spacing, Raylib_CsLo.Color tint)
    {
        var size = Raylib.MeasureTextEx(font, text, fontSize, spacing);

        Raylib.DrawTextEx(font, text, position - new Vector2(0, size.Y/2), fontSize, spacing, tint);
    }

    public static Raylib_CsLo.Rectangle GetMaxRectInContainer(Vector2 containerSize, Vector2 itemSize)
    {
        var maxScale = Math.Min(containerSize.X / itemSize.X, containerSize.Y / itemSize.Y); ;
        var scaledSize = itemSize * maxScale;

        return new Raylib_CsLo.Rectangle(
            (containerSize.X - scaledSize.X) / 2,
            (containerSize.Y - scaledSize.Y) / 2,
            scaledSize.X,
            scaledSize.Y
        );
    }

    public static AsepriteSlice? GetSlice(AsepriteFile ase, string name)
    {
        foreach (var slice in ase.Slices)
        {
            if (slice.Name == name)
            {
                return slice;
            }
        }

        return null;
    }

    public static Vector2 GetSlicePivot(AsepriteFile ase, string name)
    {
        var slice = GetSlice(ase, name);
        Debug.Assert(slice != null);
        Debug.Assert(slice.HasPivot);

        Debug.Assert(slice.Keys.Length == 1);
        var key = slice.Keys[0];

        return new Vector2(key.Bounds.X + key.Pivot.X, key.Bounds.Y + key.Pivot.Y);
    }

    public static Raylib_CsLo.Rectangle GetSliceBounds(AsepriteFile ase, string name)
    {
        var slice = GetSlice(ase, name);
        Debug.Assert(slice != null);

        Debug.Assert(slice.Keys.Length == 1);
        var key = slice.Keys[0];

        return new Raylib_CsLo.Rectangle(key.Bounds.X, key.Bounds.Y, key.Bounds.Width, key.Bounds.Height);
    }

    public static Vector2 GetAngledVector2(float angle, float length = 1)
    {
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;
    }

    public static Vector2 Vector2Rotate(Vector2 vec, float angle)
    {
        return new Vector2(
            vec.X * (float)Math.Cos(angle) - vec.Y * (float)Math.Sin(angle),
            vec.X * (float)Math.Sin(angle) + vec.Y * (float)Math.Cos(angle)
        );
    }

    public static bool IsAngleClose(float angle1, float angle2, float margin = 0.01f)
    {
        return Math.Abs(AngleDifference(angle1, angle2)) < margin;
    }

    public static float Lerp(float x, float min, float max)
    {
        return min + x * (max - min);
    }

    public static float Remap(float x, float fromMin, float fromMax, float toMin, float toMax)
    {
        var t = (x - fromMin) / (fromMax - fromMin);
        return Lerp(t, toMin, toMax);
    }

    public static float RandRange(Random rng, float from, float to)
    {
        return Lerp(rng.NextSingle(), from, to);
    }

    public static float RandRange(Random rng, Range range)
    {
        return RandRange(rng, range.from, range.to);
    }

    public static Vector2 RotateAroundPivot(Vector2 point, Vector2 pivot, float angle)
    {
        return pivot + Vector2Rotate(point - pivot, angle);
    }

    public static Vector2 TextureSize(Raylib_CsLo.Texture texture)
    {
        return new Vector2(texture.width, texture.height);
    }

    public static void PlaySoundRandom(Random rng, List<Sound> sounds)
    {
        if (sounds.Count == 0) return;

        var index = rng.Next(sounds.Count);
        Raylib.PlaySoundMulti(sounds[index]);
    }

    public static Vector2 GetScreenSize()
    {
        return new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }
}
