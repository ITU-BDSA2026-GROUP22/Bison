namespace Bison.Razor.Tests;

public class PictureTests {
    [Fact]
    public void GetContentType_JpegBytesGivesJpeg() {
        byte[] data = { 0xFF, 0xD8, 0xFF, 0xE0 };

        string? contentType = PictureUpload.GetContentType(data);

        Assert.Equal("image/jpeg", contentType);
    }

    [Fact]
    public void GetContentType_PngBytesGivesPng() {
        byte[] data = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };

        string? contentType = PictureUpload.GetContentType(data);

        Assert.Equal("image/png", contentType);
    }

    [Fact]
    public void GetContentType_TextBytesGivesNull() {
        byte[] data = { 0x48, 0x65, 0x6C, 0x6C, 0x6F };

        string? contentType = PictureUpload.GetContentType(data);

        Assert.Null(contentType);
    }
}
