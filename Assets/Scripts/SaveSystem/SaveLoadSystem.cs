using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class SaveLoadSystem
{
    public static string GetSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetSlotPath(slot));
    }

    public static void DeleteSlot(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }

    public static void Save(GameState state, int slot)
    {
        string path = GetSlotPath(slot);

        SaveGameData data = new SaveGameData
        {
            config = state.config,
            mapSize = state.mapSize,
            tiles = FlattenTiles(state.tiles),
            units = ConvertUnits(state.units),
            buildings = ConvertBuildings(state.buildings),
            players = ConvertPlayers(state.players),
            currentPlayer = state.currentPlayer,
            gameOver = state.gameOver,
            winner = state.winner
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

    }

    public static GameState Load(int slot)
    {
        string path = GetSlotPath(slot);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        SaveGameData data = JsonUtility.FromJson<SaveGameData>(json);

        if (data == null)
            return null;

        return new GameState
        {
            config = data.config ?? new GameConfig(),
            mapSize = data.mapSize,
            tiles = RebuildTiles(data.tiles, data.mapSize),
            units = RebuildUnits(data.units),
            buildings = RebuildBuildings(data.buildings),
            players = RebuildPlayers(data.players),
            currentPlayer = data.currentPlayer,
            gameOver = data.gameOver,
            winner = data.winner
        };
    }

    [System.Serializable]
    private class SaveGameData
    {
        public GameConfig config;
        public int mapSize;
        public SaveTile[] tiles;
        public List<SaveUnit> units;
        public List<SaveBuilding> buildings;
        public List<SavePlayer> players;
        public PlayerId currentPlayer;
        public bool gameOver;
        public PlayerId winner;
    }

    [System.Serializable] private class SaveTile { public int x, y, unitId, buildingId; public TileTerrain terrain; }
    [System.Serializable] private class SaveUnit { public int id, x, y, hp, maxHp, moveRange, actionsLeft, moveCooldownRemaining; public bool hasActedThisTurn, isDead; public PlayerId owner; public UnitType type; }
    [System.Serializable] private class SaveBuilding { public int id, x, y, hp, maxHp; public PlayerId owner; public BuildingType type; }
    [System.Serializable] private class SavePlayer { public PlayerId id; public int gold; }

    private static SaveTile[] FlattenTiles(TileState[,] tiles)
    {
        int size = tiles.GetLength(0);
        var result = new SaveTile[size * size];
        int index = 0;

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            var t = tiles[x, y];
            result[index++] = new SaveTile
            {
                x = t.x,
                y = t.y,
                terrain = t.terrain,
                unitId = t.unitId,
                buildingId = t.buildingId
            };
        }

        return result;
    }

    private static TileState[,] RebuildTiles(SaveTile[] flat, int size)
    {
        var tiles = new TileState[size, size];

        foreach (var t in flat)
        {
            tiles[t.x, t.y] = new TileState
            {
                x = t.x,
                y = t.y,
                terrain = t.terrain,
                unitId = t.unitId,
                buildingId = t.buildingId
            };
        }

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
            if (tiles[x, y] == null)
                tiles[x, y] = new TileState { x = x, y = y };

        return tiles;
    }

    private static List<SaveUnit> ConvertUnits(List<UnitState> units)
    {
        var result = new List<SaveUnit>();
        foreach (var u in units)
        {
            result.Add(new SaveUnit
            {
                id = u.id,
                owner = u.owner,
                type = u.type,
                x = u.x,
                y = u.y,
                hp = u.hp,
                maxHp = u.maxHp,
                moveRange = u.moveRange,
                actionsLeft = u.actionsLeft,
                hasActedThisTurn = u.hasActedThisTurn,
                moveCooldownRemaining = u.moveCooldownRemaining,
                isDead = u.isDead
            });
        }
        return result;
    }

    private static List<UnitState> RebuildUnits(List<SaveUnit> saved)
    {
        var result = new List<UnitState>();
        foreach (var u in saved)
        {
            result.Add(new UnitState
            {
                id = u.id,
                owner = u.owner,
                type = u.type,
                x = u.x,
                y = u.y,
                hp = u.hp,
                maxHp = u.maxHp,
                moveRange = u.moveRange,
                actionsLeft = u.actionsLeft,
                hasActedThisTurn = u.hasActedThisTurn,
                moveCooldownRemaining = u.moveCooldownRemaining,
                isDead = u.isDead
            });
        }
        return result;
    }

    private static List<SaveBuilding> ConvertBuildings(List<BuildingState> buildings)
    {
        var list = new List<SaveBuilding>();
        foreach (var b in buildings)
        {
            list.Add(new SaveBuilding
            {
                id = b.id,
                owner = b.owner,
                type = b.type,
                x = b.x,
                y = b.y,
                hp = b.hp,
                maxHp = b.maxHp
            });
        }
        return list;
    }

    private static List<BuildingState> RebuildBuildings(List<SaveBuilding> saved)
    {
        var list = new List<BuildingState>();
        foreach (var b in saved)
        {
            list.Add(new BuildingState
            {
                id = b.id,
                owner = b.owner,
                type = b.type,
                x = b.x,
                y = b.y,
                hp = b.hp,
                maxHp = b.maxHp
            });
        }
        return list;
    }

    private static List<SavePlayer> ConvertPlayers(List<PlayerState> players)
    {
        var list = new List<SavePlayer>();
        foreach (var p in players)
        {
            list.Add(new SavePlayer { id = p.id, gold = p.gold });
        }
        return list;
    }

    private static List<PlayerState> RebuildPlayers(List<SavePlayer> saved)
    {
        var list = new List<PlayerState>();
        foreach (var p in saved)
        {
            list.Add(new PlayerState { id = p.id, gold = p.gold });
        }
        return list;
    }
}
