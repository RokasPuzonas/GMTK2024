using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024;

enum TowerType {
    Revolver
};

internal class Tower
{
    public Vector2 position;
    public Vector2 size;
    public TowerType type;

    public Rectangle GetRect()
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }
}
