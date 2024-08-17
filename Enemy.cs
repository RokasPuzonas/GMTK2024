using Raylib_CsLo;
using System;
using System.Numerics;

namespace GMTK2024;

enum EnemyType
{
    Slime
};

internal class Enemy
{
    public bool dead = false;
    public int targetEndpoint = 0;
    public float friction = 0.075f;
    public Vector2 velocity = Vector2.Zero;
    public Vector2 position = Vector2.Zero;
    public EnemyType type;
    public Vector2 size = new Vector2(16, 16);
    public int health;
    public int maxHealth;

    public int animationIndex;
    public float animationTimer;

    public float aim;
    public float targetAim;

    public float jumpCooldown = 0;

    public Rectangle GetRect()
    {
        return new Rectangle(
            position.X - size.X / 2,
            position.Y - size.Y / 2,
            size.X,
            size.Y
        );
    }
}
