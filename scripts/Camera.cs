using Godot;
using System;

public class Camera : Godot.Camera {
	public override void _Input(InputEvent e) {
		if (e is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured) {
			if (-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y >= 0 && RotationDegrees.x >= 90) return;
			if (-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y <= 0  && RotationDegrees.x <= -80) return;
			RotateX(-Player.SENSITIVITY_Y * (e as InputEventMouseMotion).Relative.y);
		}
	}
}
