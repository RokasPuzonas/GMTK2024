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
}
