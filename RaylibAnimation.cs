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

    public void PlayOnce(float dt, ref AnimationState state, ref bool played)
    {
        if (!played)
        {
            played = UpdateOnce(dt, ref state);
        }
        else
        {
            state.frame = 0;
        }
    }

    public void DrawCentered(int frameIndex, Vector2 position, float rotation, float scale, Color tint)
    {
        Utils.DrawTextureCentered(frames[frameIndex].texture, position, rotation, scale, tint);
    }

    public void DrawCentered(AnimationState state, Vector2 position, float rotation, float scale, Color tint)
    {
        DrawCentered(state.frame, position, rotation, scale, tint);
    }

    public void Draw(int frameIndex, Vector2 position, Vector2 origin, float rotation, float scale, Color tint)
    {
        var texture = frames[frameIndex].texture;

        var source = new Rectangle(0, 0, texture.width, texture.height);
        var dest = new Rectangle(position.X, position.Y, texture.width * scale, texture.height * scale);
        Raylib.DrawTexturePro(texture, source, dest, origin, rotation, tint);
    }

    public void Draw(AnimationState state, Vector2 position, Vector2 origin, float rotation, float scale, Color tint)
    {
        Draw(state.frame, position, origin, rotation, scale, tint);
    }
}
