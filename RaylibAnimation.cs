using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

internal class RaylibAnimationFrame
{
    public float duration;
    public Texture texture;
}

internal class RaylibAnimation
{
    public Vector2 size;
    public List<RaylibAnimationFrame> frames = new List<RaylibAnimationFrame>();

    public float GetDuration()
    {
        float duration = 0;
        foreach (var frame in frames)
        {
            duration += frame.duration;
        }
        return duration;
    }

    public void UpdateAnimation(float dt, ref float animationTimer, ref int animationIndex, bool loop = true)
    {
        animationTimer += dt;
        while (animationTimer > frames[animationIndex].duration)
        {
            animationTimer -= frames[animationIndex].duration;
            if (loop == true)
            {
                animationIndex = (animationIndex + 1) % frames.Count;
            } else
            {
                animationIndex = Math.Min(animationIndex + 1, frames.Count - 1);
            }
        }
    }
}
