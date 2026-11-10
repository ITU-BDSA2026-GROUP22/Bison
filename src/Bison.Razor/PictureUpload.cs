using Bison.Razor.Models;

namespace Bison.Razor;

public static class PictureUpload {
    public const int MaxSizeInBytes = 2 * 1024 * 1024;
    public const string ErrorMessage = "The picture has to be a jpeg or a png file of max 2 MB";

    private static readonly byte[] JpegStart = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngStart = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    public static async Task<Picture?> ReadAsync(IFormFile file) {
        if (file.Length == 0 || file.Length > MaxSizeInBytes) {
            return null;
        }

        using MemoryStream stream = new MemoryStream();
        await file.CopyToAsync(stream);
        byte[] data = stream.ToArray();

        string? contentType = GetContentType(data);

        if (contentType == null) {
            return null;
        }

        return new Picture {
            Data = data,
            ContentType = contentType
        };
    }

    // checks the first bytes of the file sicne the file name and the type that the browser sends can be anything
    public static string? GetContentType(byte[] data) {
        if (StartsWith(data, JpegStart)) {
            return "image/jpeg";
        }

        if (StartsWith(data, PngStart)) {
            return "image/png";
        }

        return null;
    }

    private static bool StartsWith(byte[] data, byte[] start) {
        if (data.Length < start.Length) {
            return false;
        }

        for (int i = 0; i < start.Length; i++) {
            if (data[i] != start[i]) {
                return false;
            }
        }

        return true;
    }
}
