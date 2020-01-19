using Godot;
using System;

public class Gui : Control {

    CenterContainer center;

    public override void _Ready() {
        center = GetNode("Center") as CenterContainer;
    }

    public override void _Process(float delta) {
        center.SetSize(new Vector2(OS.WindowSize.x, OS.WindowSize.y));
    }
}
