using Godot;

namespace TurnBasedDuel.Scripts;

public record HistoryEntry(
    Vector2I PlayerCell,
    Vector2I EnemyCell,
    uint PlayerHealthPoints,
    uint EnemyHealthPoints,
    GameState GameState
);
