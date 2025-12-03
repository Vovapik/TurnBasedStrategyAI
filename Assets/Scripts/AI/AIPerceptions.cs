public class AIPerception
{
    public void BuildBlackboard(AIBlackboard bb)
    {
        var state = bb.State;
        var cfg = state.config;  
        bb.Config = cfg;

        bb.MyUnits.Clear();
        bb.EnemyUnits.Clear();
        bb.MyEngineers.Clear();
        bb.MyArchers.Clear();
        bb.MyCatapults.Clear();
        bb.MyBuildings.Clear();
        bb.EnemyBuildings.Clear();
        bb.GoldTiles.Clear();
        bb.MyCastle = null;
        bb.EnemyCastle = null;

        var myPlayer = state.players.Find(p => p.id == bb.MyId);
        bb.MyGold = myPlayer != null ? myPlayer.gold : 0;  

        int n = state.mapSize;
        bb.DangerMap = new int[n, n];
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                TileState t = state.tiles[x, y];  
                if (t.terrain == TileTerrain.Gold)
                    bb.GoldTiles.Add(t);
            }
        }

        foreach (var u in state.units)
        {
            if (u.isDead) continue;  

            if (AIUtils.IsMyUnit(u, bb.MyId))
            {
                bb.MyUnits.Add(u);
                switch (u.type)
                {
                    case UnitType.Engineer: bb.MyEngineers.Add(u); break;
                    case UnitType.Archer:   bb.MyArchers.Add(u);   break;
                    case UnitType.Catapult: bb.MyCatapults.Add(u); break;
                }
            }
            else if (AIUtils.IsEnemyUnit(u, bb.MyId))
            {
                bb.EnemyUnits.Add(u);
            }
        }

        foreach (var b in state.buildings)
        {
            if (b.owner == bb.MyId)
            {
                bb.MyBuildings.Add(b);
                if (b.type == BuildingType.Castle)
                    bb.MyCastle = b;
            }
            else if (b.owner == bb.EnemyId)
            {
                bb.EnemyBuildings.Add(b);
                if (b.type == BuildingType.Castle)
                    bb.EnemyCastle = b;
            }
        }

        BuildDangerMap(bb);
        DetectCatapultThreat(bb);
    }

    private void DetectCatapultThreat(AIBlackboard bb)
    {
        bb.EnemyCatapultThreatensCastleOrFort = false;
        bb.ThreateningCatapult = null;

        var cfg = bb.Config;
        foreach (var enemy in bb.EnemyUnits)
        {
            if (enemy.type != UnitType.Catapult)
                continue;

            foreach (var b in bb.MyBuildings)
            {
                int dist = AIUtils.Manhattan(enemy.x, enemy.y, b.x, b.y);

                if (dist >= 1 && dist <= cfg.catapultRange)
                {
                    bb.EnemyCatapultThreatensCastleOrFort = true;
                    bb.ThreateningCatapult = enemy;
                    return;
                }

                if (dist == cfg.catapultRange + 1)
                {
                    bb.EnemyCatapultThreatensCastleOrFort = true;
                    bb.ThreateningCatapult = enemy;
                    return;
                }
            }
        }
    }

    private void BuildDangerMap(AIBlackboard bb)
    {
  
        int n = bb.State.mapSize;
        var cfg = bb.Config;

        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                bb.DangerMap[x, y] = 0;

        foreach (var enemy in bb.EnemyUnits)
        {
            int effectiveRange = GetEffectiveThreatRange(enemy, cfg);

            int baseThreat = GetUnitThreat(enemy, cfg);

            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    int dist = AIUtils.Manhattan(enemy.x, enemy.y, x, y);
                    if (dist <= effectiveRange)
                    {
                        bb.DangerMap[x, y] += baseThreat;
                    }
                }
            }
        }
    }

    private int GetEffectiveThreatRange(UnitState u, GameConfig cfg)
    {
        switch (u.type)
        {
            case UnitType.Warrior:
            case UnitType.Chivalry:
            case UnitType.Engineer:
                return u.moveRange + 1;

            case UnitType.Archer:
                return u.moveRange + cfg.archerRange;

            case UnitType.Catapult:
                return u.moveRange + cfg.catapultRange;

            default:
                return u.moveRange;
        }
    }

    private int GetUnitThreat(UnitState u, GameConfig cfg)
    {
        switch (u.type)
        {
            case UnitType.Warrior:   return cfg.warriorDamage;
            case UnitType.Archer:    return cfg.archerDamage;
            case UnitType.Chivalry:  return cfg.chivalryDamage;
            case UnitType.Engineer:  return cfg.engineerDamage;
            case UnitType.Catapult:  return cfg.catapultDamage * 2; 
            default: return 1;
        }
    }
}
