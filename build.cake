using System.Dynamic;
using System.IO.Directory;

#load "./build/utilities.cake"

var builder = new Builder(Context);
dynamic vars = new ExpandoObject();

vars.commands = new List<BuildCommand>();
vars.root = GetCurrentDirectory();

// Setup CafeLib components (name, version).
public static readonly IDictionary<string, string> Components = new Dictionary<string, string>
{
    {"BsvSharp", "2.0.9"},
    {"BsvSharp.Api", "2.0.9"}
};

// Get arguments.
vars.build = Argument<string>("build", "");
vars.config = Argument<string>("config", "Debug");
vars.nugetKey = Argument<string>("nugetKey", null);
vars.nugetServer = Argument<string>("nugetServer", "C:/Nuget/repo");
vars.nugetDebug = Argument<bool>("nugetDebug", false);
vars.nugetSkipDup = Argument<bool>("nugetSkipDup", false);

var target = Argument("target", "Build");

Task("SelectComponents")
    .Does(() => {
        if (Components.TryGetValue(vars.build, out string component))
        {
            vars.components = new Dictionary<string, string>();
            vars.components.Add(vars.build, component);
        }
        else
        {
            vars.components = Components;
        }     
    });

Task("ConstructCommands")
    .IsDependentOn("SelectComponents")
    .Does(() => {
        foreach (var component in vars.components)
        {
            var cmd = new BuildCommand();
            cmd.ComponentName = $"{component.Key}";
            cmd.BuildPath = $"{GetCurrentDirectory()}/{component.Key}";
            cmd.Options["component"] = component.Key;
            cmd.Options["buildversion"] = component.Value;
            cmd.Options["config"] = vars.config;
            if (vars.nugetKey != null) cmd.Options["nugetKey"] = vars.nugetKey;
            if (vars.nugetServer != null) cmd.Options["nugetServer"] = vars.nugetServer;
            if (vars.nugetDebug != null) cmd.Options["nugetDebug"] = vars.nugetDebug.ToString().ToLower();
            if (vars.nugetSkipDup != null) cmd.Options["nugetSkipDup"] = vars.nugetSkipDup.ToString().ToLower();
            vars.commands.Add(cmd);
        }
    });

Task("Build")
    .IsDependentOn("ConstructCommands")
    .Does(() => {
        builder.BuildComponents(vars.commands);
    });

Task("Clean")
    .IsDependentOn("ConstructCommands")
    .Does(() => {
        builder.CleanComponents(vars.commands);
    });

Task("Publish")  // Build pack publish to nuget server.
    .IsDependentOn("ConstructCommands")
    .Does(() => {
        builder.PublishComponents(vars.commands);
    });    

Task("Test") 
    .IsDependentOn("ConstructCommands")
    .Does(() => {
        builder.TestComponents(vars.commands);
    });    

RunTarget(target);
