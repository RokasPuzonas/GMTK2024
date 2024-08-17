using Raylib_CsLo;

namespace GMTK2024;

internal class RaylibAnimationFrame
{
    public float duration;
    public Texture texture;
}

internal class RaylibAnimation
{
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

    public void UpdateAnimation(float dt, ref float animationTimer, ref int animationIndex)
    {
        animationTimer += dt;
        while (animationTimer > frames[animationIndex].duration)
        {
            animationTimer -= frames[animationIndex].duration;
            animationIndex = (animationIndex + 1) % frames.Count;
        }
    }
}
