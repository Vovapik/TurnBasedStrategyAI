using System;
using System.Collections.Generic;

public class AIUnitController
{
    public void ControlUnits(AIBlackboard bb)
    {
        var state = bb.State;

        List<int> myUnitIds = new List<int>();
        for (int i = 0; i < state.units.Count; i++)
        {
            UnitState u = state.units[i];
            if (u.isDead) continue;
            if (u.owner == bb.MyId)
                myUnitIds.Add(i);
        }

        myUnitIds.Sort((a, b) =>
        {
            UnitType ta = state.units[a].type;
            UnitType tb = state.units[b].type;
            int ra = GetTypePriority(ta);
            int rb = GetTypePriority(tb);
            return ra.CompareTo(rb);
        });

        foreach (int id in myUnitIds)
        {
            UnitState u = state.units[id];
            if (u.isDead) continue;

            while (!u.isDead && u.owner == bb.MyId && u.actionsLeft > 0 && !bb.State.gameOver)
            {
                bool acted = false;

                if (u.type == UnitType.Engineer)
                {
                    acted = ControlEngineer(u, bb);
                }
                else if (u.type == UnitType.Catapult)
                {
                    acted = ControlCatapult(u, bb);
                }
                else if (u.type == UnitType.Archer)
                {
                    acted = ControlArcher(u, bb);
                }
                else
                {
                    acted = ControlMelee(u, bb);
                }
                
                if (!acted)
                {
                    break;
                }
            }
        }

    }

    private int GetTypePriority(UnitType t)
    {
        switch (t)
        {
            case UnitType.Catapult: return 0;
            case UnitType.Archer:   return 1;
            case UnitType.Warrior:
            case UnitType.Chivalry: return 2;
            case UnitType.Engineer: return 3;
            default: return 4;
        }
    }

    private bool TryAttackBestTarget(UnitState attacker, AIBlackboard bb)
    {
        var rules = bb.Rules;
        var state = bb.State;


        float bestScore = float.MinValue;
        int bestX = -1;
        int bestY = -1;

        int n = state.mapSize;

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                if (!rules.CanAttack(attacker.id, x, y))
                    continue;

                float score = 0f;
                TileState tile = state.tiles[x, y];

                if (tile.unitId != -1)
                {
                    var target = state.units[tile.unitId];

                    int baseValue = AIUtils.GetUnitValue(target.type, bb.Config);
                    score += baseValue;

                    if (target.type == UnitType.Catapult)
                    {
                        score += 50f; 
                    }
                    
                    if (bb.EnemyCatapultThreatensCastleOrFort &&
                        target.type == UnitType.Catapult)
                    {
                        score += 200f;  
                    }

                    int projectedDmg = GetAttackDamage(attacker, target, bb);
                    bool kill = target.hp <= projectedDmg;
                    if (kill)
                    {
                        score += 30f;

                        if (target.type == UnitType.Catapult)
                        {
                            score += 80f;
                        }
                    }

                }
                else if (tile.buildingId != -1)
                {
                    var b = state.buildings[tile.buildingId];

                    float bVal = AIUtils.GetBuildingValue(b.type, bb.Config);
                    score += bVal * 0.2f;

                    if (b.type == BuildingType.Castle)
                    {
                        score += 40f;

                        if (attacker.type == UnitType.Catapult)
                            score += 40f;
                    }

                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        if (bestX != -1)
        {
            bool ok = rules.Attack(attacker.id, bestX, bestY);
            return ok;
        }

        return false;
    }

    private int GetAttackDamage(UnitState attacker, UnitState target, AIBlackboard bb)
    {
        switch (attacker.type)
        {
            case UnitType.Warrior:   return bb.Config.warriorDamage;
            case UnitType.Archer:    return bb.Config.archerDamage;
            case UnitType.Chivalry:  return bb.Config.chivalryDamage;
            case UnitType.Engineer:  return bb.Config.engineerDamage;
            case UnitType.Catapult:  return bb.Config.catapultDamageVsUnits;
        }
        return 0;
    }


    private bool TryMoveToBetterTile(UnitState u, AIBlackboard bb, Func<int, int, float> scoreFunc)
    {
        var rules = bb.Rules;
        var state = bb.State;


        float bestScore = float.MinValue;
        int bestX = u.x, bestY = u.y;

        float stayScore = scoreFunc(u.x, u.y);
        bestScore = stayScore;

        for (int x = 0; x < state.mapSize; x++)
        {
            for (int y = 0; y < state.mapSize; y++)
            {
                if (!rules.CanMoveUnit(u.id, x, y))
                    continue;

                TileState tile = state.tiles[x, y];

                if (tile.buildingId != -1)
                {
                    BuildingState b = state.buildings[tile.buildingId];

                    if (b.owner != u.owner)
                    {
                        continue;
                    }
                }

                float score = scoreFunc(x, y);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = x;
                    bestY = y;
                }
            }
        }


        if (bestX != u.x || bestY != u.y)
        {
            bool moved = rules.MoveUnit(u.id, bestX, bestY);
            return moved;
        }

        return false;
    }

    private bool ControlMelee(UnitState u, AIBlackboard bb)
    {

        if (TryAttackBestTarget(u, bb))
            return true;

        int tx = bb.EnemyCastle.x;
        int ty = bb.EnemyCastle.y;

        bool moved = TryMoveToBetterTile(u, bb, (x, y) =>
            MovementScore(u, x, y, tx, ty, bb)
        );

        return moved;
    }

    private bool ControlArcher(UnitState u, AIBlackboard bb)
    {

        if (TryAttackBestTarget(u, bb))
            return true;

        int tx = bb.EnemyCastle.x;
        int ty = bb.EnemyCastle.y;

        bool moved = TryMoveToBetterTile(u, bb, (x, y) =>
            MovementScore(u, x, y, tx, ty, bb)
        );

        return moved;
    }

    private bool ControlCatapult(UnitState u, AIBlackboard bb)
    {

        if (TryAttackBestTarget(u, bb))
            return true;

        if (u.moveCooldownRemaining > 0)
        {
            return false;
        }

        int tx = bb.EnemyCastle.x;
        int ty = bb.EnemyCastle.y;

        bool moved = TryMoveToBetterTile(u, bb, (x, y) =>
            MovementScore(u, x, y, tx, ty, bb)
        );

        return moved;
    }

    private bool ControlEngineer(UnitState u, AIBlackboard bb)
    {

        if (bb.Rules.CanPlaceFortpost(u.id))
        {
            bb.Rules.PlaceFortpost(u.id);
            return true;
        }

        if (bb.GoldTiles.Count == 0)
            return false;

        TileState target = AIUtils.FindClosestTile(u.x, u.y, bb.GoldTiles);

        bool moved = TryMoveToBetterTile(u, bb, (x, y) =>
            MovementScore(u, x, y, target.x, target.y, bb)
        );

        return moved;
    }


    private float MovementScore(UnitState u, int x, int y, int targetX, int targetY, AIBlackboard bb)
    {
        float score = 0f;

        var cfg = bb.Config;

        int curDist = AIUtils.Manhattan(u.x, u.y, targetX, targetY);
        int newDist = AIUtils.Manhattan(x, y, targetX, targetY);
        score += (curDist - newDist) * 4f; 

        int baseDanger = bb.DangerMap[x, y];

        bool adjacentToCatapult = false;
        int catapultThreatPenalty = 0;
        
        if (bb.EnemyCatapultThreatensCastleOrFort && bb.ThreateningCatapult != null)
        {
            int distNow = AIUtils.Manhattan(u.x, u.y, bb.ThreateningCatapult.x, bb.ThreateningCatapult.y);
            int distNew = AIUtils.Manhattan(x, y, bb.ThreateningCatapult.x, bb.ThreateningCatapult.y);

            score += (distNow - distNew) * 30f;   
            if (distNew == 1 && (u.type == UnitType.Warrior || u.type == UnitType.Chivalry))
                score += 120f;

            if (distNew >= 1 && distNew <= bb.Config.archerRange && u.type == UnitType.Archer)
                score += 80f;
        }

        foreach (var e in bb.EnemyUnits)
        {
            if (e.type != UnitType.Catapult)
                continue;

            int d = AIUtils.Manhattan(x, y, e.x, e.y);

            if (d == 1)
                adjacentToCatapult = true;

            if (d >= 1 && d <= cfg.catapultRange)
            {
                int localPenalty = 0;
                switch (u.type)
                {
                    case UnitType.Archer:
                        localPenalty = 30; 
                        break;
                    case UnitType.Engineer:
                        localPenalty = 40; 
                        break;
                    case UnitType.Warrior:
                    case UnitType.Chivalry:
                        localPenalty = 15; 
                        break;
                    default:
                        localPenalty = 20;
                        break;
                }
                catapultThreatPenalty += localPenalty;
            }

            if (u.type == UnitType.Archer && d >= 1 && d <= cfg.archerRange)
            {
                score += 35f; 
            }
        }

        if (u.type == UnitType.Archer)
        {
            foreach (var e in bb.EnemyUnits)
            {
                int d = AIUtils.Manhattan(x, y, e.x, e.y);
                if (d >= 1 && d <= cfg.archerRange)
                    score += 10f; 

                if (d <= 1)
                    score -= 10f; 
            }
        }

        if (u.type == UnitType.Catapult && bb.EnemyCastle != null)
        {
            int dCastle = AIUtils.Manhattan(x, y, bb.EnemyCastle.x, bb.EnemyCastle.y);
            if (dCastle >= 1 && dCastle <= cfg.catapultRange)
                score += 40f; 
        }


        if (adjacentToCatapult && (u.type == UnitType.Warrior || u.type == UnitType.Chivalry || u.type == UnitType.Engineer))
        {
            score += 40f;        
            score -= baseDanger * 0.2f;
            score -= catapultThreatPenalty * 0.2f;
        }
        else
        {
            score -= baseDanger * 2f;
            score -= catapultThreatPenalty * 1.0f;
        }

        return score;
    }
}
