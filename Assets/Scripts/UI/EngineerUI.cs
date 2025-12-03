using UnityEngine;

public class EngineerUI : MonoBehaviour
{
    [Header("References")]
    public GameController gameController;

    [Header("UI")]
    public GameObject panel;

    private UnitState currentEngineer;

    public void Hide()
    {
        panel.SetActive(false);
        currentEngineer = null;
    }

    public void ShowEngineer(UnitState engineer)
    {
        currentEngineer = engineer;
        panel.SetActive(true);
    }

    public void BuildFortpost()
    {
        if (currentEngineer == null)
            return;

        int id = currentEngineer.id;

        if (!gameController.rules.PlaceFortpost(id))
        {
            return;
        }

        var b = gameController.gameState.buildings[^1];
        gameController.CreateBuildingView(b);

        Hide();
    }
}