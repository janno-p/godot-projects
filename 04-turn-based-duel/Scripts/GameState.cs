namespace TurnBasedDuel.Scripts;

public record PlayerTurn;

public record EnemyTurn;

public record FinishedDuel(bool PlayerWin);

public readonly union GameState(PlayerTurn, EnemyTurn, FinishedDuel)
{
    public override string ToString()
    {
        return this switch
        {
            PlayerTurn => nameof(PlayerTurn),
            EnemyTurn => nameof(EnemyTurn),
            FinishedDuel => nameof(FinishedDuel),
        };
    }
}
