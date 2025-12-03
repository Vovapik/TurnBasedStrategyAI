using UnityEngine;

public class CastleUI : MonoBehaviour
{
    [Header("References")]
    public GameController gameController;

    [Header("UI")]
    public GameObject panel;
    public TMPro.TextMeshProUGUI hpText;

    private BuildingState currentCastle;

    public void Hide()
    {
        panel.SetActive(false);
        currentCastle = null;
    }

    public void ShowCastle(BuildingState castle)
    {
        currentCastle = castle;
        panel.SetActive(true);

        hpText.text = $"Castle HP: {castle.hp}";
    }

    private void Spawn(UnitType type)
    {
        if (currentCastle == null)
            return;

        var rules = gameController.rules;

        if (!rules.CanCreateUnit(currentCastle.owner, currentCastle.id, type))
        {
            return;
        }

        UnitState u = rules.CreateUnit(currentCastle.owner, currentCastle.id, type);
        gameController.CreateUnitView(u);

        hpText.text = $"Castle HP: {currentCastle.hp}";
    }

    public void SpawnWarrior()  => Spawn(UnitType.Warrior);
    public void SpawnArcher()   => Spawn(UnitType.Archer);
    public void SpawnChivalry() => Spawn(UnitType.Chivalry);
    public void SpawnEngineer() => Spawn(UnitType.Engineer);
    public void SpawnCatapult() => Spawn(UnitType.Catapult);
}