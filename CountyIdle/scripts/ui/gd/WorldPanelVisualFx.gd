extends Node

const INK_MAIN := Color(0.176471, 0.145098, 0.12549, 1.0)
const INK_MUTED := Color(0.368627, 0.313725, 0.258824, 1.0)
const PAPER_MAIN := Color(0.94902, 0.92549, 0.862745, 1.0)
const PAPER_SOFT := Color(0.94902, 0.92549, 0.862745, 0.12)
const BORDER_INK := Color(0.270588, 0.231373, 0.192157, 0.34)
const BUTTON_FILL := Color(0.27451, 0.223529, 0.168627, 0.82)
const BUTTON_BORDER := Color(0.560784, 0.470588, 0.333333, 0.85)

var _top_tab_row: Control
var _map_viewport: Control
var _world_map_view: Control
var _secondary_map_view: Control
var _county_town_map_view: Control
var _map_directive_row: Control
var _map_status_label: Label
var _map_primary_action_button: Button
var _map_secondary_action_button: Button
var _world_site_vbox: Control
var _world_site_title: Label
var _world_site_subtitle: Label
var _world_site_hint: Label
var _world_site_back_button: Button
var _world_site_action_button: Button
var _world_site_header: Control
var _world_site_summary: Control
var _world_site_template: Control
var _world_site_description: Control
var _world_site_action_row: Control
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_top_tab_row = root.get_node("TopTabRow")
	_map_directive_row = root.get_node("MapDirectiveRow")
	_map_status_label = root.get_node("MapDirectiveRow/MapStatusLabel")
	_map_primary_action_button = root.get_node("MapDirectiveRow/MapPrimaryActionButton")
	_map_secondary_action_button = root.get_node("MapDirectiveRow/MapSecondaryActionButton")
	_map_viewport = root.get_node("MapViewport")
	_world_map_view = root.get_node("MapViewport/MapPages/WorldMapView")
	_secondary_map_view = root.get_node("MapViewport/MapPages/SecondaryMapView")
	_county_town_map_view = root.get_node("MapViewport/MapPages/CountyTownMapView")
	_world_site_vbox = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox")
	_world_site_header = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HeaderBox")
	_world_site_summary = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox")
	_world_site_template = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid")
	_world_site_description = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/DescriptionCard")
	_world_site_action_row = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/ActionRow")
	_world_site_title = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HeaderBox/TitleLabel")
	_world_site_subtitle = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HeaderBox/SubtitleLabel")
	_world_site_hint = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/HintLabel")
	_world_site_back_button = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/ActionRow/BackButton")
	_world_site_action_button = root.get_node("MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/ActionRow/ActionButton")
	apply_world_site_theme_styles()
	reset_state()


func apply_world_site_theme_styles() -> void:
	for path in [
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/TypeCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RegionCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RarityCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/UnlockCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/FocusCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/YieldCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/RiskCard",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/DescriptionCard"
	]:
		var panel: PanelContainer = get_parent().get_node(path)
		panel.add_theme_stylebox_override("panel", _create_world_site_card_style())

	for path in [
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/TypeCard/TypeMargin/TypeVBox/Label",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RegionCard/RegionMargin/RegionVBox/Label",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RarityCard/RarityMargin/RarityVBox/Label",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/UnlockCard/UnlockMargin/UnlockVBox/Label",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/FocusCard/FocusMargin/FocusVBox/Label",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/YieldCard/YieldMargin/YieldVBox/Label",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/RiskCard/RiskMargin/RiskVBox/Label"
	]:
		var label: Label = get_parent().get_node(path)
		label.add_theme_color_override("font_color", INK_MUTED)

	for path in [
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/TypeCard/TypeMargin/TypeVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RegionCard/RegionMargin/RegionVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/RarityCard/RarityMargin/RarityVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/SummaryBox/UnlockCard/UnlockMargin/UnlockVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/FocusCard/FocusMargin/FocusVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/YieldCard/YieldMargin/YieldVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/TemplateGrid/RiskCard/RiskMargin/RiskVBox/Value",
		"MapViewport/MapPages/SecondaryMapView/SecondaryMapPadding/SecondaryMapVBox/DescriptionCard/DescriptionMargin/DescriptionLabel"
	]:
		var value_label: Label = get_parent().get_node(path)
		value_label.add_theme_color_override("font_color", INK_MAIN)

	_world_site_hint.add_theme_color_override("font_color", INK_MUTED)
	_apply_world_site_button_style(_world_site_back_button, false)
	_apply_world_site_button_style(_world_site_action_button, true)


func style_world_site_sandbox_shell(sandbox_view: Control) -> void:
	if sandbox_view == null:
		return
	sandbox_view.add_theme_stylebox_override("panel", _create_world_site_sandbox_style())
	var map_hint := sandbox_view.get_node_or_null("MapHintLabel")
	if map_hint is Label:
		map_hint.visible = false
		map_hint.add_theme_color_override("font_color", INK_MUTED)
	var regenerate := sandbox_view.get_node_or_null("RegenerateButton")
	if regenerate is Button:
		regenerate.visible = false
		_apply_world_site_button_style(regenerate, false)


func set_world_site_sandbox_visible(visible: bool) -> void:
	var sandbox_view := _world_site_vbox.get_node_or_null("GeneratedSecondarySandboxView")
	if sandbox_view == null:
		return

	_kill_tween()
	if visible:
		sandbox_view.visible = true
		sandbox_view.modulate.a = 1.0
		sandbox_view.scale = Vector2.ONE
	else:
		sandbox_view.visible = false
		sandbox_view.modulate.a = 1.0
		sandbox_view.scale = Vector2.ONE

func set_world_site_intro_visible(visible: bool) -> void:
	_world_site_header.visible = visible
	_world_site_summary.visible = visible
	_world_site_template.visible = visible
	_world_site_description.visible = visible
	_world_site_action_row.visible = visible
	_world_site_hint.visible = visible


func apply_world_site_tone(primary_type: String) -> void:
	var accent := _resolve_world_site_accent(primary_type)
	_world_site_title.add_theme_color_override("font_color", accent)
	_world_site_subtitle.add_theme_color_override("font_color", accent.lightened(0.12))
	var emphasis := accent.lightened(0.08)
	_world_site_action_button.add_theme_color_override("font_color", emphasis)
	_world_site_action_button.add_theme_color_override("font_hover_color", PAPER_MAIN)
	_world_site_action_button.add_theme_color_override("font_pressed_color", PAPER_MAIN)
	_world_site_action_button.add_theme_stylebox_override("normal", _create_world_site_button_style(emphasis, false, true))
	_world_site_action_button.add_theme_stylebox_override("hover", _create_world_site_button_style(emphasis, true, true))
	_world_site_action_button.add_theme_stylebox_override("pressed", _create_world_site_button_style(emphasis, true, true))
	_world_site_action_button.add_theme_stylebox_override("focus", _create_world_site_button_style(emphasis, true, true))


func apply_map_directive_tone(accent: Color, primary_enabled: bool, secondary_enabled: bool, row_visible: bool = true) -> void:
	if not row_visible:
		reset_map_directive_tone()
		return

	var emphasis := accent.lightened(0.10)
	_map_status_label.add_theme_color_override("font_color", emphasis)
	_apply_map_directive_button_tone(_map_primary_action_button, emphasis, primary_enabled)
	_apply_map_directive_button_tone(_map_secondary_action_button, emphasis.darkened(0.04), secondary_enabled)


func reset_map_directive_tone() -> void:
	_map_status_label.add_theme_color_override("font_color", INK_MUTED)
	_apply_map_directive_button_tone(_map_primary_action_button, INK_MAIN, not _map_primary_action_button.disabled)
	_apply_map_directive_button_tone(_map_secondary_action_button, INK_MAIN, not _map_secondary_action_button.disabled)


func play_tab_switch(tab_name: String) -> void:
	_kill_tween()
	var target_view: Control = _resolve_view(tab_name)
	_top_tab_row.modulate.a = 0.88
	_map_viewport.scale = Vector2.ONE
	target_view.modulate.a = 0.76
	target_view.scale = Vector2.ONE
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_top_tab_row, "modulate:a", 1.0, 0.12)
	_current_tween.tween_property(target_view, "modulate:a", 1.0, 0.14)

func pulse_world_site_panel() -> void:
	if not _secondary_map_view.visible:
		return

	_kill_tween()
	_secondary_map_view.scale = Vector2.ONE
	_secondary_map_view.modulate.a = 0.85
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_secondary_map_view, "modulate:a", 1.0, 0.1)


func reset_state() -> void:
	_kill_tween()
	_top_tab_row.modulate.a = 1.0
	reset_map_directive_tone()
	_map_viewport.scale = Vector2.ONE
	for view in [_world_map_view, _secondary_map_view, _county_town_map_view]:
		view.modulate.a = 1.0
		view.scale = Vector2.ONE


func _resolve_view(tab_name: String) -> Control:
	match tab_name:
		"World":
			return _world_map_view
		"WorldSite":
			return _secondary_map_view
		_:
			return _county_town_map_view


func _apply_world_site_button_style(button: Button, emphasized: bool) -> void:
	button.flat = true
	button.add_theme_font_size_override("font_size", 14)
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", PAPER_MAIN)
	button.add_theme_color_override("font_pressed_color", PAPER_MAIN)
	button.add_theme_stylebox_override("normal", _create_world_site_button_style(INK_MAIN, false, emphasized))
	button.add_theme_stylebox_override("hover", _create_world_site_button_style(INK_MAIN, true, emphasized))
	button.add_theme_stylebox_override("pressed", _create_world_site_button_style(INK_MAIN, true, emphasized))
	button.add_theme_stylebox_override("focus", _create_world_site_button_style(INK_MAIN, true, emphasized))


func _apply_map_directive_button_tone(button: Button, accent: Color, enabled: bool) -> void:
	button.flat = true
	button.add_theme_color_override("font_color", accent if enabled else Color(INK_MUTED.r, INK_MUTED.g, INK_MUTED.b, 0.72))
	button.add_theme_color_override("font_hover_color", PAPER_MAIN)
	button.add_theme_color_override("font_pressed_color", PAPER_MAIN)
	button.add_theme_color_override("font_disabled_color", Color(INK_MUTED.r, INK_MUTED.g, INK_MUTED.b, 0.52))
	button.add_theme_stylebox_override("normal", _create_map_directive_button_style(accent, false, enabled))
	button.add_theme_stylebox_override("hover", _create_map_directive_button_style(accent, true, enabled))
	button.add_theme_stylebox_override("pressed", _create_map_directive_button_style(accent, true, enabled))
	button.add_theme_stylebox_override("focus", _create_map_directive_button_style(accent, true, enabled))
	button.add_theme_stylebox_override("disabled", _create_map_directive_button_style(accent, false, false))


func _create_world_site_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.20)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(BORDER_INK.r, BORDER_INK.g, BORDER_INK.b, 0.75)
	return style


func _create_world_site_sandbox_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_SOFT
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = BORDER_INK
	return style


func _create_world_site_button_style(accent: Color, active: bool, emphasized: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = accent if active else Color(BUTTON_FILL.r, BUTTON_FILL.g, BUTTON_FILL.b, 0.0 if not emphasized else 0.22)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = accent if emphasized else BUTTON_BORDER
	style.content_margin_left = 14
	style.content_margin_top = 8
	style.content_margin_right = 14
	style.content_margin_bottom = 8
	return style


func _create_map_directive_button_style(accent: Color, active: bool, enabled: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = accent if active and enabled else Color(accent.r, accent.g, accent.b, 0.12 if enabled else 0.05)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(accent.r, accent.g, accent.b, 0.88 if enabled else 0.32)
	style.content_margin_left = 12
	style.content_margin_top = 6
	style.content_margin_right = 12
	style.content_margin_bottom = 6
	style.corner_radius_top_left = 3
	style.corner_radius_top_right = 3
	style.corner_radius_bottom_right = 3
	style.corner_radius_bottom_left = 3
	return style


func _resolve_world_site_accent(primary_type: String) -> Color:
	match primary_type:
		"Sect":
			return Color(0.24, 0.47, 0.29, 1.0)
		"MortalRealm":
			return Color(0.56, 0.41, 0.20, 1.0)
		"Market":
			return Color(0.69, 0.31, 0.16, 1.0)
		"Wilderness":
			return Color(0.22, 0.45, 0.40, 1.0)
		"CultivatorClan":
			return Color(0.46, 0.36, 0.16, 1.0)
		"ImmortalCity":
			return Color(0.16, 0.42, 0.53, 1.0)
		"Ruin":
			return Color(0.42, 0.30, 0.34, 1.0)
		_:
			return Color(0.24, 0.21, 0.18, 1.0)


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null