using Godot;
using System.Runtime.InteropServices;

namespace BasicStructure;

public record class Success(string Message);
public record class Failure(string ErrorCode);

public readonly union Result(Success, Failure);

public partial class Main : Control
{
	private const string LabelPath = "CenterContainer/VBoxContainer/Label";

	private int _counter = 0;

	private Label Label => GetNode<Label>(LabelPath);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateLabelText();

		GD.Print($".NET Version: {System.Environment.Version}");
		GD.Print($"=> {RuntimeInformation.FrameworkDescription}");
		GD.Print(Describe(new Success("YEI")));
		GD.Print(Describe(new Failure("NOOO")));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnButtonPressed()
	{
		_counter += 1;
		UpdateLabelText();
	}

	private void UpdateLabelText()
	{
		Label.Text = $"Counter value: {_counter}";
	}

	private static string Describe(Result result)
	{
		return result switch
		{
			Success s => $"Success: {s.Message}",
			Failure f => $"Failure: {f.ErrorCode}",
		};
	}
}
