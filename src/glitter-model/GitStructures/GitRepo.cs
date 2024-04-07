namespace Plisky.Glitter;

public class GitRepo {
    protected Assessor qcheck = new Assessor();

    public string? GitDirectory { get; internal set; }
    public string? RepoDirectory { get; internal set; }
    public int UntrackedCount { get; internal set; }
    public int ModifiedCount { get; internal set; }
    public int AddedCount { get; internal set; }
    public int DeletedCount { get; internal set; }
    public int RenamedCount { get; internal set; }
    public int CopiedCount { get; internal set; }
    public int UnmergedCount { get; internal set; }
    public int IgnoredCount { get; internal set; }
    public GitRepoStatus Status { get; set; }
    public int BranchCount { get; internal set; }
    public int RemoteBranchCount { get; internal set; }
    public bool IsInError { get; internal set; }
    public Exception? ErrorException { get; internal set; }
    public GitRepoFile[] RepositoryFiles { get; internal set; } = new GitRepoFile[0];

    protected List<AuthorContribution> AuthorContributions { get; set; } = new List<AuthorContribution>();

    public AuthorContribution? GetAuthorContribution(string authorName) {
        return AuthorContributions.SingleOrDefault(x => x.Name == authorName);
    }

    public void AddAuthorContribution(AuthorContribution ac) {
        AuthorContribution? axc = AuthorContributions.FirstOrDefault(x => x.Name == ac.Name);
        if (axc == null) {
            AuthorContributions.Add(ac);
        } else {
            axc.Merge(ac);
        }
    }

    public string[] GetAllAuthors() {
        lock (AuthorContributions) {
            return AuthorContributions.Select(x => x.Name).ToArray();
        }
    }

    public void AddToMasterAuthorsList(List<string> authors) {
        lock (AuthorContributions) {
            foreach (string a in authors) {
                if (!AuthorContributions.Select(x => x.Name == a).Any()) {
                    AuthorContributions.Add(new AuthorContribution(a));
                }
            }
        }
    }

    public void ProcessAuthorsCommits(string authorName, GitMessage[] individualCommitTexts) {
        AuthorContribution? author = AuthorContributions.FirstOrDefault(x => x.Name == authorName);
        if (author == null) {
            author = new AuthorContribution(authorName);
            AuthorContributions.Add(author);
        }

        foreach (GitMessage cm in individualCommitTexts) {
            GitCommitMessage result = GetCommitMessage(cm);
            author.AddCommit(result);
        }
    }

    public GitCommitMessage GetCommitMessage(GitMessage commitText) {
        GitCommitMessage result = new GitCommitMessage(commitText);
        result.Parse(qcheck.ConventionFormat);

        return result;
    }

    public void AddAssessor(Assessor v) {
        qcheck = v;
    }
}