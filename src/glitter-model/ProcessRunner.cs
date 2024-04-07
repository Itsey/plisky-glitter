using Plisky.Diagnostics;
using System.Diagnostics;

namespace Plisky.Glitter;

public class ProcessRunner {
    protected Bilge b = new Bilge("glitter-processrunner");
    protected Dictionary<string, string> exePaths = new Dictionary<string, string>();

    public ProcessRunner() {
        SetupDefaultSystemCommands();
    }

    protected void SetupDefaultSystemCommands() {
        string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        RegisterExe("where", Path.Combine(sys32, "where.exe"));
    }

    public void RegisterExe(string exeName, string exePath) {
        exePaths.Add(exeName, exePath);
    }

    public async Task RegisterCommandFromWhere(string v1, string v2) {
        ProcessResult resp = await Execute("where", ".", v2);
        string gitFn = resp.Output.Trim();
        if (!File.Exists(gitFn)) {
            throw new FileNotFoundException();
        }
        RegisterExe(v1, gitFn);
    }

    public async Task<ProcessResult> Execute(string namedExe, string folder, string args) {
        string exe = exePaths[namedExe];
        ProcessStartInfo psi = new ProcessStartInfo(exe);
        psi.WorkingDirectory = folder;
        psi.Arguments = args;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Process? p = Process.Start(psi);
        b.Info.Log(exe + " " + args);

        //p.out();

        await p.WaitForExitAsync();

        string output = await p.StandardOutput.ReadToEndAsync();
        string error = await p.StandardError.ReadToEndAsync();

        return new ProcessResult() { ExitCode = p.ExitCode, Output = output, Error = error };
    }

    public void RunGit() {
    }
}