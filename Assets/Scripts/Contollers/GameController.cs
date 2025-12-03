using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public Tilemap groundTilemap;
    public CastleUI unitUI;
    public GameClickController clickController;
    public CastleUI castleUI;
    public EngineerUI engineerUI;
    public AIManager aiManager;
    public VictoryPanelUI victoryPanel;
    public GameObject gameplayHUD;

    [Header("Building Prefabs / Player")]
    public GameObject playerCastlePrefab;
    public GameObject playerFortpostPrefab;

    [Header("Building Prefabs / Enemy")]
    public GameObject enemyCastlePrefab;
    public GameObject enemyFortpostPrefab;

    public GameObject goldPrefab;

    [Header("Unit Prefabs / Player")]
    public GameObject playerWarriorPrefab;
    public GameObject playerArcherPrefab;
    public GameObject playerChivalryPrefab;
    public GameObject playerEngineerPrefab;
    public GameObject playerCatapultPrefab;

    [Header("Unit Prefabs / Enemy")]
    public GameObject enemyWarriorPrefab;
    public GameObject enemyArcherPrefab;
    public GameObject enemyChivalryPrefab;
    public GameObject enemyEngineerPrefab;
    public GameObject enemyCatapultPrefab;


    public GameObject healthBarPrefab;

    public GameState gameState;
    public GameRules rules;

    private readonly Dictionary<int, GameObject> unitViews = new();
    private readonly Dictionary<int, GameObject> buildingViews = new();
    private readonly Dictionary<Vector2Int, GameObject> goldViews = new();
    private readonly Dictionary<int, GameObject> healthBars = new();
    private readonly Dictionary<int, GameObject> buildingHealthBars = new();

    private void Start()
    {
        bool loaded = SaveSession.isLoadRequested;

        if (loaded && SaveLoadSystem.HasSave(SaveSession.loadSlot))
        {
            gameState = SaveLoadSystem.Load(SaveSession.loadSlot);
            Debug.Log("Loaded save slot " + SaveSession.loadSlot);
        }
        else
        {
            CreateNewGame();
            loaded = false;
        }

        SaveSession.isLoadRequested = false; 


        mapGenerator.mapSize = gameState.mapSize;
        mapGenerator.gameController = this;
        mapGenerator.GenerateTilemap();

        SpawnGoldViews();
        BuildInitialBuildingViews();
        RebuildUnitViews();

        rules = new GameRules(gameState);

        if (!loaded)
            rules.StartTurn();

        aiManager = new AIManager(gameState, rules);

        FindObjectOfType<GoldUI>()?.Refresh();
    }
    
    private void Update()
    {
        if (gameState != null && gameState.gameOver)
            HandleGameOver();
    }


    private void CreateNewGame()
    {
        int size = mapGenerator.mapSize;
        var config = new GameConfig { mapSize = size };

        gameState = GameFactory.CreateNewGame(size, config);
        rules = new GameRules(gameState);
        gameState.matchStatistics = new MatchStatistics();
        gameState.matchStatistics.Reset();
    }

    private void BuildInitialBuildingViews()
    {
        foreach (var kv in buildingViews)
            if (kv.Value != null) Destroy(kv.Value);

        buildingViews.Clear();

        foreach (var b in gameState.buildings)
            if (b.hp > 0) CreateBuildingView(b);
    }

    public void CreateBuildingView(BuildingState b)
    {
        GameObject prefab = null;

        if (b.owner == PlayerId.Human)
        {
            prefab = b.type switch
            {
                BuildingType.Castle    => playerCastlePrefab,
                BuildingType.Fortpost  => playerFortpostPrefab,
                _ => null
            };
        }
        else // AI
        {
            prefab = b.type switch
            {
                BuildingType.Castle    => enemyCastlePrefab,
                BuildingType.Fortpost  => enemyFortpostPrefab,
                _ => null
            };
        }


        if (prefab == null)
        {
            Debug.LogWarning("Missing prefab for building: " + b.type);
            return;
        }

        Vector3 worldPos = mapGenerator.GridToWorldPosition(new Vector2Int(b.x, b.y));
        worldPos.z = -0.7f;

        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);
        AutoScaleToTile(go, 1.7f);

        buildingViews[b.id] = go;
        
        if (healthBarPrefab != null)
        {
            GameObject bar = Instantiate(healthBarPrefab);
            var hb = bar.GetComponent<HealthBar>();
            hb.Init(go.transform, b.maxHp);
            hb.SetHealth(b.hp);

            buildingHealthBars[b.id] = bar;
        }
    }

    private void SpawnGoldViews()
    {
        foreach (var kv in goldViews)
            if (kv.Value != null) Destroy(kv.Value);
        goldViews.Clear();

        int n = gameState.mapSize;
        for (int x = 0; x < n; x++)
        for (int y = 0; y < n; y++)
        {
            if (gameState.tiles[x, y].terrain == TileTerrain.Gold)
            {
                Vector3 world = mapGenerator.GridToWorldPosition(new Vector2Int(x, y));
                world.z = -0.6f;

                GameObject gold = Instantiate(goldPrefab, world, Quaternion.identity);
                AutoScaleToTile(gold, 1.5f);

                goldViews[new Vector2Int(x, y)] = gold;
            }
        }
    }

    private void RebuildUnitViews()
    {
        foreach (var v in unitViews.Values)
            if (v != null) Destroy(v);
        unitViews.Clear();

        foreach (var bar in healthBars.Values)
            if (bar != null) Destroy(bar);
        healthBars.Clear();

        foreach (var u in gameState.units)
            if (!u.isDead) CreateUnitView(u);
    }

    private GameObject GetUnitPrefab(UnitState u)
    {
        bool isPlayer = (u.owner == PlayerId.Human);

        if (isPlayer)
        {
            return u.type switch
            {
                UnitType.Warrior   => playerWarriorPrefab,
                UnitType.Archer    => playerArcherPrefab,
                UnitType.Chivalry  => playerChivalryPrefab,
                UnitType.Engineer  => playerEngineerPrefab,
                UnitType.Catapult  => playerCatapultPrefab,
                _ => null
            };
        }
        else 
        {
            return u.type switch
            {
                UnitType.Warrior   => enemyWarriorPrefab,
                UnitType.Archer    => enemyArcherPrefab,
                UnitType.Chivalry  => enemyChivalryPrefab,
                UnitType.Engineer  => enemyEngineerPrefab,
                UnitType.Catapult  => enemyCatapultPrefab,
                _ => null
            };
        }
    }


    public void CreateUnitView(UnitState u)
    {
        GameObject prefab = GetUnitPrefab(u);
        if (prefab == null) return;

        Vector3 worldPos = mapGenerator.GridToWorldPosition(new Vector2Int(u.x, u.y));
        worldPos.z = -1f;

        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);
        AutoScaleToTile(go, 1.3f);

        unitViews[u.id] = go;

        if (healthBarPrefab != null)
        {
            GameObject bar = Instantiate(healthBarPrefab);
            var hb = bar.GetComponent<HealthBar>();
            hb.Init(go.transform, u.maxHp);
            hb.SetHealth(u.hp);

            healthBars[u.id] = bar;
        }
    }

    private void AutoScaleToTile(GameObject obj, float factor = 1.0f)
    {
        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            return;

        float tileSize = mapGenerator.GetTileWorldSize();
        float maxSide = Mathf.Max(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
        float scale = (tileSize / maxSide) * factor;

        obj.transform.localScale = new Vector3(scale, scale, 1f);
    }


    public void EndPlayerTurn()
    {
        rules.EndTurn();  

        clickController.ClearSelectionOnTurnEnd();
        FindObjectOfType<GoldUI>()?.Refresh();

        if (gameState.currentPlayer == PlayerId.AI && !gameState.gameOver)
        {
            aiManager.TakeTurn();
            RefreshAllViews();
            FindObjectOfType<GoldUI>()?.Refresh();
        }
    }


    public void RefreshAllViews()
    {
        foreach (var u in gameState.units)
            if (!u.isDead && !unitViews.ContainsKey(u.id))
                CreateUnitView(u);

        foreach (var b in gameState.buildings)
            if (b.hp > 0 && !buildingViews.ContainsKey(b.id))
                CreateBuildingView(b);

        foreach (var u in gameState.units)
        {
            if (u.isDead) continue;

            if (healthBars.TryGetValue(u.id, out GameObject bar))
                bar.GetComponent<HealthBar>().SetHealth(u.hp);

            if (unitViews.TryGetValue(u.id, out GameObject go))
            {
                Vector3 world = mapGenerator.GridToWorldPosition(new Vector2Int(u.x, u.y));
                world.z = -1f;
                go.transform.position = world;
            }
        }
        
        foreach (var b in gameState.buildings)
        {
            if (b.hp > 0 && buildingHealthBars.TryGetValue(b.id, out GameObject bar))
                bar.GetComponent<HealthBar>().SetHealth(b.hp);

            if (b.hp > 0 && buildingViews.TryGetValue(b.id, out GameObject go))
            {
                Vector3 world = mapGenerator.GridToWorldPosition(new Vector2Int(b.x, b.y));
                world.z = -0.7f;
                go.transform.position = world;
            }
        }

        RemoveDeadUnits();
        RemoveDestroyedBuildings();
    }

    private void RemoveDeadUnits()
    {
        var toRemove = new List<int>();

        foreach (var u in gameState.units)
        {
            if (u.isDead && unitViews.ContainsKey(u.id))
            {
                Destroy(unitViews[u.id]);
                toRemove.Add(u.id);

                if (healthBars.ContainsKey(u.id))
                {
                    Destroy(healthBars[u.id]);
                    healthBars.Remove(u.id);
                }
            }
        }

        foreach (var id in toRemove)
            unitViews.Remove(id);
    }

    private void RemoveDestroyedBuildings()
    {
        var toRemove = new List<int>();

        foreach (var b in gameState.buildings)
        {
            if (b.hp <= 0 && buildingViews.ContainsKey(b.id))
            {
                Destroy(buildingViews[b.id]);
                toRemove.Add(b.id);
                
                if (buildingHealthBars.ContainsKey(b.id))
                {
                    Destroy(buildingHealthBars[b.id]);
                    buildingHealthBars.Remove(b.id);
                }
            }
        }

        foreach (var id in toRemove)
            buildingViews.Remove(id);
    }

    public void SaveCurrentGame(int slot)
    {
        SaveLoadSystem.Save(gameState, slot);
    }
    
    public int GetPlayerGold(PlayerId id)
    {
        var p = gameState.players.Find(p => p.id == id);
        return p != null ? p.gold : 0;
    }
    
    private void HandleGameOver()
    {
        if (!gameState.gameOver)
            return;  

        if (victoryPanel.panel.activeSelf)
            return;  

        gameplayHUD.SetActive(false);
        clickController.enabled = false;

        victoryPanel.Show(gameState.winner);
    }
    
    public int GetExpectedIncome(PlayerId player)
    {
        return rules.GetExpectedIncome(player);
    }

    public void MoveUnitView(int unitId) { GameObject view; if (!unitViews.TryGetValue(unitId, out view)) return; var u = gameState.units[unitId]; Vector3 world = mapGenerator.GridToWorldPosition(new Vector2Int(u.x, u.y)); world.z = -1f; view.transform.position = world; }
}
