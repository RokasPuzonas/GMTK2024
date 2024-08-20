using Raylib_CsLo;
using System.Numerics;

namespace GMTK2024;

enum EnemyType
{
    Slime,
    SmallSlime,
    BigSlime
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
    public int damage;
    public int maxHealth;
    public float knockbackResistance = 0;

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

    public static Enemy Create(EnemyType type, Vector2 position)
    {
        switch (type)
        {
        case EnemyType.Slime:
            return new Enemy
            {
                position = position,
                targetEndpoint = 1,
                type = type,
                goldValue = Program.slimeGoldDrop,
                state = EnemyState.SlimeCooldown,
                size = Program.slimeJump.size,
                maxHealth = Program.slimeHealth,
                health = Program.slimeHealth,
                collisionRadius = Program.slimeCollisionRadius,
                damage = Program.slimeDamage,
                knockbackResistance = Program.slimeKnockbackResistance
            };
        case EnemyType.BigSlime:
            return new Enemy
            {
                position = position,
                targetEndpoint = 1,
                type = type,
                goldValue = Program.bigSlimeGoldDrop,
                state = EnemyState.SlimeCooldown,
                size = Program.bigSlimeJump.size,
                maxHealth = Program.bigSlimeHealth,
                health = Program.bigSlimeHealth,
                collisionRadius = Program.bigSlimeCollisionRadius,
                damage = Program.bigSlimeDamage,
                knockbackResistance = Program.bigSlimeKnockbackResistance
            };
        case EnemyType.SmallSlime:
            return new Enemy
            {
                position = position,
                targetEndpoint = 1,
                type = type,
                goldValue = Program.smallSlimeGoldDrop,
                state = EnemyState.SlimeCooldown,
                size = Program.smallSlimeJump.size,
                maxHealth = Program.smallSlimeHealth,
                health = Program.smallSlimeHealth,
                collisionRadius = Program.smallSlimeCollisionRadius,
                damage = Program.smallSlimeDamage,
                knockbackResistance = Program.smallSlimeKnockbackResistance
            };
        default:
            throw new NotImplementedException();
        }
    }

    public static RaylibAnimation GetWindupAnimation(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Slime:
                return Program.slimeWindup;
            case EnemyType.BigSlime:
                return Program.bigSlimeWindup;
            case EnemyType.SmallSlime:
                return Program.smallSlimeWindup;
            default:
                throw new NotImplementedException();
        }
    }

    public static RaylibAnimation GetJumpAnimation(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Slime:
                return Program.slimeJump;
            case EnemyType.BigSlime:
                return Program.bigSlimeJump;
            case EnemyType.SmallSlime:
                return Program.smallSlimeJump;
            default:
                throw new NotImplementedException();
        }
    }
}
