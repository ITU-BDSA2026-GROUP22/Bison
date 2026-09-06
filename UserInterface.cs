public static class UserInterface {

    public static void PrintObservations(IEnumerable<Observation> observations) {
        foreach (Observation observation in observations) {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(observation.Timestamp).ToLocalTime();

            //Whn u write $ in front of string it make the string "Interpolated String" 
            //which makes it possible to put variables inside the strng using {...}
            Console.WriteLine($"ID: {observation.ID} {observation.Author} @ {date:MM/dd/yy HH:mm:ss}: {observation.Message}");
        }
    }

    public static void PrintComments(IEnumerable<Comment> comments) {
        foreach (Comment comment in comments) {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(comment.Timestamp).ToLocalTime();
            Console.WriteLine($"{comment.Author} @ {date:MM/dd/yy HH:mm:ss}: {comment.Message}");
        }
    }
}