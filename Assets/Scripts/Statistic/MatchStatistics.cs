using System;

[Serializable]
public class MatchStatistics
{
    public int unitsCreated;
    public int unitsKilled;
    public int goldEarned;
    public int goldSpent;
    public int turnsPlayed;

    public void Reset()
    {
        unitsCreated = 0;
        unitsKilled  = 0;
        goldEarned   = 0;
        goldSpent    = 0;
        turnsPlayed  = 0;
    }
}