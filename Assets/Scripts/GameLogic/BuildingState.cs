using System;

[Serializable]
public class BuildingState
{
    public int id;
    public PlayerId owner;

    public BuildingType type;

    public int x;
    public int y;

    public int hp;
    public int maxHp;
}