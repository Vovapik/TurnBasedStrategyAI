using UnityEngine;

public class UnitSelectButton : MonoBehaviour
{
    public UnitPlacer unitPlacer;
    public GameObject unitPrefab;

    public void OnSelectUnit()
    {
        unitPlacer.SetUnitPrefab(unitPrefab);
    }
}