extends Node

const PAPER_MAIN := Color(0.9569, 0.9451, 0.9176, 1.0)
const PAPER_SOFT := Color(0.985, 0.98, 0.965, 0.9)
const PAPER_DARK := Color(0.902, 0.874, 0.815, 1.0)
const INK_MAIN := Color(0.18, 0.17, 0.15, 1.0)
const INK_MUTED := Color(0.47, 0.43, 0.38, 1.0)
const SEAL_RED := Color(0.698, 0.133, 0.133, 1.0)
const SEAL_RED_SOFT := Color(0.698, 0.133, 0.133, 0.08)
const JADE_LIGHT := Color(0.98, 0.99, 0.985, 1.0)
const JADE_DARK := Color(0.80, 0.86, 0.82, 1.0)
const GOLD_LINE := Color(0.55, 0.45, 0.29, 0.35)
const FIELD_LINE := Color(0.62, 0.53, 0.38, 0.28)

const MA_FONT := preload("res://assets/ui/fonts/MaShanZheng-Regular.ttf")
const SERIF_FONT := preload("res://assets/ui/fonts/NotoSerifSC[wght].ttf")
const SLIDER_GRABBER_ICON := preload("res://assets/ui/icons/slider_grabber.svg")
const SLIDER_GRABBER_HIGHLIGHT_ICON := preload("res://assets/ui/icons/slider_grabber_hl.svg")

var _root_row_path := "PaperRoot/RootRow"
var _content_column_path := ""
var _audio_page_path := ""
var _shortcut_page_path := ""
var _language_page_path := ""
var _audio_settings_path := ""
var _shortcut_settings_path := ""
var _language_settings_path := ""
var _root: Control
var _backdrop: CanvasItem
var _frame: Control
var _poem_waterfall: Node
var _current_tween: Tween
var _page_tween: Tween
var _shortcut_focus_path := ""
var _active_tab_name := "AudioVisual"
var _visible_state := false


func _ready() -> void:
	_root = get_parent()
	_backdrop = _root.get_node_or_null("Backdrop")
	if _backdrop == null:
		# 新版设置卷已移除独立 Backdrop，直接让根节点承担淡入淡出占位，避免路径漂移时报错。
		_backdrop = _root

	_frame = _root.get_node_or_null("PaperRoot")
	if _frame == null:
		_frame = _root.get_node_or_null("TextureRect/PaperRoot")
	if _frame == null:
		# 若未来布局再次收口，至少保留根节点作为动画承载，避免空节点调用导致整卷失效。
		_frame = _root

	_resolve_layout_paths()

	_poem_waterfall = _root.get_node_or_null("PaperRoot/PoemWaterfall")
	if _poem_waterfall == null:
		_poem_waterfall = _root.get_node_or_null("TextureRect/PaperRoot/PoemWaterfall")

	apply_theme_styles()
	reset_state()
	_visible_state = _root.visible
	_update_poem_waterfall_state(_visible_state)

func _resolve_layout_paths() -> void:
	# 根据当前场景树解析路径，兼容 PaperRoot 被包裹到 TextureRect 的版本。
	var root_row := "PaperRoot/RootRow"
	if not _root.has_node(root_row) and _root.has_node("TextureRect/PaperRoot/RootRow"):
		root_row = "TextureRect/PaperRoot/RootRow"
	_root_row_path = root_row
	_content_column_path = _root_row_path + "/ContentColumn"
	_audio_page_path = _content_column_path + "/ContentScroll/ContentPages/AudioPage"
	_shortcut_page_path = _content_column_path + "/ContentScroll/ContentPages/ShortcutPage"
	_language_page_path = _content_column_path + "/ContentScroll/ContentPages/LanguagePage"
	_audio_settings_path = _audio_page_path + "/AudioSettings"
	_shortcut_settings_path = _shortcut_page_path + "/ShortcutSettings"
	_language_settings_path = _language_page_path + "/LanguageSettings"

func _process(_delta: float) -> void:
	if _visible_state == _root.visible:
		return

	_visible_state = _root.visible
	_update_poem_waterfall_state(_visible_state)
	if not _visible_state:
		clear_shortcut_focus()


func apply_theme_styles() -> void:
	_apply_root_fonts()
	_apply_frame_style()
	_apply_header_style()
	_apply_hint_style()
	_apply_tab_styles()
	_apply_row_label_styles()
	_apply_field_styles()
	_apply_slider_styles()
	_apply_choice_button_styles()
	_apply_shortcut_button_styles()
	_apply_shortcut_visual_card_styles()
	_apply_summary_card_styles()
	_apply_footer_styles()
	_apply_fullscreen_seal_style(false)
	apply_tab_button_state(_active_tab_name)


func play_open() -> void:
	_kill_tween(_current_tween)
	_backdrop.modulate.a = 0.0
	_frame.modulate.a = 0.0
	_frame.scale = Vector2(0.985, 0.985)
	_current_tween = create_tween()
	_current_tween.set_parallel(true)
	_current_tween.tween_property(_backdrop, "modulate:a", 1.0, 0.22)
	_current_tween.tween_property(_frame, "modulate:a", 1.0, 0.28)
	_current_tween.tween_property(_frame, "scale", Vector2.ONE, 0.24).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	_update_poem_waterfall_state(true)


func play_tab_switch(page_path: String) -> void:
	if not _root.has_node(page_path):
		return

	var page: Control = _root.get_node(page_path)
	_kill_tween(_page_tween)
	page.modulate.a = 0.0
	page.position.x += 18.0
	_page_tween = create_tween().set_parallel(true)
	_page_tween.tween_property(page, "modulate:a", 1.0, 0.26)
	_page_tween.tween_property(page, "position:x", page.position.x - 18.0, 0.26).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)


func apply_tab_button_state(tab_name: String) -> void:
	_active_tab_name = tab_name
	var tab_map := {
		"AudioVisual": _root.get_node_or_null("%s/NavColumn/TabAudioVisual" % _root_row_path),
		"Shortcuts": _root.get_node_or_null("%s/NavColumn/TabShortcuts" % _root_row_path),
		"Language": _root.get_node_or_null("%s/NavColumn/TabLanguage" % _root_row_path)
	}

	for key in tab_map.keys():
		var button: Button = tab_map[key]
		if button == null:
			# 路径缺失时跳过，避免主题刷新报错。
			continue
		var tab_size := button.size
		if tab_size == Vector2.ZERO:
			tab_size = button.custom_minimum_size
		# 以中心点缩放，避免选中放大后错位。
		button.pivot_offset = tab_size * 0.5
		var active: bool = String(key) == tab_name
		button.add_theme_stylebox_override("normal", _create_transparent_style())
		button.add_theme_stylebox_override("hover", _create_transparent_style())
		button.add_theme_stylebox_override("pressed", _create_transparent_style())
		button.add_theme_stylebox_override("focus", _create_transparent_style())
		button.add_theme_color_override("font_color", PAPER_MAIN if active else INK_MAIN)
		button.add_theme_color_override("font_hover_color", PAPER_MAIN if active else INK_MAIN)
		button.add_theme_color_override("font_pressed_color", PAPER_MAIN if active else INK_MAIN)
		# 玉简切换时同步做轻微放大，贴近参考图里“浮起一枚签”的观感。
		button.scale = Vector2(1.05, 1.05) if active else Vector2.ONE
		_sync_jade_tab_texture(button, active)


func pulse_shortcut(button_path: String) -> void:
	_shortcut_focus_path = button_path
	_apply_shortcut_button_styles()
	if not _root.has_node(button_path):
		return

	var button: Control = _root.get_node(button_path)
	button.scale = Vector2.ONE
	_kill_tween(_current_tween)
	_current_tween = create_tween().set_parallel(true)
	_current_tween.tween_property(button, "scale", Vector2(1.02, 1.02), 0.12).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	_current_tween.tween_property(button, "modulate:a", 1.0, 0.12)


func clear_shortcut_focus() -> void:
	_shortcut_focus_path = ""
	_apply_shortcut_button_styles()
	for button_path in _get_shortcut_button_paths():
		if _root.has_node(button_path):
			var button: Control = _root.get_node(button_path)
			button.scale = Vector2.ONE
			button.modulate = Color(1, 1, 1, 1)


func sync_fullscreen_seal(is_pressed: bool) -> void:
	_apply_fullscreen_seal_style(is_pressed)


func sync_toggle_seal(button_path: String, is_pressed: bool) -> void:
	if not _root.has_node(button_path):
		return
	_apply_seal_style(_root.get_node(button_path), is_pressed)


func sync_choice_button(button_path: String, is_selected: bool) -> void:
	if not _root.has_node(button_path):
		return
	var button: Button = _root.get_node(button_path)
	button.flat = true
	button.add_theme_stylebox_override("normal", _create_choice_button_style(is_selected))
	button.add_theme_stylebox_override("hover", _create_choice_button_style(is_selected))
	button.add_theme_stylebox_override("pressed", _create_choice_button_style(is_selected))
	button.add_theme_stylebox_override("focus", _create_choice_button_style(true))
	button.add_theme_color_override("font_color", SEAL_RED if is_selected else Color(0.55, 0.55, 0.55, 0.65))
	button.add_theme_color_override("font_hover_color", SEAL_RED if is_selected else INK_MAIN)
	button.add_theme_color_override("font_pressed_color", SEAL_RED)


func reset_state() -> void:
	_kill_tween(_current_tween)
	_kill_tween(_page_tween)
	_backdrop.modulate.a = 1.0
	_frame.modulate.a = 1.0
	_frame.scale = Vector2.ONE
	clear_shortcut_focus()
	_update_poem_waterfall_state(_root.visible)


func _apply_root_fonts() -> void:
	for label_path in [
		"%s/HeaderRow/HeadingColumn/TitleLabel" % _content_column_path,
		"%s/HeaderRow/HeadingColumn/SubtitleLabel" % _content_column_path,
		"%s/HintLabel" % _content_column_path,
		"%s/ShortcutIntro" % _shortcut_page_path,
		"%s/LanguageLead" % _language_page_path,
		"%s/AudioSettings/VolumeRow/LabelBox/VolumeCn" % _audio_page_path,
		"%s/AudioSettings/VolumeRow/LabelBox/VolumeEn" % _audio_page_path,
		"%s/AudioSettings/BgmRow/LabelBox/BgmCn" % _audio_page_path,
		"%s/AudioSettings/BgmRow/LabelBox/BgmEn" % _audio_page_path,
		"%s/AudioSettings/SfxRow/LabelBox/SfxCn" % _audio_page_path,
		"%s/AudioSettings/SfxRow/LabelBox/SfxEn" % _audio_page_path,
		"%s/AudioSettings/FullscreenRow/LabelBox/FullscreenCn" % _audio_page_path,
		"%s/AudioSettings/FullscreenRow/LabelBox/FullscreenEn" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/LabelBox/ResolutionCn" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/LabelBox/ResolutionEn" % _audio_page_path,
		"%s/AudioSettings/QualityRow/LabelBox/QualityCn" % _audio_page_path,
		"%s/AudioSettings/QualityRow/LabelBox/QualityEn" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/LabelBox/ZoomCn" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/LabelBox/ZoomEn" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySettingsCard/CardMargin/CardColumn/SummarySettingsLabel" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummaryWarehouseCard/CardMargin/CardColumn/SummaryWarehouseLabel" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySpeedCard/CardMargin/CardColumn/SummarySpeedLabel" % _audio_page_path,
		"%s/LanguageSettings/LanguageRow/LanguageLabel" % _language_page_path,
		"%s/LanguageSettings/AutoSaveRow/LabelBox/AutoSaveCn" % _language_page_path,
		"%s/LanguageSettings/AutoSaveRow/LabelBox/AutoSaveEn" % _language_page_path,
		"%s/LanguageSettings/AutoSaveIntervalRow/LabelBox/AutoSaveIntervalCn" % _language_page_path,
		"%s/LanguageSettings/AutoSaveIntervalRow/LabelBox/AutoSaveIntervalEn" % _language_page_path,
		"%s/LanguageSettings/DamageTextRow/LabelBox/DamageTextCn" % _language_page_path,
		"%s/LanguageSettings/DamageTextRow/LabelBox/DamageTextEn" % _language_page_path,
		"%s/LanguageSettings/BloodGoreRow/LabelBox/BloodGoreCn" % _language_page_path,
		"%s/LanguageSettings/BloodGoreRow/LabelBox/BloodGoreEn" % _language_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementTitle" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiTitle" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveUpCard/Row/MoveUpLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveDownCard/Row/MoveDownLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveLeftCard/Row/MoveLeftLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleSpeedCard/Row/ToggleSpeedLabelVisual" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleExplorationCard/Row/ToggleExplorationLabelVisual" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/CastSpellCard/Row/CastSpellLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenWarehouseCard/Row/OpenWarehouseVisualLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/QuestCard/Row/QuestLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenSettingsCard/Row/OpenSettingsVisualLabel" % _shortcut_page_path,
		"%s/OpenSettingsKeyRow/OpenSettingsKeyLabel" % _shortcut_settings_path,
		"%s/OpenWarehouseKeyRow/OpenWarehouseKeyLabel" % _shortcut_settings_path,
		"%s/ToggleExplorationKeyRow/ToggleExplorationKeyLabel" % _shortcut_settings_path,
		"%s/ToggleSpeedKeyRow/ToggleSpeedKeyLabel" % _shortcut_settings_path,
		"%s/QuickSaveKeyRow/QuickSaveKeyLabel" % _shortcut_settings_path,
		"%s/QuickLoadKeyRow/QuickLoadKeyLabel" % _shortcut_settings_path,
		"%s/QuickResetKeyRow/QuickResetKeyLabel" % _shortcut_settings_path
	]:
		var label: Label = _root.get_node(label_path)
		label.add_theme_font_override("font", SERIF_FONT)

	for label_path in [
		"%s/HeaderRow/HeadingColumn/TitleLabel" % _content_column_path,
		"%s/HeaderRow/HeadingColumn/SubtitleLabel" % _content_column_path,
		"%s/AudioSettings/VolumeRow/VolumeValue" % _audio_page_path,
		"%s/AudioSettings/BgmRow/BgmValue" % _audio_page_path,
		"%s/AudioSettings/SfxRow/SfxValue" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/ZoomValue" % _audio_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveUpCard/Row/MoveUpKeyText" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveDownCard/Row/MoveDownKeyText" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveLeftCard/Row/MoveLeftKeyText" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/CastSpellCard/Row/CastSpellKeyText" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/QuestCard/Row/QuestKeyText" % _shortcut_page_path,
		"%s/ShortcutPreviewGrid/SummarySettingsCard/CardMargin/CardColumn/SummarySettingsValue" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummaryWarehouseCard/CardMargin/CardColumn/SummaryWarehouseValue" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySpeedCard/CardMargin/CardColumn/SummarySpeedValue" % _audio_page_path
	]:
		var label: Label = _root.get_node(label_path)
		label.add_theme_font_override("font", MA_FONT)

	for button_path in [
		"%s/NavColumn/TabAudioVisual" % _root_row_path,
		"%s/NavColumn/TabShortcuts" % _root_row_path,
		"%s/NavColumn/TabLanguage" % _root_row_path,
		"%s/FooterRow/ApplyButton" % _content_column_path,
		"%s/AudioSettings/FullscreenRow/FullscreenSealButton" % _audio_page_path,
		"%s/LanguageSettings/AutoSaveRow/AutoSaveSealButton" % _language_page_path,
		"%s/LanguageSettings/DamageTextRow/DamageTextSealButton" % _language_page_path,
		"%s/LanguageSettings/BloodGoreRow/BloodGoreSealButton" % _language_page_path
	]:
		var button: Button = _root.get_node(button_path)
		button.add_theme_font_override("font", MA_FONT)

	for button_path in _get_shortcut_button_paths():
		var button: Button = _root.get_node(button_path)
		button.add_theme_font_override("font", MA_FONT)

	var cancel_button: Button = _root.get_node("%s/FooterRow/CancelButton" % _content_column_path)
	cancel_button.add_theme_font_override("font", SERIF_FONT)
	var close_button: Button = _root.get_node("%s/HeaderRow/CloseButton" % _content_column_path)
	close_button.add_theme_font_override("font", SERIF_FONT)


func _apply_frame_style() -> void:
	_frame.add_theme_stylebox_override("panel", _create_frame_style())


func _apply_header_style() -> void:
	var title_label: Label = _root.get_node("%s/HeaderRow/HeadingColumn/TitleLabel" % _content_column_path)
	title_label.add_theme_font_size_override("font_size", 60)
	title_label.add_theme_color_override("font_color", INK_MAIN)
	if title_label.label_settings == null:
		title_label.label_settings = LabelSettings.new()
	title_label.label_settings.shadow_color = Color(0.45, 0.37, 0.28, 0.18)
	title_label.label_settings.shadow_size = 8
	title_label.label_settings.shadow_offset = Vector2(0, 4)

	var subtitle_label: Label = _root.get_node("%s/HeaderRow/HeadingColumn/SubtitleLabel" % _content_column_path)
	subtitle_label.add_theme_font_size_override("font_size", 13)
	subtitle_label.add_theme_color_override("font_color", Color(0.63, 0.54, 0.40, 0.75))
	subtitle_label.add_theme_constant_override("line_spacing", 4)

	var close_button: Button = _root.get_node("%s/HeaderRow/CloseButton" % _content_column_path)
	close_button.flat = true
	close_button.add_theme_font_size_override("font_size", 22)
	close_button.add_theme_stylebox_override("normal", _create_transparent_style())
	close_button.add_theme_stylebox_override("hover", _create_transparent_style())
	close_button.add_theme_stylebox_override("pressed", _create_transparent_style())
	close_button.add_theme_color_override("font_color", INK_MUTED)
	close_button.add_theme_color_override("font_hover_color", SEAL_RED)
	close_button.add_theme_color_override("font_pressed_color", SEAL_RED)


func _apply_hint_style() -> void:
	var hint_label: Label = _root.get_node("%s/HintLabel" % _content_column_path)
	hint_label.add_theme_font_size_override("font_size", 14)
	hint_label.add_theme_color_override("font_color", INK_MUTED)


func _apply_tab_styles() -> void:
	for button_path in [
		"%s/NavColumn/TabAudioVisual" % _root_row_path,
		"%s/NavColumn/TabShortcuts" % _root_row_path,
		"%s/NavColumn/TabLanguage" % _root_row_path
	]:
		var button: Button = _root.get_node(button_path)
		button.flat = true
		button.add_theme_font_size_override("font_size", 20)
		button.add_theme_stylebox_override("normal", _create_transparent_style())
		button.add_theme_stylebox_override("hover", _create_transparent_style())
		button.add_theme_stylebox_override("pressed", _create_transparent_style())
		button.add_theme_stylebox_override("focus", _create_transparent_style())
		button.add_theme_color_override("font_color", INK_MAIN)
		button.add_theme_color_override("font_hover_color", INK_MAIN)
		button.add_theme_color_override("font_pressed_color", INK_MAIN)


func _apply_row_label_styles() -> void:
	for label_path in [
		"%s/AudioSettings/VolumeRow/LabelBox/VolumeCn" % _audio_page_path,
		"%s/AudioSettings/BgmRow/LabelBox/BgmCn" % _audio_page_path,
		"%s/AudioSettings/SfxRow/LabelBox/SfxCn" % _audio_page_path,
		"%s/AudioSettings/FullscreenRow/LabelBox/FullscreenCn" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/LabelBox/ResolutionCn" % _audio_page_path,
		"%s/AudioSettings/QualityRow/LabelBox/QualityCn" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/LabelBox/ZoomCn" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySettingsCard/CardMargin/CardColumn/SummarySettingsLabel" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummaryWarehouseCard/CardMargin/CardColumn/SummaryWarehouseLabel" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySpeedCard/CardMargin/CardColumn/SummarySpeedLabel" % _audio_page_path,
		"%s/LanguageSettings/LanguageRow/LanguageLabel" % _language_page_path,
		"%s/LanguageSettings/AutoSaveRow/LabelBox/AutoSaveCn" % _language_page_path,
		"%s/LanguageSettings/AutoSaveIntervalRow/LabelBox/AutoSaveIntervalCn" % _language_page_path,
		"%s/LanguageSettings/DamageTextRow/LabelBox/DamageTextCn" % _language_page_path,
		"%s/LanguageSettings/BloodGoreRow/LabelBox/BloodGoreCn" % _language_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementTitle" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiTitle" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveUpCard/Row/MoveUpLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveDownCard/Row/MoveDownLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveLeftCard/Row/MoveLeftLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleSpeedCard/Row/ToggleSpeedLabelVisual" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleExplorationCard/Row/ToggleExplorationLabelVisual" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/CastSpellCard/Row/CastSpellLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenWarehouseCard/Row/OpenWarehouseVisualLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/QuestCard/Row/QuestLabel" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenSettingsCard/Row/OpenSettingsVisualLabel" % _shortcut_page_path,
		"%s/OpenSettingsKeyRow/OpenSettingsKeyLabel" % _shortcut_settings_path,
		"%s/OpenWarehouseKeyRow/OpenWarehouseKeyLabel" % _shortcut_settings_path,
		"%s/ToggleExplorationKeyRow/ToggleExplorationKeyLabel" % _shortcut_settings_path,
		"%s/ToggleSpeedKeyRow/ToggleSpeedKeyLabel" % _shortcut_settings_path,
		"%s/QuickSaveKeyRow/QuickSaveKeyLabel" % _shortcut_settings_path,
		"%s/QuickLoadKeyRow/QuickLoadKeyLabel" % _shortcut_settings_path,
		"%s/QuickResetKeyRow/QuickResetKeyLabel" % _shortcut_settings_path
	]:
		var label: Label = _root.get_node(label_path)
		label.add_theme_font_size_override("font_size", 22 if label_path.find("Summary") >= 0 else 17)
		label.add_theme_color_override("font_color", INK_MAIN)

	for label_path in [
		"%s/AudioSettings/VolumeRow/LabelBox/VolumeEn" % _audio_page_path,
		"%s/AudioSettings/BgmRow/LabelBox/BgmEn" % _audio_page_path,
		"%s/AudioSettings/SfxRow/LabelBox/SfxEn" % _audio_page_path,
		"%s/AudioSettings/FullscreenRow/LabelBox/FullscreenEn" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/LabelBox/ResolutionEn" % _audio_page_path,
		"%s/AudioSettings/QualityRow/LabelBox/QualityEn" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/LabelBox/ZoomEn" % _audio_page_path,
		"%s/LanguageSettings/AutoSaveRow/LabelBox/AutoSaveEn" % _language_page_path,
		"%s/LanguageSettings/DamageTextRow/LabelBox/DamageTextEn" % _language_page_path,
		"%s/LanguageSettings/BloodGoreRow/LabelBox/BloodGoreEn" % _language_page_path
	]:
		var label: Label = _root.get_node(label_path)
		label.add_theme_font_size_override("font_size", 12)
		label.add_theme_color_override("font_color", Color(0.62, 0.53, 0.39, 0.82))

	for label_path in [
		"%s/ShortcutIntro" % _shortcut_page_path,
		"%s/LanguageLead" % _language_page_path
	]:
		var label: Label = _root.get_node(label_path)
		label.add_theme_font_size_override("font_size", 15)
		label.add_theme_color_override("font_color", INK_MUTED)

	for label_path in [
		"%s/AudioSettings/VolumeRow/VolumeValue" % _audio_page_path,
		"%s/AudioSettings/BgmRow/BgmValue" % _audio_page_path,
		"%s/AudioSettings/SfxRow/SfxValue" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/ZoomValue" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySettingsCard/CardMargin/CardColumn/SummarySettingsValue" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummaryWarehouseCard/CardMargin/CardColumn/SummaryWarehouseValue" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySpeedCard/CardMargin/CardColumn/SummarySpeedValue" % _audio_page_path
	]:
		var label: Label = _root.get_node(label_path)
		label.add_theme_font_size_override("font_size", 34 if label_path.find("Summary") >= 0 else 26)
		label.add_theme_color_override("font_color", SEAL_RED)


func _apply_field_styles() -> void:
	for button_path in [
		"%s/AudioSettings/ResolutionRow/ResolutionOption" % _audio_page_path,
		"%s/AudioSettings/QualityRow/QualityOption" % _audio_page_path,
		"%s/LanguageSettings/LanguageRow/LanguageOption" % _language_page_path,
		"%s/LanguageSettings/AutoSaveIntervalRow/AutoSaveIntervalOption" % _language_page_path
	]:
		var button: BaseButton = _root.get_node(button_path)
		button.add_theme_stylebox_override("normal", _create_field_style(false))
		button.add_theme_stylebox_override("hover", _create_field_style(true))
		button.add_theme_stylebox_override("pressed", _create_field_style(true))
		button.add_theme_stylebox_override("focus", _create_field_style(true))
		button.add_theme_font_size_override("font_size", 18)
		button.add_theme_color_override("font_color", INK_MAIN)
		button.add_theme_color_override("font_hover_color", INK_MAIN)
		button.add_theme_color_override("font_pressed_color", INK_MAIN)


func _apply_choice_button_styles() -> void:
	for button_path in [
		"%s/AudioSettings/QualityRow/QualityChoices/QualityLowButton" % _audio_page_path,
		"%s/AudioSettings/QualityRow/QualityChoices/QualityMediumButton" % _audio_page_path,
		"%s/AudioSettings/QualityRow/QualityChoices/QualityHighButton" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/ResolutionChoices/Resolution1080Button" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/ResolutionChoices/Resolution2KButton" % _audio_page_path,
		"%s/AudioSettings/ResolutionRow/ResolutionChoices/Resolution4KButton" % _audio_page_path,
		"%s/LanguageSettings/LanguageRow/LanguageChoices/LanguageZhButton" % _language_page_path,
		"%s/LanguageSettings/LanguageRow/LanguageChoices/LanguageEnButton" % _language_page_path
	]:
		if not _root.has_node(button_path):
			continue
		var button: Button = _root.get_node(button_path)
		button.flat = true
		button.add_theme_font_override("font", MA_FONT)
		button.add_theme_font_size_override("font_size", 18 if button_path.find("Language") >= 0 else 22)
		sync_choice_button(button_path, false)


func _apply_slider_styles() -> void:
	for slider_path in [
		"%s/AudioSettings/VolumeRow/VolumeSlider" % _audio_page_path,
		"%s/AudioSettings/BgmRow/BgmSlider" % _audio_page_path,
		"%s/AudioSettings/SfxRow/SfxSlider" % _audio_page_path,
		"%s/AudioSettings/ZoomRow/ZoomSlider" % _audio_page_path
	]:
		var slider: Range = _root.get_node(slider_path)
		slider.add_theme_stylebox_override("slider", _create_slider_track_style())
		slider.add_theme_stylebox_override("grabber_area", _create_transparent_style())
		slider.add_theme_stylebox_override("grabber_area_highlight", _create_transparent_style())
		slider.add_theme_icon_override("grabber", SLIDER_GRABBER_ICON)
		slider.add_theme_icon_override("grabber_highlight", SLIDER_GRABBER_HIGHLIGHT_ICON)
		slider.add_theme_icon_override("grabber_disabled", SLIDER_GRABBER_ICON)


func _apply_shortcut_button_styles() -> void:
	for button_path in _get_shortcut_button_paths():
		var button: Button = _root.get_node(button_path)
		var focused := button_path == _shortcut_focus_path
		button.flat = true
		button.alignment = HORIZONTAL_ALIGNMENT_RIGHT
		button.add_theme_font_size_override("font_size", 26)
		button.add_theme_stylebox_override("normal", _create_shortcut_button_style(focused, false))
		button.add_theme_stylebox_override("hover", _create_shortcut_button_style(focused, true))
		button.add_theme_stylebox_override("pressed", _create_shortcut_button_style(focused, true))
		button.add_theme_stylebox_override("focus", _create_shortcut_button_style(true, true))
		button.add_theme_color_override("font_color", SEAL_RED)
		button.add_theme_color_override("font_hover_color", SEAL_RED)
		button.add_theme_color_override("font_pressed_color", SEAL_RED)


func _apply_shortcut_visual_card_styles() -> void:
	for card_path in [
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveUpCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveDownCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/MoveLeftCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleSpeedCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleExplorationCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/CastSpellCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenWarehouseCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/QuestCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenSettingsCard" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/QuickResetCard" % _shortcut_page_path
	]:
		if not _root.has_node(card_path):
			continue
		var card: PanelContainer = _root.get_node(card_path)
		card.add_theme_stylebox_override("panel", _create_shortcut_card_style())


func _apply_summary_card_styles() -> void:
	for card_path in [
		"%s/ShortcutPreviewGrid/SummarySettingsCard" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummaryWarehouseCard" % _audio_page_path,
		"%s/ShortcutPreviewGrid/SummarySpeedCard" % _audio_page_path
	]:
		var card: PanelContainer = _root.get_node(card_path)
		card.add_theme_stylebox_override("panel", _create_summary_card_style())


func _apply_footer_styles() -> void:
	var cancel_button: Button = _root.get_node("%s/FooterRow/CancelButton" % _content_column_path)
	cancel_button.flat = true
	cancel_button.add_theme_font_size_override("font_size", 18)
	cancel_button.add_theme_stylebox_override("normal", _create_transparent_style())
	cancel_button.add_theme_stylebox_override("hover", _create_transparent_style())
	cancel_button.add_theme_stylebox_override("pressed", _create_transparent_style())
	cancel_button.add_theme_color_override("font_color", Color(0.55, 0.55, 0.55, 1.0))
	cancel_button.add_theme_color_override("font_hover_color", INK_MAIN)
	cancel_button.add_theme_color_override("font_pressed_color", INK_MAIN)
	_sync_seal_button_texture(cancel_button, Color(0.55, 0.55, 0.55, 0.9))

	var apply_button: Button = _root.get_node("%s/FooterRow/ApplyButton" % _content_column_path)
	apply_button.add_theme_font_size_override("font_size", 28)


func _apply_fullscreen_seal_style(is_pressed: bool) -> void:
	var button: Button = _root.get_node("%s/AudioSettings/FullscreenRow/FullscreenSealButton" % _audio_page_path)
	_apply_seal_style(button, is_pressed)


func _apply_seal_style(button: Button, is_pressed: bool) -> void:
	button.text = ""
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_CENTER
	button.add_theme_font_size_override("font_size", 28)
	button.add_theme_stylebox_override("normal", _create_seal_style(is_pressed, false))
	button.add_theme_stylebox_override("hover", _create_seal_style(is_pressed, true))
	button.add_theme_stylebox_override("pressed", _create_seal_style(is_pressed, true))
	button.add_theme_stylebox_override("focus", _create_seal_style(true, true))
	button.add_theme_color_override("font_color", SEAL_RED)
	button.add_theme_color_override("font_hover_color", SEAL_RED)
	button.add_theme_color_override("font_pressed_color", SEAL_RED)
	var seal_texture: TextureRect = button.get_node_or_null("SealCharacter")
	if seal_texture != null:
		# 朱砂“敕”字走真实 SVG 显隐，不再依赖按钮文本伪装。
		seal_texture.visible = is_pressed
		seal_texture.modulate = Color(1, 1, 1, 0.92 if is_pressed else 0.0)
		seal_texture.scale = Vector2(0.95, 0.95) if is_pressed else Vector2(0.88, 0.88)


func _get_shortcut_button_paths() -> Array[String]:
	return [
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenSettingsCard/Row/OpenSettingsKeyOption" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/OpenWarehouseCard/Row/OpenWarehouseKeyOption" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleExplorationCard/Row/ToggleExplorationKeyOption" % _shortcut_page_path,
		"%s/ShortcutVisualGrid/MovementSection/MovementGrid/ToggleSpeedCard/Row/ToggleSpeedKeyOption" % _shortcut_page_path,
		"%s/QuickSaveKeyRow/QuickSaveKeyOption" % _shortcut_settings_path,
		"%s/QuickLoadKeyRow/QuickLoadKeyOption" % _shortcut_settings_path,
		"%s/ShortcutVisualGrid/UiSection/UiGrid/QuickResetCard/Row/QuickResetKeyOption" % _shortcut_page_path
	]


func _update_poem_waterfall_state(active: bool) -> void:
	if _poem_waterfall == null:
		return
	if _poem_waterfall.has_method("SetActive"):
		_poem_waterfall.call("SetActive", active)


func _create_frame_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_MAIN.r, PAPER_MAIN.g, PAPER_MAIN.b, 0.0)
	style.border_width_left = 0
	style.border_width_top = 0
	style.border_width_right = 0
	style.border_width_bottom = 0
	style.border_color = Color(0.56, 0.45, 0.29, 0.0)
	style.corner_radius_top_left = 0
	style.corner_radius_top_right = 0
	style.corner_radius_bottom_right = 0
	style.corner_radius_bottom_left = 0
	style.shadow_size = 0
	style.shadow_color = Color(0.15, 0.12, 0.10, 0.0)
	return style


func _create_tab_style(active: bool, hovered: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = SEAL_RED if active else (JADE_LIGHT if hovered else JADE_DARK)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.55, 0.45, 0.29, 0.26)
	style.corner_radius_top_left = 28
	style.corner_radius_top_right = 28
	style.corner_radius_bottom_right = 28
	style.corner_radius_bottom_left = 28
	style.shadow_size = 10 if active or hovered else 6
	style.shadow_color = Color(0.15, 0.12, 0.10, 0.08)
	return style


func _create_field_style(focused: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_SOFT.r, PAPER_SOFT.g, PAPER_SOFT.b, 0.82)
	style.border_width_bottom = 1
	style.border_color = SEAL_RED if focused else FIELD_LINE
	style.corner_radius_top_left = 4
	style.corner_radius_top_right = 4
	style.content_margin_left = 18
	style.content_margin_top = 14
	style.content_margin_right = 18
	style.content_margin_bottom = 14
	return style


func _create_slider_track_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_bottom = 1
	style.border_color = GOLD_LINE
	style.content_margin_top = 9
	style.content_margin_bottom = 9
	return style


func _create_shortcut_button_style(focused: bool, hovered: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = SEAL_RED_SOFT if focused else Color(PAPER_SOFT.r, PAPER_SOFT.g, PAPER_SOFT.b, 0.42 if hovered else 0.30)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = SEAL_RED if focused else Color(0.55, 0.45, 0.29, 0.18 if hovered else 0.12)
	style.content_margin_left = 18
	style.content_margin_top = 14
	style.content_margin_right = 18
	style.content_margin_bottom = 14
	return style


func _create_choice_button_style(selected: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	style.border_width_bottom = 1 if selected else 0
	style.border_color = SEAL_RED
	style.content_margin_left = 6
	style.content_margin_right = 6
	style.content_margin_bottom = 6
	return style


func _create_shortcut_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_SOFT.r, PAPER_SOFT.g, PAPER_SOFT.b, 0.18)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.55, 0.45, 0.29, 0.14)
	style.content_margin_left = 18
	style.content_margin_top = 18
	style.content_margin_right = 18
	style.content_margin_bottom = 18
	return style


func _create_summary_card_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(PAPER_SOFT.r, PAPER_SOFT.g, PAPER_SOFT.b, 0.42)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.55, 0.45, 0.29, 0.14)
	style.content_margin_left = 12
	style.content_margin_top = 12
	style.content_margin_right = 12
	style.content_margin_bottom = 12
	return style


func _sync_jade_tab_texture(button: Button, active: bool) -> void:
	var texture_rect: TextureRect = button.get_node_or_null("JadeTexture")
	if texture_rect == null:
		return
	# 选中签偏朱砂，未选中签保留清玉底色，形成“纯净玉简 + 朱砂批红”的层次。
	texture_rect.modulate = Color(0.78, 0.30, 0.27, 0.98) if active else (Color(0.99, 1.0, 0.99, 0.98) if button.is_hovered() else Color(0.95, 0.98, 0.96, 0.95))


func _sync_seal_button_texture(button: Button, tint: Color) -> void:
	var texture_rect: TextureRect = button.get_node_or_null("SealShape")
	if texture_rect == null:
		return
	# 卷尾按钮统一走 SVG 印章边框，颜色仅由外层状态轻调。
	texture_rect.modulate = tint


func _create_seal_style(active: bool, hovered: bool) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = SEAL_RED_SOFT if active else Color(1, 1, 1, 0)
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = Color(0.77, 0.18, 0.18, 1.0 if active or hovered else 0.75)
	style.corner_radius_top_left = 31
	style.corner_radius_top_right = 31
	style.corner_radius_bottom_right = 31
	style.corner_radius_bottom_left = 31
	style.shadow_size = 8 if active or hovered else 0
	style.shadow_color = Color(0.70, 0.13, 0.13, 0.14)
	return style


func _create_transparent_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0, 0, 0, 0)
	return style


func _kill_tween(tween: Tween) -> void:
	if tween != null and tween.is_running():
		tween.kill()
