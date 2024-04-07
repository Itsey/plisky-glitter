using System.Text.Json;

namespace Plisky.Glitter;

public class ReportGenerator {

    public string GenerateQualityReport(GitRepoFile afm) {
        var bob = new { first = "first", Second = "second" };
        return JsonSerializer.Serialize(afm); ;
    }
}