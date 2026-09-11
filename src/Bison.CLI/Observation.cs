public record Observation(
    int ID,
    string Author,
    string Message,
    long Timestamp,
    string Location = ""
) : Cheep(Author, Message, Timestamp);