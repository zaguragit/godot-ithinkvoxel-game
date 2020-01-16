extends Spatial
 
var voxel_dictionary = {
	"Stone":
		{"transparent":false, "solid":true,
		"texture":Vector2(0, 0)},
	"Bedrock":
		{"transparent":false, "solid":true,
		"texture":Vector2(2, 0)},
	"Cobble":
		{"transparent":false, "solid":true,
		"texture":Vector2(1, 0)},
	"Dirt":
		{"transparent":false, "solid":true,
		"texture":Vector2(0, 1)},
	"Grass":
		{"transparent":false, "solid":true,
		"texture":Vector2(0, 1), "texture_TOP":Vector2(2, 1),
		"texture_NORTH":Vector2(1, 1), "texture_SOUTH":Vector2(1, 1), "texture_EAST":Vector2(1, 1), "texture_WEST":Vector2(1, 1)},
}

var voxel_list = [];
export (int) var voxel_texture_size = 96;
export (int) var voxel_texture_tile_size = 32;
var voxel_texture_unit;
var chunk_scene = preload("res://scenes/Chunk.tscn");
var chunk_holder_node;
var VOXEL_UNIT_SIZE = 1;

func _ready():
	chunk_holder_node = get_node("Chunks");
	voxel_texture_unit = 1.0 / (voxel_texture_size / voxel_texture_tile_size);
	for voxel_name in voxel_dictionary.keys():
		voxel_list.append(voxel_name);
	make_voxel_world(Vector3(4, 1, 4), Vector3(16, 16, 16));

func make_voxel_world(world_size, chunk_size):
	for child in chunk_holder_node.get_children():
		child.queue_free();
	for x in range(0, world_size.x):
		for y in range(0, world_size.y):
			for z in range(0, world_size.z):
				var new_chunk = chunk_scene.instance();
				chunk_holder_node.add_child(new_chunk);
				new_chunk.global_transform.origin = Vector3(
                        x * (chunk_size.x * VOXEL_UNIT_SIZE),
                        y * (chunk_size.y * VOXEL_UNIT_SIZE),
                        z * (chunk_size.z * VOXEL_UNIT_SIZE));
				new_chunk.voxel_world = self;
				new_chunk.setup(chunk_size.x, chunk_size.y, chunk_size.z, VOXEL_UNIT_SIZE);
	print ("Done making voxel world!");

func get_voxel_data_from_string(voxel_name):
	if (voxel_dictionary.has(voxel_name) == true):
		return voxel_dictionary[voxel_name];
	return null;
 
func get_voxel_data_from_int(voxel_integer):
	return voxel_dictionary[voxel_list[voxel_integer]];

func get_voxel_int_from_string(voxel_name):
	return voxel_list.find(voxel_name);

func set_world_voxel(position, voxel):
	var result = false;
	for chunk in chunk_holder_node.get_children():
		result = chunk.set_voxel_at_position(position, voxel);
		if (result == true):
			break;