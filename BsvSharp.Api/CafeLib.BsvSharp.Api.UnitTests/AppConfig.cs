using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CafeLib.BsvSharp.Api.UnitTests
{
    public static class AppConfig
    {
        public static void SetupApiEnvironmentVariable(string apiVar)
        {
            Environment.SetEnvironmentVariable(apiVar, GetApiKey());
        }

        #region Helpers

        private static string GetApiKey([CallerFilePath] string path = null)
        {
            // appsettings.workspace.json for custom developer configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(path))
                .AddJsonFile("apikey.json")
                .Build();

            return configuration["ApiKey"];
        }

        #endregion

    }
}
