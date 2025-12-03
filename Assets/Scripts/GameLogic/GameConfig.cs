using System;

[Serializable]
public class GameConfig
{
    public int mapSize = 8;
    public int goldTileCount = 6;

    public int startingGold = 40;          
    public int castleIncomePerTurn = 10;
    public int fortpostIncomePerTurn = 7;    

    public int costFortpost = 70;          
    public int costWarrior = 10;
    public int costArcher = 16;
    public int costChivalry = 28;
    public int costEngineer = 15;
    public int costCatapult = 38;

    public int castleHp = 80;                
    public int fortpostHp = 40;              


    public int warriorHp = 24;
    public int warriorDamage = 8;
    public int warriorMoveRange = 1;

    public int archerHp = 14;
    public int archerDamage = 6;
    public int archerMoveRange = 1;
    public int archerRange = 3;

    public int chivalryHp = 32;
    public int chivalryDamage = 8;
    public int chivalryMoveRange = 1;

    public int engineerHp = 14;
    public int engineerDamage = 2;
    public int engineerMoveRange = 1;

    public int catapultHp = 26;
    public int catapultDamage = 14;
    public int catapultDamageVsUnits = 6;          
    public int catapultRange = 4;
    public int catapultMoveCooldownTurns = 1;

    public float damageMultiplierOnCastleFromNonCatapult = 0.5f;  
    public float damageMultiplierOnFortpostFromNonCatapult = 0.65f;
    
    public float catapultCastleMultiplier = 1.6f;  
    public float catapultFortMultiplier = 1.3f;

    public int GetUnitCost(UnitType type)
    {
        switch (type)
        {
            case UnitType.Warrior:   return costWarrior;
            case UnitType.Archer:    return costArcher;
            case UnitType.Chivalry:  return costChivalry;
            case UnitType.Engineer:  return costEngineer;
            case UnitType.Catapult:  return costCatapult;
            default: return 0;
        }
    }
}
