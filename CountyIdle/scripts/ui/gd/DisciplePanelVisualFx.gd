extends Node

const PAPER_BG := Color(0.956, 0.945, 0.918, 1.0)
const INK_BLACK := Color(0.173, 0.173, 0.173, 1.0)
const INK_GRAY := Color(0.478, 0.478, 0.478, 1.0)
const CINNABAR := Color(0.651, 0.192, 0.165, 1.0)
const CELADON := Color(0.439, 0.553, 0.506, 1.0)

var _overlay: ColorRect
var _wrapper: Control
var _roster_frame: Control
var _profile_panel: Control
var _right_panel: Control
var _current_tween: Tween


func _ready() -> void:
	var root: Node = get_parent()
	_overlay = root.get_node("Overlay")
	_wrapper = root.get_node("Overlay/Wrapper")
	_roster_frame = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/RosterFrame")
	_profile_panel = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel")
	_right_panel = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel")
	apply_theme_styles()
	reset_state()


func apply_theme_styles() -> void:
	var root := get_parent()
	_apply_panel_style("Overlay/Wrapper", _create_paper_style())
	_apply_panel_style("Overlay/Wrapper/RootColumn/HeaderPanel", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.78), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.45)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.32), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.25)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.58), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.35)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.46), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.32)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/RosterFrame", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.62), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.30)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.62), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.35)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/RootCircle", _create_circle_style(Color(1, 1, 1, 0), CINNABAR))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel", _create_inset_paper_style(Color(0, 0, 0, 0.03), Color(0.74, 0.68, 0.60, 1.0)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel", _create_inset_paper_style(Color(0, 0, 0, 0.02), Color(0.78, 0.71, 0.61, 1.0)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/TraitPanel", _create_inset_paper_style(Color(0, 0, 0, 0.02), Color(0.78, 0.71, 0.61, 1.0)))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/AnnotationPanel", _create_inset_paper_style(Color(0.97, 0.96, 0.94, 1.0), Color(0.56, 0.48, 0.37, 0.95), 2))
	_apply_panel_style("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag", _create_combat_tag_style())

	for key in ["Insight", "Potential", "Health", "Craft", "Mood", "HeartState"]:
		_apply_metric_tile_style(key)

	for entry in [
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/ProfileName", 26, INK_BLACK],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/ProfileMeta", 12, INK_GRAY],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/ProfileStatus", 12, INK_BLACK],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/ProfilePanel/ProfileMargin/ProfileHeader/RootCircle/RootCircleLabel", 12, CINNABAR],
		["Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel/SummaryMargin/SummaryColumn/SummaryLabel", 12, INK_BLACK],
		["Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/SummaryPanel/SummaryMargin/SummaryColumn/GovernanceLabel", 11, INK_GRAY],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmStatus", 13, INK_BLACK],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmHint", 11, INK_GRAY],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaHint", 11, INK_GRAY],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatMain", 22, CINNABAR],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatHint", 12, INK_GRAY],
		["Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/AnnotationPanel/AnnotationMargin/AnnotationColumn/AnnotationText", 13, Color(0.25, 0.25, 0.25, 1.0)],
		["Overlay/Wrapper/RootColumn/HintLabel", 11, INK_GRAY]
	]:
		_apply_label_style(entry[0], entry[1], entry[2])

	var realm_hint: Label = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmHint")
	realm_hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	var qi_hint: Label = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaHint")
	qi_hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	var combat_main: Label = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatMain")
	combat_main.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	var combat_hint: Label = root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/CombatTag/CombatMargin/CombatColumn/CombatHint")
	combat_hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

	var close_button: Button = root.get_node("Overlay/Wrapper/RootColumn/HeaderPanel/HeaderMargin/HeaderRow/CloseButton")
	close_button.add_theme_font_size_override("font_size", 22)
	close_button.add_theme_color_override("font_color", INK_BLACK)
	close_button.add_theme_color_override("font_hover_color", CINNABAR)
	close_button.add_theme_color_override("font_pressed_color", CINNABAR)
	for state in ["normal", "hover", "pressed", "focus"]:
		close_button.add_theme_stylebox_override(state, _create_transparent_style())

	_style_paper_option_button(root.get_node("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel/FilterMargin/FilterColumn/FilterOption"))
	_style_paper_option_button(root.get_node("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/FilterPanel/FilterMargin/FilterColumn/SortOption"))
	_style_roster_tree(root.get_node("Overlay/Wrapper/RootColumn/BodyRow/LeftPanel/LeftMargin/RosterColumn/RosterFrame/RosterMargin/RosterTree"))
	_style_ink_progress_bar(root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/RealmBox/RealmProgress"), INK_BLACK, Color(0.91, 0.89, 0.84, 1.0))
	_style_ink_progress_bar(root.get_node("Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/CultivationPanel/CultivationMargin/CultivationColumn/QiSeaBox/QiSeaProgress"), CELADON, Color(0.91, 0.89, 0.84, 1.0))


func play_open() -> void:
	_kill_tween()
	_overlay.modulate.a = 0.0
	_wrapper.modulate.a = 0.0
	_wrapper.scale = Vector2.ONE
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_overlay, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_wrapper, "modulate:a", 1.0, 0.2)


func pulse_roster_refresh() -> void:
	_kill_tween()
	_roster_frame.scale = Vector2.ONE
	_roster_frame.modulate.a = 0.82
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_roster_frame, "modulate:a", 1.0, 0.1)


func transition_profile_card() -> void:
	_kill_tween()
	_profile_panel.scale = Vector2.ONE
	_profile_panel.modulate.a = 0.84
	_right_panel.modulate.a = 0.9
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_profile_panel, "modulate:a", 1.0, 0.12)
	_current_tween.tween_property(_right_panel, "modulate:a", 1.0, 0.12)


func apply_metric_value_tone(value_label: Label, value: int) -> void:
	var clamped: int = clampi(value, 0, 100)
	var color := INK_GRAY
	if clamped >= 85:
		color = CINNABAR
	elif clamped >= 65:
		color = INK_BLACK
	elif clamped >= 45:
		color = CELADON
	value_label.add_theme_color_override("font_color", color)


func style_trait_tag(panel: PanelContainer, label: Label) -> void:
	panel.add_theme_stylebox_override("panel", _create_trait_tag_style())
	label.add_theme_font_size_override("font_size", 12)
	label.add_theme_color_override("font_color", CINNABAR)


func reset_state() -> void:
	_kill_tween()
	_overlay.modulate.a = 1.0
	_wrapper.modulate.a = 1.0
	_wrapper.scale = Vector2.ONE
	_roster_frame.scale = Vector2.ONE
	_roster_frame.modulate.a = 1.0
	_profile_panel.scale = Vector2.ONE
	_profile_panel.modulate.a = 1.0
	_right_panel.modulate.a = 1.0


func _apply_panel_style(path: String, style: StyleBox) -> void:
	var panel := get_parent().get_node(path)
	if panel is Control:
		panel.add_theme_stylebox_override("panel", style)


func _apply_label_style(path: String, font_size: int, color: Color) -> void:
	var label: Label = get_parent().get_node(path)
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)


func _apply_metric_tile_style(key: String) -> void:
	var tile_path := "Overlay/Wrapper/RootColumn/BodyRow/RightPanel/RightMargin/RightColumn/DashboardRow/FoundationPanel/FoundationMargin/FoundationColumn/StatsCenter/MetricGrid/%sTile" % key
	_apply_panel_style(tile_path, _create_inset_paper_style(Color(0, 0, 0, 0.03), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.26)))
	var title_label: Label = get_parent().get_node("%s/%sMargin/%sColumn/%sTitle" % [tile_path, key, key, key])
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.add_theme_font_size_override("font_size", 12)
	title_label.add_theme_color_override("font_color", INK_GRAY)
	var value_label: Label = get_parent().get_node("%s/%sMargin/%sColumn/%sValue" % [tile_path, key, key, key])
	value_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	value_label.add_theme_font_size_override("font_size", 20)
	value_label.add_theme_color_override("font_color", INK_BLACK)


func _style_paper_option_button(option_button: OptionButton) -> void:
	option_button.custom_minimum_size = Vector2(180, 0)
	option_button.add_theme_font_size_override("font_size", 12)
	option_button.add_theme_stylebox_override("normal", _create_paper_button_style())
	option_button.add_theme_stylebox_override("hover", _create_paper_button_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.08)))
	option_button.add_theme_stylebox_override("pressed", _create_paper_button_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.14)))
	option_button.add_theme_stylebox_override("focus", _create_paper_button_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.08)))
	option_button.add_theme_color_override("font_color", INK_BLACK)


func _style_roster_tree(tree: Tree) -> void:
	tree.add_theme_color_override("font_color", INK_BLACK)
	tree.add_theme_color_override("font_selected_color", CINNABAR)
	tree.add_theme_color_override("guide_color", Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.45))
	tree.add_theme_color_override("relationship_line_color", Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.35))
	tree.add_theme_stylebox_override("selected", _create_inset_paper_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.08), Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.45)))
	tree.add_theme_stylebox_override("selected_focus", _create_inset_paper_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.12), Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.65)))
	tree.add_theme_stylebox_override("cursor", _create_inset_paper_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.08), Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.45)))
	tree.add_theme_stylebox_override("cursor_unfocused", _create_inset_paper_style(Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.05), Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.25)))
	tree.add_theme_stylebox_override("panel", _create_inset_paper_style(Color(PAPER_BG.r, PAPER_BG.g, PAPER_BG.b, 0.08), Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.18)))


func _style_ink_progress_bar(progress_bar: ProgressBar, fill_color: Color, background_color: Color) -> void:
	progress_bar.add_theme_stylebox_override("fill", _create_progress_fill_style(fill_color))
	progress_bar.add_theme_stylebox_override("background", _create_progress_bar_style(background_color))
	progress_bar.custom_minimum_size = Vector2(0, 14)


func _create_inset_paper_style(color: Color, border_color: Color, border_width: int = 1) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.border_width_left = border_width
	style.border_width_top = border_width
	style.border_width_right = border_width
	style.border_width_bottom = border_width
	style.border_color = border_color
	return style


func _create_paper_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PAPER_BG
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.48, 0.42, 0.35, 0.45)
	style.shadow_color = Color(0, 0, 0, 0.35)
	style.shadow_size = 10
	return style


func _create_circle_style(background: Color, border_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = border_color
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


func _create_paper_button_style(color: Color = Color(1, 1, 1, 0.03)) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(INK_GRAY.r, INK_GRAY.g, INK_GRAY.b, 0.45)
	style.content_margin_left = 12
	style.content_margin_top = 6
	style.content_margin_right = 12
	style.content_margin_bottom = 6
	return style


func _create_combat_tag_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.03)
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = CINNABAR
	return style


func _create_trait_tag_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.03)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(CINNABAR.r, CINNABAR.g, CINNABAR.b, 0.75)
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(1, 1, 1, 0)
	return style


func _create_progress_bar_style(background_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background_color
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = INK_GRAY
	return style


func _create_progress_fill_style(fill_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = fill_color
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
