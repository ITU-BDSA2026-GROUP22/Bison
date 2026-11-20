public record ObservationViewModel(
    int ID,
    string Author,
    string Message,
    string Timestamp,
    int? PictureId = null
);
