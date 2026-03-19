extends Button

# 玉简卡组件：负责维护基础/激活态皮肤与按下时的微动效。
@export var icon_texture: Texture2D
@export var icon_fallback_text := "技"
@export var title_text := "技能修炼"
@export var subtitle_text := "演武参玄"
@export_multiline var description_text := "“以演武与拆招为主，稳步抬升弟子临阵手感与执行。”"
@export var time_text := "1 时辰"
@export var primary_title := "主收益"
@export var primary_value := "少量修为 +"
@export var secondary_title := "附带偏向"
@export var secondary_value := "悟性偏盛"

@onready var _icon_texture_rect: TextureRect = $CardMargin/CardColumn/CardHeaderRow/IconBadge/IconTexture
@onready var _icon_label: Label = $CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel
@onready var _title_label: Label = $CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/TitleLabel
@onready var _subtitle_label: Label = $CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SubtitleLabel
@onready var _desc_label: Label = $CardMargin/CardColumn/DescLabel
@onready var _time_text_label: Label = $CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeTextLabel
@onready var _primary_title_label: Label = $CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/TitleLabel
@onready var _primary_value_label: Label = $CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/ValueLabel
@onready var _secondary_title_label: Label = $CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/TitleLabel
@onready var _secondary_value_label: Label = $CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/ValueLabel

var _last_pressed_state := false


func _ready() -> void:
	# 组件自己的脚本只负责局部表现，不接管外层权威业务逻辑。
	toggle_mode = true
	flat = true
	_apply_static_texts()
	_apply_icon()
	_refresh_theme_variation()


func _process(_delta: float) -> void:
	if _last_pressed_state != button_pressed:
		_last_pressed_state = button_pressed
		_refresh_theme_variation()
		if button_pressed:
			_play_burst_feedback()


func set_active_state(is_active: bool) -> void:
	# 允许外层显式同步激活态，避免只依赖输入事件。
	button_pressed = is_active
	_refresh_theme_variation()


func _apply_static_texts() -> void:
	_title_label.text = title_text
	_subtitle_label.text = subtitle_text
	_desc_label.text = description_text
	_time_text_label.text = time_text
	_primary_title_label.text = primary_title
	_primary_value_label.text = primary_value
	_secondary_title_label.text = secondary_title
	_secondary_value_label.text = secondary_value


func _apply_icon() -> void:
	if icon_texture != null:
		_icon_texture_rect.texture = icon_texture
		_icon_texture_rect.visible = true
		_icon_label.visible = false
	else:
		_icon_texture_rect.visible = false
		_icon_label.visible = true
		_icon_label.text = icon_fallback_text


func _refresh_theme_variation() -> void:
	theme_type_variation = &"ButtonJadeactive" if button_pressed else &"ButtonJadecard"


func _play_burst_feedback() -> void:
	# 选中时只做轻微弹性，不抢夺主界面注意力。
	scale = Vector2.ONE * 0.985
	var tween := create_tween()
	tween.tween_property(self, "scale", Vector2.ONE, 0.16)
