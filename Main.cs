using System;
using Godot;

public partial class Main : Node
{
    [Export] public PackedScene minesweeperScene;
    private Minesweeper minesweeper;

    public Control MainMenu { get; private set; }
    public Button StartGame { get; private set; }
    public OptionButton Difficulty { get; private set; }

    public override void _Ready()
    {
        MainMenu = FindChild("MainMenu") as Control;
        StartGame = FindChild("StartGame") as Button;
        Difficulty = FindChild("Difficulty") as OptionButton;
        StartGame.Pressed += StartNewGame;
    }


    private void StartNewGame()
    {
        MainMenu.Visible = false;
        minesweeper = minesweeperScene.Instantiate() as Minesweeper;
        int numBombs, rows, cols;
        if (Difficulty.Text == "Easy")
        {
            numBombs = 10; rows = 8; cols = 10;
        }
        else if (Difficulty.Text == "Medium")
        {
            numBombs = 40; rows = 14; cols = 18;
        }
        else
        {
            numBombs = 99; rows = 20; cols = 24;
        }
        minesweeper.Init(numBombs, rows, cols);
        AddChild(minesweeper);
        minesweeper.StartOver.Pressed += () =>
        {
            minesweeper.QueueFree();
            StartNewGame();
        };
    }
}
