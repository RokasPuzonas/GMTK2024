
using System.Numerics;

namespace GMTK2024;

internal class Range
{
    public float from;
    public float to;

    public Range(float from, float to)
    {
        this.from = from;
        this.to = to;
    }
}

internal class SmokeParticle
{
    public float createdAt;
    public float duration;
    public float scale;
    public float rotation;
    public Vector2 position;
    public Vector2 velocity;
}
