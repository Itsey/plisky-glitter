using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System.Diagnostics;

namespace Plisky.Glitter;

internal class Program {

    private static async Task Main(DirectoryInfo dir, bool Debug) {
        Console.WriteLine("Glitter - Online.");
        Bilge b = new Bilge(tl: SourceLevels.Verbose);
        b.AddHandler(new TCPHandler("127.0.0.1", 9060));

        ProcessRunner processRunner = new ProcessRunner();
        await processRunner.RegisterCommandFromWhere("git", "git.exe");

        GitRepoExplorer gre = new GitRepoExplorer(processRunner);
        GitRepo[] fl = gre.FindAllGitRepos(dir);

        Console.WriteLine($"Found {fl.Length} repositories.");
        List<Task> tasks = new List<Task>();
        foreach (GitRepo f in fl) {
            b.Verbose.Log($"queuing status check for {f.RepoDirectory}");
            tasks.Add(gre.UpdateStatus(f, new StatusOptions()));
        }

        Task.WhenAll(tasks).Wait();
        b.Verbose.Log("Task execution completed, moving on to reporting.");

        await gre.WriteAllReports(new GlitterConsoleLogger());
        await b.Flush();
    }
}