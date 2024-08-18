﻿using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

enum EnemyType
{
    Slime
};

enum EnemyState
{
    SlimeWindup,
    SlimeJump,
    SlimeCooldown
}

internal class Enemy
{
    public bool dead = false;
    public int targetEndpoint = 0;
    public float friction = 0.075f;
    public Vector2 velocity = Vector2.Zero;
    public Vector2 position = Vector2.Zero;
    public EnemyType type;
    public EnemyState state;
    public Vector2 size = new Vector2(16, 16);
    public int health;
    public int maxHealth;

    public AnimationState animation = new AnimationState();

    public float aim;
    public float targetAim;

    public float jumpCooldown = 0;
    public float collisionRadius = 0;
    public int goldValue = 10;


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
