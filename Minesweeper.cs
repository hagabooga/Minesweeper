using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using static Godot.GD;


public partial class Minesweeper : CanvasLayer
{
    [Export] public int numBombs, rows, cols;
    [Export]
    public Texture2D
        bombSprite,
        flagSprite,
        tileSprite,
        revealedSprite;
    [Export] public PackedScene tileScene;
    [Export] public Array<Tile> tiles = new(), cleanTiles = new(), bombs = new();
    [Export] public Array<Color> colors;


    public Control Gameover { get; private set; }
    public Label GameoverLabel { get; private set; }
    public Button StartOver { get; private set; }


    public void Init(int numBombs, int rows, int cols)
    {
        this.numBombs = numBombs;
        this.rows = rows;
        this.cols = cols;
        var totalCols = (cols * 30);
        var totalRows = (rows * 30);
        if (totalCols < 400) return;
        var gcf = GetGCF(totalCols, totalRows);
        var ratio = new Vector2(totalCols / gcf, totalRows / gcf) - Vector2.One;
        Print(ratio);
        float width = ((int)((400 / ratio.X) * 0.9)) * ratio.X;
        float height = ((int)((300 / ratio.Y) * 0.9)) * ratio.Y;
        Print($"width: {width}");
        Print($"height: {height}");
        Scale = new(width / totalCols, height / totalRows);
        Offset = new(
            (Math.Abs(400 - totalCols) / 2) * Scale.X + (400 - width) / 2,
            (Math.Abs(300 - totalRows) / 2) * Scale.Y + (300 - height) / 2);
    }


    public override void _Ready()
    {
        Gameover = FindChild("Gameover") as Control;
        GameoverLabel = FindChild("GameoverLabel") as Label;
        StartOver = FindChild("StartOver") as Button;

        var numTiles = rows * cols;
        var grid = FindChild("GridContainer") as GridContainer;
        grid.Columns = cols;
        for (int i = 0; i < numTiles; i++)
        {
            var tile = tileScene.Instantiate() as Tile;
            grid.AddChild(tile);
            tiles.Add(tile);
            cleanTiles.Add(tile);
            tile.Index = i;
            tile.Name = $"Tile{i}";
        }

        System.Random random = new();
        for (int i = 0; i < numBombs; i++)
        {
            var index = random.Next(0, tiles.Count);
            var bomb = tiles[index];
            bomb.TextureNormal = bombSprite;
            bomb.IsBomb = true;
            bomb.Index = cleanTiles.IndexOf(bomb);
            bombs.Add(bomb);
            tiles.Remove(bomb);
        }

        foreach (var bomb in bombs)
        {
            var index = bomb.Index;
            var row = index / cols;
            var col = index % cols;
            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = col - 1; j <= col + 1; j++)
                {
                    if (i == row && j == col)
                    {
                        continue;
                    }
                    if (i >= 0 && i < rows && j >= 0 && j < cols)
                    {
                        var tile = cleanTiles[i * cols + j];
                        tile.Label.Text = $"{++tile.Number}";
                        // tile.Label.Visible = true;
                    }
                }
            }
        }

        foreach (var tile in cleanTiles)
        {
            tile.Pressed += () => TilePressed(tile);
            tile.Flagged += flagged =>
            {
                if (tile.IsRevealed) return;
                tile.TextureNormal = flagged ? flagSprite : tileSprite;
                if (bombs.All(x => x.IsFlagged))
                {
                    Gameover.Visible = true;
                    GameoverLabel.Text = "You win!";
                }
            };
        }

        foreach (var bomb in bombs)
        {
            bomb.TextureNormal = tileSprite;
        }
    }

    private void TilePressed(Tile tile)
    {
        if (tile.IsRevealed) return;
        if (tile.IsFlagged) return;
        if (tile.IsBomb)
        {
            Gameover.Visible = true;
            foreach (var bomb in bombs)
            {
                bomb.TextureNormal = bombSprite;
            }
            return;
        }

        HashSet<int> visited = new();
        Queue<Tile> queue = new();
        queue.Enqueue(tile);
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            visited.Add(currentTile.Index);
            currentTile.IsRevealed = true;
            currentTile.TextureNormal = revealedSprite;
            if (currentTile.Number != 0)
            {
                currentTile.Label.Visible = true;
                currentTile.Label.Modulate = colors[currentTile.Number - 1];
                continue;
            }
            var index = currentTile.Index;
            var row = index / cols;
            var col = index % cols;
            foreach (var (newRow, newCol) in new[]
            {
                (row + 1, col),
                (row - 1, col),
                (row, col + 1),
                (row, col - 1),
            })
            {
                if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols)
                {
                    var newIndex = newRow * cols + newCol;
                    var newTile = cleanTiles[newIndex];
                    if (newTile.IsFlagged) continue;
                    if (visited.Contains(newIndex)) continue;
                    visited.Add(newIndex);
                    queue.Enqueue(newTile);
                }
            }
        }

    }

    static int GetGCF(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}
