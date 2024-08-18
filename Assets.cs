using AsepriteDotNet;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Raylib_CsLo;
using System.Diagnostics;
using System.Reflection;

namespace GMTK2024;

internal class Assets
{
    static string basename = "GMTK2024.assets.";

    public List<string> ListNames(string extension)
    {
        var assembly = Assembly.GetEntryAssembly();
        Debug.Assert(assembly != null);

        var names = new List<string>();
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(basename) && name.EndsWith(extension))
            {
                names.Add(name.Substring(basename.Length));
            }
        }

        return names;
    }

    public Stream LoadStream(string name)
    {
        var assembly = Assembly.GetEntryAssembly();
        Debug.Assert(assembly != null);

        var stream = assembly.GetManifestResourceStream(basename + name);
        Debug.Assert(stream != null);

        return stream;
    }

    public byte[] LoadBytes(string name)
    {
        var stream = LoadStream(name);

        var dataSize = (int)stream.Length;
        var data = new byte[dataSize];

        stream.ReadExactly(data, 0, dataSize);

        return data;
    }

    public AsepriteFile LoadAseprite(string name)
    {
        return AsepriteFileLoader.FromStream(name, LoadStream(name));
    }

    public Image LoadImage(string name)
    {
        var extension = Path.GetExtension(name);
        var data = LoadBytes(name);
        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                return Raylib.LoadImageFromMemory(extension, dataPtr, data.Length);
            }
        }
    }

    public Raylib_CsLo.Texture LoadTexture(string name)
    {
        var image = LoadImage(name);
        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        return texture;
    }

    public Wave LoadWave(string name)
    {
        var data = LoadBytes(name);

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                return Raylib.LoadWaveFromMemory(Path.GetExtension(name), dataPtr, data.Length);
            }
        }
    }

    public Sound LoadSound(string name)
    {
        var wave = LoadWave(name);
        var sound = Raylib.LoadSoundFromWave(wave);
        //Raylib.UnloadWave(wave);
        return sound;
    }
}
