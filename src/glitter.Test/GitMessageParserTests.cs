namespace Plisky.Glitter.Test;

public class GitMessageParserTests {

    public static IEnumerable<object[]> TestData =>
        new List<object[]> {
        new object[] {"""
            {x}Docs update
            {y}12/08/22 13:30:16
            """,
            "Docs update",
            new DateTime(2022,8,12,13,30,16)},
        new object[] {"""
            {x}Added README.md, .gitignore (VisualStudio) files
            {y}23/06/21 20:17:11
            """,
            "Added README.md, .gitignore (VisualStudio) files",
            new DateTime(2021,6,23,20,17,11)},
        new object[] {"""
            Added README.md, .gitignore (VisualStudio) files
            {y}23/06/21 20:17:11
            """,
            "Added README.md, .gitignore (VisualStudio) files",
            new DateTime(2021,6,23,20,17,11)},
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public void Token_parser_identifies_body_and_time(string messageBody, string expectedBody, DateTime expectedDate) {
        TokenisedGitMessageParser sut = new TokenisedGitMessageParser("{y}", "{x}");
        GitMessage msg = sut.Parse(messageBody);

        Assert.Equal(expectedBody, msg.FullDescription);
        Assert.Equal(expectedDate, msg.CommitDate);
    }

    [Theory]
    [InlineData("Docs update", "Docs update")]
    public void Default_parser_just_copies_message_as_description(string messageBody, string expectedBody) {
        DefaultGitMessageParser sut = new DefaultGitMessageParser();
        GitMessage msg = sut.Parse(messageBody);

        Assert.Equal(expectedBody, msg.FullDescription);
    }
}