using Godot;

namespace CountyIdle;

public partial class Main
{
    private void UpdateSavePreviewForSlot(string slotKey)
    {
        if (TryCaptureAndSavePreview(slotKey, out var previewMessage))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(previewMessage))
        {
            AppendLog(previewMessage);
        }
    }

    private bool TryCaptureAndSavePreview(string slotKey, out string message)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            message = "存档截图生成失败：槽位无效。";
            return false;
        }

        var viewport = GetViewport();
        if (viewport == null)
        {
            message = "存档截图生成失败：未找到当前视口。";
            return false;
        }

        var image = viewport.GetTexture().GetImage();
        if (image == null || image.GetWidth() <= 0 || image.GetHeight() <= 0)
        {
            message = "存档截图生成失败：当前画面为空。";
            return false;
        }

        image.FlipY();
        return _saveSystem.SavePreview(slotKey, image, out message);
    }
}
