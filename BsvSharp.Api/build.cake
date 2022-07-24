#load "../build/arguments.cake"
#load "../build/utilities.cake"

var builder = new Builder(Context);
var target = Argument("target", "Build");

Task("CleanSolution")
    .IsDependentOn("FindSolution")
    .Does(()=> {
        builder.CleanSolution(vars.solution, vars);
    });

Task("FindSolution")
    .Does(()=> {
        vars.solution = builder.FindSolution("./*.sln");
    });

Task("BuildSolution")
    .IsDependentOn("FindSolution")
    .Does(() => {
        builder.BuildSolution(vars.solution, vars);
    });

Task("BuildPackages")
    .Does(()=> {
        builder.BuildPackages(vars.solution, vars, "UnitTest");
    });

Task("PublishPackages")
    .Does(()=> {
        builder.PublishPackages(vars.solution, vars, "UnitTest");
    });

Task("RunUnitTests")
    .Does(()=> {
        builder.RunUnitTests(vars.solution, vars);
    });

Task("Build")
    .IsDependentOn("BuildSolution")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("BuildPackages")
    .IsDependentOn("PublishPackages");

Task("Clean")
    .IsDependentOn("CleanSolution");

Task("Publish")
    .IsDependentOn("FindSolution")
    .IsDependentOn("PublishPackages");

Task("Test")
    .IsDependentOn("FindSolution")
    .IsDependentOn("RunUnitTests");

RunTarget(target);
