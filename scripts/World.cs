using Godot;
using System;
using System.Collections.Generic;

public class World: Spatial {
    
	Dictionary<string, Voxel> voxel_dictionary = new Dictionary<string, Voxel> {
        { "Grass", new Voxel(false, true, new Vector2(0, 0)) },
        { "Stone", new Voxel(false, true, new Vector2(1, 0)) },
        { "MoonStone", new Voxel(false, true, new Vector2(2, 0)) },
        { "MoonStoneBricks", new Voxel(false, true, new Vector2(3, 0)) },
        { "LightBricks", new Voxel(false, true, new Vector2(4, 0)) },
        { "Wood", new Voxel(false, true, new Vector2(5, 0)) }
    };

    List<string> voxel_list = new List<string>();
    public int voxel_texture_size = 96;
    public int voxel_texture_tile_size = 16;
    public float voxel_texture_unit;
    PackedScene chunk_scene = ResourceLoader.Load("res://scenes/Chunk.tscn") as PackedScene;
    Node chunk_holder_node;
	
	public override void _Ready() {
        chunk_holder_node = GetNode("Chunks");
    	voxel_texture_unit = 1.0f / (voxel_texture_size / voxel_texture_tile_size);
        foreach (string voxelName in voxel_dictionary.Keys)
            voxel_list.Add(voxelName);
        make_voxel_world(new Vector3(12, 3, 12));
    }


    public void make_voxel_world(Vector3 world_size) {
        foreach (Node child in chunk_holder_node.GetChildren())
            child.QueueFree();
        for (int x = 0; x < world_size.x; x++)
            for (int y = 0; y < world_size.y; y++)
                for (int z = 0; z < world_size.z; z++) {
                    var new_chunk = (Chunk) chunk_scene.Instance();
                    chunk_holder_node.AddChild(new_chunk);
                    var gt = new_chunk.GlobalTransform;
                    gt.origin = new Vector3(
                        x * (Chunk.CHUNK_SIZE),
                        y * (Chunk.CHUNK_SIZE),
                        z * (Chunk.CHUNK_SIZE)
                    );
                    new_chunk.SetGlobalTransform(gt);
                    new_chunk.voxel_world = this;
                    new_chunk.setup();
                }
    }

    public Voxel get_voxel_data_from_string(string voxel_name) {
        if (voxel_dictionary.ContainsKey(voxel_name) == true)
            return voxel_dictionary[voxel_name];
        return null;
    }

    public Voxel get_voxel_data_from_int(int voxel_integer) {
        return voxel_dictionary[voxel_list[voxel_integer]];
    }

    public int get_voxel_int_from_string(string voxelName) {
        for (int i = 0; i < voxel_list.Count; i++) {
            if (voxel_list[i].Equals(voxelName)) return i;
        }
        return -1;
    }

    void set_world_voxel(Vector3 position, int voxelId) {
        var result = false;
        foreach (Chunk chunk in chunk_holder_node.GetChildren()) {
            result = chunk.set_voxel_at_position(position, voxelId);
            if (result == true) break;
        }
    }
}