using System.Collections.Generic;
using System.Linq;

public class AIEconomy
{
    public void RunEconomy(AIBlackboard bb)
    {
        TryPlaceFortposts(bb);

        TryCreateUnits(bb, maxUnitsToCreate: 2);
    }

    private void TryPlaceFortposts(AIBlackboard bb)
    {
        foreach (var eng in bb.MyEngineers)
        {
            if (eng.isDead) continue;
            if (bb.Rules.CanPlaceFortpost(eng.id))
            {
                bb.Rules.PlaceFortpost(eng.id);
            }
        }
    }

    private void TryCreateUnits(AIBlackboard bb, int maxUnitsToCreate)
    {
        var state = bb.State;
        var cfg = bb.Config;

        List<BuildingState> producers = new List<BuildingState>();
        foreach (var b in state.buildings)
        {
            if (b.owner == bb.MyId &&
                (b.type == BuildingType.Castle || b.type == BuildingType.Fortpost))
            {
                producers.Add(b);
            }
        }

        int created = 0;
        bool canStillAfford = true;

        while (created < maxUnitsToCreate && canStillAfford)
        {
            UnitType? choice = ChooseUnitType(bb);

            if (!choice.HasValue)
                break;

            int chosenCost = cfg.GetUnitCost(choice.Value);
            if (bb.MyGold < chosenCost)
                break;

            bool success = false;
            foreach (var prod in producers)
            {
                if (bb.Rules.CanCreateUnit(bb.MyId, prod.id, choice.Value))
                {
                    var unit = bb.Rules.CreateUnit(bb.MyId, prod.id, choice.Value);
                    if (unit != null)
                    {
                        bb.MyGold -= chosenCost;
                        created++;
                        success = true;
                        break;
                    }
                }
            }

            if (!success)
            {
                canStillAfford = false;
            }
        }
    }

    private UnitType? ChooseUnitType(AIBlackboard bb)
    {
        var cfg = bb.Config;

        float bestScore = float.MinValue;
        UnitType? bestType = null;

        EvaluateType(UnitType.Warrior, bb, ref bestType, ref bestScore);
        EvaluateType(UnitType.Archer, bb, ref bestType, ref bestScore);
        EvaluateType(UnitType.Chivalry, bb, ref bestType, ref bestScore);
        EvaluateType(UnitType.Engineer, bb, ref bestType, ref bestScore);
        EvaluateType(UnitType.Catapult, bb, ref bestType, ref bestScore);

        return bestType;
    }

    private void EvaluateType(
        UnitType type,
        AIBlackboard bb,
        ref UnitType? bestType,
        ref float bestScore)
    {
        int cost = bb.Config.GetUnitCost(type);
        if (bb.MyGold < cost) return;

        float score = 0f;

        switch (type)
        {
            case UnitType.Warrior:   score += 10f; break;
            case UnitType.Archer:    score += 12f; break;
            case UnitType.Chivalry:  score += 11f; break;
            case UnitType.Engineer:  score += 2f;  break;
            case UnitType.Catapult:  score += 0f;  break;
        }

        switch (bb.GlobalGoal)
        {
            case AIGlobalGoal.DefendCastle:
                if (type == UnitType.Warrior || type == UnitType.Archer)
                    score += 15f;
                break;

            case AIGlobalGoal.DestroyCastle:
                if (type == UnitType.Catapult)
                    score += 30f;
                if (type == UnitType.Chivalry)
                    score += 10f;
                break;

            case AIGlobalGoal.ExpandEconomy:
                if (type == UnitType.Engineer)
                    score += 25f;
                break;

            case AIGlobalGoal.BuildArmy:
                if (type == UnitType.Warrior || type == UnitType.Archer)
                    score += 20f;
                break;

            case AIGlobalGoal.Advance:
                if (type == UnitType.Warrior || type == UnitType.Archer || type == UnitType.Chivalry)
                    score += 10f;
                break;
        }

        int myMeleeCount = bb.MyUnits.Count(u => u.type == UnitType.Warrior || u.type == UnitType.Chivalry);
        int myRangedCount = bb.MyUnits.Count(u => u.type == UnitType.Archer || u.type == UnitType.Catapult);

        if (type == UnitType.Archer && myMeleeCount > myRangedCount)
            score += 8f;  

        if (type == UnitType.Warrior && myMeleeCount < myRangedCount)
            score += 5f;  

        if (type == UnitType.Engineer && bb.GoldTiles.Count > 0 && bb.MyEngineers.Count == 0)
            score += 15f;

        if (type == UnitType.Catapult && bb.EnemyCastle != null && bb.MyCatapults.Count == 0)
            score += 15f;

        if (bb.MyGold < cost * 2)
            score -= cost * 0.2f;

        if (score > bestScore)
        {
            bestScore = score;
            bestType = type;
        }
    }
}
