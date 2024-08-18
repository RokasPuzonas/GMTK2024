using System.Numerics;

namespace GMTK2024;

internal class Bullet
{
    public bool dead = false;
    public TowerType type;
    public Vector2 shotFrom;
    public Vector2 position;
    public Vector2 direction;
    public float speed = 0;
    public int damage = 0;

    public bool explodes = false;
    public float knockback = 0;
    public int pierce = 0;
    public float explosionRadius = 0;
    public float maxDistance = 0;
    public int smearLength = 0;

    public List<Enemy> hitEnemies = new List<Enemy>();
    public List<Vector2> smear = new List<Vector2>();
}
