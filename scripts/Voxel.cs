using Godot;

public class Voxel {

    public bool transparent { get; }
    public bool solid { get; }
    public Vector2 texture { get; }

    public Voxel(bool transparent, bool solid, Vector2 texture) {
        this.transparent = transparent;
        this.solid = solid;
        this.texture = texture;
    }
}
