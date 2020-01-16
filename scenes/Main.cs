using Godot;
using System;

public class Main : Spatial {

    Sprite crosshair;

    public override void _Ready() {
        crosshair = GetNode("Crosshair") as Sprite;
        crosshair.Scale = new Vector2(2, 2);
    }

    public override void _Process(float delta) {
        crosshair.Position = new Vector2(OS.WindowSize.x / 2, OS.WindowSize.y / 2);
    }
}
