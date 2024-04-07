namespace Plisky.Glitter;

public class AuthorMapping {
    public List<GitRepoFile> Maps { get; set; } = new List<GitRepoFile>();

    public AuthorMapping() {
    }

    public void Add(GitRepoFile afm) {
        Maps.Add(afm);
    }
}