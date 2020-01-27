using Godot;

public class Main : Spatial {

    public override void _Ready() {
        Client.init();
    }
}
