using Godot;
using System;
using LightsOut.Scripts;
using System.Linq;

namespace LightsOut;

public partial class Main : Control
{
	private const int ColCount = 3;
	private const int RowCount = 3;

	private readonly LightState[] _board = new LightState[ColCount * RowCount];

	private GridContainer Board { get; set; } = null!;
	private Button[] Buttons { get; set; } = null!;
	private Label Label { get; set; } = null!;
	private Button RestartButton { get; set; } = null!;

	private static readonly Color onStyleColor = Color.Color8(0xff, 0xc3, 0x00);
	private static readonly Color offStyleColor = Color.Color8(0xbf, 0xc3, 0xc7);
	private static readonly Color textColor = Color.Color8(0x29, 0x29, 0x29);
	private static readonly Color altTextColor = Color.Color8(0xf2, 0xf2, 0xf2);

	private readonly StyleBoxFlat onStyle = new() { BgColor = onStyleColor };
	private readonly StyleBoxFlat onHoverStyle = new() { BgColor = onStyleColor.Lightened(0.2f) };
	private readonly StyleBoxFlat onPressedStyle = new() { BgColor = onStyleColor.Darkened(0.2f) };

	private readonly StyleBoxFlat offStyle = new() { BgColor = offStyleColor };
	private readonly StyleBoxFlat offHoverStyle = new() { BgColor = offStyleColor.Lightened(0.2f) };
	private readonly StyleBoxFlat offPressedStyle = new() { BgColor = offStyleColor.Darkened(0.2f) };

	public override void _Ready()
	{
		Board = GetNode<GridContainer>("CenterContainer/VBoxContainer/Board");
		Buttons = [.. Board.GetChildren().OfType<Button>()];
		Label = GetNode<Label>("CenterContainer/VBoxContainer/Label");
		RestartButton = GetNode<Button>("CenterContainer/VBoxContainer/RestartButton");

		InitializeButtons();

		RestartButton.Pressed += Reset;

		Reset();
	}

	public override void _Process(double delta)
	{
	}

	private void InitializeButtons()
	{
		for (var i = 0; i < Buttons.Length; i++)
		{
			var button = Buttons[i];
			var index = i;

			button.Pressed += () =>
			{
				ToggleButtons(index);
				UpdateButtonTexts();
				UpdateStatusLabel();
			};

			button.AddThemeStyleboxOverride("disabled", offStyle);

			button.AddThemeColorOverride("font_color", textColor);
			button.AddThemeColorOverride("font_disabled_color", textColor);
		}
	}

	private void Reset()
	{
		var initialState = GetInitialState();
		Array.Copy(initialState, _board, initialState.Length);

		UpdateButtonTexts();
		UpdateStatusLabel();

		Array.ForEach(Buttons, x => x.Disabled = false);
	}

	private void ToggleButtons(int index)
	{
		ToggleButton(index);

		var col = index % ColCount;
		var row = index / ColCount;

		if (row > 0)
		{
			ToggleButton(index - ColCount);
		}

		if (row < RowCount - 1)
		{
			ToggleButton(index + ColCount);
		}

		if (col > 0)
		{
			ToggleButton(index - 1);
		}

		if (col < ColCount - 1)
		{
			ToggleButton(index + 1);
		}

		if (IsSolved())
		{
			Array.ForEach(Buttons, x => x.Disabled = true);
		}
	}

	private void ToggleButton(int index)
	{
		_board[index] = _board[index].Toggle();
	}

	private void UpdateButtonTexts()
	{
		for (var index = 0; index < _board.Length; index++)
		{
			var button = Buttons[index];

			var (text, normalStyle, hoverStyle, pressedStyle, hoverTextColor, pressedTextColor) =
				_board[index] switch
				{
					On => ("ON", onStyle, onHoverStyle, onPressedStyle, textColor, altTextColor),
					Off => ("OFF", offStyle, offHoverStyle, offPressedStyle, textColor, altTextColor),
				};

			button.Text = text;

			button.AddThemeStyleboxOverride("normal", normalStyle);
			button.AddThemeStyleboxOverride("hover", hoverStyle);
			button.AddThemeStyleboxOverride("pressed", pressedStyle);

			button.AddThemeColorOverride("font_hover_color", hoverTextColor);
			button.AddThemeColorOverride("font_pressed_color", pressedTextColor);
		}
	}

	private void UpdateStatusLabel()
	{
		Label.Text = IsSolved() ? "Solved!" : "Turn all lights off";
	}

	private bool IsSolved()
	{
		return Array.TrueForAll(_board, x => x is Off);
	}

	private static LightState[] GetInitialState()
	{
		return
		[
			On.Value, Off.Value, Off.Value,
			Off.Value, On.Value, Off.Value,
			Off.Value, Off.Value, On.Value,
		];
	}
}
