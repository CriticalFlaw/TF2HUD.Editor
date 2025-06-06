using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace HUDEdit.Classes;

public static class ImageCache
{
    private static readonly string CacheDir = Path.Combine(AppContext.BaseDirectory, "ImageCache");

    static ImageCache()
    {
        Directory.CreateDirectory(CacheDir);
    }

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
            using var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(cachePath, bytes);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    public static Bitmap? GetImage(string url)
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
                File.Delete(cachePath);
            }
        }

        try
        {
            using var client = new HttpClient();
            var bytes = client.GetByteArrayAsync(url).Result;
            File.WriteAllBytes(cachePath, bytes);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}