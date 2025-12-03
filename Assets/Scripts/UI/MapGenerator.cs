using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("References")]
    public GameController gameController;
    public Tilemap groundTilemap;
    public Sprite groundSprite;
    public Camera gameCamera; 
    
    [Header("Settings")]
    public int mapSize = 8;
    
    [Header("Tile Settings")]
    public float tileScale = 1f; 
    
    private void Start()
    {
        GenerateTilemap();
        SetupCamera();
    }
    
    [ContextMenu("Generate Tilemap")]
    public void GenerateTilemap()
    {
        mapSize = PlayerPrefs.GetInt("MapSize", 8);
    
        if (groundTilemap == null)
        {
            return;
        }
    
        if (groundSprite == null)
        {
            return;
        }
    
        groundTilemap.ClearAllTiles();
    
        int offsetX = -mapSize / 2;
        int offsetY = -mapSize / 2;
    
        TileState[,] tiles = gameController.gameState.tiles;

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                Vector3Int cellPos = new Vector3Int(offsetX + x, offsetY + y, 0);

                HighlightableTile tile = ScriptableObject.CreateInstance<HighlightableTile>();
                
                tile.sprite = groundSprite;

                tile.transform = Matrix4x4.Scale(new Vector3(tileScale, tileScale, 1f));

                groundTilemap.SetTile(cellPos, tile);
            }
        }

    
        
        float half = mapSize / 2f;

        CameraController controller = gameCamera.GetComponent<CameraController>();
        if (controller != null)
        {
            controller.SetBounds(
                -half,   
                half,    
                -half,   
                half     
            );
        }

    }
    
    void SetupCamera()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
            {
                Debug.LogError("No camera found! Please assign a camera.");
                return;
            }
        }
        
        gameCamera.transform.rotation = Quaternion.Euler(0, 0, 45);
        
        CenterCamera();
    }
    
    void CenterCamera()
    {
        if (gameCamera == null) return;
        
        float diagonalSize = mapSize * 1.414f;
        float cameraZ = -diagonalSize * 0.7f;
        
        gameCamera.transform.position = new Vector3(0, 0, cameraZ);
        
        if (gameCamera.orthographic)
        {
            gameCamera.orthographicSize = diagonalSize * 0.6f;
        }
    }
    
    [ContextMenu("Clear Tilemap")]
    public void ClearTilemap()
    {
        if (groundTilemap != null)
        {
            groundTilemap.ClearAllTiles();
        }
    }
    
    [ContextMenu("Regenerate Tilemap")]
    public void RegenerateTilemap()
    {
        GenerateTilemap();
        SetupCamera();
    }
    
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPosition);

        int logicX = cell.x + mapSize / 2;
        int logicY = cell.y + mapSize / 2;

        return new Vector2Int(logicX, logicY);
    }
    
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        int offsetX = -mapSize / 2;
        int offsetY = -mapSize / 2;

        Vector3Int cell = new Vector3Int(
            offsetX + gridPos.x,
            offsetY + gridPos.y,
            0
        );

        return groundTilemap.GetCellCenterWorld(cell);
    }


    public float GetTileWorldSize()
    {
        return tileScale;
    }
    
    public Vector3Int GridToCell(Vector2Int g)
    {
        int offsetX = -mapSize / 2;
        int offsetY = -mapSize / 2;

        return new Vector3Int(offsetX + g.x, offsetY + g.y, 0);
    }

    
}