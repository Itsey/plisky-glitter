namespace Plisky.Glitter;

public class ProcessResult {
    public string Error { get; internal set; }
    public string Output { get; internal set; }
    public int ExitCode { get; internal set; }
}