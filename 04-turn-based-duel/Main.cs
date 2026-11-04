using Godot;
using System;
using TurnBasedDuel.Scripts;

namespace TurnBasedDuel;

public partial class Main : Node2D
{
	private readonly Stack<HistoryEntry> _history = new();
	private readonly Stack<HistoryEntry> _revertedHistory = new();

	private GameState _gameState = new PlayerTurn();

	private Vector2I _playerCell = Vector2I.Zero;
	private Vector2I _enemyCell = Vector2I.Zero;

	private Actor Player { get; set; } = null!;
	private Actor Enemy { get; set; } = null!;
	private Label PlayerHP { get; set; } = null!;
	private Label EnemyHP { get; set; } = null!;
	private Label Status { get; set; } = null!;
	private Label UndoLabel { get; set; } = null!;
	private Label RedoLabel { get; set; } = null!;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_history.Clear();
		_revertedHistory.Clear();

		_playerCell = new Vector2I(1, 2);
		_enemyCell = new Vector2I(4, 2);

		Player = GetNode<Actor>("Player");
		Enemy = GetNode<Actor>("Enemy");
		PlayerHP = GetNode<Label>("PlayerHP");
		EnemyHP = GetNode<Label>("EnemyHP");
		Status = GetNode<Label>("Status");
		UndoLabel = GetNode<Label>("UndoLabel");
		RedoLabel = GetNode<Label>("RedoLabel");

		_gameState = new PlayerTurn();
		GD.Print($"Game ready, it's {_gameState}");

		UpdateScreen();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("restart"))
		{
			GetTree().ReloadCurrentScene();
			return;
		}

		if (@event.IsActionPressed("undo"))
		{
			LoadLatestHistoryEntry();
			UpdateScreen();
			return;
		}

		if (@event.IsActionPressed("redo"))
		{
			LoadLatestRevertedHistoryEntry();
			UpdateScreen();
			return;
		}

		if (_gameState is not PlayerTurn)
		{
			return;
		}

		var movement = Vector2I.Zero;

		if (@event.IsActionPressed("move_right"))
			movement = Vector2I.Right;
		else if (@event.IsActionPressed("move_left"))
			movement = Vector2I.Left;
		else if (@event.IsActionPressed("move_up"))
			movement = Vector2I.Up;
		else if (@event.IsActionPressed("move_down"))
			movement = Vector2I.Down;

		if (movement == Vector2I.Zero)
		{
			return;
		}

		PlayerAction(_playerCell + movement);
		EnemyAction();

		if (_gameState is FinishedDuel { PlayerWin: var isPlayerWin })
		{
			var winner = isPlayerWin ? "Player" : "Enemy";
			GD.Print($"Player: {Player.HealthPoints}HP; Enemy: {Enemy.HealthPoints}HP; {winner} wins");
		}

		UpdateScreen();
	}

	private void PlayerAction(Vector2I candidateCell)
	{
		if (candidateCell.X < 0 || candidateCell.X > 5 || candidateCell.Y < 0 || candidateCell.Y > 4)
			return;

		AddHistoryEntry();
		_revertedHistory.Clear();

		if (candidateCell == _enemyCell)
		{
			GD.Print("Player attacks");
			Enemy.DealDamage();

			if (Enemy.HealthPoints == 0)
			{
				_gameState = new FinishedDuel(PlayerWin: true);
				return;
			}
		}
		else
		{
			GD.Print("Player moves");
			_playerCell = candidateCell;
		}

		GD.Print($"Player: {Player.HealthPoints}HP; Enemy: {Enemy.HealthPoints}HP");

		ChangeTurn(new EnemyTurn());
	}

	private void AddHistoryEntry()
	{
		_history.Push(new HistoryEntry(_playerCell, _enemyCell, Player.HealthPoints, Enemy.HealthPoints, _gameState));
	}

	private void AddRevertedHistoryEntry()
	{
		_revertedHistory.Push(new HistoryEntry(_playerCell, _enemyCell, Player.HealthPoints, Enemy.HealthPoints, _gameState));
	}

	private void LoadLatestHistoryEntry()
	{
		if (!_history.TryPop(out var entry))
		{
			return;
		}

		AddRevertedHistoryEntry();

		RestoreStateFromEntry(entry);
	}

	private void LoadLatestRevertedHistoryEntry()
	{
		if (!_revertedHistory.TryPop(out var entry))
		{
			return;
		}

		AddHistoryEntry();

		RestoreStateFromEntry(entry);
	}

	private void RestoreStateFromEntry(HistoryEntry entry)
	{
		_playerCell = entry.PlayerCell;
		_enemyCell = entry.EnemyCell;

		Player.HealthPoints = entry.PlayerHealthPoints;
		Enemy.HealthPoints = entry.EnemyHealthPoints;

		_gameState = entry.GameState;
	}

	private void EnemyAction()
	{
		if (_gameState is not EnemyTurn)
		{
			return;
		}

		var distance = _enemyCell - _playerCell;
		if ((Math.Abs(distance.X) + Math.Abs(distance.Y)) == 1)
		{
			GD.Print("Enemy attacks");
			Player.DealDamage();

			if (Player.HealthPoints == 0)
			{
				_gameState = new FinishedDuel(PlayerWin: false);
				return;
			}
		}
		else
		{
			var candidateCell = distance.X != 0
				? _enemyCell - new Vector2I(distance.Sign().X, 0)
				: _enemyCell - new Vector2I(0, distance.Sign().Y);

			GD.Print("Enemy moves");
			_enemyCell = candidateCell;
		}

		GD.Print($"Player: {Player.HealthPoints}HP; Enemy: {Enemy.HealthPoints}HP");

		ChangeTurn(new PlayerTurn());
	}

	private void ChangeTurn(GameState newState)
	{
		var previousState = _gameState;
		_gameState = newState;
		GD.Print($"{previousState} completed: waiting for {_gameState}");
	}

	private void UpdateScreen()
	{
		UndoLabel.Visible = _history.Count > 0;
		RedoLabel.Visible = _revertedHistory.Count > 0;

		Player.Position = CellToScreen(_playerCell);
		Enemy.Position = CellToScreen(_enemyCell);

		PlayerHP.Text = $"Player HP: {Player.HealthPoints}";
		EnemyHP.Text = $"Enemy HP: {Enemy.HealthPoints}";

		Status.Text = _gameState switch
		{
			PlayerTurn => "Your turn",
			EnemyTurn => "Enemys turn",
			FinishedDuel { PlayerWin: true } => "You win",
			FinishedDuel { PlayerWin: false } => "You lose",
		};
	}

	private static Vector2 CellToScreen(Vector2I vector)
	{
		return new Vector2(vector.X * 48.0f, vector.Y * 48.0f);
	}

	private static Vector2I ScreenToCell(Vector2 vector)
	{
		return new Vector2I(Convert.ToInt32(vector.X) / 48, Convert.ToInt32(vector.Y) / 48);
	}
}
