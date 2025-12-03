using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class GameState
{
    public GameConfig config = new GameConfig();
    public int mapSize;
    public TileState[,] tiles;
    public List<UnitState> units = new List<UnitState>();
    public List<BuildingState> buildings = new List<BuildingState>();
    public List<PlayerState> players = new List<PlayerState>();
    public PlayerId currentPlayer = PlayerId.Human;
    public bool gameOver = false;
    public PlayerId winner;
    [FormerlySerializedAs("matchStats")] public MatchStatistics matchStatistics = new MatchStatistics();
}