namespace Plisky.Glitter;

public abstract class GitMessageParser {
    protected GitMessage actualMessage;

    protected virtual void TidyupActualMessage() {
        if (!string.IsNullOrEmpty(actualMessage.FullDescription)) {
            if (actualMessage.FullDescription.EndsWith(Environment.NewLine)) {
                actualMessage.FullDescription = actualMessage.FullDescription.Substring(0, actualMessage.FullDescription.Length - Environment.NewLine.Length);
            }
            actualMessage.FullDescription = actualMessage.FullDescription.Trim();
        }
    }

    protected virtual void PerforInitialParse(string rm) {
        actualMessage = new GitMessage();
        actualMessage.RawMessage = rm.Trim();
        actualMessage.LowerMessage = rm.ToLowerInvariant();
    }

    protected virtual void PerformParse() {
    }

    public virtual GitMessage Parse(string rawMessage) {
        PerforInitialParse(rawMessage);
        PerformParse();
        return actualMessage;
    }
}