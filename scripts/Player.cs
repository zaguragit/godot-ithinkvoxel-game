using Godot;

public class Player : KinematicBody {

	public const float SENSITIVITY = 0.01f;
    const float MAX_WALK_SPEED = 12f;
    const float INIT_WALK_SPEED = 6f;
    const int ACCELERATION = 1;
	const int JUMP_SPEED = 4;
	
	Camera camera;
	float Walk_Speed = INIT_WALK_SPEED;
	
	public override void _Ready() {
		camera = GetNode("Camera") as Camera;
        //Input.SetMouseMode(Input.MouseMode.Captured);
		SetProcess(true);
    }
	
	bool lastEscapePressed;
	public override void _Process(float delta) {
		if (lastEscapePressed == true && !IsKeyPressed(KeyList.Escape)) {
			if (Input.GetMouseMode() == Input.MouseMode.Captured)
				Input.SetMouseMode(Input.MouseMode.Visible);
			else if (Input.GetMouseMode() == Input.MouseMode.Visible)
				Input.SetMouseMode(Input.MouseMode.Captured);
		} lastEscapePressed = IsKeyPressed(KeyList.Escape);
	}
	
	public bool IsKeyPressed(KeyList key) { return Input.IsKeyPressed((int) key); }

    public override void _PhysicsProcess(float delta) {
        Vector3 velocity = new Vector3(0, 0, 0);
        if (IsKeyPressed(KeyList.W) || IsKeyPressed(KeyList.A) || IsKeyPressed(KeyList.S) || IsKeyPressed(KeyList.D) || IsKeyPressed(KeyList.Up) || IsKeyPressed(KeyList.Down) || IsKeyPressed(KeyList.Left) || IsKeyPressed(KeyList.Right)) {
            camera.bob(delta);
            Walk_Speed += ACCELERATION;
            if (Walk_Speed > MAX_WALK_SPEED) Walk_Speed = MAX_WALK_SPEED;
            float movX = Mathf.Sin(camera.Rotation.y) * Walk_Speed * delta;
            float movZ = Mathf.Cos(camera.Rotation.y) * Walk_Speed * delta;
            if (IsKeyPressed(KeyList.W) || IsKeyPressed(KeyList.Up)) {
                velocity.x -= movX;
                velocity.z -= movZ;
            } if (IsKeyPressed(KeyList.S) || IsKeyPressed(KeyList.Down)) {
                velocity.x += movX;
                velocity.z += movZ;
            } if (IsKeyPressed(KeyList.A) || IsKeyPressed(KeyList.Left)) {
                velocity.x -= movZ;
                velocity.z += movX;
            } if (IsKeyPressed(KeyList.D) || IsKeyPressed(KeyList.Right)) {
                velocity.x += movZ;
                velocity.z -= movX;
            }
        } else {
            velocity.x = 0;
            velocity.z = 0;
            Walk_Speed = INIT_WALK_SPEED;
        }
        if (IsKeyPressed(KeyList.Space)) velocity.y = JUMP_SPEED * delta;
        if (IsKeyPressed(KeyList.Shift)) velocity.y = -JUMP_SPEED * delta;
		Translate(velocity);
    }
}
