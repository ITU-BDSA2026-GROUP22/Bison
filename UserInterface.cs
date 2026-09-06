public static class UserInterface {

    public static void PrintObservations(IEnumerable<Cheep> cheeps) {
        foreach (Cheep cheep in cheeps) {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();

            //Whn u write $ in front of string it make the string "Interpolated String" 
            //which makes it possible to put variables inside the strng using {...}
            Console.WriteLine($"{cheep.Author} @ {date:MM/dd/yy HH:mm:ss}: {cheep.Message}");
        }
    }
}