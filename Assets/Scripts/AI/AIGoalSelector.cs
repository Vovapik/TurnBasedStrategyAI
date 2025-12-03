using System.Linq;

public class AIGoalSelector
{
    public AIGlobalGoal DecideGoal(AIBlackboard bb)
    {
        if (bb.EnemyCatapultThreatensCastleOrFort)
            return AIGlobalGoal.KillCatapult;

        if (IsMyCastleInDanger(bb))
            return AIGlobalGoal.DefendCastle;

        if (CanPressureEnemyCastle(bb))
            return AIGlobalGoal.DestroyCastle;

        if (ShouldExpandEconomy(bb))
            return AIGlobalGoal.ExpandEconomy;

        if (ArmyTooSmall(bb))
            return AIGlobalGoal.BuildArmy;

        return AIGlobalGoal.Advance;
    }


    private bool IsMyCastleInDanger(AIBlackboard bb)
    {
        if (bb.MyCastle == null) return false;

        int danger = bb.DangerMap[bb.MyCastle.x, bb.MyCastle.y];
        if (danger > 0) return true;

        foreach (var e in bb.EnemyUnits)
        {
            int dist = AIUtils.Manhattan(e.x, e.y, bb.MyCastle.x, bb.MyCastle.y);
            if (dist <= 3) return true;
        }

        return false;
    }

    private bool CanPressureEnemyCastle(AIBlackboard bb)
    {
        if (bb.EnemyCastle == null) return false;

        if (bb.MyCatapults.Count > 0)
            return true;

        foreach (var u in bb.MyUnits)
        {
            if (u.type == UnitType.Warrior || u.type == UnitType.Chivalry)
            {
                int dist = AIUtils.Manhattan(u.x, u.y, bb.EnemyCastle.x, bb.EnemyCastle.y);
                if (dist <= 5)
                    return true;
            }
        }

        return false;
    }

    private bool ShouldExpandEconomy(AIBlackboard bb)
    {
        int fortposts = bb.MyBuildings.Count(b => b.type == BuildingType.Fortpost);

        if (bb.GoldTiles.Count == 0) return false;
        if (fortposts >= 2) return false;

        if (bb.MyEngineers.Count == 0) return false;

        return true;
    }

    private bool ArmyTooSmall(AIBlackboard bb)
    {
        if (bb.MyUnits.Count <= 2 && bb.EnemyUnits.Count >= 2)
            return true;

        if (bb.MyUnits.Count + 1 < bb.EnemyUnits.Count)
            return true;

        return false;
    }
}
