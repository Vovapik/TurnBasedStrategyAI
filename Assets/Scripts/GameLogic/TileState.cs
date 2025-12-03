using System;

[Serializable]
public class TileState
{
    public int x;
    public int y;

    public TileTerrain terrain = TileTerrain.Plain;

    public int unitId = -1;      
    public int buildingId = -1;
}