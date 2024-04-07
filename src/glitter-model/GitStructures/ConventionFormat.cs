namespace Plisky.Glitter;

public class ConventionFormat {
    public string DateConventionDelimiter { get; internal set; }
    public char ConventionDelimiter { get; internal set; } = ':';
    public List<string> ConventionTerms { get; internal set; }
    public char WorkItemStartDelimiter { get; internal set; } = '#';

    public ConventionFormat(char delim = ':', params string[] terms) {
        ConventionDelimiter = delim;
        ConventionTerms = new List<string>();
        ConventionTerms.AddRange(terms.Select(x => x.ToLowerInvariant()));
    }
}