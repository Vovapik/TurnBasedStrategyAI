using System;
using UnityEngine; 

public class GameRules
{
    private readonly GameState state;

    public GameState State => state;

    public GameRules(GameState state)
    {
        this.state = state;
    }
    
    public void StartTurn()
    {
        RefreshUnits(state.currentPlayer);
        GrantIncome(state.currentPlayer);
    }

    private void RefreshUnits(PlayerId player)
    {
        foreach (var u in state.units)
        {
            if (u.owner != player || u.isDead)
                continue;

            u.hasActedThisTurn = false;

            u.actionsLeft = (u.type == UnitType.Chivalry ? 2 : 1);

            if (u.type == UnitType.Catapult && u.moveCooldownRemaining > 0)
                u.moveCooldownRemaining--;
        }
    }

    private void GrantIncome(PlayerId player)
    {
        GameConfig cfg = state.config;
        PlayerState ps = GetPlayer(player);

        int income = 0;

        foreach (var b in state.buildings)
        {
            if (b.owner != player) continue;

            if (b.type == BuildingType.Castle)
                income += cfg.castleIncomePerTurn;
            else if (b.type == BuildingType.Fortpost)
                income += cfg.fortpostIncomePerTurn;
        }

        ps.gold += income;
        StatsManager.stats.totalGoldEarned += income;
        state.matchStatistics.goldEarned += income;
        StatsManager.Save();

    }

    public void EndTurn()
    {
        if (state.gameOver) return;

        state.currentPlayer = 
            (state.currentPlayer == PlayerId.Human ? PlayerId.AI : PlayerId.Human);
        StatsManager.stats.totalTurnsPlayed++;
        StatsManager.Save();
        state.matchStatistics.turnsPlayed++;

        StartTurn();
    }

    private PlayerState GetPlayer(PlayerId id)
    {
        return state.players.Find(p => p.id == id);
    }


    public bool CanCreateUnit(PlayerId player, int buildingId, UnitType type)
    {
        if (buildingId < 0 || buildingId >= state.buildings.Count)
            return false;

        BuildingState b = state.buildings[buildingId];
        if (b.owner != player) return false;
        if (b.type != BuildingType.Castle && b.type != BuildingType.Fortpost)
            return false;

        PlayerState ps = GetPlayer(player);
        int cost = state.config.GetUnitCost(type);
        if (ps.gold < cost) return false;

        TileState tile = state.tiles[b.x, b.y];
        if (tile.unitId != -1) return false;

        return true;
    }

    public UnitState CreateUnit(PlayerId player, int buildingId, UnitType type)
    {
        if (!CanCreateUnit(player, buildingId, type))
            return null;

        BuildingState b = state.buildings[buildingId];
        PlayerState ps = GetPlayer(player);
        int cost = state.config.GetUnitCost(type);

        ps.gold -= cost;
        StatsManager.stats.totalGoldSpent += cost;
        StatsManager.stats.totalUnitsCreated++;
        StatsManager.Save();
        state.matchStatistics.unitsCreated++;
        state.matchStatistics.goldSpent += cost;

        int x = b.x;
        int y = b.y;

        UnitState u = BuildUnit(type, player, x, y);
        state.units.Add(u);
        state.tiles[x, y].unitId = u.id;

        return u;
    }

    private UnitState BuildUnit(UnitType type, PlayerId owner, int x, int y)
    {
        GameConfig cfg = state.config;

        UnitState u = new UnitState
        {
            id = state.units.Count,
            owner = owner,
            type = type,
            x = x,
            y = y
        };

        switch (type)
        {
            case UnitType.Warrior:
                u.hp = u.maxHp = cfg.warriorHp;
                u.moveRange = cfg.warriorMoveRange;
                break;

            case UnitType.Archer:
                u.hp = u.maxHp = cfg.archerHp;
                u.moveRange = cfg.archerMoveRange;
                break;

            case UnitType.Chivalry:
                u.hp = u.maxHp = cfg.chivalryHp;
                u.moveRange = cfg.chivalryMoveRange;
                break;

            case UnitType.Engineer:
                u.hp = u.maxHp = cfg.engineerHp;
                u.moveRange = cfg.engineerMoveRange;
                break;

            case UnitType.Catapult:
                u.hp = u.maxHp = cfg.catapultHp;
                u.moveRange = 1;
                u.moveCooldownRemaining = 0;
                break;
        }

        u.actionsLeft = (type == UnitType.Chivalry ? 2 : 1);

        return u;
    }




    public bool CanMoveUnit(int unitId, int targetX, int targetY)
    {
        if (unitId < 0 || unitId >= state.units.Count)
            return false;

        UnitState u = state.units[unitId];

        if (u.owner != state.currentPlayer) return false;
        if (u.actionsLeft <= 0) return false;

        if (!InsideBoard(targetX, targetY)) return false;

        TileState tile = state.tiles[targetX, targetY];
        if (tile.unitId != -1) return false;

        int dist = Math.Abs(targetX - u.x) + Math.Abs(targetY - u.y);
        if (dist > u.moveRange) return false;

        if (u.type == UnitType.Catapult && u.moveCooldownRemaining > 0)
            return false;

        return true;
    }

    public bool MoveUnit(int unitId, int targetX, int targetY)
    {
        if (!CanMoveUnit(unitId, targetX, targetY)) return false;

        UnitState u = state.units[unitId];

        state.tiles[u.x, u.y].unitId = -1;

        u.x = targetX;
        u.y = targetY;

        state.tiles[targetX, targetY].unitId = u.id;

        u.actionsLeft--;

        if (u.actionsLeft <= 0)
            u.hasActedThisTurn = true;

        if (u.type == UnitType.Catapult)
            u.moveCooldownRemaining = state.config.catapultMoveCooldownTurns;

        return true;

    }

    private bool InsideBoard(int x, int y)
    {
        return x >= 0 && x < state.mapSize && y >= 0 && y < state.mapSize;
    }
    

public bool CanAttack(int attackerId, int targetX, int targetY)
{
    if (attackerId < 0 || attackerId >= state.units.Count)
        return false;

    UnitState attacker = state.units[attackerId];

    if (attacker.owner != state.currentPlayer) return false;
    if (attacker.actionsLeft <= 0) return false;    
    if (!InsideBoard(targetX, targetY)) return false;

    TileState tile = state.tiles[targetX, targetY];

    int targetUnitId = tile.unitId;
    int targetBuildingId = tile.buildingId;

    if (targetUnitId == -1 && targetBuildingId == -1)
        return false;

    if (targetUnitId != -1 && state.units[targetUnitId].owner == attacker.owner)
        return false;

    if (targetBuildingId != -1 && state.buildings[targetBuildingId].owner == attacker.owner)
        return false;

    int dist = Math.Abs(attacker.x - targetX) + Math.Abs(attacker.y - targetY);

    GameConfig cfg = state.config;

    switch (attacker.type)
    {
        case UnitType.Warrior:
        case UnitType.Chivalry:
        case UnitType.Engineer:
            return dist == 1; 

        case UnitType.Archer:
            return dist >= 1 && dist <= cfg.archerRange;

        case UnitType.Catapult:
            return dist >= 1 && dist <= cfg.catapultRange;

        default:
            return false;
    }
}

public bool Attack(int attackerId, int targetX, int targetY)
{
    if (!CanAttack(attackerId, targetX, targetY))
        return false;

    UnitState attacker = state.units[attackerId];
    GameConfig cfg = state.config;

    int dmg = GetUnitBaseDamage(attacker.type);

    TileState tile = state.tiles[targetX, targetY];

    if (tile.unitId != -1)
    {
        UnitState target = state.units[tile.unitId];
        ApplyDamageToUnit(target, dmg);
    }
    else if (tile.buildingId != -1)
    {
        BuildingState b = state.buildings[tile.buildingId];
        float multiplier = 1f;
        int baseDamage = dmg; 

        if (attacker.type == UnitType.Catapult)
        {
            baseDamage = state.config.catapultDamage;

            if (b.type == BuildingType.Castle)
                multiplier = state.config.catapultCastleMultiplier;
            else if (b.type == BuildingType.Fortpost)
                multiplier = state.config.catapultFortMultiplier;
        }
        else
        {
            if (b.type == BuildingType.Castle)
                multiplier = state.config.damageMultiplierOnCastleFromNonCatapult;
            if (b.type == BuildingType.Fortpost)
                multiplier = state.config.damageMultiplierOnFortpostFromNonCatapult;
        }

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        ApplyDamageToBuilding(b, finalDamage);

    }


    attacker.actionsLeft--;

    if (attacker.actionsLeft <= 0)
        attacker.hasActedThisTurn = true;

    return true;

}


private int GetUnitBaseDamage(UnitType type)
{
    var cfg = state.config;

    return type switch
    {
        UnitType.Warrior   => cfg.warriorDamage,
        UnitType.Archer    => cfg.archerDamage,
        UnitType.Chivalry  => cfg.chivalryDamage,
        UnitType.Engineer  => cfg.engineerDamage,
        UnitType.Catapult  => cfg.catapultDamageVsUnits,  
        _ => 0
    };
}



private void ApplyDamageToUnit(UnitState target, int dmg)
{
    target.hp -= dmg;

    if (target.hp <= 0)
    {
        target.hp = 0;
        target.isDead = true;

        state.tiles[target.x, target.y].unitId = -1;
        StatsManager.stats.totalUnitsKilled++;
        StatsManager.Save();
        state.matchStatistics.unitsKilled++;
    }
}




    private void ApplyDamageToBuilding(BuildingState b, int dmg)
    {
        b.hp -= dmg;

        if (b.hp > 0) return;

        state.tiles[b.x, b.y].buildingId = -1;

        if (b.type == BuildingType.Castle)
        {
            state.gameOver = true;
            state.winner = (b.owner == PlayerId.Human) ? PlayerId.AI : PlayerId.Human;
        }
    }
    

    public bool CanPlaceFortpost(int engineerUnitId)
    {
        if (engineerUnitId < 0 || engineerUnitId >= state.units.Count)
            return false;

        UnitState u = state.units[engineerUnitId];

        if (u.owner != state.currentPlayer) return false;
        if (u.type != UnitType.Engineer) return false;
        if (u.hasActedThisTurn) return false;

        TileState tile = state.tiles[u.x, u.y];
        if (tile.terrain != TileTerrain.Gold) return false;
        if (tile.buildingId != -1) return false;

        PlayerState ps = GetPlayer(u.owner);
        if (ps.gold < state.config.costFortpost) return false;

        return true;
    }

    public bool PlaceFortpost(int engineerUnitId)
    {
        if (!CanPlaceFortpost(engineerUnitId)) return false;

        UnitState u = state.units[engineerUnitId];
        PlayerState ps = GetPlayer(u.owner);
        GameConfig cfg = state.config;
        TileState tile = state.tiles[u.x, u.y];

        ps.gold -= cfg.costFortpost;

        BuildingState b = new BuildingState
        {
            id = state.buildings.Count,
            owner = u.owner,
            type = BuildingType.Fortpost,
            x = u.x,
            y = u.y,
            hp = cfg.fortpostHp,
            maxHp = cfg.fortpostHp
        };

        state.buildings.Add(b);
        tile.buildingId = b.id;

        u.hasActedThisTurn = true;
        return true;
    }
    
    public int GetExpectedIncome(PlayerId player)
    {
        int income = 0;

        foreach (var b in state.buildings)
        {
            if (b.owner != player)
                continue;

            if (b.type == BuildingType.Castle)
                income += state.config.castleIncomePerTurn;

            if (b.type == BuildingType.Fortpost)
                income += state.config.fortpostIncomePerTurn;
        }

        return income;
    }

    
    
}
