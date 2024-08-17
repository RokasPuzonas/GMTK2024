namespace GMTK2024;

internal static class Calc
{
    //
    // Summary:
    //     Returns a value that indicates if the specified flag is set for the given value.
    //
    //
    // Parameters:
    //   value:
    //     The value to check for the flag.
    //
    //   flag:
    //     The flag to check against the value.
    //
    // Returns:
    //     true if the flag is set in the value; otherwise, false.
    internal static bool HasFlag(this uint value, uint flag)
    {
        return (value & flag) != 0;
    }

    //
    // Summary:
    //     Returns a value that indicates if the specified flag is not set for the given
    //     value.
    //
    // Parameters:
    //   value:
    //     The value to check for the flag.
    //
    //   flag:
    //     The flag to check against the value.
    //
    // Returns:
    //     true if the flag is not set in the value otherwise, false.
    internal static bool DoesNotHaveFlag(this uint value, uint flag)
    {
        return !value.HasFlag(flag);
    }

    //
    // Summary:
    //     Returns the reference to the value that is the maximum value between two double
    //     values specified.
    //
    // Parameters:
    //   a:
    //     The first double value.
    //
    //   b:
    //     The second double value.
    //
    // Returns:
    //     The reference to the maximum value between a and b
    internal static ref double RefMax(ref double a, ref double b)
    {
        if (!(a >= b))
        {
            return ref b;
        }

        return ref a;
    }

    //
    // Summary:
    //     Returns the reference to the value that is the minimum value between two double
    //     values specified.
    //
    // Parameters:
    //   a:
    //     The first double value.
    //
    //   b:
    //     The second double value.
    //
    // Returns:
    //     The reference to the minimum value between a and b
    internal static ref double RefMin(ref double a, ref double b)
    {
        if (!(a <= b))
        {
            return ref b;
        }

        return ref a;
    }

    //
    // Summary:
    //     Returns the reference to the value that is the middle value between the three
    //     double values specified.
    //
    // Parameters:
    //   a:
    //     The first double value.
    //
    //   b:
    //     The second double value.
    //
    //   c:
    //     The third double value.
    //
    // Returns:
    //     The reference to the middle value between a, b, and c.
    internal static ref double RefMid(ref double a, ref double b, ref double c)
    {
        double num = Math.Min(Math.Min(a, b), c);
        double num2 = Math.Max(Math.Max(a, b), c);
        if (a != num && a != num2)
        {
            return ref a;
        }

        if (b != num && b != num2)
        {
            return ref b;
        }

        return ref c;
    }

    //
    // Summary:
    //     Returns the result of multiplying two unsigned 8-bit values.
    //
    // Parameters:
    //   a:
    //     The multiplicand
    //
    //   b:
    //     The multiplier
    //
    // Returns:
    //     The result of multiplying two unsigned 8-bit values.
    public static byte MultiplyUnsigned8Bit(byte a, int b)
    {
        int num = a * b + 128;
        return (byte)((num >> 8) + num >> 8);
    }

    //
    // Summary:
    //     Returns the result of multiplying two unsigned 8-bit values.
    //
    // Parameters:
    //   a:
    //     The multiplicand
    //
    //   b:
    //     The multiplier
    //
    // Returns:
    //     The result of multiplying two unsigned 8-bit values.
    public static byte MultiplyUnsigned8Bit(int a, int b)
    {
        return MultiplyUnsigned8Bit((byte)a, b);
    }

    //
    // Summary:
    //     Returns the result of dividing two unsigned 8-bit values.
    //
    // Parameters:
    //   a:
    //     The dividend
    //
    //   b:
    //     The divisor
    //
    // Returns:
    //     The result of multiplying two unsigned 8-bit values.
    internal static byte DivideUnsigned8Bit(byte a, int b)
    {
        return (byte)((a * 255 + b / 2) / b);
    }

    //
    // Summary:
    //     Returns the result of dividing two unsigned 8-bit values.
    //
    // Parameters:
    //   a:
    //     The dividend
    //
    //   b:
    //     The divisor
    //
    // Returns:
    //     The result of multiplying two unsigned 8-bit values.
    internal static byte DivideUnsigned8Bit(int a, int b)
    {
        return DivideUnsigned8Bit((byte)a, b);
    }
}