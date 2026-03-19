extends PanelContainer

# 火候刻度组件：负责静态图标与菱形宝石皮肤的初始化。
@export var track_icon: Texture2D
@export var track_icon_fallback := "技"
@export var track_title := "技能修炼"
@export var tag_text := "未起"

@onready var _track_icon_texture: TextureRect = $TrackMargin/TrackColumn/TrackHeaderRow/TrackIconTexture
@onready var _track_icon_label: Label = $TrackMargin/TrackColumn/TrackHeaderRow/TrackIconLabel
@onready var _track_name_label: Label = $TrackMargin/TrackColumn/TrackHeaderRow/TrackNameLabel
@onready var _track_tag_label: Label = $TrackMargin/TrackColumn/TrackHeaderRow/TrackTag/TrackTagLabel


func _ready() -> void:
	# 标题/图标由组件统一初始化，具体进度仍由 C# 结算逻辑刷新。
	_track_name_label.text = track_title
	_track_tag_label.text = tag_text
	if track_icon != null:
		_track_icon_texture.texture = track_icon
		_track_icon_texture.visible = true
		_track_icon_label.visible = false
	else:
		_track_icon_texture.visible = false
		_track_icon_label.visible = true
		_track_icon_label.text = track_icon_fallback
