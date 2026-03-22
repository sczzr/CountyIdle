extends TextureButton

## 底栏令箭：负责悬停拔出、选中停驻的动画表现，不承载业务逻辑。
@export var base_y: float = 30.0
@export var hover_y: float = 12.0
@export var active_y: float = 0.0

var _tween: Tween


func _ready() -> void:
	toggle_mode = true
	position.y = base_y
	mouse_entered.connect(_on_mouse_entered)
	mouse_exited.connect(_on_mouse_exited)
	toggled.connect(_on_toggled)


func _on_mouse_entered() -> void:
	if not button_pressed:
		_animate_to(hover_y)


func _on_mouse_exited() -> void:
	if not button_pressed:
		_animate_to(base_y)


func _on_toggled(is_pressed: bool) -> void:
	_animate_to(active_y if is_pressed else base_y)


func _animate_to(target_y: float) -> void:
	if _tween != null and _tween.is_running():
		_tween.kill()

	_tween = create_tween().set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	_tween.tween_property(self, "position:y", target_y, 0.24)
