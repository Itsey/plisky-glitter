using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System.Linq;

internal class Build : NukeBuild {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.UnitTest);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    private readonly Solution Solution;

    [GitRepository]
    private readonly GitRepository GitRepository;

    private AbsolutePath SourceDirectory => RootDirectory / "src";

    private Target Clean => _ => _
        .Before(Restore)
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
        });

    private Target Restore => _ => _
        .Executes(() => {
        });

    private Target Compile => _ => _
        .DependsOn(Restore)
        .Triggers(UnitTest)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
               .SetProjectFile(Solution)
               .SetConfiguration(Configuration)
               .EnableNoRestore()
               .SetDeterministic(IsServerBuild)
               .SetContinuousIntegrationBuild(IsServerBuild)
           );

            /*
            DotNetTasks.DotNetPublish(s => s
                .SetProject(Solution.AllProjects.First(x => x.Name == "Quartz.Examples.Worker"))
                .SetConfiguration(Configuration)
            );
            DotNetTasks.DotNetPublish(s => s
                .SetProject(Solution.AllProjects.First(x => x.Name == "Quartz.Examples.AspNetCore"))
                .SetConfiguration(Configuration)
            );
            */
        });

    private Target UnitTest => _ => _
        .After(Compile)
        .Executes(() => {
            Project[] testProjects = Solution.Projects.Where(x => x.Name.EndsWith(".Test")).ToArray();
            if (testProjects.Any()) {
                DotNetTasks.DotNetTest(s => s
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetProjectFile("giTing.Test"));
                //.SetConfiguration(Configuration)
                //.SetLoggers(GitHubActions.Instance is not null ? new[] { "GitHubActions" } : Array.Empty<string>())
                //.CombineWith(testProjects, (_, testProject) => _
                //    .SetProjectFile(Solution.GetAllProjects(testProject).First())
                //));
            }
        });

    private Target Test => _ => _
      .DependsOn(Compile)
      .Executes(() => {
          DotNetTasks.DotNetTest(_ => _
            .SetProjectFile("giTing.Test")
            .SetConfiguration(Configuration)
            .EnableNoBuild());
      });

    private Target Print => _ => _
        .Executes(() => {
            Log.Information("Solution path = {Value}", Solution);
            Log.Information("Solution directory = {Value}", Solution.Directory);
        });
}