using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Highlightable Tile", menuName = "2D/Tiles/Highlightable Tile")]
[System.Serializable]
public class HighlightableTile : Tile
{
    [Header("Highlight Settings")]
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1.2f, 1.2f, 1f, 1f);
    
    [System.NonSerialized] private bool isHighlighted = false;
    
    public void SetHighlighted(bool highlight)
    {
        isHighlighted = highlight;
        UpdateColor();
    }
    
    private void UpdateColor()
    {

        if (isHighlighted)
            this.color = highlightColor;
        else
            this.color = normalColor;
    }
    
    public void SetHighlightColor(Color c)
    {
        this.color = c;
    }

    public void ResetColor()
    {
        this.color = normalColor;
    }

}