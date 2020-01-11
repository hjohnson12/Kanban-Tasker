using LeaderAnalytics.AdaptiveClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace KanbanTasker.Services
{
    


    public static class ConnectionstringUtility
    {
        private static IConfigurationRoot config;

        private static void BuildConfig(string filePath)
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            string configFile = Path.Combine(filePath, "appsettings.Development.json");

            if (!File.Exists(configFile))
                throw new Exception($"File not found {configFile}.");

            configBuilder.AddJsonFile(configFile);
            config = configBuilder.Build();
        }


        public static string GetConnectionString(string filePath, string apiName, string providerName)
        {
            IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints(filePath, false);
            return endPoints.First(x => x.API_Name == apiName && x.ProviderName == providerName).ConnectionString;
        }


        public static void PopulateConnectionStrings(string filePath, IEnumerable<IEndPointConfiguration> endPoints)
        {
            if (! (endPoints?.Any() ?? false))
                return;

            BuildConfig(filePath);

            foreach (IEndPointConfiguration ep in endPoints)
                ep.ConnectionString = BuildConnectionString(ep.ConnectionString);

        }

        public static string BuildConnectionString(string connectionString)
        {
                connectionString = connectionString.Replace("{MySQL_UserName}", config["Data:MySQLUserName"]);
                connectionString = connectionString.Replace("{MySQL_Password}", config["Data:MySQLPassword"]);

            return connectionString;
        }
    }
}
