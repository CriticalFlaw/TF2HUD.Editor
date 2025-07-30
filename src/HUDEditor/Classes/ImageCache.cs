using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.Classes;

public static class ImageCache
{
    private static readonly string CacheDir = Path.Combine(AppContext.BaseDirectory, "cache");

    static ImageCache() => Directory.CreateDirectory(CacheDir);

    private static string GetCachePath(string url)
    {
        using var sha1 = SHA1.Create();
        var hash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(url))).Replace("-", "");
        var ext = Path.GetExtension(new Uri(url).AbsolutePath);
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
                using var client = new HttpClient();
                bytes = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(cachePath, bytes);
            }
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
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
                fileStream.Position = 0; // Reset stream position after hashing
                await using var destStream = File.Create(cachePath);
                await fileStream.CopyToAsync(destStream);
            }

            return cachePath;
        }
        catch
        {
            return null;
        }
    }
}