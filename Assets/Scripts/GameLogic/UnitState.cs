using System;

[Serializable]
public class UnitState
{
    public int id;
    public PlayerId owner;
    public UnitType type;

    public int x;
    public int y;

    public int hp;
    public int maxHp;

    public int moveRange; 
    public bool hasActedThisTurn = false;
    public int actionsLeft = 1;

    public int moveCooldownRemaining = 0;
    
    public bool isDead;
}