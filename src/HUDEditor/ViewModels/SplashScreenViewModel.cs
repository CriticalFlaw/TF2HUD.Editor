using HUDEditor.Classes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HUDEditor.ViewModels;

public partial class SplashScreenViewModel : ViewModelBase
{
    private int _imagesDownloaded;
    public int ImagesDownloaded
    {
        get => _imagesDownloaded;
        private set
        {
            _imagesDownloaded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StartupMessage));
        }
    }

    private int _totalImages;
    public int TotalImages
    {
        get => _totalImages;
        private set
        {
            _totalImages = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StartupMessage));
        }
    }

    public string ImageDownloadStatus =>
        TotalImages == 0
            ? Assets.Resources.ui_splash_images
            : string.Format(Assets.Resources.ui_splash_images_download, ImagesDownloaded, TotalImages);

    private string _staticStartupMessage = Assets.Resources.ui_splash_initialize;
    private bool _isDownloadingImages;

    public string StartupMessage =>
        _isDownloadingImages
            ? ImageDownloadStatus
            : _staticStartupMessage;

    public void SetStartupMessage(string message)
    {
        _staticStartupMessage = message;
        OnPropertyChanged(nameof(StartupMessage));
    }

    public void Cancel() => _cts.Cancel();

    private readonly CancellationTokenSource _cts = new();
    public CancellationToken CancellationToken => _cts.Token;

    /// <summary>
    /// Downloads and caches thumbnail and screenshot images for all HUDs.
    /// </summary>
    public async Task DownloadImages(IEnumerable<HUD> hudList, CancellationToken cancellationToken = default)
    {
        // Link the external token with the internal one so either can cancel.
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
        var ct = linked.Token;

        try
        {
            _isDownloadingImages = true;
            OnPropertyChanged(nameof(StartupMessage));

            var imageUrls = new List<string>();
            foreach (var hud in hudList)
            {
                if (!string.IsNullOrWhiteSpace(hud.Thumbnail))
                    imageUrls.Add(hud.Thumbnail);

                if (hud.Screenshots != null)
                    imageUrls.AddRange(hud.Screenshots);
            }

            TotalImages = imageUrls.Count;
            ImagesDownloaded = 0;

            foreach (var hud in hudList)
            {
                ct.ThrowIfCancellationRequested();
                hud.ThumbnailImage = await DownloadAndReportAsync(hud.Thumbnail, ct);

                hud.ScreenshotImages = [];
                if (hud.Screenshots.Length > 0)
                {
                    foreach (var screenshotUrl in hud.Screenshots)
                    {
                        ct.ThrowIfCancellationRequested();
                        var img = await DownloadAndReportAsync(screenshotUrl, ct);
                        if (img != null)
                            hud.ScreenshotImages.Add(img);
                    }
                }
            }
        }
        finally
        {
            _isDownloadingImages = false;
            OnPropertyChanged(nameof(StartupMessage));
        }
    }

    private async Task<Avalonia.Media.Imaging.Bitmap?> DownloadAndReportAsync(string? url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            ImagesDownloaded++;
            return null;
        }

        ct.ThrowIfCancellationRequested();
        var bitmap = await ImageCache.GetImageAsync(url);
        ImagesDownloaded++;
        return bitmap;
    }
}