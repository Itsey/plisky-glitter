using Plisky.Diagnostics;
using System.Diagnostics;

namespace Plisky.Glitter.Test;

public class Integration {

    public class FileSystem {
        protected Bilge b = new Bilge(tl: SourceLevels.Verbose);

        [Fact(Skip = "integration-skipped")]
        public async Task FindGit() {
            b.Info.Flow();
            string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

            ProcessRunner processRunner = new ProcessRunner();
            processRunner.RegisterExe("where", Path.Combine(sys32, "where.exe"));
            ProcessResult resp = await processRunner.Execute("where", sys32, "git.exe");

            string gitFn = resp.Output.Trim();

            if (!File.Exists(gitFn)) {
                throw new FileNotFoundException();
            }
            b.Info.Log($"{resp.ExitCode} {gitFn}");
            await b.Flush();
        }

        [Fact(Skip = "integration-skipped")]
        public async Task GetFileAuthors() {
            string gitFolder = @"C:\files\code\github\maui";
            gitFolder = @"C:\files\code\git\mollycoddle";

            GitRepoExplorer gre = new GitRepoExplorer(null);
            string[] l = gre.GetAllFilesInGitRepo(gitFolder);
            b.Info.Log($"{l.Length} files found");

            string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            ProcessRunner processRunner = new ProcessRunner();
            processRunner.RegisterExe("where", Path.Combine(sys32, "where.exe"));
            ProcessResult resp = await processRunner.Execute("where", sys32, "git.exe");
            processRunner.RegisterExe("git", resp.Output.Trim());

            AuthorMapping a = new AuthorMapping();
            foreach (string f in l) {
                ProcessResult resp2 = await processRunner.Execute("git", gitFolder, $"shortlog --summary --numbered --email {f}");

                GitRepoFile afm = gre.ParseAuthorLog(resp2.Output);
                afm.Filename = f;
                b.Info.Log($"{resp2.ExitCode} {resp2.Output} {f} {afm.Authors.Count}");
                a.Add(afm);
            }

            Assessor assessor = new Assessor();
            List<string> team = new List<string>();
            assessor.GenerateQualityReport(a, team);
        }
    }
}