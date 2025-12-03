using System;
using System.Collections.Generic;

public static class AIUtils
{
    public static TileState FindClosestTile(int startX, int startY, List<TileState> tiles)
    {
        TileState best = null;
        int bestDist = int.MaxValue;

        foreach (var t in tiles)
        {
            int d = Manhattan(startX, startY, t.x, t.y);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        return best;
    }

    public static int Manhattan(int x1, int y1, int x2, int y2)
    {
        return System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2);
    }

    public static int GetUnitValue(UnitType type, GameConfig cfg)
    { return cfg.GetUnitCost(type);
    }

    public static int GetBuildingValue(BuildingType type, GameConfig cfg)
    {
        switch (type)
        {
            case BuildingType.Castle:   return cfg.castleHp;
            case BuildingType.Fortpost: return cfg.fortpostHp;
            default: return 0;
        }
    }

    public static bool IsEnemyUnit(UnitState u, PlayerId myId)
    {
        return !u.isDead && u.owner != myId;
    }

    public static bool IsMyUnit(UnitState u, PlayerId myId)
    {
        return !u.isDead && u.owner == myId;
    }
}