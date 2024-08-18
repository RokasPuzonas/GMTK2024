using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024.Aseprite;

internal static class AsepriteColorUtilities
{
    //
    // Summary:
    //     Converts an array of System.Byte data to an array of AsepriteDotNet.Common.Rgba32
    //     values based on the specified AsepriteDotNet.Aseprite.AsepriteColorDepth.
    //
    // Parameters:
    //   pixels:
    //     The array of System.Byte data that represents the color data.
    //
    //   depth:
    //     The color depth.
    //
    //   preMultiplyAlpha:
    //     Indicates whether color values should be translated to a premultiplied alpha
    //     value.
    //
    //   palette:
    //     The palette used for ColorDepth.Index. Optional, only required when depth is
    //     equal to ColorDepth.Indexed.
    //
    // Returns:
    //     An array of AsepriteDotNet.Common.Rgba32 values converted from the data.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     pixels is null.
    //
    //   T:System.InvalidOperationException:
    //     depth is an unknown AsepriteDotNet.Aseprite.AsepriteColorDepth value.
    internal static Rgba32[] PixelsToColor(byte[] pixels, AsepriteColorDepth depth, bool preMultiplyAlpha, AsepritePalette? palette = null)
    {
        ArgumentNullException.ThrowIfNull(pixels, "pixels");
        int num = (int)depth / 8;
        Rgba32[] array = new Rgba32[pixels.Length / num];
        int num2 = 0;
        int num3 = 0;
        while (num2 < array.Length)
        {
            byte g;
            byte b;
            byte a;
            byte r = g = b = a = 0;
            switch (depth)
            {
                case AsepriteColorDepth.RGBA:
                    r = pixels[num3];
                    g = pixels[num3 + 1];
                    b = pixels[num3 + 2];
                    a = pixels[num3 + 3];
                    break;
                case AsepriteColorDepth.Grayscale:
                    r = g = b = pixels[num3];
                    a = pixels[num3 + 1];
                    break;
                case AsepriteColorDepth.Indexed:
                    {
                        int num4 = pixels[num2];
                        if (num4 != palette?.TransparentIndex)
                        {
                            palette?.Colors[num4].Deconstruct(out r, out g, out b, out a);
                        }

                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unknown Color Depth: {depth}");
            }

            array[num2] = preMultiplyAlpha ? Rgba32.FromNonPreMultiplied(r, g, b, a) : new Rgba32(r, g, b, a);
            num2++;
            num3 += num;
        }

        return array;
    }

    //
    // Summary:
    //     Calculates the saturation value based on the given RGB color component values.
    //
    //
    // Parameters:
    //   r:
    //     The red color component value (0 to 1).
    //
    //   g:
    //     The green color component value (0 to 1).
    //
    //   b:
    //     The blue color component value (0 to 1).
    //
    // Returns:
    //     The saturation value calculated.
    internal static double CalculateSaturation(double r, double g, double b)
    {
        double num = Math.Max(r, Math.Max(g, b));
        double num2 = Math.Min(r, Math.Min(g, b));
        return num - num2;
    }

    //
    // Summary:
    //     Calculates the luminance value based on the given RGB color component values.
    //
    //
    // Parameters:
    //   r:
    //     The red color component value (0 to 1).
    //
    //   g:
    //     The green color component value (0 to 1).
    //
    //   b:
    //     The blue color component value (0 to 1).
    internal static double CalculateLuminance(double r, double g, double b)
    {
        r *= 0.3;
        g *= 0.59;
        b *= 0.11;
        return r + g + b;
    }

    //
    // Summary:
    //     Modifies the saturation of the specified RGB color component values.
    //
    // Parameters:
    //   r:
    //     The red color component value (0 to 1).
    //
    //   g:
    //     The green color component value (0 to 1).
    //
    //   b:
    //     The blue color component value (0 to 1).
    //
    //   s:
    //     The saturation factor to adjust the color components by.
    internal static void AdjustSaturation(ref double r, ref double g, ref double b, double s)
    {
        ref double reference = ref Calc.RefMin(ref Calc.RefMin(ref r, ref g), ref b);
        ref double reference2 = ref Calc.RefMax(ref Calc.RefMax(ref r, ref g), ref b);
        ref double reference3 = ref Calc.RefMid(ref r, ref g, ref b);
        if (reference2 > reference)
        {
            reference3 = (reference3 - reference) * s / (reference2 - reference);
            reference2 = s;
        }
        else
        {
            reference3 = reference2 = 0.0;
        }

        reference = 0.0;
    }

    //
    // Summary:
    //     Modifies the luminosity of the specified RGB color component values.
    //
    // Parameters:
    //   r:
    //     The red color component value (0 to 1).
    //
    //   g:
    //     The green color component value (0 to 1).
    //
    //   b:
    //     The blue color component value (0 to 1).
    //
    //   l:
    //     The desired luminosity value to apply.
    internal static void AdjustLumanice(ref double r, ref double g, ref double b, double l)
    {
        double num = CalculateLuminance(r, g, b);
        double num2 = l - num;
        r += num2;
        g += num2;
        b += num2;
        NormalizeColor(ref r, ref g, ref b);
    }

    //
    // Summary:
    //     Normalizes the specified RGB color component values to ensure they are within
    //     the valid range of 0 to 1. Clips the values of the specified RGB color
    //
    // Parameters:
    //   r:
    //     The red color component value.
    //
    //   g:
    //     The green color component value.
    //
    //   b:
    //     The blue color component value.
    internal static void NormalizeColor(ref double r, ref double g, ref double b)
    {
        double num = CalculateLuminance(r, g, b);
        double num2 = Math.Min(Math.Min(r, g), b);
        double num3 = Math.Max(Math.Max(r, g), b);
        if (num2 < 0.0)
        {
            double num4 = num / (num - num2);
            r = num + (r - num) * num4;
            g = num + (g - num) * num4;
            b = num + (b - num) * num4;
        }

        if (num3 > 1.0)
        {
            double num5 = (1.0 - num) / (num3 - num);
            r = num + (r - num) * num5;
            g = num + (g - num) * num5;
            b = num + (b - num) * num5;
        }
    }

    //
    // Summary:
    //     Blends two AsepriteDotNet.Common.Rgba32 values using the specified AsepriteDotNet.Aseprite.AsepriteBlendMode
    //     and opacity.
    //
    // Parameters:
    //   backdrop:
    //     The backdrop color.
    //
    //   source:
    //     The source color to be blended onto the backdrop.
    //
    //   opacity:
    //     The opacity of the blending operation.
    //
    //   blendMode:
    //     The AsepriteDotNet.Aseprite.AsepriteBlendMode to use for the blending operation.
    //
    //
    // Returns:
    //     The resulting AsepriteDotNet.Common.Rgba32 value created from the blending.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     blendMode is an unknown AsepriteDotNet.Aseprite.AsepriteBlendMode value.
    internal static Rgba32 Blend(Rgba32 backdrop, Rgba32 source, int opacity, AsepriteBlendMode blendMode)
    {
        if (backdrop.A == 0 && source.A == 0)
        {
            return new Rgba32(0, 0, 0, 0);
        }

        if (backdrop.A == 0)
        {
            return source;
        }

        if (source.A == 0)
        {
            return backdrop;
        }

        return blendMode switch
        {
            AsepriteBlendMode.Normal => Normal(backdrop, source, opacity),
            AsepriteBlendMode.Multiply => Multiply(backdrop, source, opacity),
            AsepriteBlendMode.Screen => Screen(backdrop, source, opacity),
            AsepriteBlendMode.Overlay => Overlay(backdrop, source, opacity),
            AsepriteBlendMode.Darken => Darken(backdrop, source, opacity),
            AsepriteBlendMode.Lighten => Lighten(backdrop, source, opacity),
            AsepriteBlendMode.ColorDodge => ColorDodge(backdrop, source, opacity),
            AsepriteBlendMode.ColorBurn => ColorBurn(backdrop, source, opacity),
            AsepriteBlendMode.HardLight => HardLight(backdrop, source, opacity),
            AsepriteBlendMode.SoftLight => SoftLight(backdrop, source, opacity),
            AsepriteBlendMode.Difference => Difference(backdrop, source, opacity),
            AsepriteBlendMode.Exclusion => Exclusion(backdrop, source, opacity),
            AsepriteBlendMode.Hue => HslHue(backdrop, source, opacity),
            AsepriteBlendMode.Saturation => HslSaturation(backdrop, source, opacity),
            AsepriteBlendMode.Color => HslColor(backdrop, source, opacity),
            AsepriteBlendMode.Luminosity => HslLuminosity(backdrop, source, opacity),
            AsepriteBlendMode.Addition => Addition(backdrop, source, opacity),
            AsepriteBlendMode.Subtract => Subtract(backdrop, source, opacity),
            AsepriteBlendMode.Divide => Divide(backdrop, source, opacity),
            _ => throw new InvalidOperationException($"Unknown blend mode '{blendMode}'"),
        };
    }

    private static Rgba32 Normal(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        if (backdrop.A == 0)
        {
            source.A = Calc.MultiplyUnsigned8Bit(source.A, opacity);
            return source;
        }

        if (source.A == 0)
        {
            return backdrop;
        }

        opacity = Calc.MultiplyUnsigned8Bit(source.A, opacity);
        int num = source.A + backdrop.A - Calc.MultiplyUnsigned8Bit(backdrop.A, source.A);
        int num2 = backdrop.R + (source.R - backdrop.R) * opacity / num;
        int num3 = backdrop.G + (source.G - backdrop.G) * opacity / num;
        int num4 = backdrop.B + (source.B - backdrop.B) * opacity / num;
        return new Rgba32((byte)num2, (byte)num3, (byte)num4, (byte)num);
    }

    private static Rgba32 Multiply(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = Calc.MultiplyUnsigned8Bit(backdrop.R, source.R);
        source.G = Calc.MultiplyUnsigned8Bit(backdrop.G, source.G);
        source.B = Calc.MultiplyUnsigned8Bit(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Screen(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)(backdrop.R + source.R - Calc.MultiplyUnsigned8Bit(backdrop.R, source.R));
        source.G = (byte)(backdrop.G + source.G - Calc.MultiplyUnsigned8Bit(backdrop.G, source.G));
        source.B = (byte)(backdrop.B + source.B - Calc.MultiplyUnsigned8Bit(backdrop.B, source.B));
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Overlay(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)overlay(backdrop.R, source.R);
        source.G = (byte)overlay(backdrop.G, source.G);
        source.B = (byte)overlay(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int overlay(int b, int s)
        {
            if (b < 128)
            {
                b <<= 1;
                return Calc.MultiplyUnsigned8Bit(s, b);
            }

            b = (b << 1) - 255;
            return s + b - Calc.MultiplyUnsigned8Bit(s, b);
        }
    }

    private static Rgba32 Darken(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = Math.Min(backdrop.R, source.R);
        source.G = Math.Min(backdrop.G, source.G);
        source.B = Math.Min(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Lighten(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = Math.Max(backdrop.R, source.R);
        source.G = Math.Max(backdrop.G, source.G);
        source.B = Math.Max(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 ColorDodge(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)dodge(backdrop.R, source.R);
        source.G = (byte)dodge(backdrop.G, source.G);
        source.B = (byte)dodge(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int dodge(int b, int s)
        {
            if (b == 0)
            {
                return 0;
            }

            s = 255 - s;
            if (b >= s)
            {
                return 255;
            }

            return Calc.DivideUnsigned8Bit(b, s);
        }
    }

    private static Rgba32 ColorBurn(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)burn(backdrop.R, source.R);
        source.G = (byte)burn(backdrop.G, source.G);
        source.B = (byte)burn(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int burn(int b, int s)
        {
            if (b == 255)
            {
                return 255;
            }

            b = 255 - b;
            if (b >= s)
            {
                return 0;
            }

            return 255 - Calc.DivideUnsigned8Bit(b, s);
        }
    }

    private static Rgba32 HardLight(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)hardlight(backdrop.R, source.R);
        source.G = (byte)hardlight(backdrop.G, source.G);
        source.B = (byte)hardlight(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int hardlight(int b, int s)
        {
            if (s < 128)
            {
                s <<= 1;
                return Calc.MultiplyUnsigned8Bit(b, s);
            }

            s = (s << 1) - 255;
            return b + s - Calc.MultiplyUnsigned8Bit(b, s);
        }
    }

    private static Rgba32 SoftLight(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)softlight(backdrop.R, source.R);
        source.G = (byte)softlight(backdrop.G, source.G);
        source.B = (byte)softlight(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int softlight(int _b, int _s)
        {
            double num = _b / 255.0;
            double num2 = _s / 255.0;
            double num3 = !(num <= 0.25) ? Math.Sqrt(num) : ((16.0 * num - 12.0) * num + 4.0) * num;
            double num4 = !(num2 <= 0.5) ? num + (2.0 * num2 - 1.0) * (num3 - num) : num - (1.0 - 2.0 * num2) * num * (1.0 - num);
            return (int)(num4 * 255.0 + 0.5);
        }
    }

    private static Rgba32 Difference(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)Math.Abs(backdrop.R - source.R);
        source.G = (byte)Math.Abs(backdrop.G - source.G);
        source.B = (byte)Math.Abs(backdrop.B - source.B);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Exclusion(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)exclusion(backdrop.R, source.R);
        source.G = (byte)exclusion(backdrop.G, source.G);
        source.B = (byte)exclusion(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int exclusion(int b, int s)
        {
            return b + s - 2 * Calc.MultiplyUnsigned8Bit(b, s);
        }
    }

    private static Rgba32 HslHue(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        double r = backdrop.R / 255.0;
        double g = backdrop.G / 255.0;
        double b = backdrop.B / 255.0;
        double s = CalculateSaturation(r, g, b);
        double l = CalculateLuminance(r, g, b);
        r = source.R / 255.0;
        g = source.G / 255.0;
        b = source.B / 255.0;
        AdjustSaturation(ref r, ref g, ref b, s);
        AdjustLumanice(ref r, ref g, ref b, l);
        source.R = (byte)(r * 255.0);
        source.G = (byte)(g * 255.0);
        source.B = (byte)(b * 255.0);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 HslSaturation(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        double r = source.R / 255.0;
        double g = source.G / 255.0;
        double b = source.B / 255.0;
        double s = CalculateSaturation(r, g, b);
        r = backdrop.R / 255.0;
        g = backdrop.G / 255.0;
        b = backdrop.B / 255.0;
        double l = CalculateLuminance(r, g, b);
        AdjustSaturation(ref r, ref g, ref b, s);
        AdjustLumanice(ref r, ref g, ref b, l);
        source.R = (byte)(r * 255.0);
        source.G = (byte)(g * 255.0);
        source.B = (byte)(b * 255.0);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 HslColor(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        double r = backdrop.R / 255.0;
        double g = backdrop.G / 255.0;
        double b = backdrop.B / 255.0;
        double l = CalculateLuminance(r, g, b);
        r = source.R / 255.0;
        g = source.G / 255.0;
        b = source.B / 255.0;
        AdjustLumanice(ref r, ref g, ref b, l);
        source.R = (byte)(r * 255.0);
        source.G = (byte)(g * 255.0);
        source.B = (byte)(b * 255.0);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 HslLuminosity(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        double r = source.R / 255.0;
        double g = source.G / 255.0;
        double b = source.B / 255.0;
        double l = CalculateLuminance(r, g, b);
        r = backdrop.R / 255.0;
        g = backdrop.G / 255.0;
        b = backdrop.B / 255.0;
        AdjustLumanice(ref r, ref g, ref b, l);
        source.R = (byte)(r * 255.0);
        source.G = (byte)(g * 255.0);
        source.B = (byte)(b * 255.0);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Addition(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)Math.Min(backdrop.R + source.R, 255);
        source.G = (byte)Math.Min(backdrop.G + source.G, 255);
        source.B = (byte)Math.Min(backdrop.B + source.B, 255);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Subtract(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)Math.Max(backdrop.R - source.R, 0);
        source.G = (byte)Math.Max(backdrop.G - source.G, 0);
        source.B = (byte)Math.Max(backdrop.B - source.B, 0);
        return Normal(backdrop, source, opacity);
    }

    private static Rgba32 Divide(Rgba32 backdrop, Rgba32 source, int opacity)
    {
        source.R = (byte)divide(backdrop.R, source.R);
        source.G = (byte)divide(backdrop.G, source.G);
        source.B = (byte)divide(backdrop.B, source.B);
        return Normal(backdrop, source, opacity);
        static int divide(int b, int s)
        {
            if (b == 0)
            {
                return 0;
            }

            if (b >= s)
            {
                return 255;
            }

            return Calc.DivideUnsigned8Bit(b, s);
        }
    }
}
