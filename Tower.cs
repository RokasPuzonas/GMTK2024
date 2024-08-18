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
    BigRevolver,
    Mortar
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
    public float maxRange = 200f;
    public float minRange = 20f;

    public Vector2 recoil = Vector2.Zero;

    public AnimationState animation = new AnimationState();

    public float shootCooldown = 0.0f;
    public bool reloaded = true;

    public static Tower Create(TowerType type, Vector2 position, Vector2 size, float aim)
    {
        switch (type)
        {
            case TowerType.Revolver:
                return CreateRevolver(position, size, aim);
            case TowerType.BigRevolver:
                return CreateBigRevolver(position, size, aim);
            case TowerType.Mortar:
                return CreateMortar(position, size, aim);
            default:
                throw new NotImplementedException();
        }
    }

    public static int GetCost(TowerType type)
    {
        switch (type)
        {
            case TowerType.Revolver:
                return Program.revolverCost;
            case TowerType.Mortar:
                return Program.mortarCost;
            default:
                throw new NotImplementedException();
        }
    }

    public static Tower CreateRevolver(Vector2 position, Vector2 size, float aim)
    {
        return new Tower
        {
            position = position,
            size = size,
            type = TowerType.Revolver,
            createdAt = (float)Raylib.GetTime(),
            targetAim = aim,
            aim = aim,

            aimSpeed = Program.revolverAimSpeed,
            minRange = Program.revolverMinRange,
            maxRange = Program.revolverMaxRange
        };
    }

    public static Tower CreateBigRevolver(Vector2 position, Vector2 size, float aim)
    {
        return new Tower
        {
            position = position,
            size = size,
            type = TowerType.BigRevolver,
            createdAt = (float)Raylib.GetTime(),
            targetAim = aim,
            aim = aim,

            aimSpeed = Program.bigRevolverAimSpeed,
            minRange = Program.bigRevolverMinRange,
            maxRange = Program.bigRevolverMaxRange
        };
    }

    public static Tower CreateMortar(Vector2 position, Vector2 size, float aim)
    {
        return new Tower
        {
            position = position,
            size = size,
            type = TowerType.Mortar,
            createdAt = (float)Raylib.GetTime(),
            targetAim = aim,
            aim = aim,

            aimSpeed = Program.mortarAimSpeed,
            minRange = Program.mortarMinRange,
            maxRange = Program.mortarMaxRange
        };
    }

    public Rectangle GetRect()
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }

    public Vector2 Center()
    {
        return position + size / 2;
    }

    // Mortar specific
    public bool fired = false;

    // Big revolver specific
    public AnimationState leftGunAnimation = new AnimationState();
    public Vector2 leftRecoil = Vector2.Zero;
    public float leftTargetAim = 0;
    public float leftAim = 0;
    public float leftShootCooldown = 0;
    public bool leftReloaded = true;

    public AnimationState rightGunAnimation = new AnimationState();
    public Vector2 rightRecoil = Vector2.Zero;
    public float rightTargetAim = 0;
    public float rightAim = 0;
    public float rightShootCooldown = 0;
    public bool rightReloaded = true;

    public Vector2 GetRightGunCenter()
    {
        return Center() + Utils.Vector2Rotate(Program.bigRevolverRightPivot - size / 2, aim + (float)Math.PI/2);
    }

    public Vector2 GetLeftGunCenter()
    {
        return Center() + Utils.Vector2Rotate(Program.bigRevolverLeftPivot - size / 2, aim + (float)Math.PI / 2);
    }
}
