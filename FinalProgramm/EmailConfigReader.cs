using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProgramm
{
    public class EmailConfigReader
    {
        private const string ConfigFileName = @"C:\Users\Dr\source\repos\FinalProgramm\FinalProgramm\emailconfig.json";

        public static dynamic ReadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            string configContent = File.ReadAllText(configPath);
            dynamic config = JsonSerializer.Deserialize<dynamic>(configContent);
            return config;
        }
    }
}
