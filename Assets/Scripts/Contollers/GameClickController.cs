using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameClickController : MonoBehaviour
{
    [Header("References")]
    public Camera gameCamera;
    public GameController gameController;
    public MapGenerator mapGenerator;
    public TileHighlighter tileHighlighter;
    
    private int selectedUnitId = -1;

    private List<Vector2Int> movableTiles = new();
    private List<Vector2Int> attackTiles  = new();

    private float lastClickTime = 0f;
    private const float doubleClickDelay = 0.25f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();
    }
    
    void HandleLeftClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (gameController == null || gameController.gameState == null)
            return;

        Vector3 world = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;

        Vector2Int g = mapGenerator.WorldToGridPosition(world);
        int x = g.x;
        int y = g.y;

        var state = gameController.gameState;

        if (!state.tiles.IsInside(x, y))
        {
            ClearSelection();
            gameController.castleUI.Hide();
            gameController.engineerUI.Hide();
            return;
        }

        TileState tile = state.tiles[x, y];

        if (tile.unitId != -1)
        {
            UnitState u = state.units[tile.unitId];
            if (!u.isDead && u.owner == state.currentPlayer)
            {
                SelectUnit(tile.unitId);
                return;
            }
        }
        if (selectedUnitId != -1)
        {
            if (TryAttack(selectedUnitId, x, y))
                return;

            if (TryMove(x, y))
                return;

            ClearSelection();
            gameController.castleUI.Hide();
            gameController.engineerUI.Hide();
            return;
        }
        
        if (tile.buildingId != -1)
        {
            BuildingState b = state.buildings[tile.buildingId];
            bool isDouble = (Time.time - lastClickTime < doubleClickDelay);
            lastClickTime = Time.time;

            if (b.owner == PlayerId.AI && b.type == BuildingType.Castle)
            {
                gameController.castleUI.Hide();
                gameController.engineerUI.Hide();

                if (isDouble)
                {
                    if (tile.unitId != -1)
                    {
                        UnitState u = state.units[tile.unitId];
                        if (!u.isDead && u.owner == PlayerId.AI)
                        {
                            SelectUnit(tile.unitId);
                            return;
                        }
                    }

                    ClearSelection();
                }
                else
                {
                    ClearSelection();
                }

                return;
            }

            if (b.owner == PlayerId.Human && b.type == BuildingType.Castle)
            {
                if (state.currentPlayer != PlayerId.Human)
                {
                    gameController.castleUI.Hide();
                    gameController.engineerUI.Hide();
                    return;
                }

                if (isDouble)
                {
                    if (tile.unitId != -1)
                    {
                        UnitState u = state.units[tile.unitId];
                        if (!u.isDead && u.owner == PlayerId.Human)
                        {
                            SelectUnit(tile.unitId);
                            return;
                        }
                    }

                    ClearSelection();
                    gameController.castleUI.Hide();
                    gameController.engineerUI.Hide();
                    return;
                }
                else
                {
                    gameController.engineerUI.Hide();
                    gameController.castleUI.ShowCastle(b);
                    ClearSelection();
                    return;
                }
            }
        }

        
        ClearSelection();
        gameController.castleUI.Hide();
        gameController.engineerUI.Hide();
    }

    void SelectUnit(int unitId)
    {
        selectedUnitId = unitId;

        var state = gameController.gameState;
        UnitState u = state.units[unitId];

        if (u.owner == state.currentPlayer && u.type == UnitType.Engineer)
        {
            gameController.engineerUI.ShowEngineer(u);
            gameController.castleUI.Hide();
        }
        else
        {
            gameController.castleUI.Hide();
            gameController.engineerUI.Hide();
        }

        movableTiles = GetMovableTiles(unitId);
        attackTiles  = GetAttackTiles(unitId);

        tileHighlighter.ShowHighlights(movableTiles, attackTiles, mapGenerator);
    }
    
    bool TryMove(int x, int y)
    {
        if (!movableTiles.Contains(new Vector2Int(x, y)))
            return false;

        if (!gameController.rules.MoveUnit(selectedUnitId, x, y))
            return false;

        gameController.MoveUnitView(selectedUnitId);

        if (gameController.gameState.units[selectedUnitId].actionsLeft > 0)
        {
            SelectUnit(selectedUnitId);
        }
        else
        {
            ClearSelection();
            gameController.castleUI.Hide();
            gameController.engineerUI.Hide();
        }

        return true;
    }

    bool TryAttack(int attackerId, int x, int y)
    {
        if (!attackTiles.Contains(new Vector2Int(x, y)))
            return false;

        if (!gameController.rules.Attack(attackerId, x, y))
            return false;

        gameController.RefreshAllViews();

        if (gameController.gameState.units[selectedUnitId].actionsLeft > 0)
        {
            SelectUnit(selectedUnitId);
        }
        else
        {
            ClearSelection();
            gameController.castleUI.Hide();
            gameController.engineerUI.Hide();
        }

        return true;
    }

    List<Vector2Int> GetMovableTiles(int unitId)
    {
        var state = gameController.gameState;
        UnitState u = state.units[unitId];

        int range = u.moveRange;
        List<Vector2Int> list = new();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                int tx = u.x + dx;
                int ty = u.y + dy;

                if (!state.tiles.IsInside(tx, ty)) continue;

                if (gameController.rules.CanMoveUnit(unitId, tx, ty))
                    list.Add(new Vector2Int(tx, ty));
            }
        }

        return list;
    }

    List<Vector2Int> GetAttackTiles(int unitId)
    {
        var state = gameController.gameState;
        UnitState u = state.units[unitId];

        List<Vector2Int> list = new();

        int maxR = 4;

        for (int dx = -maxR; dx <= maxR; dx++)
        {
            for (int dy = -maxR; dy <= maxR; dy++)
            {
                int tx = u.x + dx;
                int ty = u.y + dy;

                if (!state.tiles.IsInside(tx, ty)) continue;

                if (gameController.rules.CanAttack(unitId, tx, ty))
                    list.Add(new Vector2Int(tx, ty));
            }
        }

        return list;
    }

    void ClearSelection()
    {
        selectedUnitId = -1;
        movableTiles.Clear();
        attackTiles.Clear();

        tileHighlighter.Clear();
    }

    public void ClearSelectionOnTurnEnd()
    {
        ClearSelection();
    }
}
