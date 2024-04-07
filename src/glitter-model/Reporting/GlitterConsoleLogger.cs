namespace Plisky.Glitter;

public class GlitterConsoleLogger {

    public GlitterConsoleLogger() {
        Console.WriteLine("Glitter - Console Log.");
    }

    internal async Task AddAuthorInfo(string? gitDirectory, string l, int commitCount, int goodCommitCount, Dictionary<string, int> commitWordStructure) {
        IEnumerable<string> mostUsedWord = commitWordStructure.OrderByDescending(p => p.Value).Select(x => x.Key).Take(3);
        string mlw = string.Empty;
        foreach (string? w in mostUsedWord) {
            mlw += w + ", ";
        }
        double pc = 0;
        if (commitCount > 0) {
            pc = goodCommitCount / commitCount * 100;
        }
        await Console.Out.WriteLineAsync($"    Author > {l} : {commitCount} ({pc}%) - ({mlw})");
    }

    internal async Task AddCommitDateHeirachy(Dictionary<DateOnly, int> commitsByDate) {
        if (commitsByDate != null && commitsByDate.Keys.Count > 0) {
            await Console.Out.WriteLineAsync("Author Commit History.");
            foreach (DateOnly k in commitsByDate.Keys) {
                await Console.Out.WriteLineAsync($"         {k} : {commitsByDate[k]}");
            }
        }
    }

    internal async Task WriteErrorAbort(string v) {
        await Console.Out.WriteLineAsync($"Fatal > {v}");
    }

    internal async Task WriteInitialHeader(string? name) {
        await Console.Out.WriteLineAsync($"Repository ____ {name} _____");
    }

    internal async Task WriteRepositoryInfo(string? gitDirectory, GitRepoStatus status, int branchCount, int remoteBranchCount, int authorCount) {
        await Console.Out.WriteLineAsync($"{gitDirectory} : [{status}] Branches: {branchCount} Remote: {remoteBranchCount} Authors:{authorCount}");
    }

    internal async Task WriteRepositoryInfo(string? gitDirectory, string message) {
        await Console.Out.WriteLineAsync($"{gitDirectory} {message}");
    }
}