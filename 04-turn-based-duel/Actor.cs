using Godot;

namespace TurnBasedDuel;

public partial class Actor : Node2D
{
	[Signal]
	public delegate void HealthChangedEventHandler();

	public uint HealthPoints
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				EmitSignal(SignalName.HealthChanged);
			}
		}
	}

	public Vector2I GridPosition { get; set; } = Vector2I.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HealthPoints = 3;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void DealDamage()
	{
		HealthPoints--;
	}
}
