using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

static class Utils
{
    public static bool IsInside(Vector2 point, Vector2 size)
    {
        return (0 <= point.X && point.X < size.X) && (0 <= point.Y && point.Y < size.Y);
    }

    public static bool IsInsideRect(Vector2 point, Rectangle rect)
    {
        return (rect.x <= point.X && point.X <= rect.x + rect.width) && (rect.y <= point.Y && point.Y <= rect.y + rect.height);
    }
}
