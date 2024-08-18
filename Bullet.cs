using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024;

internal class Bullet
{
    public bool dead = false;
    public Vector2 position;
    public Vector2 direction;
    public float speed = 0;
    public int damage = 0;

    public float knockback = 0;
    public int pierce = 0;
    public float radius = 0;
    public float maxDistance = 0;

    public List<Enemy> hitEnemies = new List<Enemy>();
}
