namespace Plisky.Glitter;

public class AuthorContribution {
    public string Name { get; private set; }
    public Dictionary<DateOnly, int> CommitsByDate { get; set; } = new Dictionary<DateOnly, int>();
    public int CommitCount { get; set; }

    public int GoodCommitCount { get; set; }

    public Dictionary<string, int> CommitWordStructure { get; set; }

    public AuthorContribution(string authorName) {
        if (string.IsNullOrWhiteSpace(authorName)) {
            throw new ArgumentOutOfRangeException(nameof(authorName));
        }
        Name = authorName;
        CommitWordStructure = new Dictionary<string, int>();
    }

    internal void AddCommit(GitCommitMessage result) {
        if (result.SourceMessage.CommitDate == DateTime.MinValue) {
            // Missing commit date, this should not happen
            // TODO: Add a warning about why this is missing.
        } else {
            DateOnly dt = DateOnly.FromDateTime(result.SourceMessage.CommitDate);
            if (CommitsByDate.ContainsKey(dt)) {
                CommitsByDate[dt]++;
            } else {
                CommitsByDate.Add(dt, 1);
            }
        }

        CommitCount++;
        if (result.MetConventions && result.MetWorkItemReference) {
            GoodCommitCount++;
        }

        foreach (string word in result.MessageAnalysis.Keys) {
            if (CommitWordStructure.ContainsKey(word)) {
                CommitWordStructure[word] += result.MessageAnalysis[word];
            } else {
                CommitWordStructure.Add(word, result.MessageAnalysis[word]);
            }
        }
    }

    internal void Merge(AuthorContribution ac) {
        this.CommitCount += ac.CommitCount;
        this.GoodCommitCount += ac.GoodCommitCount;
        foreach (string word in ac.CommitWordStructure.Keys) {
            if (CommitWordStructure.ContainsKey(word)) {
                CommitWordStructure[word] += ac.CommitWordStructure[word];
            } else {
                CommitWordStructure.Add(word, ac.CommitWordStructure[word]);
            }
        }
        foreach (DateOnly cbd in ac.CommitsByDate.Keys) {
            if (CommitsByDate.ContainsKey(cbd)) {
                CommitsByDate[cbd] += ac.CommitsByDate[cbd];
            } else {
                CommitsByDate.Add(cbd, ac.CommitsByDate[cbd]);
            }
        }
    }
}