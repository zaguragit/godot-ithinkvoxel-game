using Godot;

public class Camera : Godot.Camera {

    public override void _Input(InputEvent e) {
		if (e is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured) {
            var rotation = Rotation;
            rotation.y -= Player.SENSITIVITY * (e as InputEventMouseMotion).Relative.x;
            var movY = -Player.SENSITIVITY * (e as InputEventMouseMotion).Relative.y;
            if ((movY < 0 || RotationDegrees.x < 90) && (movY > 0 || RotationDegrees.x > -80)) {
                rotation.x += movY;
                if (rotation.x > Mathf.Pi / 2)
                    rotation.x = Mathf.Pi / 2;
                else if (rotation.x < -1.396263f)
                    rotation.x = -1.396263f;
            }

            //if (-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y >= 0 && RotationDegrees.x >= 90) return;
            //if (-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y <= 0 && RotationDegrees.x <= -80) return;

            Rotation = rotation;
        }
    }

    private const float MAX_OFFSET_X = 0.5f;
    private const float MAX_OFFSET_Y = 0.1f;
    private Vector2 offset = new Vector2(0, 0);
    private Vector2 direction = new Vector2(0, 0);
    private float stepX = 0.3f;
    private float stepY = 0.3f;

    public void bob(float delta) {
        if (offset.x < -MAX_OFFSET_X) direction.x = 1;
        else if (offset.x > MAX_OFFSET_X) direction.x = -1;
        offset.x += direction.x * stepX * delta;
        if (offset.y < -MAX_OFFSET_Y) direction.y = 1;
        else if (offset.y > MAX_OFFSET_Y) direction.y = -1;
        offset.y += direction.y * stepY * delta;
        Translate(new Vector3(direction.x * stepX * delta, direction.y * stepY * delta, 0f));
	}
}
