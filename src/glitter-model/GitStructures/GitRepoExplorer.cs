using GlobExpressions;
using Plisky.Diagnostics;

namespace Plisky.Glitter;

public class GitRepoExplorer {
    protected Bilge b = new Bilge(tl: System.Diagnostics.SourceLevels.Verbose);
    protected List<GitRepo> gitRepos;
    protected ProcessRunner processRunner;

    public GitRepoExplorer(ProcessRunner processRunner) {
        this.processRunner = processRunner;
    }

    public GitRepo[] FindAllGitRepos(DirectoryInfo rootToSearchFrom) {
        gitRepos = new List<GitRepo>();

        EnumerationOptions enopts = new EnumerationOptions();
        enopts.RecurseSubdirectories = true;
        enopts.MatchCasing = MatchCasing.PlatformDefault;
        enopts.ReturnSpecialDirectories = false;
        enopts.IgnoreInaccessible = true;
        enopts.AttributesToSkip = 0;
        enopts.MatchType = MatchType.Simple;

        List<string> fl = Directory.EnumerateDirectories(rootToSearchFrom.FullName, ".git", enopts).Select(p => p).ToList();
        foreach (string? file in fl) {
            DirectoryInfo fi = new DirectoryInfo(file);
            string pi = fi.Parent.FullName;
            b.Info.Log($"{fi} in {pi}");
            gitRepos.Add(new GitRepo() {
                RepoDirectory = pi,
                GitDirectory = fi.FullName
            });
        }
        return gitRepos.ToArray();
    }

    public string[] GetAllFilesInGitRepo(string repoPath) {
        List<string> result = new List<string>();
        IEnumerable<string> g = Glob.Files(repoPath, "**/*.cs");
        foreach (string? f in g) {
            if (Glob.IsMatch(f, "**/bin/**/*")) {
                //b.Info.Log($"SKIP {f}");
            } else if (Glob.IsMatch(f, "**/obj/**/*")) {
                //b.Info.Log($"SKIP {f}");
            } else {
                b.Verbose.Log(f);
                result.Add(f);
            }
        }
        return result.ToArray();
    }

    public void ParseAuthorLog(GitRepoFile fileToCheck, string output) {
        List<string> auths = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0).ToList();

        foreach (string? l in auths) {
            string nm = l.Substring(l.IndexOf('\t') + 1).Trim();
            nm = nm.Substring(0, nm.IndexOf('>') + 1).Trim();

            if (!fileToCheck.Authors.Contains(nm)) {
                b.Verbose.Log($"Name {nm}");
                fileToCheck.Authors.Add(nm);
            }
        }
    }

    public GitRepoFile ParseAuthorLog(string output) {
        GitRepoFile result = new GitRepoFile();
        ParseAuthorLog(result, output);
        return result;
    }

    private const string GIT_MARKER_PERCOMMIT = "_^_^_^_";
    private const string GIT_MARKER_DATEID = "_^DDXX^_";

    public Task UpdateStatus(GitRepo f, StatusOptions opt) {
        Task t = Task.Run(async () => {
            try {
                List<Task> pendingTasks = new List<Task>();
                b.Info.Log($"Repository Processing Starts {f.RepoDirectory}");

                pendingTasks.Add(processRunner.Execute("git", f.RepoDirectory, "status --porcelain").ContinueWith((t) => {
                    if (t.Result.ExitCode != 0) {
                        f.IsInError = true;
                        f.ErrorException = new Exception($"Error in git status {t.Result.Error}");
                    } else {
                        ParseStatusLog(f, t.Result.Output);
                        b.Info.Log($"Git Status Check, Complete. {f.Status}");
                    }
                }));

                pendingTasks.Add(processRunner.Execute("git", f.RepoDirectory, "branch --no-color -a -l --no-show-current -q").ContinueWith((t) => {
                    if (t.Result.ExitCode != 0) {
                        f.IsInError = true;
                        f.ErrorException = new Exception($"Error in git status {t.Result.Error}");
                    } else {
                        ParseBranchLog(f, t.Result.Output);
                        b.Info.Log($"Git Branch Check, Complete {f.BranchCount} branches.");
                    }
                }));

                if (opt.PerformFileAnalysis) {
                    PopulateRepoFiles(f);
                    b.Info.Log($"Files Populated. {f.RepositoryFiles.Length} files.");
                    foreach (GitRepoFile fileToCheck in f.RepositoryFiles) {
                        pendingTasks.Add(processRunner.Execute("git", f.RepoDirectory, $"shortlog --summary --numbered --email {fileToCheck.Filename}").ContinueWith((t) => {
                            ParseAuthorLog(fileToCheck, t.Result.Output);
                            if (fileToCheck.Authors != null && fileToCheck.Authors.Count > 0) {
                                b.Verbose.Log($"Additional {fileToCheck.Authors.Count} authors found, adding to master list.");
                                f.AddToMasterAuthorsList(fileToCheck.Authors);
                            }
                        }));
                    }
                }
                await Task.WhenAll(pendingTasks);

                pendingTasks.Clear();

                if (opt.PerformCommitAnalysis) {
                    b.Info.Log("Performing Commit Analysis.");

                    DateTime dt = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0));
                    string format = dt.ToString("dd/MM/yyyy");
                    foreach (string authorToCheck in f.GetAllAuthors()) {
                        b.Info.Log($"Checking for commits by {authorToCheck} in repo {f.RepoDirectory}");
                        pendingTasks.Add(processRunner.Execute("git", f.RepoDirectory, $" --no-pager log --author=\"{authorToCheck}\" --format=\"{GIT_MARKER_PERCOMMIT}%B{GIT_MARKER_DATEID}%ad\" --date=format-local:'%d/%m/%y-%H:%M:%S' --since {format}").ContinueWith((tres) => {
                            if (tres.Result.ExitCode != 0) {
                                f.IsInError = true;
                                f.ErrorException = new Exception($"Error in git status {tres.Result.Error}");
                            } else {
                                AuthorContribution ac = ParseAuthorCommitLog(authorToCheck, tres.Result.Output, new Assessor());
                                f.AddAuthorContribution(ac);
                            }
                        }));
                    }
                }

                await Task.WhenAll(pendingTasks);

                b.Verbose.Log($"Repository task processing ends {f.RepoDirectory}");
            } catch (Exception ex) {
                f.IsInError = true;
                f.ErrorException = ex;
                b.Error.Dump(ex, "Exception in git");
            }
        });
        return t;
    }

    public AuthorContribution ParseAuthorCommitLog(string authorName, string output, Assessor checkMode) {
        AuthorContribution result = new AuthorContribution(authorName);

        string[] s = output.Split(GIT_MARKER_PERCOMMIT);
        foreach (string l in s) {
            if (string.IsNullOrWhiteSpace(l)) {
                continue;
            }

            TokenisedGitMessageParser tgmp = new TokenisedGitMessageParser(GIT_MARKER_DATEID, GIT_MARKER_PERCOMMIT);
            GitMessage cma = tgmp.Parse(l);
            GitCommitMessage cm = new GitCommitMessage(cma);
            cm.Parse(checkMode.ConventionFormat);
            result.AddCommit(cm);
        }
        return result;
    }

    private void PopulateRepoFiles(GitRepo f) {
        b.Info.Flow();

        string[] l = GetAllFilesInGitRepo(f.RepoDirectory);
        f.RepositoryFiles = l.Select(p => new GitRepoFile() { Filename = p }).ToArray();
        b.Verbose.Log($"{f.RepositoryFiles.Length} files found.");
    }

    private void ParseBranchLog(GitRepo f, string output) {
        b.Verbose.Log($"{f.RepoDirectory} using {output}");
        f.BranchCount = 0;

        if (string.IsNullOrWhiteSpace(output)) {
            return;
        }

        string[] branches = output.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (string l in branches) {
            string branchName;
            if (l.StartsWith('*')) {
                branchName = l.Substring(1).Trim();
            } else {
                branchName = l.Trim();
            }

            if (!branchName.StartsWith("remotes/")) {
                b.Verbose.Log($"Match {branchName} Local Branch.");
                f.BranchCount += 1;
            } else {
                f.RemoteBranchCount += 1;
            }
        }
    }

    private void ParseStatusLog(GitRepo f, string output) {
        b.Verbose.Log($"{f.RepoDirectory} using {output}");
        if (string.IsNullOrWhiteSpace(output)) {
            f.Status = GitRepoStatus.Clean;
        } else {
            string[] outputLines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (string l in outputLines) {
                if (string.IsNullOrWhiteSpace(l)) {
                    continue;
                }

                f.Status = GitRepoStatus.Dirty;
                if (l.StartsWith("??")) {
                    f.UntrackedCount += 1;
                } else {
                    string chr = l.Substring(0, 1);
                    switch (chr) {
                        case "M": f.ModifiedCount += 1; break;
                        case "A": f.AddedCount += 1; break;
                        case "D": f.DeletedCount += 1; break;
                        case "R": f.RenamedCount += 1; break;
                        case "C": f.CopiedCount += 1; break;
                        case "U": f.UnmergedCount += 1; break;
                        case "!": f.IgnoredCount += 1; break;
                    }
                }
            }
        }
    }

    public async Task WriteAllReports(GlitterConsoleLogger glitterConsoleLogger) {
        if (gitRepos == null) {
            glitterConsoleLogger.WriteErrorAbort("No Repositories Found");
            return;
        }

        foreach (GitRepo x in gitRepos) {
            string? name = Path.GetFileName(x.RepoDirectory);
            await glitterConsoleLogger.WriteInitialHeader(name);

            if (x.IsInError) {
                await glitterConsoleLogger.WriteRepositoryInfo(x.GitDirectory, x.ErrorException.Message);
            } else {
                await glitterConsoleLogger.WriteRepositoryInfo(x.GitDirectory, x.Status, x.BranchCount, x.RemoteBranchCount, x.GetAllAuthors().Length);
            }
            foreach (string l in x.GetAllAuthors()) {
                AuthorContribution? ac = x.GetAuthorContribution(l);

                await glitterConsoleLogger.AddAuthorInfo(x.GitDirectory, l, ac.CommitCount, ac.GoodCommitCount, ac.CommitWordStructure);
                await glitterConsoleLogger.AddCommitDateHeirachy(ac.CommitsByDate);
            }
        }
    }
}