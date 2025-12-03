using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public GameController gameController;
    public PlayerId playerToShow = PlayerId.Human;

    public TMP_Text goldText;

    private void Update()
    {
        int gold = gameController.GetPlayerGold(playerToShow);
        int income = gameController.GetExpectedIncome(playerToShow);

        goldText.text = $"Gold: {gold}   (+{income})";
    }

    public void Refresh()
    {
        if (gameController == null)
            gameController = FindObjectOfType<GameController>();

        if (gameController != null)
        {
            int gold = gameController.GetPlayerGold(PlayerId.Human);
            int income = gameController.GetExpectedIncome(PlayerId.Human);

            goldText.text = $"Gold: {gold}   (+{income})";
        }
    }
}