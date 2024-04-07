namespace Plisky.Glitter;

public class Assessor {
    private ConventionFormat cf = null;
    private int fileCount = 0;
    private int teamMembersToPassQualityGate = 0;

    public IEnumerable<string> Conventions {
        get {
            if (cf != null) {
                return cf.ConventionTerms;
            } else {
                return new string[0];
            }
        }
    }

    public ConventionFormat ConventionFormat {
        get {
            return cf;
        }
    }

    public Assessor() {
    }

    public bool FileQualityAssessment(GitRepoFile afm, List<string> tl) {
        int ct = GetActiveAuthors(afm.Authors, tl);
        return ct >= teamMembersToPassQualityGate;
    }

    public void GenerateQualityReport(AuthorMapping afm, List<string> team) {
        int activeMemberCount = 0;
        foreach (GitRepoFile l in afm.Maps) {
            activeMemberCount = GetActiveAuthors(l.Authors, team);
        }
    }

    public int GetActiveAuthors(List<string> authors, List<string> team) {
        return team.Count(item => authors.Any(author => author.Equals(item, StringComparison.OrdinalIgnoreCase)));
    }

    public void SetFileQualityRule(int minimumTeamMemberCount) {
        teamMembersToPassQualityGate = minimumTeamMemberCount;
    }

    public void RegisterConventions(char v1, params string[] conventions) {
        cf = new ConventionFormat(v1, conventions);
    }
}