namespace Plisky.Glitter;

public class TokenisedGitMessageParser : GitMessageParser {
    protected string dateTimeStartMarker;
    protected string bodyStartMarker;

    protected override void PerformParse() {
        int bodyStart = actualMessage.RawMessage.IndexOf(bodyStartMarker);

        string messagePart = actualMessage.RawMessage;
        if (bodyStart >= 0) {
            messagePart = actualMessage.RawMessage.Substring(bodyStart + bodyStartMarker.Length);
        }

        int dateTimeStart = messagePart.IndexOf(dateTimeStartMarker);

        if (dateTimeStart < 0) {
            // There was no date time located.
            actualMessage.FullDescription = messagePart;
            actualMessage.CommitDate = DateTime.MinValue;
        } else {
            actualMessage.FullDescription = messagePart.Substring(0, dateTimeStart).Trim();

            string partialDate = messagePart.Substring(dateTimeStart + dateTimeStartMarker.Length);
            partialDate = partialDate.Replace("'", "").Replace("-", " ").Trim();
            int endDatePart = partialDate.IndexOf("\n");
            if (endDatePart < 0) {
                endDatePart = partialDate.Length;
            }
            string datePart = partialDate.Substring(0, endDatePart);
            if (DateTime.TryParse(datePart, out DateTime dt)) {
                actualMessage.CommitDate = dt;
            } else {
                actualMessage.CommitDate = DateTime.MinValue;
            }
        }
        TidyupActualMessage();
    }

    public TokenisedGitMessageParser(string dateTimeToken, string bodyToken) {
        dateTimeStartMarker = dateTimeToken;
        bodyStartMarker = bodyToken;
    }
}