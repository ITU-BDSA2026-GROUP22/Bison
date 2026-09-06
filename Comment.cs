public record Comment(
    int ObservationID,
    string Author,
    string Message,
    long Timestamp
) : Cheep(Author, Message, Timestamp);