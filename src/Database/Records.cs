namespace SimpleDB;

public record Observation(int ID, string Author, string Message, long Timestamp, string Location = "");
public record NewObservationRequest(string Author, string Message, long Timestamp, string Location = "");
public record Comment(int ObservationId, string Author, string Message, long Timestamp);
public record Proposal(int ObservationId, string Author, string TaxonId, long Timestamp);