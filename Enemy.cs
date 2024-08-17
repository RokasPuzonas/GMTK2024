using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

enum EnemyType
{
    Slime
};

internal class Enemy
{
    public bool alive = true;
    public int targetEndpoint = 0;
    public Vector2 position = Vector2.Zero;
    public EnemyType type;
}
