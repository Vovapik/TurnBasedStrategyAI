using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitPlacer : MonoBehaviour
{
    [Header("References")]
    public Camera gameCamera;
    public Tilemap groundTilemap;
    public MapGenerator mapGenerator; 

    [Header("Unit Placing")]
    public GameObject selectedUnitPrefab;   
    public float unitHeightOffset = 0.1f;

    private readonly System.Collections.Generic.Dictionary<Vector2Int, GameObject> placedUnits
        = new System.Collections.Generic.Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceUnitAtMouse();
        }
    }

    public void SetUnitPrefab(GameObject prefab)
    {
        selectedUnitPrefab = prefab;
    }

    void TryPlaceUnitAtMouse()
    {
        if (selectedUnitPrefab == null)
        {
            return;
        }

        Vector3 mouseWorld = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3Int cell = groundTilemap.WorldToCell(mouseWorld);

        Vector2Int gridPos = new Vector2Int(cell.x + mapGenerator.mapSize / 2,
                                            cell.y + mapGenerator.mapSize / 2);

        if (!IsInsideMap(cell))
            return;

        if (placedUnits.ContainsKey(gridPos))
            return;

        Vector3 spawnPos = groundTilemap.GetCellCenterWorld(cell);
        spawnPos.z = -1f;
        spawnPos.y += unitHeightOffset;

        GameObject newUnit = Instantiate(selectedUnitPrefab, spawnPos, Quaternion.identity);
        placedUnits[gridPos] = newUnit;


        SpriteRenderer sr = newUnit.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            float tileSize = mapGenerator.tileScale;
            Vector2 spriteSize = sr.sprite.bounds.size;

            float scaleFactor = Mathf.Min(
                tileSize / spriteSize.x,
                tileSize / spriteSize.y
            );

            newUnit.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }

        selectedUnitPrefab = null;

    }

    bool IsInsideMap(Vector3Int cell)
    {
        int half = mapGenerator.mapSize / 2;
        return cell.x >= -half && cell.x < half &&
               cell.y >= -half && cell.y < half;
    }
}
