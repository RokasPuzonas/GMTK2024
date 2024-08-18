using Raylib_CsLo;
using System;
using System.Numerics;

namespace GMTK2024;

internal class AnimationState
{
    public int frame;
    public float timer;
}

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

    public void UpdateLooped(float dt, ref AnimationState state)
    {
        state.timer += dt;
        while (state.timer > frames[state.frame].duration)
        {
            state.timer -= frames[state.frame].duration;
            state.frame = (state.frame + 1) % frames.Count;
        }
    }

    public bool UpdateOnce(float dt, ref AnimationState state)
    {
        state.timer += dt;
        while (state.timer > frames[state.frame].duration)
        {
            state.timer -= frames[state.frame].duration;

            if (state.frame == frames.Count - 1)
            {
                return true;
            }

            state.frame++;
        }

        return false;
    }

    public void DrawCentered(int frameIndex, Vector2 position, float rotation, float scale, Color tint)
    {
        Utils.DrawTextureCentered(frames[frameIndex].texture, position, rotation, scale, tint);
    }
}
