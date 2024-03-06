using System;
using Godot;

public partial class Tile : TextureButton
{
    public event Action<bool> Flagged;


    [Export] public bool IsRevealed { get; set; }
    [Export] public bool IsBomb { get; set; }
    [Export] public bool IsFlagged { get; set; }
    [Export] public int Number { get; set; }

    public int Index { get; set; }

    public Label Label { get; private set; }

    public override void _Ready()
    {
        Label = FindChild("Label") as Label;
        Label.Visible = false;
        Label.Text = "0";
    }

    public override void _Process(double delta)
    {
        Modulate = IsHovered() ?
           Colors.White :
           Colors.Silver;
    }

    public override void _GuiInput(InputEvent e)
    {

        if (e is InputEventMouseButton)
        {
            var mouse = e as InputEventMouseButton;
            if (mouse.Pressed && mouse.ButtonIndex == MouseButton.Right)
            {
                IsFlagged = !IsFlagged;
                Flagged.Invoke(IsFlagged);
            }
        }
    }

}
