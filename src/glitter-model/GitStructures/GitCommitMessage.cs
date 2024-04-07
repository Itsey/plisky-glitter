namespace Plisky.Glitter;

public class GitCommitMessage {
    private GitMessage rawMessage;

    public GitMessage SourceMessage {
        get => rawMessage;
    }

    public Dictionary<string, int> MessageAnalysis { get; set; } = new Dictionary<string, int>();
    public bool MetConventions { get; set; }
    public bool MetWorkItemReference { get; set; }

    public GitCommitMessage(GitMessage cma) {
        this.rawMessage = cma;
    }

    public GitCommitMessage() {
    }

    public void Parse(ConventionFormat cf = null) {
        if (cf != null) {
            string? workingText = rawMessage.FullDescription;

            int conventionMarkerEndPoint = workingText.IndexOf(cf.ConventionDelimiter);
            if (conventionMarkerEndPoint > 0) {
                string conventionMarker = workingText.Substring(0, conventionMarkerEndPoint);
                conventionMarker = conventionMarker.Trim().ToLower();
                if (cf.ConventionTerms.Contains(conventionMarker)) {
                    MetConventions = true;
                }
            }

            int workitemMarkerStartPoint = workingText.IndexOf(cf.WorkItemStartDelimiter);
            if (workitemMarkerStartPoint > 0) {
                int length = 0;
                int offset = workitemMarkerStartPoint + 1;
                while (offset < workingText.Length && Char.IsAsciiDigit(workingText[offset])) {
                    length++;
                    offset++;
                }

                if (length > 0) {
                    MetWorkItemReference = true;
                }
            }
        }

        string[] words = rawMessage.FullDescription.Split(new char[] { ',', '.', ' ', ';', '"' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, int> wc = new Dictionary<string, int>();
        foreach (string l in words) {
            string nxt = l.ToLowerInvariant().Trim();
            if (nxt.Length > 0) {
                if (!wc.ContainsKey(nxt)) {
                    wc.Add(nxt, 1);
                } else {
                    wc[nxt] += 1;
                }
            }
        }
        MessageAnalysis = wc;
    }

    private GitMessageParser GetParserFromStructure(GitParseStructure gps) {
        if (gps == null) {
            return new DefaultGitMessageParser();
        } else {
            throw new NotImplementedException();
        }
    }
}