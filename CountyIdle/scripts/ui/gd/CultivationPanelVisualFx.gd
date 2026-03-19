extends Node

const BG_DARK := Color(0.027, 0.035, 0.031, 0.98)
const PANEL_DARK := Color(0.078, 0.094, 0.086, 0.82)
const CARD_DARK := Color(0.094, 0.110, 0.102, 0.92)
const CARD_DARK_ACTIVE := Color(0.125, 0.140, 0.130, 0.98)
const PAPER_SOFT := Color(1, 1, 1, 0.06)
const GOLD := Color(0.894, 0.753, 0.310, 1.0)
const GOLD_DIM := Color(0.420, 0.384, 0.267, 1.0)
const INK_MAIN := Color(0.910, 0.922, 0.914, 1.0)
const INK_MUTED := Color(0.557, 0.596, 0.580, 1.0)
const INK_DIM := Color(0.420, 0.384, 0.267, 0.85)
const SEAL_RED := Color(0.639, 0.176, 0.176, 1.0)
const POSITIVE := Color(0.247, 0.851, 0.659, 1.0)
const WARNING := Color(0.839, 0.698, 0.290, 1.0)
const CALM := Color(0.839, 0.698, 0.290, 1.0)
# 修炼卷字体与标签纹理资源，保持与设计稿一致的书法气质。
const FONT_TITLE := preload("res://assets/ui/fonts/MaShanZheng-Regular.ttf")
const FONT_BODY := preload("res://assets/ui/fonts/NotoSerifSC[wght].ttf")
const TAG_POLY_TEXTURE := preload("res://assets/ui/shapes/tag_poly.svg")
const RING_DASHED_TEXTURE := preload("res://assets/ui/shapes/ring_dashed.svg")
const RING_THIN_TEXTURE := preload("res://assets/ui/shapes/ring_thin.svg")

var _backdrop: ColorRect
var _root_column: Control
var _soul_core_outer: Control
var _soul_core_inner: Control
var _core_glow_outer: Control
var _core_glow_inner: Control
var _ring_dashed: TextureRect
var _ring_thin: TextureRect
var _top_overlay: Control
var _current_tween: Tween
var _pulse_time := 0.0


# 卷册布局可能调整，统一做安全取节点与样式注入，避免空节点直接报错。
func _apply_stylebox_safe(root: Node, path: String, theme_type: String, style: StyleBox) -> void:
	var control := root.get_node_or_null(path) as Control
	if control == null:
		push_warning("CultivationPanelVisualFx missing node: %s" % path)
		return
	control.add_theme_stylebox_override(theme_type, style)


func _set_visible_safe(root: Node, path: String, is_visible: bool) -> void:
	var canvas_item := root.get_node_or_null(path) as CanvasItem
	if canvas_item == null:
		push_warning("CultivationPanelVisualFx missing node: %s" % path)
		return
	canvas_item.visible = is_visible


func _ready() -> void:
	# 该脚本只负责视觉样式与轻动效，权威数据仍由 C# 面板逻辑维护。
	var root: Node = get_parent()
	_backdrop = root.get_node("Backdrop")
	_root_column = root.get_node("ScreenMargin/ScreenRoot/RootColumn")
	# 顶部返回层在部分卷面迭代中可能暂未挂载，视觉脚本需允许缺席后降级运行。
	_top_overlay = root.get_node_or_null("TopOverlay")
	_soul_core_outer = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/SoulCoreOuter")
	_soul_core_inner = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/SoulCoreInner")
	_core_glow_outer = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/CoreGlowOuter")
	_core_glow_inner = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/CoreGlowInner")
	_ring_thin = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/SoulRingThin")
	_ring_dashed = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/SoulRingDashed")
	apply_theme_styles()
	reset_state()


func _process(delta: float) -> void:
	# 阵眼轻微起伏，让左侧弟子 HUD 保持“聚灵阵”活感。
	_pulse_time += delta
	var outer_scale := 1.0 + sin(_pulse_time * 1.4) * 0.018
	var inner_scale := 1.0 + sin((_pulse_time * 1.9) + 0.9) * 0.012
	_soul_core_outer.scale = Vector2.ONE * outer_scale
	_soul_core_inner.scale = Vector2.ONE * inner_scale
	_core_glow_outer.scale = Vector2.ONE * (1.0 + sin(_pulse_time * 1.1) * 0.02)
	_core_glow_inner.scale = Vector2.ONE * (1.0 + sin((_pulse_time * 1.6) + 0.6) * 0.016)


func apply_theme_styles() -> void:
	var root := get_parent()
	_backdrop.color = BG_DARK
	_ring_dashed.texture = RING_DASHED_TEXTURE
	_ring_thin.texture = RING_THIN_TEXTURE
	_ring_dashed.self_modulate = Color(1, 1, 1, 0.85)
	_ring_thin.self_modulate = Color(1, 1, 1, 0.55)

	for path in [
		"ScreenMargin/ScreenRoot/FrameOuter",
		"ScreenMargin/ScreenRoot/FrameInner"
	]:
		_set_visible_safe(root, path, false)

	for path in [
		"ScreenMargin/ScreenRoot/CornerTL",
		"ScreenMargin/ScreenRoot/CornerTR",
		"ScreenMargin/ScreenRoot/CornerBL",
		"ScreenMargin/ScreenRoot/CornerBR"
	]:
		_set_visible_safe(root, path, false)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_panel_glass_style())

	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter", "panel", _create_footer_style())

	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/HintPanel", "panel", _create_hint_style())

	# 顶部返回圆钮与底部状态槽图标样式。
	_apply_stylebox_safe(root, "TopOverlay/TopMargin/TopRow/BackRow/BackIconFrame", "panel", _create_back_icon_style())
	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterIcon", "panel", _create_footer_icon_style())

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_track_box_style())

	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/ActionTitleRow/ActionPointBadge", "panel", _create_action_point_badge_style())
	# 行动力圆点：首颗点亮，其余描边。
	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/ActionTitleRow/ActionPointBadge/ActionPointRow/ActionPointDot1",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/ActionTitleRow/ActionPointBadge/ActionPointRow/ActionPointDot2",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/ActionTitleRow/ActionPointBadge/ActionPointRow/ActionPointDot3"
	]:
		var dot: PanelContainer = root.get_node(path)
		dot.add_theme_stylebox_override("panel", _create_action_point_dot_style(path.ends_with("Dot1")))

	# 提示行左侧的绿色指示点。
	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/HintPanel/HintMargin/HintRow/HintDot", "panel", _create_hint_dot_style())

	# 卡片右上角的时辰徽记。
	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_time_badge_style())

	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/RealmBadge", "panel", _create_tag_poly_style())

	var focus_line_left: ColorRect = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusBanner/FocusBannerLineLeft")
	var focus_line_right: ColorRect = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusBanner/FocusBannerLineRight")
	focus_line_left.color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.6)
	focus_line_right.color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.6)

	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/TopBar/TitleGroup/TitleSeal", "panel", _create_title_seal_style())

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/SoulCoreOuter",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/SoulCoreInner"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_circle_style(path.ends_with("Outer")))

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/CoreGlowOuter",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/CoreGlowInner"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_core_glow_style())

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/GlyphBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/IconBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/IconBadge"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_badge_style())

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/AgeTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/ResidenceTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/SkillTrackTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TechniqueTrackTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/CraftTrackTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/MeditationTrackTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/TagColumn/AssignmentTag",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/TagColumn/BranchTag"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_tag_poly_style())

	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/DutyTag", "panel", _create_tag_pill_style())

	# 标签色相分层：境界标签更亮，其余保持低饱和。
	var realm_tag: Control = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/RealmBadge")
	realm_tag.modulate = Color(1, 0.98, 0.85, 1)
	var age_tag: Control = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/AgeTag")
	age_tag.modulate = Color(0.72, 0.70, 0.62, 0.85)
	var residence_tag: Control = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/ResidenceTag")
	residence_tag.modulate = Color(0.72, 0.70, 0.62, 0.85)

	var roster_marker: ColorRect = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/SelectionMarker")
	roster_marker.color = Color(GOLD.r, GOLD.g, GOLD.b, 0.85)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_seal_style())

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/SecondaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/PrimaryEffect",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/SecondaryEffect"
	]:
		_apply_stylebox_safe(root, path, "panel", _create_effect_box_style())

	# 主收益图标底色，与设计稿一致的色块提示。
	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon", "panel", _create_effect_icon_style(POSITIVE))
	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon", "panel", _create_effect_icon_style(POSITIVE))
	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon", "panel", _create_effect_icon_style(INK_MAIN))
	_apply_stylebox_safe(root, "ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon", "panel", _create_effect_icon_style(SEAL_RED))

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard"
	]:
		var action_card := root.get_node_or_null(path) as Button
		if action_card == null:
			push_warning("CultivationPanelVisualFx missing node: %s" % path)
			continue
		_apply_action_card_style(action_card)

	var prev_button: Button = root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/PreviousDiscipleButton") as Button
	if prev_button == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/PreviousDiscipleButton")
	else:
		_apply_nav_button_style(prev_button, true)
	var next_button: Button = root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/NextDiscipleButton") as Button
	if next_button == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/NextDiscipleButton")
	else:
		_apply_nav_button_style(next_button, false)

	var roster_button_template := root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate") as Button
	if roster_button_template == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate")
	else:
		_apply_roster_button_style(roster_button_template)

	var close_button := root.get_node_or_null("TopOverlay/TopMargin/TopRow/CloseButton") as Button
	if close_button == null:
		push_warning("CultivationPanelVisualFx missing node: TopOverlay/TopMargin/TopRow/CloseButton")
	else:
		_apply_close_button_style(close_button)

	var skill_progress := root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/SkillTrackProgress") as ProgressBar
	if skill_progress == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/SkillTrackProgress")
	else:
		_apply_progress_bar_style(skill_progress, GOLD)
	var technique_progress := root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TechniqueTrackProgress") as ProgressBar
	if technique_progress == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TechniqueTrackProgress")
	else:
		_apply_progress_bar_style(technique_progress, GOLD)
	var craft_progress := root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/CraftTrackProgress") as ProgressBar
	if craft_progress == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/CraftTrackProgress")
	else:
		_apply_progress_bar_style(craft_progress, GOLD)
	var meditation_progress := root.get_node_or_null("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/MeditationTrackProgress") as ProgressBar
	if meditation_progress == null:
		push_warning("CultivationPanelVisualFx missing node: ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/MeditationTrackProgress")
	else:
		_apply_progress_bar_style(meditation_progress, GOLD)

	_apply_label_styles(root)


func play_open() -> void:
	# 开场让整张修炼卷从暗色场景中缓慢浮出。
	_kill_tween()
	_backdrop.modulate.a = 0.0
	_root_column.modulate.a = 0.0
	# 顶部返回层若尚未挂回场景，则仅跳过其淡入，不影响主卷打开。
	if _top_overlay != null:
		_top_overlay.modulate.a = 0.0
	_root_column.scale = Vector2(0.985, 0.985)
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_backdrop, "modulate:a", 1.0, 0.18)
	_current_tween.tween_property(_root_column, "modulate:a", 1.0, 0.22)
	if _top_overlay != null:
		_current_tween.tween_property(_top_overlay, "modulate:a", 1.0, 0.22)
	_current_tween.tween_property(_root_column, "scale", Vector2.ONE, 0.22)


func reset_state() -> void:
	_kill_tween()
	_backdrop.modulate.a = 1.0
	_root_column.modulate.a = 1.0
	if _top_overlay != null:
		_top_overlay.modulate.a = 1.0
	_root_column.scale = Vector2.ONE


func pulse_soul_core() -> void:
	# 切换弟子时重置脉冲节奏，给玩家一个“阵眼换人”的细小反馈。
	_pulse_time = 0.0


func _apply_label_styles(root: Node) -> void:
	# 全局默认正文字体，局部再覆盖书法体与强调字重。
	var root_control := root as Control
	if root_control != null:
		root_control.add_theme_font_override("font", FONT_BODY)

	var title_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/TopBar/TitleGroup/TitleColumn/TitleLabel")
	title_label.add_theme_font_size_override("font_size", 28)
	title_label.add_theme_color_override("font_color", INK_MAIN)

	var subtitle_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/TopBar/TitleGroup/TitleColumn/SubtitleLabel")
	subtitle_label.add_theme_font_size_override("font_size", 13)
	subtitle_label.add_theme_color_override("font_color", INK_MUTED)

	var seal_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/TopBar/TitleGroup/TitleSeal/SealLabel")
	seal_label.add_theme_font_size_override("font_size", 28)
	seal_label.add_theme_color_override("font_color", Color.WHITE)
	seal_label.add_theme_font_override("font", FONT_TITLE)

	# 响应式改版期间顶部返回节点可能被拆分/延后接回，缺失时仅跳过样式而不让整卷报错。
	var back_icon := root.get_node_or_null("TopOverlay/TopMargin/TopRow/BackRow/BackIconFrame/BackIconLabel") as Label
	if back_icon != null:
		back_icon.add_theme_font_size_override("font_size", 18)
		back_icon.add_theme_color_override("font_color", GOLD)
	else:
		push_warning("CultivationPanelVisualFx missing TopOverlay back icon label.")

	var back_text := root.get_node_or_null("TopOverlay/TopMargin/TopRow/BackRow/BackTextLabel") as Label
	if back_text != null:
		back_text.add_theme_font_size_override("font_size", 13)
		back_text.add_theme_color_override("font_color", INK_MAIN)
	else:
		push_warning("CultivationPanelVisualFx missing TopOverlay back text label.")

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/PopulationCard/CardMargin/CardColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/TechCard/CardMargin/CardColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/ResourceCard/CardMargin/CardColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/RegisterCard/CardMargin/CardColumn/TitleLabel"
	]:
		var summary_title: Label = root.get_node(path)
		summary_title.add_theme_font_size_override("font_size", 12)
		summary_title.add_theme_color_override("font_color", INK_DIM)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/PopulationCard/CardMargin/CardColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/TechCard/CardMargin/CardColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/ResourceCard/CardMargin/CardColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/TopBar/SummaryRow/RegisterCard/CardMargin/CardColumn/ValueLabel"
	]:
		var summary_value: Label = root.get_node(path)
		summary_value.add_theme_font_size_override("font_size", 18)
		summary_value.add_theme_color_override("font_color", INK_MAIN)

	var focus_banner: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusBanner/FocusBannerLabel")
	focus_banner.add_theme_font_size_override("font_size", 12)
	focus_banner.add_theme_color_override("font_color", GOLD_DIM)

	var footer_title: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterTextColumn/FooterTitleLabel")
	footer_title.add_theme_font_size_override("font_size", 10)
	footer_title.add_theme_color_override("font_color", GOLD_DIM)

	var action_title: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/ActionTitleRow/ActionTitleLabel")
	action_title.add_theme_font_size_override("font_size", 28)
	action_title.add_theme_color_override("font_color", INK_MAIN)
	action_title.add_theme_font_override("font", FONT_TITLE)

	var nav_hint: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/DiscipleNavRow/NavHintLabel")
	nav_hint.add_theme_font_size_override("font_size", 12)
	nav_hint.add_theme_color_override("font_color", INK_MUTED)

	var action_point: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/ActionTitleRow/ActionPointBadge/ActionPointRow/ActionPointTextLabel")
	action_point.add_theme_font_size_override("font_size", 12)
	action_point.add_theme_color_override("font_color", INK_MUTED)

	var name_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/SelectedDiscipleNameLabel")
	name_label.add_theme_font_size_override("font_size", 30)
	name_label.add_theme_color_override("font_color", INK_MAIN)
	name_label.add_theme_font_override("font", FONT_TITLE)

	var roster_glyph_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/GlyphBadge/GlyphLabel")
	roster_glyph_label.add_theme_font_size_override("font_size", 20)
	roster_glyph_label.add_theme_color_override("font_color", GOLD)
	roster_glyph_label.add_theme_font_override("font", FONT_TITLE)

	var roster_name_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/BodyColumn/NameLabel")
	roster_name_label.add_theme_font_size_override("font_size", 15)
	roster_name_label.add_theme_color_override("font_color", INK_MAIN)

	var roster_meta_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/BodyColumn/MetaLabel")
	roster_meta_label.add_theme_font_size_override("font_size", 11)
	roster_meta_label.add_theme_color_override("font_color", INK_MUTED)

	var roster_assignment_tag_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/TagColumn/AssignmentTag/AssignmentTagLabel")
	roster_assignment_tag_label.add_theme_font_size_override("font_size", 11)
	roster_assignment_tag_label.add_theme_color_override("font_color", GOLD)

	var roster_branch_tag_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterScroll/DiscipleRosterList/DiscipleRosterButtonTemplate/ButtonMargin/ButtonRow/TagColumn/BranchTag/BranchTagLabel")
	roster_branch_tag_label.add_theme_font_size_override("font_size", 11)
	roster_branch_tag_label.add_theme_color_override("font_color", GOLD)

	var meta_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/SelectedDiscipleMetaLabel")
	meta_label.add_theme_font_size_override("font_size", 13)
	meta_label.add_theme_color_override("font_color", INK_MUTED)
	meta_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

	var realm_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/RealmBadge/RealmLabel")
	realm_label.add_theme_font_size_override("font_size", 12)
	realm_label.add_theme_color_override("font_color", GOLD)

	var age_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/AgeTag/AgeTagLabel")
	age_label.add_theme_font_size_override("font_size", 12)
	age_label.add_theme_color_override("font_color", INK_MUTED)

	var residence_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/TagRow/ResidenceTag/ResidenceTagLabel")
	residence_label.add_theme_font_size_override("font_size", 12)
	residence_label.add_theme_color_override("font_color", INK_MUTED)

	var duty_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreInfoColumn/DutyTag/DutyLabel")
	duty_label.add_theme_font_size_override("font_size", 12)
	duty_label.add_theme_color_override("font_color", INK_MAIN)

	var footer_icon: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterIcon/FooterIconLabel")
	footer_icon.add_theme_font_size_override("font_size", 24)
	footer_icon.add_theme_color_override("font_color", GOLD)
	footer_icon.add_theme_font_override("font", FONT_TITLE)

	var core_letter: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/CoreSection/SoulCoreWrap/SoulCoreStack/CoreLetterLabel")
	core_letter.add_theme_font_size_override("font_size", 54)
	core_letter.add_theme_color_override("font_color", INK_MAIN)
	core_letter.add_theme_font_override("font", FONT_TITLE)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/RosterPanel/RosterMargin/RosterColumn/RosterHintLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterTextColumn/SelectedDiscipleStatusLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/SelectedDiscipleStatusHighlightLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/SelectedDiscipleInsightLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/HintPanel/HintMargin/HintRow/HintLabel"
	]:
		var note_label: Label = root.get_node(path)
		note_label.add_theme_font_size_override("font_size", 13)
		note_label.add_theme_color_override("font_color", INK_MUTED)

	# 强制提示文案保持单行，避免压缩导致竖排换行。
	var hint_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionHeaderRow/HintPanel/HintMargin/HintRow/HintLabel")
	hint_label.autowrap_mode = TextServer.AUTOWRAP_OFF
	hint_label.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS

	var footer_assignment: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/FooterTextColumn/SelectedDiscipleStatusLabel")
	footer_assignment.add_theme_font_size_override("font_size", 16)
	footer_assignment.add_theme_color_override("font_color", INK_MAIN)

	var highlight_label: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/FocusFooter/FooterMargin/FooterColumn/FooterHeaderRow/SelectedDiscipleStatusHighlightLabel")
	highlight_label.add_theme_color_override("font_color", POSITIVE)
	highlight_label.add_theme_font_size_override("font_size", 18)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackNameLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackNameLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackNameLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackNameLabel"
	]:
		var track_name: Label = root.get_node(path)
		track_name.add_theme_font_size_override("font_size", 15)
		track_name.add_theme_color_override("font_color", INK_MAIN)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TrackIconLabel"
	]:
		var track_icon: Label = root.get_node(path)
		track_icon.add_theme_font_size_override("font_size", 18)
		track_icon.add_theme_color_override("font_color", POSITIVE)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/SkillTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/SkillTrackTag/SkillTrackTagLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/TechniqueTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/TechniqueTrackTag/TechniqueTrackTagLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/CraftTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/CraftTrackTag/CraftTrackTagLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/LeftPanel/LeftMargin/LeftColumn/TrackSection/MeditationTrackBox/TrackMargin/TrackColumn/TrackHeaderRow/MeditationTrackTag/MeditationTrackTagLabel"
	]:
		var tag_label: Label = root.get_node(path)
		tag_label.add_theme_font_size_override("font_size", 11)
		tag_label.add_theme_color_override("font_color", GOLD_DIM)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/TitleLabel"
	]:
		var card_title: Label = root.get_node(path)
		card_title.add_theme_font_size_override("font_size", 24)
		card_title.add_theme_color_override("font_color", INK_MAIN)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SubtitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SubtitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SubtitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SubtitleLabel"
	]:
		var card_subtitle: Label = root.get_node(path)
		card_subtitle.add_theme_font_size_override("font_size", 12)
		card_subtitle.add_theme_color_override("font_color", GOLD_DIM)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeIconLabel"
	]:
		var time_icon: Label = root.get_node(path)
		time_icon.add_theme_font_size_override("font_size", 12)
		time_icon.add_theme_color_override("font_color", GOLD_DIM)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeTextLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeTextLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeTextLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/TimeBadge/TimeRow/TimeTextLabel"
	]:
		var time_text: Label = root.get_node(path)
		time_text.add_theme_font_size_override("font_size", 12)
		time_text.add_theme_color_override("font_color", INK_MUTED)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/DescLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/DescLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/DescLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/DescLabel"
	]:
		var desc_label: Label = root.get_node(path)
		desc_label.add_theme_font_size_override("font_size", 14)
		desc_label.add_theme_color_override("font_color", INK_MUTED)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/StatusLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/StatusLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/StatusLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/StatusLabel"
	]:
		var status_label: Label = root.get_node(path)
		status_label.add_theme_font_size_override("font_size", 13)
		status_label.add_theme_color_override("font_color", INK_MUTED)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/IconBadge/IconLabel"
	]:
		var icon_label: Label = root.get_node(path)
		icon_label.add_theme_font_size_override("font_size", 32)
		icon_label.add_theme_color_override("font_color", GOLD_DIM)
		icon_label.add_theme_font_override("font", FONT_TITLE)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge/SealLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge/SealLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge/SealLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/CardHeaderRow/HeaderTextColumn/SealBadge/SealLabel"
	]:
		var seal_text: Label = root.get_node(path)
		seal_text.add_theme_font_size_override("font_size", 12)
		seal_text.add_theme_color_override("font_color", BG_DARK)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/TitleLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/TitleLabel"
	]:
		var effect_title: Label = root.get_node(path)
		effect_title.add_theme_font_size_override("font_size", 10)
		effect_title.add_theme_color_override("font_color", INK_MUTED)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectColumn/ValueLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/SecondaryEffect/EffectMargin/EffectColumn/ValueLabel"
	]:
		var effect_value: Label = root.get_node(path)
		effect_value.add_theme_font_size_override("font_size", 14)
		effect_value.add_theme_color_override("font_color", INK_MAIN if path.contains("PrimaryEffect") else GOLD)

	for path in [
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/SkillTrainingCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon/EffectIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/TechniquePolishCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon/EffectIconLabel",
		"ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/CraftPracticeCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon/EffectIconLabel"
	]:
		var icon_label: Label = root.get_node(path)
		icon_label.add_theme_font_size_override("font_size", 16)
		icon_label.add_theme_color_override("font_color", BG_DARK)
		icon_label.add_theme_font_override("font", FONT_BODY)

	var meditation_icon: Label = root.get_node("ScreenMargin/ScreenRoot/RootColumn/BodyRow/RightLayer/RightColumn/ActionGrid/MeditationCard/CardMargin/CardColumn/EffectRow/PrimaryEffect/EffectMargin/EffectContentRow/EffectIcon/EffectIconLabel")
	meditation_icon.add_theme_font_size_override("font_size", 16)
	meditation_icon.add_theme_color_override("font_color", Color.WHITE)
	meditation_icon.add_theme_font_override("font", FONT_BODY)

	var watermark: Label = root.get_node("WatermarkLabel")
	watermark.add_theme_font_size_override("font_size", 600)
	watermark.add_theme_color_override("font_color", Color(GOLD.r, GOLD.g, GOLD.b, 0.03))
	watermark.add_theme_font_override("font", FONT_TITLE)


func _apply_action_card_style(button: Button) -> void:
	button.flat = true
	# 释放卡片阴影/高光的外扩空间，避免层次被裁切。
	button.clip_contents = false
	button.add_theme_stylebox_override("normal", _create_action_card_box(false, false))
	button.add_theme_stylebox_override("hover", _create_action_card_box(true, false))
	button.add_theme_stylebox_override("pressed", _create_action_card_box(true, true))
	button.add_theme_stylebox_override("focus", _create_action_card_box(true, true))
	button.add_theme_stylebox_override("disabled", _create_action_card_box(false, false, true))
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_disabled_color", INK_DIM)


func _apply_nav_button_style(button: Button, is_left: bool) -> void:
	button.flat = true
	button.add_theme_font_size_override("font_size", 18)
	button.add_theme_stylebox_override("normal", _create_nav_box(false, is_left))
	button.add_theme_stylebox_override("hover", _create_nav_box(true, is_left))
	button.add_theme_stylebox_override("pressed", _create_nav_box(true, is_left))
	button.add_theme_stylebox_override("focus", _create_nav_box(true, is_left))
	button.add_theme_stylebox_override("disabled", _create_nav_box(false, is_left, true))
	button.add_theme_color_override("font_color", GOLD_DIM)
	button.add_theme_color_override("font_hover_color", GOLD)
	button.add_theme_color_override("font_disabled_color", INK_DIM)


func _apply_roster_button_style(button: Button) -> void:
	button.flat = true
	button.clip_text = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	button.add_theme_font_size_override("font_size", 12)
	button.add_theme_stylebox_override("normal", _create_roster_button_box(false))
	button.add_theme_stylebox_override("hover", _create_roster_button_box(true))
	button.add_theme_stylebox_override("pressed", _create_roster_button_box(true, true))
	button.add_theme_stylebox_override("focus", _create_roster_button_box(true, true))
	button.add_theme_stylebox_override("disabled", _create_roster_button_box(false, false, true))
	button.add_theme_color_override("font_color", INK_MAIN)
	button.add_theme_color_override("font_hover_color", INK_MAIN)
	button.add_theme_color_override("font_pressed_color", INK_MAIN)
	button.add_theme_color_override("font_disabled_color", INK_DIM)


func _apply_close_button_style(button: Button) -> void:
	button.flat = true
	button.add_theme_font_size_override("font_size", 24)
	button.add_theme_stylebox_override("normal", _create_close_box(false))
	button.add_theme_stylebox_override("hover", _create_close_box(true))
	button.add_theme_stylebox_override("pressed", _create_close_box(true))
	button.add_theme_stylebox_override("focus", _create_close_box(true))
	button.add_theme_color_override("font_color", INK_MUTED)
	button.add_theme_color_override("font_hover_color", SEAL_RED)


func _apply_progress_bar_style(progress_bar: ProgressBar, fill_color: Color) -> void:
	progress_bar.add_theme_stylebox_override("background", _create_progress_background())
	progress_bar.add_theme_stylebox_override("fill", _create_progress_fill(fill_color))


func _create_frame_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.3)
	return style


func _create_corner_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_color = GOLD
	return style


func _create_surface_style() -> StyleBoxFlat:
	return _create_panel_glass_style()


func _create_panel_glass_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = PANEL_DARK
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.35)
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_right = 8
	style.corner_radius_bottom_left = 8
	style.shadow_color = Color(0, 0, 0, 0.65)
	style.shadow_size = 28
	style.shadow_offset = Vector2(0, 12)
	return style


func _create_footer_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.035, 0.047, 0.043, 0.9)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.65)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.shadow_color = Color(0, 0, 0, 0.55)
	style.shadow_size = 10
	style.shadow_offset = Vector2(0, 6)
	return style


func _create_hint_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_left = 0
	style.border_width_top = 0
	style.border_width_right = 0
	style.border_width_bottom = 0
	return style


func _create_track_box_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_left = 0
	style.border_width_top = 0
	style.border_width_right = 0
	style.border_width_bottom = 0
	return style


func _create_action_point_badge_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0.55)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(1, 1, 1, 0.16)
	style.corner_radius_top_left = 14
	style.corner_radius_top_right = 14
	style.corner_radius_bottom_right = 14
	style.corner_radius_bottom_left = 14
	style.content_margin_left = 12
	style.content_margin_right = 12
	style.content_margin_top = 6
	style.content_margin_bottom = 6
	return style


# 主收益图标的小方块底色。
func _create_effect_icon_style(bg_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = bg_color
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


# 斜切玉牌标签，用于境界/骨龄/居所等关键信息的“标签化”展示。
func _create_tag_poly_style() -> StyleBoxTexture:
	var style := StyleBoxTexture.new()
	style.texture = TAG_POLY_TEXTURE
	style.content_margin_left = 10
	style.content_margin_right = 10
	style.content_margin_top = 2
	style.content_margin_bottom = 2
	return style


func _create_core_glow_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.02)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.15)
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


func _create_title_seal_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = SEAL_RED
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = GOLD
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	style.shadow_color = Color(0, 0, 0, 0.20)
	style.shadow_size = 6
	return style


func _create_circle_style(is_outer: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var fill_color := Color(0.957, 0.961, 0.941, 1.0) if is_outer else Color(0.721, 0.741, 0.729, 1.0)
	var border_alpha := 0.25 if is_outer else 0.18
	style.bg_color = fill_color
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, border_alpha)
	style.corner_radius_top_left = 120
	style.corner_radius_top_right = 120
	style.corner_radius_bottom_right = 120
	style.corner_radius_bottom_left = 120
	style.shadow_color = Color(0, 0, 0, 0.45)
	style.shadow_size = 12
	return style


func _create_badge_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.03)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.9)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	return style


# 圆角胶囊标签，用于“差事”类状态提示。
func _create_tag_pill_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0.35)
	style.border_width_left = 2
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.8)
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


# 返回圆钮（名册入口）的玉牌描边样式。
func _create_back_icon_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0.3)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.8)
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


# 当前运转周天状态槽的图标底板。
func _create_footer_icon_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.15)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.9)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	return style


# 行动力圆点样式，首颗点亮，其余为空心。
func _create_action_point_dot_style(is_active: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	if is_active:
		style.bg_color = GOLD
		style.border_width_left = 0
		style.border_width_top = 0
		style.border_width_right = 0
		style.border_width_bottom = 0
	else:
		style.bg_color = Color(0, 0, 0, 0.0)
		style.border_width_left = 1
		style.border_width_top = 1
		style.border_width_right = 1
		style.border_width_bottom = 1
		style.border_color = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.6)
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


# 提示行绿点，强调结算提示。
func _create_hint_dot_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = POSITIVE
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


# 卡片右上时辰徽记底板。
func _create_time_badge_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0.35)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(1, 1, 1, 0.15)
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.content_margin_left = 6
	style.content_margin_right = 6
	style.content_margin_top = 2
	style.content_margin_bottom = 2
	return style


func _create_seal_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = GOLD
	style.border_width_left = 0
	style.border_width_top = 0
	style.border_width_right = 0
	style.border_width_bottom = 0
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	style.shadow_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.3)
	style.shadow_size = 6
	return style


func _create_effect_box_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.06)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.38)
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.corner_radius_bottom_right = 4
	style.corner_radius_bottom_left = 4
	style.shadow_color = Color(0, 0, 0, 0.25)
	style.shadow_size = 6
	style.shadow_offset = Vector2(0, 4)
	return style


func _create_action_card_box(is_hovered: bool, is_active: bool, is_disabled: bool = false) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var background := CARD_DARK
	var border := Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.45)
	var shadow_color := Color(0, 0, 0, 0.6)
	var shadow_offset := Vector2(0, 10)
	var shadow_size := 18
	if is_hovered and not is_disabled:
		background = Color(CARD_DARK_ACTIVE.r, CARD_DARK_ACTIVE.g, CARD_DARK_ACTIVE.b, 0.95)
		border = GOLD
		shadow_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.24)
		shadow_offset = Vector2.ZERO
		shadow_size = 26
	if is_active and not is_disabled:
		background = CARD_DARK_ACTIVE
		border = GOLD
		shadow_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.32)
		shadow_offset = Vector2.ZERO
		shadow_size = 28
	if is_disabled:
		background = Color(CARD_DARK.r, CARD_DARK.g, CARD_DARK.b, 0.35)
		border = Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, 0.25)
		shadow_color = Color(0, 0, 0, 0.2)
		shadow_offset = Vector2(0, 6)
		shadow_size = 10
	style.bg_color = background
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	if is_active:
		style.border_width_top = 2
	style.border_color = border
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_right = 8
	style.corner_radius_bottom_left = 8
	style.shadow_color = shadow_color
	style.shadow_size = shadow_size
	style.shadow_offset = shadow_offset
	return style


func _create_nav_box(is_hovered: bool, is_left: bool, is_disabled: bool = false) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var bg_alpha := 0.18 if is_hovered else 0.10
	var border_alpha := 0.65 if is_hovered else 0.45
	style.bg_color = Color(0.06, 0.08, 0.07, bg_alpha)
	style.border_width_left = 1
	style.border_width_right = 1
	style.border_width_top = 1
	style.border_width_bottom = 1
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, border_alpha)
	style.corner_radius_top_left = 999
	style.corner_radius_top_right = 999
	style.corner_radius_bottom_right = 999
	style.corner_radius_bottom_left = 999
	return style


func _create_roster_button_box(is_hovered: bool, is_active: bool = false, is_disabled: bool = false) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var bg_alpha := 0.05
	var border_alpha := 0.18
	var shadow_alpha := 0.08
	if is_hovered:
		bg_alpha = 0.10
		border_alpha = 0.28
	if is_active:
		bg_alpha = 0.16
		border_alpha = 0.58
		shadow_alpha = 0.18
	if is_disabled:
		bg_alpha = 0.03
		border_alpha = 0.10
	style.bg_color = Color(PAPER_SOFT.r, PAPER_SOFT.g, PAPER_SOFT.b, bg_alpha)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	if is_active:
		style.border_width_left = 2
		style.border_width_top = 2
		style.border_width_right = 2
		style.border_width_bottom = 2
	style.border_color = Color(GOLD.r, GOLD.g, GOLD.b, border_alpha)
	style.corner_radius_top_left = 8
	style.corner_radius_top_right = 8
	style.corner_radius_bottom_right = 8
	style.corner_radius_bottom_left = 8
	style.content_margin_left = 10
	style.content_margin_top = 8
	style.content_margin_right = 10
	style.content_margin_bottom = 8
	style.shadow_color = Color(GOLD.r, GOLD.g, GOLD.b, shadow_alpha)
	style.shadow_size = 6 if not is_active else 10
	return style


func _create_close_box(is_hovered: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	var bg_alpha := 0.5
	var border_alpha := 0.6
	style.bg_color = Color(0, 0, 0, bg_alpha)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = SEAL_RED if is_hovered else Color(GOLD_DIM.r, GOLD_DIM.g, GOLD_DIM.b, border_alpha)
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	return style


func _create_progress_background() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(1, 1, 1, 0.06)
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	return style


func _create_progress_fill(fill_color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = fill_color
	style.corner_radius_top_left = 2
	style.corner_radius_top_right = 2
	style.corner_radius_bottom_right = 2
	style.corner_radius_bottom_left = 2
	style.shadow_color = Color(GOLD.r, GOLD.g, GOLD.b, 0.35)
	style.shadow_size = 6
	return style


func _kill_tween() -> void:
	if _current_tween != null and _current_tween.is_running():
		_current_tween.kill()
	_current_tween = null
