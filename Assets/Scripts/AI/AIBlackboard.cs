using System.Collections.Generic;

public enum AIGlobalGoal
{
    KillCatapult,
    DefendCastle,
    DestroyCastle,
    ExpandEconomy,
    BuildArmy,
    Advance
}


public class AIBlackboard
{
    public GameState State;
    public GameRules Rules;
    public GameConfig Config;

    public PlayerId MyId = PlayerId.AI;
    public PlayerId EnemyId = PlayerId.Human;

    public List<UnitState> MyUnits = new List<UnitState>();
    public List<UnitState> EnemyUnits = new List<UnitState>();

    public List<UnitState> MyEngineers = new List<UnitState>();
    public List<UnitState> MyArchers = new List<UnitState>();
    public List<UnitState> MyCatapults = new List<UnitState>();

    public List<BuildingState> MyBuildings = new List<BuildingState>();
    public List<BuildingState> EnemyBuildings = new List<BuildingState>();

    public BuildingState MyCastle;
    public BuildingState EnemyCastle;

    public List<TileState> GoldTiles = new List<TileState>();

    public int[,] DangerMap;

    public int MyGold;
    
    public AIGlobalGoal GlobalGoal;
    
    public bool EnemyCatapultThreatensCastleOrFort;
    public UnitState ThreateningCatapult;
}