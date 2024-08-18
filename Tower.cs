using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024;

enum TowerType {
    Revolver,
    BigRevolver
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
    public Vector2? targetPosition;

    public float targetAim = 0;
    public float aimSpeed = (float)Math.PI;
    public float aim = 0;
    public float range = 200f;

    public TowerState state = TowerState.Idle;
    public AnimationState animation = new AnimationState();

    public float shootCooldown = 0.0f;

    public Rectangle GetRect()
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }

    public Vector2 Center()
    {
        return position + size / 2;
    }

    // Big revolver specific
    public float leftTargetAim = 0;
    public float leftAim = 0;
    public float rightTargetAim;
    public float rightAim = 0;

    public Vector2 GetRightGunCenter()
    {
        return Center() + Utils.Vector2Rotate(Program.bigRevolverRightPivot - size / 2, aim + (float)Math.PI/2);
    }

    public Vector2 GetLeftGunCenter()
    {
        return Center() + Utils.Vector2Rotate(Program.bigRevolverLeftPivot - size / 2, aim + (float)Math.PI / 2);
    }
}
