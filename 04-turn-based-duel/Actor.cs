using Godot;
using System;

namespace TurnBasedDuel;

public partial class Actor : Node2D
{
	public uint HealthPoints { get; set; }

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
