using GlobExpressions;
using Plisky.Diagnostics;
using System.Diagnostics;

namespace Plisky.Glitter.Test;

/// <summary>
/// Experimental test area, not real tests just experimenting and testing how some code works.
/// </summary>
public class Experimental {
    protected Bilge b = new Bilge(tl: SourceLevels.Verbose);

    [Fact]
    public async Task GetAllFilesInRepo() {
        EnumerationOptions enopts = new EnumerationOptions();
        enopts.RecurseSubdirectories = true;
        enopts.MatchCasing = MatchCasing.PlatformDefault;
        enopts.ReturnSpecialDirectories = false;
        enopts.IgnoreInaccessible = true;
        enopts.AttributesToSkip = 0;
        enopts.MatchType = MatchType.Simple;
        List<string> fl = Directory.EnumerateFiles(@"C:\files\code\github\maui", "*.cs", enopts).Select(p => p).ToList();
        List<Tuple<string, string[]>> authorsByFile = new();
        List<string> authors = new();

        IEnumerable<string> g = Glob.Files(@"C:\files\code\github\maui", "**/*.cs");
        foreach (string? f in g) {
            if (Glob.IsMatch(f, "**/bin/**/*")) {
                //b.Info.Log($"SKIP {f}");
            } else if (Glob.IsMatch(f, "**/obj/**/*")) {
                //b.Info.Log($"SKIP {f}");
            } else {
                b.Info.Log(f);
            }

            ProcessStartInfo psi = new ProcessStartInfo("C:\\Program Files\\Git\\bin\\git.exe");
            psi.WorkingDirectory = @"C:\files\code\github\maui";
            psi.Arguments = "shortlog --summary --numbered --email";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process? p = Process.Start(psi);
            await p.WaitForExitAsync();

            string output = await p.StandardOutput.ReadToEndAsync();
            string error = await p.StandardError.ReadToEndAsync();
            b.Info.Log($"Output1 {p.ExitCode} out> {output} err> {error} ");

            List<string> auths = output.Split('\n').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            List<string> authsThisFile = new List<string>();
            foreach (string? l in auths) {
                string nm = l.Substring(l.IndexOf('\t') + 1).Trim();
                nm = nm.Substring(0, nm.IndexOf('<')).Trim();
                b.Info.Log($"Name {nm}");
                if (!authsThisFile.Contains(nm)) {
                    authsThisFile.Add(nm);
                }
            }
            foreach (string l in authsThisFile) {
                if (!authors.Contains(l)) {
                    authors.Add(l);
                }
            }
            authorsByFile.Add(new Tuple<string, string[]>(f, authsThisFile.ToArray()));
        }

        b.Info.Log($"{authorsByFile.Count} files found unique authors {authors.Count}");

        await b.Flush();
    }

    [Fact]
    public async Task GitExperimental1() {
        b.Info.Flow();
        List<Task> tasks = new List<Task>();

        EnumerationOptions enopts = new EnumerationOptions();
        enopts.RecurseSubdirectories = true;
        enopts.MatchCasing = MatchCasing.PlatformDefault;
        enopts.ReturnSpecialDirectories = false;
        enopts.IgnoreInaccessible = true;
        enopts.AttributesToSkip = 0;
        enopts.MatchType = MatchType.Simple;

        List<string> fl = Directory.EnumerateDirectories(@"C:\files\code\git\mollycoddle", ".git", enopts).Select(p => p).ToList();

        foreach (string? file in fl) {
            DirectoryInfo fi = new DirectoryInfo(file);
            string pi = fi.Parent.FullName;
            b.Info.Log($"{fi} in {pi}");

            Task<Task> t = Task.Factory.StartNew(async () => {
                try {
                    b.Info.Log("Probing");
                    ProcessStartInfo psi = new ProcessStartInfo("C:\\Program Files\\Git\\bin\\git.exe");
                    psi.WorkingDirectory = pi;
                    psi.Arguments = "status --porcelain";
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;

                    Process? p = Process.Start(psi);
                    await p.WaitForExitAsync();

                    string output = await p.StandardOutput.ReadToEndAsync();
                    string error = await p.StandardError.ReadToEndAsync();
                    b.Info.Log($"Output1 {p.ExitCode} out> {output} err> {error} ");
                    psi.Arguments = "shortlog --summary --numbered --email";

                    Process? p2 = Process.Start(psi);
                    await p2.WaitForExitAsync();

                    string output2 = await p2.StandardOutput.ReadToEndAsync();
                    string error2 = await p2.StandardError.ReadToEndAsync();
                    b.Info.Log($"Output2 {p2.ExitCode} out> {output2} err> {error2} ");
                } catch (Exception ex) {
                    b.Error.Dump(ex, "Exception in git");
                }
            });
            tasks.Add(t);
        }

        Task.WaitAll(tasks.ToArray());

        await b.Flush();
        // Task.WaitAll(tasks.ToArray());
        b.Info.Log("All done");
    }
}