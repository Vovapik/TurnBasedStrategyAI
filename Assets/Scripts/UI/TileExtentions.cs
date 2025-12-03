public static class TileExtensions
{
    public static bool IsInside(this TileState[,] tiles, int x, int y)
    {
        int n = tiles.GetLength(0);
        return x >= 0 && x < n && y >= 0 && y < n;
    }
}