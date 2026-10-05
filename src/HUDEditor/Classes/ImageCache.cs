using Avalonia.Controls.Shapes;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace HUDEditor.Classes;

public static class ImageCache
{
    public static readonly string CacheDir = Path.Combine(AppContext.BaseDirectory, "cache");

    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };

    static ImageCache() => Directory.CreateDirectory(CacheDir);

    private static string GetCachePath(string url)
    {
        Directory.CreateDirectory(CacheDir); // The cache may have been cleared while the app is running.
        using var sha1 = SHA1.Create();
        var hash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(url))).Replace("-", "");
        var ext = Path.GetExtension(new Uri(url).LocalPath);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5) ext = ".img";
        return Path.Combine(CacheDir, $"{hash}{ext}");
    }

    public static async Task<Bitmap?> GetImageAsync(string url)
    {
        var cachePath = GetCachePath(url);

        if (File.Exists(cachePath))
        {
            try
            {
                using var stream = File.OpenRead(cachePath);
                return new Bitmap(stream);
            }
            catch
            {
                File.Delete(cachePath); // Remove corrupted cache
            }
        }

        try
        {
            byte[] bytes;

            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                var localPath = new Uri(url).LocalPath; // safely converts file:// to local path
                bytes = await File.ReadAllBytesAsync(localPath);
            }
            else
            {
                bytes = await Client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(cachePath, bytes);
                App.Logger.Info($"Downloaded: {cachePath}");
            }
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Synchronous variant of <see cref="GetImageAsync"/> for callers that cannot await (e.g. value converters).
    /// Reads local files and cached images directly; downloads with a short timeout otherwise.
    /// </summary>
    public static Bitmap? GetImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase) || File.Exists(url))
            {
                var localPath = Utilities.ToLocalPath(url);
                return File.Exists(localPath) ? new Bitmap(localPath) : null;
            }

            var cachePath = GetCachePath(url);
            if (!File.Exists(cachePath))
            {
                var bytes = Task.Run(() => Client.GetByteArrayAsync(url)).GetAwaiter().GetResult();
                File.WriteAllBytes(cachePath, bytes);
            }

            return new Bitmap(cachePath);
        }
        catch (Exception e)
        {
            App.Logger.Error($"Error loading image \"{url}\": {e.Message}");
            return null;
        }
    }

    public static async Task<string?> SaveToCacheAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || !File.Exists(url)) return null;

        try
        {
            var cachePath = GetCachePath(url);

            // Only copy if not already cached
            if (!File.Exists(cachePath))
            {
                await using var fileStream = File.OpenRead(url);
                await using var destStream = File.Create(cachePath);
                await fileStream.CopyToAsync(destStream);
            }

            return cachePath;
        }
        catch
        {
            App.Logger.Error($"Failed to Download: {url}");
            return null;
        }
    }
}