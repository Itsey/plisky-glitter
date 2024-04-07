namespace Plisky.Glitter;

public class DefaultGitMessageParser : GitMessageParser {

    protected override void PerformParse() {
        actualMessage.FullDescription = actualMessage.RawMessage;
    }
}