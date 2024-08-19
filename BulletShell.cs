using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

class BulletShell
{
    public TowerType type;
    public float createdAt;
    public float spin;
    public Vector2 start;
    public Vector2 destination;

    public Vector2 position;
    public float rotation;

    public float GetProgress()
    {
        return Vector2.Distance(start, position) / Vector2.Distance(destination, start);
    }

    public float TimeSinceCreation()
    {
        return (float)Raylib.GetTime() - createdAt;
    }
}
