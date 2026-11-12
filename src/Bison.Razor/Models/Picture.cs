namespace Bison.Razor.Models;

public class Picture {
    public int PictureId { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "";
}
