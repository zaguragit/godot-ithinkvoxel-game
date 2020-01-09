extends Camera

var SENSITIVITY_Y = 0
const MAX_Y = 90

func _ready():
	SENSITIVITY_Y = self.get_parent().Sensitivity_Y
	pass
	
func _input(event):
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		if -SENSITIVITY_Y * event.relative.y >= 0 and self.rotation_degrees.x >= MAX_Y:
			return
		if -SENSITIVITY_Y * event.relative.y <= 0  and self.rotation_degrees.x <= -MAX_Y:
			return
		rotate_x(-SENSITIVITY_Y * event.relative.y)