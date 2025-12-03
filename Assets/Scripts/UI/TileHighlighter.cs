using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlighter : MonoBehaviour
{
    public Tilemap tilemap;

    public Color moveColor = new Color(0f, 1f, 0f, 0.35f);
    public Color attackColor = new Color(1f, 0f, 0f, 0.35f);

    private readonly List<Vector3Int> highlightedCells = new();

    public void ShowHighlights(
        List<Vector2Int> moveTiles,
        List<Vector2Int> attackTiles,
        MapGenerator map)
    {
        Clear();

        foreach (var g in moveTiles)
        {
            Vector3Int cell = map.GridToCell(g);
            Highlight(cell, moveColor);
        }

        foreach (var g in attackTiles)
        {
            Vector3Int cell = map.GridToCell(g);
            Highlight(cell, attackColor);
        }
    }

    private void Highlight(Vector3Int cell, Color c)
    {
        TileBase t = tilemap.GetTile(cell);
        if (t is HighlightableTile ht)
        {
            ht.SetHighlightColor(c);
            tilemap.RefreshTile(cell);
            highlightedCells.Add(cell);
        }
    }

    public void Clear()
    {
        foreach (var cell in highlightedCells)
        {
            TileBase t = tilemap.GetTile(cell);
            if (t is HighlightableTile ht)
            {
                ht.ResetColor();
                tilemap.RefreshTile(cell);
            }
        }

        highlightedCells.Clear();
    }
}