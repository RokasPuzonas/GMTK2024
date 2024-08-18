using Raylib_CsLo;
using System;
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

    public void UpdateLooped(float dt, ref float animationTimer, ref int animationIndex)
    {
        animationTimer += dt;
        while (animationTimer > frames[animationIndex].duration)
        {
            animationTimer -= frames[animationIndex].duration;
            animationIndex = (animationIndex + 1) % frames.Count;
        }
    }

    public bool UpdateOnce(float dt, ref float animationTimer, ref int animationIndex)
    {
        animationTimer += dt;
        while (animationTimer > frames[animationIndex].duration)
        {
            animationTimer -= frames[animationIndex].duration;

            if (animationIndex == frames.Count - 1)
            {
                return true;
            }

            animationIndex++;
        }

        return false;
    }

    public void DrawCentered(int frameIndex, Vector2 position, float rotation, float scale, Color tint)
    {
        Utils.DrawTextureCentered(frames[frameIndex].texture, position, rotation, scale, tint);
    }
}
