using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle.UI;

public partial class SectNamePanel : PopupPanelBase
{
    private const string RootPath = "Overlay/Center/Frame/RootColumn";
    private const string HeaderPath = RootPath + "/HeaderRow";
    private const string BodyPath = RootPath + "/BodyMargin/BodyColumn";
    private const string EntriesPath = BodyPath + "/EntriesScroll/EntriesColumn";
    private const string FooterPath = RootPath + "/FooterRow";
    private const string HintPath = RootPath + "/HintLabel";

    private sealed class NameEntryBinding
    {
        public NameEntryBinding(SectNameEntry entry, Label label, LineEdit input)
        {
            Entry = entry;
            Label = label;
            Input = input;
        }

        public SectNameEntry Entry { get; }
        public Label Label { get; }
        public LineEdit Input { get; }
    }

    private Label _titleLabel = null!;
    private Button _closeButton = null!;
    private Button _resetButton = null!;
    private Button _applyButton = null!;
    private VBoxContainer _entriesColumn = null!;
    private Label _hintLabel = null!;

    private readonly List<NameEntryBinding> _bindings = new();
    private GameState _state = new();

    public event Action<IReadOnlyDictionary<string, string>>? ApplyRequested;

    public override void _Ready()
    {
        BindUiNodes();
        BuildEntries();
        BindEvents();
        InitializePopupHint(_hintLabel);
        Hide();
    }

    public override void _Process(double delta)
    {
        TickPopupStatus(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!TryHandlePopupClose(@event))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    public void Open(GameState state)
    {
        RefreshState(state);
        OpenPopup();
    }

    public void ClosePanel()
    {
        ClosePopup();
    }

    public void RefreshState(GameState state)
    {
        _state = state.Clone();
        SectNamingRules.EnsureDefaults(_state);
        RefreshEntryTexts();
        RefreshPopupHint();
    }

    protected override string GetPopupHintText()
    {
        if (!string.IsNullOrWhiteSpace(PopupStatusMessage))
        {
            return PopupStatusMessage!;
        }

        return "可在此批注宗门、峰脉与堂口称谓；留空将回落默认名。按 Esc 可收卷。";
    }

    private void BindUiNodes()
    {
        _titleLabel = GetNode<Label>($"{HeaderPath}/TitleLabel");
        _closeButton = GetNode<Button>($"{HeaderPath}/CloseButton");
        _entriesColumn = GetNode<VBoxContainer>(EntriesPath);
        _resetButton = GetNode<Button>($"{FooterPath}/ResetButton");
        _applyButton = GetNode<Button>($"{FooterPath}/ApplyButton");
        _hintLabel = GetNode<Label>(HintPath);
    }

    private void BindEvents()
    {
        _closeButton.Pressed += ClosePopup;
        _resetButton.Pressed += ResetToDefaults;
        _applyButton.Pressed += ApplyChanges;
    }

    private void BuildEntries()
    {
        _bindings.Clear();
        ClearChildren(_entriesColumn);

        string? currentGroup = null;
        foreach (var entry in SectNamingRules.GetEntries())
        {
            if (!string.Equals(currentGroup, entry.Group, StringComparison.Ordinal))
            {
                currentGroup = entry.Group;
                var groupLabel = new Label
                {
                    Text = entry.Group,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart
                };
                groupLabel.AddThemeColorOverride("font_color", new Color(0.30f, 0.26f, 0.22f, 0.92f));
                _entriesColumn.AddChild(groupLabel);
            }

            var row = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var label = new Label
            {
                Text = entry.Label,
                CustomMinimumSize = new Vector2(140, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var input = new LineEdit
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                PlaceholderText = entry.DefaultName
            };
            input.MaxLength = SectNamingRules.MaxNameLength;
            input.ClearButtonEnabled = true;

            row.AddChild(label);
            row.AddChild(input);
            _entriesColumn.AddChild(row);

            _bindings.Add(new NameEntryBinding(entry, label, input));
        }
    }

    private void RefreshEntryTexts()
    {
        var sectName = SectNamingRules.GetName(_state, SectNamingRules.SectNameKey);
        _titleLabel.Text = $"{sectName}·宗门档案";

        foreach (var binding in _bindings)
        {
            binding.Input.Text = SectNamingRules.GetName(_state, binding.Entry.Key);
        }
    }

    private void ResetToDefaults()
    {
        foreach (var binding in _bindings)
        {
            binding.Input.Text = binding.Entry.DefaultName;
        }

        ShowPopupStatusMessage("已恢复默认命名。");
    }

    private void ApplyChanges()
    {
        var updates = new Dictionary<string, string>();
        foreach (var binding in _bindings)
        {
            updates[binding.Entry.Key] = binding.Input.Text;
        }

        ApplyRequested?.Invoke(updates);
        ClosePopup();
    }

    private static void ClearChildren(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }
}
