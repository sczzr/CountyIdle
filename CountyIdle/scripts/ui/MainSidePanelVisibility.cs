using Godot;
using CountyIdle.Models;

namespace CountyIdle;

public partial class Main
{
	private Control? _jobsPanelRoot;
	private Control? _eventLogPanelRoot;
	private BaseButton? _eventLogToggleButton;
	private bool _isEventLogPanelExpanded;
	private Tween? _jobsPanelTween;

	private void BindSidePanelVisibilityNodes()
	{
		_jobsPanelRoot = GetNodeOrNull<Control>(LeftPanelPath);
		_eventLogPanelRoot = GetNodeOrNull<Control>(RightPanelPath);
		_eventLogToggleButton = GetNodeOrNull<BaseButton>($"{BottomBarPath}/RightPanelToggleButton");

		if (_eventLogToggleButton != null)
		{
			_eventLogToggleButton.Pressed += ToggleEventLogPanelVisibility;
		}

		// 当前院域机宜卷改为开局常显，进入主界面即可看到全部地块说明与建造入口。
		ApplyJobPanelVisibility(true);
		ApplyEventLogPanelVisibility(false);
	}

	private void UnbindSidePanelVisibilityNodes()
	{
		if (_eventLogToggleButton != null)
		{
			_eventLogToggleButton.Pressed -= ToggleEventLogPanelVisibility;
		}
	}

	private void ToggleEventLogPanelVisibility()
	{
		ApplyEventLogPanelVisibility(!_isEventLogPanelExpanded);
	}

	private void ApplyJobPanelVisibility(bool isVisible)
	{
		if (_jobsPanelRoot != null)
		{
			// 左侧检视卷改为浮动面板，显隐只影响卷册本体，不再占用中部地图排版宽度。
			_jobsPanelRoot.MouseFilter = isVisible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
			PlayJobPanelVisibilityTween(isVisible);
		}
	}

	private void PlayJobPanelVisibilityTween(bool isVisible)
	{
		if (_jobsPanelRoot == null)
		{
			return;
		}

		if (_jobsPanelTween != null && _jobsPanelTween.IsRunning())
		{
			_jobsPanelTween.Kill();
		}

		if (isVisible)
		{
			_jobsPanelRoot.Visible = true;
			_jobsPanelRoot.Modulate = new Color(1f, 1f, 1f, 0f);
			_jobsPanelRoot.Scale = new Vector2(0.96f, 0.96f);
			_jobsPanelRoot.PivotOffset = _jobsPanelRoot.Size * 0.5f;
			_jobsPanelTween = CreateTween();
			_jobsPanelTween.SetParallel(true);
			_jobsPanelTween.TweenProperty(_jobsPanelRoot, "modulate:a", 1.0f, 0.16f);
			_jobsPanelTween.TweenProperty(_jobsPanelRoot, "scale", Vector2.One, 0.18f);
			return;
		}

		if (!_jobsPanelRoot.Visible)
		{
			return;
		}

		_jobsPanelTween = CreateTween();
		_jobsPanelTween.SetParallel(true);
		_jobsPanelTween.TweenProperty(_jobsPanelRoot, "modulate:a", 0.0f, 0.12f);
		_jobsPanelTween.TweenProperty(_jobsPanelRoot, "scale", new Vector2(0.97f, 0.97f), 0.12f);
		_jobsPanelTween.Finished += () =>
		{
			if (_jobsPanelRoot == null)
			{
				return;
			}

			_jobsPanelRoot.Visible = false;
			_jobsPanelRoot.Modulate = Colors.White;
			_jobsPanelRoot.Scale = Vector2.One;
		};
	}

	private void ApplyEventLogPanelVisibility(bool isVisible)
	{
		_isEventLogPanelExpanded = isVisible;

		if (_eventLogPanelRoot != null)
		{
			_eventLogPanelRoot.Visible = isVisible;
		}


	}

	private void UpdateJobPanelVisibilityForSectSelection(TownMapSelectionSummary summary)
	{
		// 院域机宜卷改为常显，点选地块仅刷新内容，不再切换卷册显隐。
		ApplyJobPanelVisibility(true);
	}

	private void UpdateJobPanelVisibilityForWorldSiteSelection(XianxiaSiteData? site)
	{
		// 世界层同样保持卷册常显，仅根据选中状态刷新内容。
		ApplyJobPanelVisibility(true);
	}
}
