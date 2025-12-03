using UnityEngine;
using System.IO;

public static class StatsManager
{
    private static readonly string StatsPath =
        Path.Combine(Application.persistentDataPath, "global_stats.json");

    public static GlobalStatistics stats = new GlobalStatistics();

    public static void Load()
    {
        if (!File.Exists(StatsPath))
        {
            stats = new GlobalStatistics();
            Save();
            return;
        }

        string json = File.ReadAllText(StatsPath);
        stats = JsonUtility.FromJson<GlobalStatistics>(json);
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(stats, true);
        File.WriteAllText(StatsPath, json);
    }
    public static void Reset()
    {
        stats = new GlobalStatistics();  
        Save();
    }

}