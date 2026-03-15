extends Node

const INK_TITLE := Color(0.176471, 0.145098, 0.12549, 1.0)
const WORLD_SUBTITLE_DEFAULT := Color(0.38, 0.33, 0.27, 0.95)
const WORLD_BADGE_DEFAULT := Color(0.52, 0.45, 0.31, 0.95)
const WORLD_STATUS_DEFAULT := Color(0.34, 0.29, 0.24, 0.92)

var _title_label: Label
var _subtitle_label: Label
var _badge_label: Label
var _status_value_label: Label


func _ready() -> void:
	var root: Node = get_parent()
	_title_label = root.get_node("RootMargin/MainLayout/BodyRow/LeftPanel/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorHeader/TileTitle")
	_subtitle_label = root.get_node("RootMargin/MainLayout/BodyRow/LeftPanel/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorHeader/TileSubtitle")
	_badge_label = root.get_node("RootMargin/MainLayout/BodyRow/LeftPanel/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/InspectorHeader/TileBadgeBox/TileBadgeLabel")
	_status_value_label = root.get_node("RootMargin/MainLayout/BodyRow/LeftPanel/PanelContent/JobsVBox/IndustryEfficiency/InspectorVBox/AttrGrid/StatusBox/AttrVBox/AttrValue")


func apply_world_inspector_tone(primary_type: String, has_selection: bool) -> void:
	if not has_selection:
		_title_label.add_theme_color_override("font_color", INK_TITLE)
		_subtitle_label.add_theme_color_override("font_color", WORLD_SUBTITLE_DEFAULT)
		_badge_label.add_theme_color_override("font_color", WORLD_BADGE_DEFAULT)
		_status_value_label.add_theme_color_override("font_color", WORLD_STATUS_DEFAULT)
		return

	var accent := _resolve_world_accent(primary_type)
	_title_label.add_theme_color_override("font_color", accent)
	_subtitle_label.add_theme_color_override("font_color", accent.lightened(0.12))
	_badge_label.add_theme_color_override("font_color", accent)
	_status_value_label.add_theme_color_override("font_color", accent.darkened(0.08))


func apply_local_inspector_tone(badge_text: String, accent_color: Color, status_color: Color) -> void:
	var muted_accent := INK_TITLE.lerp(accent_color, 0.42)
	_badge_label.text = badge_text
	_badge_label.add_theme_color_override("font_color", muted_accent)
	_title_label.add_theme_color_override("font_color", INK_TITLE)
	_subtitle_label.add_theme_color_override("font_color", muted_accent)
	_status_value_label.add_theme_color_override("font_color", status_color)


func _resolve_world_accent(primary_type: String) -> Color:
	match primary_type:
		"Sect":
			return Color(0.27, 0.50, 0.31, 1.0)
		"MortalRealm":
			return Color(0.56, 0.41, 0.20, 1.0)
		"Market":
			return Color(0.69, 0.31, 0.16, 1.0)
		"Wilderness":
			return Color(0.23, 0.46, 0.40, 1.0)
		"CultivatorClan":
			return Color(0.48, 0.38, 0.17, 1.0)
		"ImmortalCity":
			return Color(0.17, 0.43, 0.52, 1.0)
		"Ruin":
			return Color(0.41, 0.31, 0.34, 1.0)
		_:
			return Color(0.32, 0.26, 0.18, 1.0)
