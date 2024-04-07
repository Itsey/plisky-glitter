namespace Plisky.Glitter;
public record GitMessage {
    public string? RawMessage;
    public string? LowerMessage;
    public DateTime CommitDate;
    public string? Author;
    public string? FullDescription;

    public GitMessage(string quickDescription) {
        FullDescription = quickDescription;
    }

    public GitMessage() {
    }
}