extends Control

# 阵眼组件：负责法阵旋转、境界签呼吸与轻微脉冲反馈。
const RING_ROTATION_SPEED := 0.18
const DASHED_ROTATION_SPEED := -0.34

@onready var _ring_thin: TextureRect = $SoulRingThin
@onready var _ring_dashed: TextureRect = $SoulRingDashed
@onready var _realm_badge: PanelContainer = $RealmBadge
@onready var _realm_label: Label = $RealmBadge/RealmLabel
@onready var _core_letter_label: Label = $CoreLetterLabel
@onready var _avatar_texture: TextureRect = $AvatarFrame/AvatarTexture

var _pulse_time := 0.0


func _process(delta: float) -> void:
	# 阵法持续旋转，让阵眼保持“活着”的感觉。
	_pulse_time += delta
	_ring_thin.rotation += delta * RING_ROTATION_SPEED
	_ring_dashed.rotation += delta * DASHED_ROTATION_SPEED
	var alpha := 0.82 + (sin(_pulse_time * 2.1) * 0.14)
	_realm_badge.modulate = Color(1, 1, 1, clamp(alpha, 0.55, 1.0))


func set_realm_text(text_value: String) -> void:
	# 由外层面板同步境界文案，保持组件数据入口单一。
	_realm_label.text = text_value


func set_core_glyph(text_value: String) -> void:
	# 阵眼主字通常取弟子姓名首字，便于快速识别。
	_core_letter_label.text = text_value


func set_avatar_texture(texture_value: Texture2D) -> void:
	# 预留头像贴图入口，当前未接正式头像时允许为空。
	_avatar_texture.texture = texture_value


func pulse_core() -> void:
	# 切换弟子时给阵眼一个轻微放缩反馈。
	scale = Vector2.ONE * 1.03
	var tween := create_tween()
	tween.tween_property(self, "scale", Vector2.ONE, 0.18)
