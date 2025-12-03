using System;

public static class GameFactory
{
    public static GameState CreateNewGame(int mapSize, GameConfig config = null, int? seed = null)
    {
        GameState state = new GameState();
        state.mapSize = mapSize;
        state.config = config ?? new GameConfig();
        state.config.mapSize = mapSize;

        state.players.Add(new PlayerState { id = PlayerId.Human, gold = state.config.startingGold });
        state.players.Add(new PlayerState { id = PlayerId.AI, gold = state.config.startingGold });

        state.tiles = new TileState[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                state.tiles[x, y] = new TileState { x = x, y = y };
            }
        }

        Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

        PlaceCastles(state);
        PlaceGold(state, rng);

        return state;
    }

    private static void PlaceCastles(GameState state)
    {
        int n = state.mapSize;

        CreateCastle(state, PlayerId.Human, 1, 1);
        CreateCastle(state, PlayerId.AI, n - 2, n - 2);
    }

    private static void CreateCastle(GameState state, PlayerId owner, int x, int y)
    {
        GameConfig cfg = state.config;

        BuildingState b = new BuildingState
        {
            id = state.buildings.Count,
            owner = owner,
            type = BuildingType.Castle,
            x = x,
            y = y,
            hp = cfg.castleHp,
            maxHp = cfg.castleHp
        };

        state.buildings.Add(b);
        state.tiles[x, y].buildingId = b.id;
    }

    private static void PlaceGold(GameState state, Random rng)
    {
        int n = state.mapSize;
        int toPlace = state.config.goldTileCount;
        int safety = n * n * 10;

        while (toPlace > 0 && safety-- > 0)
        {
            int x = rng.Next(0, n);
            int y = rng.Next(0, n);

            TileState tile = state.tiles[x, y];

            if (tile.buildingId != -1) continue;

            if (tile.terrain == TileTerrain.Gold) continue;

            tile.terrain = TileTerrain.Gold;
            toPlace--;
        }
    }
}
