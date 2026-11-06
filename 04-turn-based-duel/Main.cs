using Godot;
using TurnBasedDuel.Scripts;

namespace TurnBasedDuel;

public partial class Main : Node2D
{
	private const double MoveAnimationSpeed = 0.2;
	private const double AttackAnimationSpeed = 0.1;

	private readonly SemaphoreSlim _animation = new(1, 1);

	private readonly Stack<HistoryEntry> _history = new();
	private readonly Stack<HistoryEntry> _revertedHistory = new();

	private GameState _gameState = new PlayerTurn();

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

		Player = GetNode<Actor>("Player");
		Enemy = GetNode<Actor>("Enemy");
		PlayerHP = GetNode<Label>("PlayerHP");
		EnemyHP = GetNode<Label>("EnemyHP");
		Status = GetNode<Label>("Status");
		UndoLabel = GetNode<Label>("UndoLabel");
		RedoLabel = GetNode<Label>("RedoLabel");

		Player.GridPosition = new Vector2I(1, 2);
		Enemy.GridPosition = new Vector2I(4, 2);

		Player.HealthChanged += UpdateLabels;
		Enemy.HealthChanged += UpdateLabels;

		_gameState = new PlayerTurn();
		GD.Print($"Game ready, it's {_gameState}");

		UpdateLabels();
		UpdateActors();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override async void _UnhandledInput(InputEvent @event)
	{
		if (!await _animation.WaitAsync(0))
		{
			return;
		}

		try
		{
			if (@event.IsActionPressed("restart"))
			{
				GetTree().ReloadCurrentScene();
				return;
			}

			if (@event.IsActionPressed("undo"))
			{
				LoadLatestHistoryEntry();
				UpdateLabels();
				UpdateActors();
				return;
			}

			if (@event.IsActionPressed("redo"))
			{
				LoadLatestRevertedHistoryEntry();
				UpdateLabels();
				UpdateActors();
				return;
			}

			if (_gameState is not PlayerTurn)
			{
				return;
			}

			var direction = Vector2I.Zero;

			if (@event.IsActionPressed("move_right"))
				direction = Vector2I.Right;
			else if (@event.IsActionPressed("move_left"))
				direction = Vector2I.Left;
			else if (@event.IsActionPressed("move_up"))
				direction = Vector2I.Up;
			else if (@event.IsActionPressed("move_down"))
				direction = Vector2I.Down;

			if (direction == Vector2I.Zero)
			{
				return;
			}

			await PlayerAction(direction);
			await EnemyAction();
		}
		catch (Exception exception)
		{
			GD.PushError("Error occurred while updating the game state", exception);
		}
		finally
		{
			_animation.Release();
		}
	}

	private async Task PlayerAction(Vector2I direction)
	{
		var candidateCell = Player.GridPosition + direction;

		if (candidateCell.X < 0 || candidateCell.X > 5 || candidateCell.Y < 0 || candidateCell.Y > 4)
		{
			return;
		}

		AddHistoryEntry();
		_revertedHistory.Clear();

		if (candidateCell == Enemy.GridPosition)
		{
			GD.Print("Player attacks");

			Enemy.DealDamage();

			if (Enemy.HealthPoints == 0)
			{
				ChangeTurn(new FinishedDuel(PlayerWin: true));
			}

			await AnimateAttack(Player, Enemy);
		}
		else
		{
			GD.Print("Player moves");
			Player.GridPosition = candidateCell;

			await AnimateActor(Player);
		}

		if (_gameState is not FinishedDuel)
		{
			ChangeTurn(new EnemyTurn());
		}
	}

	private void PrintGameState()
	{
		var winnerText = "";

		if (_gameState is FinishedDuel { PlayerWin: var isPlayerWin })
		{
			var winner = isPlayerWin ? "Player" : "Enemy";
			winnerText = $"; {winner} wins";
		}

		GD.Print($"Player: {Player.HealthPoints}HP; Enemy: {Enemy.HealthPoints}HP{winnerText}");
	}

	private void AddHistoryEntry()
	{
		_history.Push(new HistoryEntry(Player.GridPosition, Enemy.GridPosition, Player.HealthPoints, Enemy.HealthPoints, _gameState));
	}

	private void AddRevertedHistoryEntry()
	{
		_revertedHistory.Push(new HistoryEntry(Player.GridPosition, Enemy.GridPosition, Player.HealthPoints, Enemy.HealthPoints, _gameState));
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
		Player.GridPosition = entry.PlayerCell;
		Player.HealthPoints = entry.PlayerHealthPoints;

		Enemy.GridPosition = entry.EnemyCell;
		Enemy.HealthPoints = entry.EnemyHealthPoints;

		_gameState = entry.GameState;
	}

	private async Task EnemyAction()
	{
		if (_gameState is not EnemyTurn)
		{
			return;
		}

		var distance = Enemy.GridPosition - Player.GridPosition;
		if ((Math.Abs(distance.X) + Math.Abs(distance.Y)) == 1)
		{
			GD.Print("Enemy attacks");

			Player.DealDamage();

			if (Player.HealthPoints == 0)
			{
				ChangeTurn(new FinishedDuel(PlayerWin: false));
			}

			await AnimateAttack(Enemy, Player);
		}
		else
		{
			var candidateCell = distance.X != 0
				? Enemy.GridPosition - new Vector2I(distance.Sign().X, 0)
				: Enemy.GridPosition - new Vector2I(0, distance.Sign().Y);

			GD.Print("Enemy moves");
			Enemy.GridPosition = candidateCell;

			await AnimateActor(Enemy);
		}

		if (_gameState is not FinishedDuel)
		{
			ChangeTurn(new PlayerTurn());
		}
	}

	private void ChangeTurn(GameState newState)
	{
		var previousState = _gameState;
		_gameState = newState;

		GD.Print($"{previousState} completed: waiting for {_gameState}");

		PrintGameState();
		UpdateLabels();
	}

	private void UpdateLabels()
	{
		UndoLabel.Visible = _history.Count > 0;
		RedoLabel.Visible = _revertedHistory.Count > 0;

		PlayerHP.Text = $"Player HP: {Player.HealthPoints}";
		EnemyHP.Text = $"Enemy HP: {Enemy.HealthPoints}";

		Status.Text = _gameState switch
		{
			PlayerTurn => "Your turn",
			EnemyTurn => "Enemy turn",
			FinishedDuel { PlayerWin: true } => "You win",
			FinishedDuel { PlayerWin: false } => "You lose",
		};
	}

	private void UpdateActors()
	{
		Player.Position = CellToScreen(Player.GridPosition);
		Enemy.Position = CellToScreen(Enemy.GridPosition);
	}

	private async Task AnimateActor(Actor actor)
	{
		using var tween = CreateTween();

		tween.TweenProperty(actor, "position", CellToScreen(actor.GridPosition), MoveAnimationSpeed);

		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task AnimateAttack(Actor attacker, Actor target)
	{
		var direction = target.GridPosition - attacker.GridPosition;

		var attackerPolygon = attacker.GetNode<Polygon2D>("Polygon2D");
		var position = attackerPolygon.Position;

		var targetPolygon = target.GetNode<Polygon2D>("Polygon2D");
		var modulate = targetPolygon.SelfModulate;

		using var tween = CreateTween();

		tween.TweenProperty(attackerPolygon, "position", position + (18 * direction), AttackAnimationSpeed);
		tween.Parallel().TweenProperty(targetPolygon, "self_modulate", modulate.Darkened(0.5f), AttackAnimationSpeed);

		tween.TweenProperty(attackerPolygon, "position", position, AttackAnimationSpeed);
		tween.Parallel().TweenProperty(targetPolygon, "self_modulate", modulate, AttackAnimationSpeed);

		await ToSignal(tween, Tween.SignalName.Finished);
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
