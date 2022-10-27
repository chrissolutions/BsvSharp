using System.Dynamic;

dynamic vars = new ExpandoObject();
vars.root = Arguments<string>("root", "..");
vars.config = Argument<string>("config", "Debug");
vars.version = Argument<string>("buildversion", null);
vars.verbosity = Argument<string>("verbosity", "minimal");
vars.nugetKey = Argument<string>("nugetKey", null);
vars.nugetServer = Argument<string>("nugetServer", null);
vars.nugetDebug = Argument<bool>("nugetDebug", false);
vars.nugetSkipDup = Argument<bool>("nugetSkipDup", false);
