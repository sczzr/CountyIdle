namespace CountyIdle.Models;

public class ClientSettings
{
    public const string DefaultLanguage = "zh_CN";
    public const int DefaultResolutionWidth = 1600;
    public const int DefaultResolutionHeight = 900;
    public const float DefaultFontScale = 1.0f;
    public const float DefaultMasterVolume = 0.8f;

    public string Language { get; set; } = DefaultLanguage;
    public int ResolutionWidth { get; set; } = DefaultResolutionWidth;
    public int ResolutionHeight { get; set; } = DefaultResolutionHeight;
    public float FontScale { get; set; } = DefaultFontScale;
    public float MasterVolume { get; set; } = DefaultMasterVolume;

    public ClientSettings Clone()
    {
        return (ClientSettings)MemberwiseClone();
    }
}
