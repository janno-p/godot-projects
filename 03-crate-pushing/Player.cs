using Godot;
using System;

namespace CratePushing;

public partial class Player : Polygon2D
{
	private readonly HashSet<Vector2I> _wallCells = [];

	private Vector2I _playerCell = Vector2I.Zero;
	private Vector2I _crateCell = Vector2I.Zero;
	private Vector2I _goalCell = Vector2I.Zero;

	private Node2D Crate { get; set; } = null!;
	private Label WinMessage { get; set; } = null!;

	private bool LevelComplete => WinMessage.Visible;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_wallCells.Clear();

		GetTree().GetNodesInGroup("walls")
			.OfType<Node2D>()
			.Select(x => ScreenToGrid(x.Position))
			.ToList()
			.ForEach(x => _wallCells.Add(x));

		Crate = GetNode<Node2D>("../Crate");
		_crateCell = ScreenToGrid(Crate.Position);

		_playerCell = ScreenToGrid(Position);

		var goal = GetNode<Node2D>("../Goal");
		_goalCell = ScreenToGrid(goal.Position);

		WinMessage = GetNode<Label>("../WinMessage");
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

		if (LevelComplete)
		{
			return;
		}

		var movement = new Vector2I(0, 0);

		if (@event.IsActionPressed("move_up"))
		{
			movement.Y--;
		}

		if (@event.IsActionPressed("move_down"))
		{
			movement.Y++;
		}

		if (@event.IsActionPressed("move_left"))
		{
			movement.X--;
		}

		if (@event.IsActionPressed("move_right"))
		{
			movement.X++;
		}

		var candidateCell = _playerCell + movement;

		if (_wallCells.Contains(candidateCell))
		{
			return;
		}

		if (candidateCell != _crateCell)
		{
			MovePlayerToCell(candidateCell);
			return;
		}

		var crateCandidateCell = candidateCell + movement;

		if (_wallCells.Contains(crateCandidateCell))
		{
			return;
		}

		MovePlayerToCell(candidateCell);
		MoveCrateToCell(crateCandidateCell);

		if (_crateCell == _goalCell)
		{
			WinMessage.Visible = true;
		}
	}

	private void MovePlayerToCell(Vector2I cell)
	{
		_playerCell = cell;
		Position = GridToScreen(_playerCell);
	}

	private void MoveCrateToCell(Vector2I cell)
	{
		_crateCell = cell;
		Crate.Position = GridToScreen(_crateCell);
	}

	private static Vector2I ScreenToGrid(Vector2 screenPos)
	{
		return new Vector2I(Convert.ToInt32(screenPos.X) / 48, Convert.ToInt32(screenPos.Y) / 48);
	}

	private static Vector2 GridToScreen(Vector2I gridPos)
	{
		return new Vector2(gridPos.X * 48.0f, gridPos.Y * 48.0f);
	}
}
