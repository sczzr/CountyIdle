using System.Collections.Generic;
using Godot;

namespace CountyIdle.UI.Figma;

public partial class TimelinePanel : PanelContainer
{
    private RichTextLabel _notificationLabel = null!;
    private RichTextLabel _timelineLabel = null!;

    public override void _Ready()
    {
        _notificationLabel = GetNode<RichTextLabel>("PanelPadding/MainColumn/NotificationSection/NotificationContent");
        _timelineLabel = GetNode<RichTextLabel>("PanelPadding/MainColumn/TimelineSection/TimelineContent");

        SetNotifications(new[]
        {
            "[刚刚] [红] 国格天伙伴：感恩兄弟，您好您其机器自品和物的地，报进...",
            "[2分钟] [黄] 大明旧社：雷暴无声，向往之行..."
        });

        SetTimeline(new[]
        {
            "[01:59] 吊 古地电筑地界，泰志己工",
            "[01:25] 左边占工，民信历史",
            "[00:29] 左 黄丁耳，转级机尘和",
            "[11:03] 王 曲集全，毒主道-李子速工号"
        });
    }

    public void SetNotifications(IReadOnlyList<string> notifications)
    {
        _notificationLabel.Text = string.Join('\n', notifications);
    }

    public void SetTimeline(IReadOnlyList<string> timelineItems)
    {
        _timelineLabel.Text = string.Join('\n', timelineItems);
    }
}
