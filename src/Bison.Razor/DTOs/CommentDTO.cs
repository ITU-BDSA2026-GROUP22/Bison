namespace Bison.Razor.DTOs;

public class CommentDTO
{
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public long Timestamp { get; set; }
}