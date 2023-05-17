using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProgramm
{
    public class EmailConfigReader
    {
        private static string projectPath;
        private static string configFileName;

        static EmailConfigReader()
        {
            projectPath = Directory.GetCurrentDirectory();
            configFileName = Path.Combine(projectPath, "emailconfig.json");
        }

        public static dynamic ReadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);
            string configContent = File.ReadAllText(configPath);
            dynamic config = JsonSerializer.Deserialize<dynamic>(configContent);
            return config;
        }
    }
}
