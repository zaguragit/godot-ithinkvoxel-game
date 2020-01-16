using Godot;

public class Camera : Godot.Camera {
	
	private const float MAX_OFFSET = 0.02f;
	
	private float offset = 0;
	private float step = 0.005f;
	private int direction = 1;
	
	public override void _Input(InputEvent e) {
		if (e is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured) {
			if (-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y >= 0 && RotationDegrees.x >= 90) return;
			if (-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y <= 0  && RotationDegrees.x <= -80) return;
			RotateX(-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y);
		}
	}
	
	public void bob() {
		if (offset < -MAX_OFFSET) direction = 1;
		else if (offset > MAX_OFFSET) direction = -1;
		offset += direction * step;
		Translate(new Vector3(0f, direction * step, 0f));
	}
}
