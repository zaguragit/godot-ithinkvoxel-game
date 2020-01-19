using Godot;
using System;
using System.Collections.Generic;

public class Chunk : Spatial {

    public const int CHUNK_SIZE = 16;
    const float voxel_size = 1;

    public World voxel_world;
    int[,,] voxels = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    ArrayMesh render_mesh;
    List<Vector3> render_mesh_vertices;
    List<Vector3> render_mesh_normals;
    List<int> render_mesh_indices;
    List<Vector2> render_mesh_uvs;
    ArrayMesh collision_mesh;
    List<Vector3> collision_mesh_vertices;
    List<int> collision_mesh_indices;
    MeshInstance mesh_instance;
    CollisionShape collision_shape;
    SurfaceTool surface_tool;
	OpenSimplexNoise noise;

    public override void _Ready() {
        mesh_instance = GetNode("MeshInstance") as MeshInstance;
        collision_shape = GetNode("StaticBody/CollisionShape") as CollisionShape;
        surface_tool = new SurfaceTool();
		noise = new OpenSimplexNoise(343256437L);
    }

    public void setup() {
        for (int x = 0; x < CHUNK_SIZE; x++)
            for (int y = 0; y < CHUNK_SIZE; y++)
                for (int z = 0; z < CHUNK_SIZE; z++)
                    voxels[x, y, z] = -1;
        make_starter_terrain();
    }

    public void make_starter_terrain() {
        for (int x = 0; x < CHUNK_SIZE; x++)
            for (int z = 0; z < CHUNK_SIZE; z++) {
				var height = 
                    noise.Evaluate((Translation.x + x) / 80, (Translation.z + z) / 80) * 36 +
                    noise.Evaluate((Translation.x + x) / 50, (Translation.z + z) / 50) * 20 +
                    noise.Evaluate((Translation.x + x) / 20, (Translation.z + z) / 20) * 10 +
                    noise.Evaluate((Translation.x + x) / 7, (Translation.z + z) / 7) * 5 +
                    7 - Translation.y;
                for (int y = 0; y < CHUNK_SIZE; y++)
                    if (y < height) {
                        if (y == (int) height)
                            voxels[x, y, z] = voxel_world.get_voxel_int_from_string("Grass");
                        else if (y == 0)
                            voxels[x, y, z] = voxel_world.get_voxel_int_from_string("MoonStone");
                        else voxels[x, y, z] = voxel_world.get_voxel_int_from_string("Stone");
                    } else voxels[x, y, z] = -1;
			}
        update_mesh();
    }

    public void update_mesh() {
        render_mesh_vertices = new List<Vector3>();
        render_mesh_normals = new List<Vector3>();
        render_mesh_indices = new List<int>();
        render_mesh_uvs = new List<Vector2>();
        collision_mesh_vertices = new List<Vector3>();
        collision_mesh_indices = new List<int>();
        for (int x = 0; x < CHUNK_SIZE; x++)
            for (int y = 0; y < CHUNK_SIZE; y++)
                for (int z = 0; z < CHUNK_SIZE; z++)
                    make_voxel(x, y, z);
        surface_tool.Clear();
        surface_tool.Begin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < render_mesh_vertices.Count; i++) {
            surface_tool.AddNormal(render_mesh_normals[i]);
            surface_tool.AddUv(render_mesh_uvs[i]);
            surface_tool.AddVertex(render_mesh_vertices[i]);
        }
        for (int i = 0; i < render_mesh_indices.Count; i++)
            surface_tool.AddIndex(render_mesh_indices[i]);

        surface_tool.GenerateTangents();

        render_mesh = surface_tool.Commit();
        mesh_instance.Mesh = render_mesh;

        surface_tool.Clear();
        surface_tool.Begin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < render_mesh_vertices.Count; i++)
            surface_tool.AddVertex(collision_mesh_vertices[i]);
        for (int i = 0; i < render_mesh_indices.Count; i++)
            surface_tool.AddIndex(collision_mesh_indices[i]);

        collision_mesh = surface_tool.Commit();
        collision_shape.Shape = collision_mesh.CreateTrimeshShape();
    }

    void make_voxel(int x, int y, int z) {
        if (voxels[x, y, z] == -1) return;
        if (_get_voxel_in_bounds(x, y + 1, z)) {
            if (_check_if_voxel_cause_render(x, y + 1, z))
                make_voxel_face(x, y, z, "TOP");
        } else make_voxel_face(x, y, z, "TOP");
        if (_get_voxel_in_bounds(x, y - 1, z)) {
            if (_check_if_voxel_cause_render(x, y - 1, z))
                make_voxel_face(x, y, z, "BOTTOM");
        } else make_voxel_face(x, y, z, "BOTTOM");
        if (_get_voxel_in_bounds(x + 1, y, z)) {
            if (_check_if_voxel_cause_render(x + 1, y, z))
                make_voxel_face(x, y, z, "EAST");
        } else make_voxel_face(x, y, z, "EAST");
        if (_get_voxel_in_bounds(x - 1, y, z)) {
            if (_check_if_voxel_cause_render(x - 1, y, z))
                make_voxel_face(x, y, z, "WEST");
        } else make_voxel_face(x, y, z, "WEST");
        if (_get_voxel_in_bounds(x, y, z + 1)) {
            if (_check_if_voxel_cause_render(x, y, z + 1))
                make_voxel_face(x, y, z, "NORTH");
        } else make_voxel_face(x, y, z, "NORTH");
        if (_get_voxel_in_bounds(x, y, z - 1)) {
            if (_check_if_voxel_cause_render(x, y, z - 1))
                make_voxel_face(x, y, z, "SOUTH");
        } else make_voxel_face(x, y, z, "SOUTH");
    }


    bool _check_if_voxel_cause_render(int x, int y, int z) {
        if (voxels[x, y, z] == -1)
            return true;
        var tmp_voxel_data = voxel_world.get_voxel_data_from_int(voxels[x, y, z]);
        if (tmp_voxel_data.transparent || !tmp_voxel_data.solid)
            return true;
        return false;
    }


    void make_voxel_face(int xx, int yy, int zz, string face) {
        var voxel_data = voxel_world.get_voxel_data_from_int(voxels[xx, yy, zz]);
        var uv_position = voxel_data.texture;
        float x = xx * voxel_size;
        float y = yy * voxel_size;
        float z = zz * voxel_size;
        switch (face) {
            case "TOP":
                _make_voxel_face_top(x, y, z, voxel_data);
                break;
            case "BOTTOM":
                _make_voxel_face_bottom(x, y, z, voxel_data);
                break;
            case "EAST":
                _make_voxel_face_east(x, y, z, voxel_data);
                break;
            case "WEST":
                _make_voxel_face_west(x, y, z, voxel_data);
                break;
            case "NORTH":
                _make_voxel_face_north(x, y, z, voxel_data);
                break;
            case "SOUTH":
                _make_voxel_face_south(x, y, z, voxel_data);
                break;
            default:
                GD.Print("ERROR: Unknown face: " + face);
                return;
        }

        var v_texture_unit = voxel_world.voxel_texture_unit;
        render_mesh_uvs.Add(new Vector2((v_texture_unit * uv_position.x), (v_texture_unit * uv_position.y) + v_texture_unit));
        render_mesh_uvs.Add(new Vector2((v_texture_unit * uv_position.x) + v_texture_unit, (v_texture_unit * uv_position.y) + v_texture_unit));
        render_mesh_uvs.Add(new Vector2((v_texture_unit * uv_position.x) + v_texture_unit, (v_texture_unit * uv_position.y)));
        render_mesh_uvs.Add(new Vector2((v_texture_unit * uv_position.x), (v_texture_unit * uv_position.y)));
        render_mesh_indices.Add(render_mesh_vertices.Count - 4);
        render_mesh_indices.Add(render_mesh_vertices.Count - 3);
        render_mesh_indices.Add(render_mesh_vertices.Count - 1);
        render_mesh_indices.Add(render_mesh_vertices.Count - 3);
        render_mesh_indices.Add(render_mesh_vertices.Count - 2);
        render_mesh_indices.Add(render_mesh_vertices.Count - 1);
        if (voxel_data.solid == true) {
            collision_mesh_indices.Add(render_mesh_vertices.Count - 4);
            collision_mesh_indices.Add(render_mesh_vertices.Count - 3);
            collision_mesh_indices.Add(render_mesh_vertices.Count - 1);
            collision_mesh_indices.Add(render_mesh_vertices.Count - 3);
            collision_mesh_indices.Add(render_mesh_vertices.Count - 2);
            collision_mesh_indices.Add(render_mesh_vertices.Count - 1);
        }
    }


    void _make_voxel_face_top(float x, float y, float z, Voxel voxel_data) {
        render_mesh_vertices.Add(new Vector3(x, y + voxel_size, z));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x, y + voxel_size, z + voxel_size));
        render_mesh_normals.Add(new Vector3(0, 1, 0));
        render_mesh_normals.Add(new Vector3(0, 1, 0));
        render_mesh_normals.Add(new Vector3(0, 1, 0));
        render_mesh_normals.Add(new Vector3(0, 1, 0));
        if (voxel_data.solid) {
            collision_mesh_vertices.Add(new Vector3(x, y + voxel_size, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z));
            collision_mesh_vertices.Add(new Vector3(x, y + voxel_size, z));
        }
    }
 
    void _make_voxel_face_bottom(float x, float y, float z, Voxel voxel_data) {
        render_mesh_vertices.Add(new Vector3(x, y, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y, z));
        render_mesh_vertices.Add(new Vector3(x, y, z));
        render_mesh_normals.Add(new Vector3(0, -1, 0));
        render_mesh_normals.Add(new Vector3(0, -1, 0));
        render_mesh_normals.Add(new Vector3(0, -1, 0));
        render_mesh_normals.Add(new Vector3(0, -1, 0));
        if (voxel_data.solid) {
            collision_mesh_vertices.Add(new Vector3(x, y, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y, z));
            collision_mesh_vertices.Add(new Vector3(x, y, z));
        }
    }
 
    void _make_voxel_face_north(float x, float y, float z, Voxel voxel_data) {
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x, y, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x, y + voxel_size, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
        render_mesh_normals.Add(new Vector3(0, 0, 1));
        render_mesh_normals.Add(new Vector3(0, 0, 1));
        render_mesh_normals.Add(new Vector3(0, 0, 1));
        render_mesh_normals.Add(new Vector3(0, 0, 1));
        if (voxel_data.solid == true) {
            collision_mesh_vertices.Add(new Vector3(x, y, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x, y + voxel_size, z + voxel_size));
        }
    }

    void _make_voxel_face_south(float x, float y, float z, Voxel voxel_data) {
        render_mesh_vertices.Add(new Vector3(x, y, z));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y, z));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z));
        render_mesh_vertices.Add(new Vector3(x, y + voxel_size, z));
        render_mesh_normals.Add(new Vector3(0, 0, -1));
        render_mesh_normals.Add(new Vector3(0, 0, -1));
        render_mesh_normals.Add(new Vector3(0, 0, -1));
        render_mesh_normals.Add(new Vector3(0, 0, -1));
        if (voxel_data.solid) {
        collision_mesh_vertices.Add(new Vector3(x, y, z));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y, z));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z));
            collision_mesh_vertices.Add(new Vector3(x, y + voxel_size, z));
        }
    }

    void _make_voxel_face_east(float x, float y, float z, Voxel voxel_data) {
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y, z));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z));
        render_mesh_normals.Add(new Vector3(1, 0, 0));
        render_mesh_normals.Add(new Vector3(1, 0, 0));
        render_mesh_normals.Add(new Vector3(1, 0, 0));
        render_mesh_normals.Add(new Vector3(1, 0, 0));
        if (voxel_data.solid) {
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y, z));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z));
            collision_mesh_vertices.Add(new Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
        }
    }
 
    void _make_voxel_face_west(float x, float y, float z, Voxel voxel_data) {
        render_mesh_vertices.Add(new Vector3(x, y, z + voxel_size));
        render_mesh_vertices.Add(new Vector3(x, y, z));
        render_mesh_vertices.Add(new Vector3(x, y + voxel_size, z));
        render_mesh_vertices.Add(new Vector3(x, y + voxel_size, z + voxel_size));
        render_mesh_normals.Add(new Vector3(-1, 0, 0));
        render_mesh_normals.Add(new Vector3(-1, 0, 0));
        render_mesh_normals.Add(new Vector3(-1, 0, 0));
        render_mesh_normals.Add(new Vector3(-1, 0, 0));
        if (voxel_data.solid) {
            collision_mesh_vertices.Add(new Vector3(x, y, z + voxel_size));
            collision_mesh_vertices.Add(new Vector3(x, y, z));
            collision_mesh_vertices.Add(new Vector3(x, y + voxel_size, z));
            collision_mesh_vertices.Add(new Vector3(x, y + voxel_size, z + voxel_size));
        }
    }
 
    public int get_voxel_at_position(Vector3 position) {
        if (position_within_chunk_bounds(position) == true) {
            position = GlobalTransform.XformInv(position);
            position.x = (float)Math.Floor(position.x / voxel_size);
            position.y = (float)Math.Floor(position.y / voxel_size);
            position.z = (float)Math.Floor(position.z / voxel_size);
            return voxels[(int)position.x, (int)position.y, (int)position.z];
        }
        return -1;
    }
 
    public bool set_voxel_at_position(Vector3 position, int voxelId) {
        if (position_within_chunk_bounds(position) == true) {
            position = GlobalTransform.XformInv(position);
            position.x = (float)Math.Floor(position.x / voxel_size);
            position.y = (float)Math.Floor(position.y / voxel_size);
            position.z = (float)Math.Floor(position.z / voxel_size);
            voxels[(int)position.x, (int)position.y, (int)position.z] = voxelId;
            update_mesh();
            return true;
        }
        return false;
    }
 
    bool position_within_chunk_bounds(Vector3 position) {
        if (position.x < GlobalTransform.origin.x + (CHUNK_SIZE * voxel_size) && position.x > GlobalTransform.origin.x)
            if (position.y < GlobalTransform.origin.y + (CHUNK_SIZE * voxel_size) && position.y > GlobalTransform.origin.y)
                if (position.z < GlobalTransform.origin.z + (CHUNK_SIZE * voxel_size) && position.z > GlobalTransform.origin.z)
                    return true;
        return false;
    }
 
 
    bool _get_voxel_in_bounds(int x, int y, int z) {
        return x >= 0 && x < CHUNK_SIZE && y >= 0 && y < CHUNK_SIZE && z >= 0 && z < CHUNK_SIZE;
    }
}
