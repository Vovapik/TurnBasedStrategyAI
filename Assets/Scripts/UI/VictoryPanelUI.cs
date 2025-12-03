using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryPanelUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text resultText;
    public TMP_Text statsText;

    public void Show(PlayerId winner)
    {
        panel.SetActive(true);

        if (winner == PlayerId.Human)
            resultText.text = "<color=green><b>YOU WON!</b></color>";
        else
            resultText.text = "<color=red><b>YOU LOST!</b></color>";

        var match = FindObjectOfType<GameController>().gameState.matchStatistics;
        SetMatchStats(match);
    }



    public void BackToMenu()
    {
        SceneManager.LoadScene("StartMenuScene");
    }
    
    public void SetMatchStats(MatchStatistics m)
    {
        statsText.text += "\n<color=#ffff00><b>This Match:</b></color>\n" +
                          $"Units Created: {m.unitsCreated}\n" +
                          $"Units Killed: {m.unitsKilled}\n" +
                          $"Gold Earned: {m.goldEarned}\n" +
                          $"Gold Spent: {m.goldSpent}\n" +
                          $"Turns Played: {m.turnsPlayed / 2}";
    }
}