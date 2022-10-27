using System.IO.Directory;

public class BuildCommand
{
    public string ComponentName { get; set; }
    public string BuildPath {get; set; }
    public IDictionary<string, string> Options { get; } = new Dictionary<string, string>();

    public string FromOptions()
    {
        var sb = new StringBuilder();
        foreach (var opt in Options)
        {
            sb.Append($"--{opt.Key}={opt.Value} ");            
        }
        sb.Length = sb.Length > 0 ? sb.Length-- : 0;
        return sb.ToString();
    }
}

public class BuildProjectSettings
{
    public string Project {get; set; }
    public string Version {get; set; }
    public string Configuration { get; set; }
    public string Verbosity { get; set; }
}

public class BuildPublishSettings : BuildProjectSettings
{
    public string ApiKey { get; set; }
    public string Server { get; set; }
    public bool Debug { get; set; }
    public bool SkipDuplicate { get; set; }
}

public class BuildTestSettings
{
    public Solution Solution {get; set; }
    public string Configuration { get; set; }
    public string Verbosity { get; set; }
}

public class Solution
{
    public Solution(ICakeContext context, string solutionPath)
    {
        SolutionFile = context.GetFiles(solutionPath).First();
    }

    public Solution(FilePath solutionPath)
    {
        SolutionFile = solutionPath;
    }

    public FilePath SolutionFile { get; }
    public string FullPath => SolutionFile.FullPath;
    public string Name => SolutionFile.GetFilenameWithoutExtension().ToString();

    public static implicit operator FilePath(Solution solution)
    {
        return solution.SolutionFile;
    }
}

public class Builder
{
    private ICakeContext _context;

    public Builder(ICakeContext context)
    {
        _context = context;
    }

    public Solution FindSolution(string solutionPath)
    {
        return new Solution(_context, solutionPath);
    }

    public void BuildComponents(IEnumerable<BuildCommand> commands)
    {
        foreach(var command in commands)
        {
            Console.WriteLine($"Building component {command.ComponentName}");
            if (!DotNet.Execute(_context, command.BuildPath, $"cake --target=build {command.FromOptions()}"))
                break;
        }
    }

    public void CleanComponents(IEnumerable<BuildCommand> commands)
    {
        foreach(var command in commands)
        {
            Console.WriteLine($"Cleaning component {command.ComponentName}");
            if (!DotNet.Execute(_context, command.BuildPath, $"cake --target=clean {command.FromOptions()}"))
                break;
        }
    }

    public void PublishComponents(IEnumerable<BuildCommand> commands)
    {
        foreach(var command in commands)
        {
            Console.WriteLine($"Publishing component {command.ComponentName}");
            if (!DotNet.Execute(_context, command.BuildPath, $"cake --target=publish {command.FromOptions()}"))
                break;
        }
    }

    public void TestComponents(IEnumerable<BuildCommand> commands)
    {
        foreach(var command in commands)
        {
            Console.WriteLine($"Testing component {command.ComponentName}");
            if (!DotNet.Execute(_context, command.BuildPath, $"cake --target=test {command.FromOptions()}"))
                break;
        }
    }

    public void BuildSolution(Solution solution, dynamic vars, params string[] excludes)
    {
        var projects = FindProjects(solution, excludes);
        foreach (var project in projects)
        {
            if (!DotNet.BuildProject(_context, new BuildProjectSettings {
                Project = project.Path.ToString(),
                Version = vars.version,
                Configuration = vars.config,
                Verbosity = vars.verbosity
            }))
            {
                throw new Exception($"{project.Name} build failed.");
            }
        }
    }

    public void CleanSolution(Solution solution, dynamic vars, params string[] excludes)
    {
        var projects = FindProjects(solution, excludes);
        foreach (var project in projects)
        {
            if (!DotNet.CleanProject(_context, new BuildProjectSettings {
                Project = project.Path.ToString(),
                Configuration = vars.config,
                Verbosity = vars.verbosity
            }))
            {
                throw new Exception($"{project.Name} clean failed.");
            }
        }
    }

    public void BuildPackages(Solution solution, dynamic vars, params string[] excludes)
    {
        var projects = FindProjects(solution, excludes);
        foreach (var project in projects)
        {
            if (!DotNet.PackageProject(_context, new BuildProjectSettings {
                Project = project.Path.ToString(),
                Version = vars.version,
                Configuration = vars.config,
                Verbosity = vars.verbosity
            }))
            {
                throw new Exception($"{project.Name} build failed.");
            }
        }
    }

    public void PublishPackages(Solution solution, dynamic vars, params string[] excludes)
    {
        var projects = FindProjects(solution, excludes);
        foreach (var project in projects)
        {
            if (!DotNet.PublishProject(_context, new BuildPublishSettings {
                Project = project.Path.ToString(),
                Version = vars.version,
                Configuration = vars.config,
                ApiKey = vars.nugetKey,
                Server = vars.nugetServer,
                Debug = vars.nugetDebug,
                SkipDuplicate = vars.nugetSkipDup
            }))
            {
                throw new Exception($"{project.Name} publish failed.");
            }
        }
    }

    public void RunUnitTests(Solution solution, dynamic vars)
    {
        if (!DotNet.RunUnitTests(_context, new BuildTestSettings {
            Solution = solution,
            Configuration = vars.config,
            Verbosity = vars.verbosity
        }))
        {
            throw new Exception($"{solution.Name} unit tests failed.");
        }
    }

    private IEnumerable<SolutionProject> FindProjects(Solution solution, string[] excludes)
    {
        var projects = _context.ParseSolution(solution).Projects;
        return FilterProjects(projects, excludes ?? Array.Empty<string>());
    }

    private IEnumerable<SolutionProject> FilterProjects(IEnumerable<SolutionProject> projects, string[] excludes)
    {
        return projects.Where(project => !excludes.Any(exclude => project.Name.Contains(exclude)));
    }
}

public static class DotNet
{
    public static bool Execute(ICakeContext context, string runPath, string command)
    {
        var popd = GetCurrentDirectory();
        SetCurrentDirectory(runPath);
        var result = context.StartProcess("dotnet", command);
        SetCurrentDirectory(popd);
        return result == 0;
    }

    public static bool BuildProject(ICakeContext context, BuildProjectSettings settings)
    {
        var options = $"build {settings.Project} -p:Version={settings.Version} -p:PackageVersion={settings.Version} -p:Configuration={settings.Configuration} --verbosity:{settings.Verbosity}";
        Console.WriteLine($"dotnet {options}");
        var result = context.StartProcess("dotnet", $"{options}");
        return result == 0;
    }

    public static bool CleanProject(ICakeContext context, BuildProjectSettings settings)
    {
        var options = $"clean {settings.Project} --configuration={settings.Configuration} --verbosity:{settings.Verbosity}";
        Console.WriteLine($"dotnet {options}");
        var result = context.StartProcess("dotnet", $"{options}");
        return result == 0;
    }

    public static bool PackageProject(ICakeContext context, BuildProjectSettings settings)
    {
        var options = $"pack {settings.Project} --no-build -p:PackageVersion={settings.Version} --configuration {settings.Configuration} --verbosity {settings.Verbosity}";
        Console.WriteLine($"dotnet {options}");
        var result = context.StartProcess("dotnet", $"{options}");
        return result == 0;
    }

    public static bool PublishProject(ICakeContext context, BuildPublishSettings settings)
    {
        var packageName = $"{System.IO.Path.GetFileNameWithoutExtension(settings.Project)}.{settings.Version}.nupkg";
        var package = $"{System.IO.Path.GetDirectoryName(settings.Project)}/bin/{settings.Configuration}/{packageName}";

        var sb = new StringBuilder();
        sb.Append("nuget push ")
            .Append($"{package} ");

        if (!string.IsNullOrEmpty(settings.ApiKey))
            sb.Append($"-k {settings.ApiKey} ");

        if (!string.IsNullOrEmpty(settings.Server))
            sb.Append($"-s {settings.Server} ");

        if (settings.Debug)
            sb.Append($"-d ");

        if (settings.SkipDuplicate)
            sb.Append($"--skip-duplicate ");
        sb.Length = sb.Length > 0 ? sb.Length-- : 0;

        var options = sb.ToString();
        Console.WriteLine($"dotnet {options}");
        var result = context.StartProcess("dotnet", $"{options}");
        return result == 0;
    }

    public static bool RunUnitTests(ICakeContext context, BuildTestSettings settings)
    {
        var options = $"test {settings.Solution.FullPath} --configuration={settings.Configuration} --verbosity:{settings.Verbosity}";
        Console.WriteLine($"dotnet {options}");
        var result = context.StartProcess("dotnet", $"{options}");
        return result == 0;
    }
}
