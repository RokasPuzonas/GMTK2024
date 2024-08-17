using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMTK2024;

internal class EnemyWaveSpawn
{
    public float delay;
    public EnemyType type;
}

internal class EnemyWave
{
    public List<EnemyWaveSpawn> spawns = new List<EnemyWaveSpawn>();
}
