using UnityEngine;

public class CastleUI : MonoBehaviour
{
    [Header("References")]
    public GameController gameController;

    [Header("UI")]
    public GameObject panel;

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
    }

    public void SpawnWarrior()  => Spawn(UnitType.Warrior);
    public void SpawnArcher()   => Spawn(UnitType.Archer);
    public void SpawnChivalry() => Spawn(UnitType.Chivalry);
    public void SpawnEngineer() => Spawn(UnitType.Engineer);
    public void SpawnCatapult() => Spawn(UnitType.Catapult);
}