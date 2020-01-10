extends Spatial

var voxel_world;
var voxels;
var chunk_size_x;
var chunk_size_y;
var chunk_size_z;
var voxel_size = 1;
var render_mesh;
var render_mesh_vertices;
var render_mesh_normals;
var render_mesh_indices;
var render_mesh_uvs;
var collision_mesh;
var collision_mesh_vertices;
var collision_mesh_indices;
var mesh_instance;
var collision_shape;
var surface_tool;

func _ready():
	mesh_instance = get_node("MeshInstance");
	collision_shape = get_node("StaticBody/CollisionShape");
	surface_tool = SurfaceTool.new();

func setup(p_chunk_size_x, p_chunk_size_y, p_chunk_size_z, p_voxel_size):
	chunk_size_x = p_chunk_size_x;
	chunk_size_y = p_chunk_size_y;
	chunk_size_z = p_chunk_size_z;
	voxel_size = p_voxel_size;
	voxels = [];
	for x in range(0, chunk_size_x):
		var row = [];
		for y in range(0, chunk_size_y):
			var column = [];
			for z in range(0, chunk_size_z):
				column.append(null);
			row.append(column);
		voxels.append(row);
	make_starter_terrain();

func make_starter_terrain():
	for x in range(0, chunk_size_x):
		for y in range(0, chunk_size_y/2):
			for z in range(0, chunk_size_z):
				if (y + 1 == chunk_size_y/2):
					voxels[x][y][z] = voxel_world.get_voxel_int_from_string("Grass");
				elif (y >= chunk_size_y/4):
					voxels[x][y][z] = voxel_world.get_voxel_int_from_string("Dirt");
				elif (y == 0):
					voxels[x][y][z] = voxel_world.get_voxel_int_from_string("Bedrock");
				else:
					voxels[x][y][z] = voxel_world.get_voxel_int_from_string("Stone");
	update_mesh();

func update_mesh():
	render_mesh_vertices = [];
	render_mesh_normals = [];
	render_mesh_indices = [];
	render_mesh_uvs = [];
	collision_mesh_vertices = [];
	collision_mesh_indices = [];
	for x in range(0, chunk_size_x):
		for y in range(0, chunk_size_y):
			for z in range(0, chunk_size_z):
				make_voxel(x, y, z);
	
	# Make the render mesh
	# ********************
	surface_tool.clear();
	surface_tool.begin(Mesh.PRIMITIVE_TRIANGLES);
	
	for i in range(0, render_mesh_vertices.size()):
		surface_tool.add_normal(render_mesh_normals[i]);
		surface_tool.add_uv(render_mesh_uvs[i]);
		surface_tool.add_vertex(render_mesh_vertices[i]);
	
	for i in range(0, render_mesh_indices.size()):
		surface_tool.add_index(render_mesh_indices[i]);
	
	surface_tool.generate_tangents();
	
	render_mesh = surface_tool.commit();
	mesh_instance.mesh = render_mesh;
	# ********************
	# Make the collision mesh
	# ********************
	surface_tool.clear();
	surface_tool.begin(Mesh.PRIMITIVE_TRIANGLES);
	
	for i in range(0, collision_mesh_vertices.size()):
		surface_tool.add_vertex(collision_mesh_vertices[i]);
	
	for i in range(0, collision_mesh_indices.size()):
		surface_tool.add_index(collision_mesh_indices[i]);
	
	collision_mesh = surface_tool.commit();
	collision_shape.shape = collision_mesh.create_trimesh_shape();
	# ********************
 
func make_voxel(x, y, z):
	if (voxels[x][y][z] == null or voxels[x][y][z] == -1):
		return;
	
	if (_get_voxel_in_bounds(x, y+1, z)):
		if (_check_if_voxel_cause_render(x, y+1, z)):
			make_voxel_face(x, y, z, "TOP");
	else:
		make_voxel_face(x, y, z, "TOP");
	
	if (_get_voxel_in_bounds(x, y-1, z)):
		if (_check_if_voxel_cause_render(x, y-1, z)):
			make_voxel_face(x, y, z, "BOTTOM");
	else:
		make_voxel_face(x, y, z, "BOTTOM");
	
	if (_get_voxel_in_bounds(x+1, y, z)):
		if (_check_if_voxel_cause_render(x+1, y, z)):
			make_voxel_face(x, y, z, "EAST");
	else:
		make_voxel_face(x, y, z, "EAST");
	
	if (_get_voxel_in_bounds(x-1, y, z)):
		if (_check_if_voxel_cause_render(x-1, y, z)):
			make_voxel_face(x, y, z, "WEST");
	else:
		make_voxel_face(x, y, z, "WEST");
	
	if (_get_voxel_in_bounds(x, y, z+1)):
		if (_check_if_voxel_cause_render(x, y, z+1)):
			make_voxel_face(x, y, z, "NORTH");
	else:
		make_voxel_face(x, y, z, "NORTH");
	
	if (_get_voxel_in_bounds(x, y, z-1)):
		if (_check_if_voxel_cause_render(x, y, z-1)):
			make_voxel_face(x, y, z, "SOUTH");
	else:
		make_voxel_face(x, y, z, "SOUTH");
 
 
func _check_if_voxel_cause_render(x, y, z):
	if (voxels[x][y][z] == null or voxels[x][y][z] == -1):
		return true;
	
	else:
		var tmp_voxel_data = voxel_world.get_voxel_data_from_int(voxels[x][y][z]);
		if (tmp_voxel_data.transparent == true or tmp_voxel_data.solid == false):
			return true;
	
	return false;
 
 
func make_voxel_face(x, y, z, face):
	var voxel_data = voxel_world.get_voxel_data_from_int(voxels[x][y][z]);
	
	var uv_position = voxel_data.texture;
	
	x = x * voxel_size;
	y = y * voxel_size;
	z = z * voxel_size;
	
	if (voxel_data.has("texture_" + face) == true):
		uv_position = voxel_data["texture_" + face];
	
	if (face == "TOP"):
		_make_voxel_face_top(x, y, z, voxel_data);
	elif (face == "BOTTOM"):
		_make_voxel_face_bottom(x, y, z, voxel_data);
	elif (face == "EAST"):
		_make_voxel_face_east(x, y, z, voxel_data);
	elif (face == "WEST"):
		_make_voxel_face_west(x, y, z, voxel_data);
	elif (face == "NORTH"):
		_make_voxel_face_north(x, y, z, voxel_data);
	elif (face == "SOUTH"):
		_make_voxel_face_south(x, y, z, voxel_data);
	else:
		print ("ERROR: Unknown face: " + face);
		return;
	
	var v_texture_unit = voxel_world.voxel_texture_unit;
	render_mesh_uvs.append(Vector2( (v_texture_unit * uv_position.x), (v_texture_unit * uv_position.y) + v_texture_unit));
	render_mesh_uvs.append(Vector2( (v_texture_unit * uv_position.x) + v_texture_unit, (v_texture_unit * uv_position.y) + v_texture_unit));
	render_mesh_uvs.append(Vector2( (v_texture_unit * uv_position.x) + v_texture_unit, (v_texture_unit * uv_position.y)) );
	render_mesh_uvs.append(Vector2( (v_texture_unit * uv_position.x), (v_texture_unit * uv_position.y) ));
	
	render_mesh_indices.append(render_mesh_vertices.size() - 4);
	render_mesh_indices.append(render_mesh_vertices.size() - 3);
	render_mesh_indices.append(render_mesh_vertices.size() - 1);
	render_mesh_indices.append(render_mesh_vertices.size() - 3);
	render_mesh_indices.append(render_mesh_vertices.size() - 2);
	render_mesh_indices.append(render_mesh_vertices.size() - 1);
	
	if (voxel_data.solid == true):
		collision_mesh_indices.append(render_mesh_vertices.size() - 4);
		collision_mesh_indices.append(render_mesh_vertices.size() - 3);
		collision_mesh_indices.append(render_mesh_vertices.size() - 1);
		collision_mesh_indices.append(render_mesh_vertices.size() - 3);
		collision_mesh_indices.append(render_mesh_vertices.size() - 2);
		collision_mesh_indices.append(render_mesh_vertices.size() - 1);
 
 
func _make_voxel_face_top(x, y, z, voxel_data):
	render_mesh_vertices.append(Vector3(x, y + voxel_size, z));
	render_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z));
	render_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
	render_mesh_vertices.append(Vector3(x, y + voxel_size, z + voxel_size));
	
	render_mesh_normals.append(Vector3(0, 1, 0));
	render_mesh_normals.append(Vector3(0, 1, 0));
	render_mesh_normals.append(Vector3(0, 1, 0));
	render_mesh_normals.append(Vector3(0, 1, 0));
	
	if (voxel_data.solid == true):
		collision_mesh_vertices.append(Vector3(x, y + voxel_size, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z));
		collision_mesh_vertices.append(Vector3(x, y + voxel_size, z));
 
func _make_voxel_face_bottom(x, y, z, voxel_data):
	render_mesh_vertices.append(Vector3(x, y, z + voxel_size));
	render_mesh_vertices.append(Vector3(x + voxel_size, y, z + voxel_size));
	render_mesh_vertices.append(Vector3(x + voxel_size, y, z));
	render_mesh_vertices.append(Vector3(x, y, z));
	
	render_mesh_normals.append(Vector3(0, -1, 0));
	render_mesh_normals.append(Vector3(0, -1, 0));
	render_mesh_normals.append(Vector3(0, -1, 0));
	render_mesh_normals.append(Vector3(0, -1, 0));
	
	if (voxel_data.solid == true):
		collision_mesh_vertices.append(Vector3(x, y, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y, z));
		collision_mesh_vertices.append(Vector3(x, y, z));
 
func _make_voxel_face_north(x, y, z, voxel_data):
	render_mesh_vertices.append(Vector3(x + voxel_size, y, z + voxel_size));
	render_mesh_vertices.append(Vector3(x, y, z + voxel_size));
	render_mesh_vertices.append(Vector3(x, y + voxel_size, z + voxel_size));
	render_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
	
	render_mesh_normals.append(Vector3(0, 0, 1));
	render_mesh_normals.append(Vector3(0, 0, 1));
	render_mesh_normals.append(Vector3(0, 0, 1));
	render_mesh_normals.append(Vector3(0, 0, 1));
	
	if (voxel_data.solid == true):
		collision_mesh_vertices.append(Vector3(x, y, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x, y + voxel_size, z + voxel_size));
 
func _make_voxel_face_south(x, y, z, voxel_data):
	render_mesh_vertices.append(Vector3(x, y, z));
	render_mesh_vertices.append(Vector3(x + voxel_size, y, z));
	render_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z));
	render_mesh_vertices.append(Vector3(x, y + voxel_size, z));
	
	render_mesh_normals.append(Vector3(0, 0, -1));
	render_mesh_normals.append(Vector3(0, 0, -1));
	render_mesh_normals.append(Vector3(0, 0, -1));
	render_mesh_normals.append(Vector3(0, 0, -1));
	
	if (voxel_data.solid == true):
		collision_mesh_vertices.append(Vector3(x, y, z));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y, z));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z));
		collision_mesh_vertices.append(Vector3(x, y + voxel_size, z));
 
func _make_voxel_face_east(x, y, z, voxel_data):
	render_mesh_vertices.append(Vector3(x + voxel_size, y, z));
	render_mesh_vertices.append(Vector3(x + voxel_size, y, z + voxel_size));
	render_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
	render_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z));
	
	render_mesh_normals.append(Vector3(1, 0, 0));
	render_mesh_normals.append(Vector3(1, 0, 0));
	render_mesh_normals.append(Vector3(1, 0, 0));
	render_mesh_normals.append(Vector3(1, 0, 0));
	
	if (voxel_data.solid == true):
		collision_mesh_vertices.append(Vector3(x + voxel_size, y, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y, z));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z));
		collision_mesh_vertices.append(Vector3(x + voxel_size, y + voxel_size, z + voxel_size));
 
func _make_voxel_face_west(x, y, z, voxel_data):
	render_mesh_vertices.append(Vector3(x, y, z + voxel_size));
	render_mesh_vertices.append(Vector3(x, y, z));
	render_mesh_vertices.append(Vector3(x, y + voxel_size, z));
	render_mesh_vertices.append(Vector3(x, y + voxel_size, z + voxel_size));
	
	render_mesh_normals.append(Vector3(-1, 0, 0));
	render_mesh_normals.append(Vector3(-1, 0, 0));
	render_mesh_normals.append(Vector3(-1, 0, 0));
	render_mesh_normals.append(Vector3(-1, 0, 0));
	
	if (voxel_data.solid == true):
		collision_mesh_vertices.append(Vector3(x, y, z + voxel_size));
		collision_mesh_vertices.append(Vector3(x, y, z));
		collision_mesh_vertices.append(Vector3(x, y + voxel_size, z));
		collision_mesh_vertices.append(Vector3(x, y + voxel_size, z + voxel_size));
 
 
func get_voxel_at_position(position):
	if (position_within_chunk_bounds(position) == true):
		position = global_transform.xform_inv(position);
		
		position.x = floor(position.x / voxel_size);
		position.y = floor(position.y / voxel_size);
		position.z = floor(position.z / voxel_size);
		
		return voxels[position.x][position.y][position.z];
	
	return null;
 
func set_voxel_at_position(position, voxel):
	if (position_within_chunk_bounds(position) == true):
		
		position = global_transform.xform_inv(position);
		
		position.x = floor(position.x / voxel_size);
		position.y = floor(position.y / voxel_size);
		position.z = floor(position.z / voxel_size);
		
		voxels[position.x][position.y][position.z] = voxel;
		
		update_mesh();
		
		return true;
	
	return false;
 
 
func position_within_chunk_bounds(position):
	if (position.x < global_transform.origin.x + (chunk_size_x * voxel_size) and position.x > global_transform.origin.x):
		if (position.y < global_transform.origin.y + (chunk_size_y * voxel_size) and position.y > global_transform.origin.y):
			if (position.z < global_transform.origin.z + (chunk_size_z * voxel_size) and position.z > global_transform.origin.z):
				return true;
	
	return false;
 
 
func _get_voxel_in_bounds(x,y,z):
	if (x < 0 || x > chunk_size_x-1):
		return false;
	elif (y < 0 || y > chunk_size_y-1):
		return false;
	elif (z < 0 || z > chunk_size_z-1):
		return false;
	
	return true;