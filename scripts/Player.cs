using Godot;

public class Player : KinematicBody {
    
	public const float SENSITIVITY_X = 0.01f;
	public const float SENSITIVITY_Y = 0.01f;
	const float MAX_WALK_SPEED = 7f;
	const int ACCELERATION = 1;
	const int JUMP_SPEED = 3;
	const float GRAVITY = 9.8f;
	
	Vector3 velocity = new Vector3(0, 0, 0);
	Camera camera;
	float forward_velocity = 0f;
	float Walk_Speed = 5f;
	
	
	public override void _Ready() {
		camera = GetNode("Camera") as Camera;
        Input.SetMouseMode(Input.MouseMode.Captured);
		forward_velocity = Walk_Speed;
		SetProcess(true);
    }
	
	bool lastEscapePressed = false;
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
		if (IsKeyPressed(KeyList.W) || IsKeyPressed(KeyList.A) || IsKeyPressed(KeyList.S) || IsKeyPressed(KeyList.D) || IsKeyPressed(KeyList.Up) || IsKeyPressed(KeyList.Down) || IsKeyPressed(KeyList.Left) || IsKeyPressed(KeyList.Right)) {
            if (IsOnFloor()) camera.bob();
            if (IsKeyPressed(KeyList.W) || IsKeyPressed(KeyList.Up)) {
                Walk_Speed += ACCELERATION * delta;
                if (Walk_Speed > MAX_WALK_SPEED) Walk_Speed = MAX_WALK_SPEED;
                velocity.x = -GlobalTransform.basis.z.x * Walk_Speed;
                velocity.z = -GlobalTransform.basis.z.z * Walk_Speed;
            } if (IsKeyPressed(KeyList.S) || IsKeyPressed(KeyList.Down)) {
                Walk_Speed += ACCELERATION * delta;
                if (Walk_Speed > MAX_WALK_SPEED) Walk_Speed = MAX_WALK_SPEED;
                velocity.x = GlobalTransform.basis.z.x * Walk_Speed;
                velocity.z = GlobalTransform.basis.z.z * Walk_Speed;
            } if (IsKeyPressed(KeyList.A) || IsKeyPressed(KeyList.Left)) {
                Walk_Speed += ACCELERATION * delta;
                if (Walk_Speed > MAX_WALK_SPEED) Walk_Speed = MAX_WALK_SPEED;
                velocity.x = -GlobalTransform.basis.x.x * Walk_Speed;
                velocity.z = -GlobalTransform.basis.x.z * Walk_Speed;
            } if (IsKeyPressed(KeyList.D) || IsKeyPressed(KeyList.Right)) {
                Walk_Speed += ACCELERATION * delta;
                if (Walk_Speed > MAX_WALK_SPEED) Walk_Speed = MAX_WALK_SPEED;
                velocity.x = GlobalTransform.basis.x.x * Walk_Speed;
                velocity.z = GlobalTransform.basis.x.z * Walk_Speed;
            }
        } else {
            velocity.x = 0;
            velocity.z = 0;
            Walk_Speed = 5;
        }
			
		if (IsOnFloor()) {
			if (IsKeyPressed(KeyList.Space)) velocity.y = JUMP_SPEED;
			else velocity.y = 0;
		} else velocity.y -= GRAVITY * delta;
		velocity = MoveAndSlide(velocity, new Vector3(0,1,0));
	}
	
	public override void _Input(InputEvent e) {
		if (e is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
			RotateY(-SENSITIVITY_X * (e as InputEventMouseMotion).Relative.x);
	}
}
