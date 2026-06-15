namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// App version upgrade information displayed by the application UI layer.
    /// </summary>
    public sealed class StartupUpgradeInfo
    {
        public bool IsForce { get; }

        public string AppDownloadUrl { get; }

        public string UpdateTitle { get; }

        public string UpdateAnnouncement { get; }

        public StartupUpgradeInfo(bool isForce, string appDownloadUrl, string updateTitle, string updateAnnouncement)
        {
            IsForce = isForce;
            AppDownloadUrl = appDownloadUrl ?? string.Empty;
            UpdateTitle = updateTitle ?? string.Empty;
            UpdateAnnouncement = updateAnnouncement ?? string.Empty;
        }
    }
}
