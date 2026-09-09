public record Observation(
    int ID,
    string Author,
    string Message,
    long Timestamp
) : Cheep(Author, Message, Timestamp);