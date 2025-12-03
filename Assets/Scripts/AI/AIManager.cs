
public class AIManager
{
    private readonly GameState _state;
    private readonly GameRules _rules;

    private readonly AIBlackboard _bb = new AIBlackboard();
    private readonly AIPerception _perception = new AIPerception();
    private readonly AIGoalSelector _goalSelector = new AIGoalSelector();
    private readonly AIEconomy _economy = new AIEconomy();
    private readonly AIUnitController _unitController = new AIUnitController();

    public AIManager(GameState state, GameRules rules)
    {
        _state = state;
        _rules = rules;

        _bb.State = state;
        _bb.Rules = rules;
    }


    public void TakeTurn()
    {
        if (_state.gameOver)
            return;

        _perception.BuildBlackboard(_bb);

        _bb.GlobalGoal = _goalSelector.DecideGoal(_bb);

        _economy.RunEconomy(_bb);

        _unitController.ControlUnits(_bb);

        _rules.EndTurn();
    }
}