using Plisky.Diagnostics;
using System.Diagnostics;

namespace Plisky.Glitter.Test;

public class MessageAnalysis {
    protected Bilge b = new Bilge(tl: SourceLevels.Verbose);

    public class AssessorTests {

        [Fact]
        public void Register_conventions_stores_conventions() {
            Assessor sut = new Assessor();
            sut.RegisterConventions(':', "chore", "feature", "bug");

            Assert.Equal(3, sut.Conventions.Count());
        }

        [Theory]
        [InlineData("a,b,c,d", "a,b", 2)]
        [InlineData("a,b,c,d", "a,x,y,z", 1)]
        [InlineData("a,b,c,d", "x,y,z", 0)]
        [InlineData("a,b,c,d,d,d,d,d,d", "d,x,y,z", 1)]
        [InlineData("alice,bob,charlie,diana", "alice", 1)]
        [InlineData("alice,bob,charlie,diana", "Alice", 1)]
        [InlineData("ALICE,bob,charlie,diana", "Alice", 1)]
        public void Author_count_matches(string authorList, string teamList, int matchCount) {
            Assessor a = new Assessor();

            List<string> al = authorList.Split(',').Select(p => p.Trim()).ToList();
            List<string> tl = teamList.Split(',').Select(p => p.Trim()).ToList();
            int result = a.GetActiveAuthors(al, tl);

            Assert.Equal(matchCount, result);
        }

        [Theory]
        [InlineData("a,b,c", "a", 1, true)]
        [InlineData("a,b,c", "a", 2, false)]
        [InlineData("a,b,c", "a,b", 2, true)]
        [InlineData("a,b,c", "a,b", 5, false)]
        [InlineData("a,b,c", "x", 0, true)]
        [InlineData("a,b,c", "x", 1, false)]
        public void File_quality_calculates_passing_index(string authorList, string teamList, int passCount, bool shouldPass) {
            Assessor a = new Assessor();

            List<string> al = authorList.Split(',').Select(p => p.Trim()).ToList();
            List<string> tl = teamList.Split(',').Select(p => p.Trim()).ToList();
            GitRepoFile afm = new GitRepoFile();
            afm.Authors = al;
            afm.Filename = "test.cs";

            a.SetFileQualityRule(passCount);
            bool result = a.FileQualityAssessment(afm, tl);

            Assert.Equal(shouldPass, result);
        }

        [Theory]
        [InlineData("the quick brown fox jumped over the lazy dog", 8, 2)]
        [InlineData("a a a;a a a a", 1, 7)]
        [InlineData("monkey monkey monkey fish fish fish", 2, 3)]
        [InlineData("monkey \r\n monkey \n\nmonkey \r\nfish fish fish", 2, 3)]
        [InlineData("", 0, 0)]
        public void Word_count_is_calculated(string sentence, int numberWords, int highestSingleWordCount) {
            GitCommitMessage sut = new GitCommitMessage(new GitMessage(sentence));
            sut.Parse();

            if (highestSingleWordCount > 0) {
                Assert.Equal(highestSingleWordCount, sut.MessageAnalysis.Values.OrderByDescending(x => x).First());
            }
            Assert.Equal(numberWords, sut.MessageAnalysis.Keys.Count());
        }

        [Fact]
        public void Author_count_parsing_works() {
            GitRepoExplorer gre = new GitRepoExplorer(new ProcessRunner());
            GitRepoFile fl = new GitRepoFile();
            gre.ParseAuthorLog(fl, "   227\tMatthias Koch <ithrowexceptions@gmail.com>\n     1\tSebastian <sebastian@karasek.io>\n     1\tSebastian Karasek <sebastian@karasek.io>\n     1\tTdMxm <franzappo@gmail.com>\n     1\tUlrich Buchgraber <ulrich.b@gmx.at>\n");
            Assert.Equal(5, fl.Authors.Count);
        }

        [Theory()]
        [InlineData("C:\\temp\\kevlog.txt", false, false)]
        [InlineData("C:\\temp\\jimlog.txt", false, false)]
        [InlineData("chore: Commit message", true, false)]
        [InlineData("bug: Commit message", true, false)]
        [InlineData("feature: Commit message", true, false)]
        [InlineData("build: Commit message", true, false)]
        [InlineData("chore: Commit message #1234", true, true)]
        [InlineData("bug: Commit message #1234 committed", true, true)]
        [InlineData("feature:#1234 committed.", true, true)]
        [InlineData("feature: Basic Implementation \r\n\r\n Work on #1234 committed.", true, true)]
        public async Task Date_conventions_are_recognised(string commitText, bool hasConvention, bool hasWorkitem) {
            List<string> conventionTerms = new List<string>();
            conventionTerms.AddRange(new string[] { " " });

            GitRepo gr = new GitRepo();
            Assessor a = new Assessor();
            a.RegisterConventions(':', "chore", "feature", "bug", "build");
            gr.AddAssessor(a);

            gr.ProcessAuthorsCommits("mockAuthor", new GitMessage[] { new GitMessage(commitText) });
            AuthorContribution? result = gr.GetAuthorContribution("mockAuthor");

            GitCommitMessage cm = gr.GetCommitMessage(new GitMessage(commitText));

            Assert.Equal(hasConvention, cm.MetConventions);
            Assert.Equal(hasWorkitem, cm.MetWorkItemReference);
        }

        [Theory()]
        [InlineData("C:\\temp\\kevlog.txt", false, false)]
        [InlineData("C:\\temp\\jimlog.txt", false, false)]
        [InlineData("chore: Commit message", true, false)]
        [InlineData("bug: Commit message", true, false)]
        [InlineData("feature: Commit message", true, false)]
        [InlineData("build: Commit message", true, false)]
        [InlineData("chore: Commit message #1234", true, true)]
        [InlineData("bug: Commit message #1234 committed", true, true)]
        [InlineData("feature:#1234 committed.", true, true)]
        [InlineData("feature: Basic Implementation \r\n\r\n Work on #1234 committed.", true, true)]
        public async Task conventions_are_identified(string commitText, bool hasConvention, bool hasWorkitem) {
            List<string> conventionTerms = new List<string>();
            conventionTerms.AddRange(new string[] { " " });

            GitRepo gr = new GitRepo();
            Assessor a = new Assessor();
            a.RegisterConventions(':', "chore", "feature", "bug", "build");
            gr.AddAssessor(a);

            gr.ProcessAuthorsCommits("mockAuthor", new GitMessage[] { new GitMessage(commitText) });
            AuthorContribution? result = gr.GetAuthorContribution("mockAuthor");

            GitCommitMessage cm = gr.GetCommitMessage(new GitMessage(commitText));

            Assert.Equal(hasConvention, cm.MetConventions);
            Assert.Equal(hasWorkitem, cm.MetWorkItemReference);
        }
    }
}