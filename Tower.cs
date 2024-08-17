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

enum TowerState
{
    Idle,
    Shoot
};

internal class Tower
{
    public float createdAt;
    public Vector2 position;
    public Vector2 size;
    public TowerType type;
    public Enemy? target;

    public float targetAim = 0;
    public float aimSpeed = (float)Math.PI;
    public float aim = 0;
    public float range = 200f;

    public TowerState state = TowerState.Idle;
    public int animationIndex = 0;
    public float animationTimer = 0;

    public float shootCooldown = 0.0f;

    public Rectangle GetRect()
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }

    public Vector2 Center()
    {
        return position + size / 2;
    }

    public bool IsShooting()
    {
        return target != null && Math.Abs(Utils.AngleDifference(targetAim, aim)) < 0.05f;
    }
}
