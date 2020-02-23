using LeaderAnalytics.AdaptiveClient;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;
using Windows.Storage;

namespace KanbanTasker.Services.Database.Components.SQLite
{
    public class EndPointValidator : IEndPointValidator
    {

        public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint)
        {
            bool result = true;
            // UWP BUG: 
            // Bug acknowledged here by EF member: https://github.com/dotnet/efcore/issues/19754
            // SQLite requires full path now because of UWP constraints with EF Core
            // Workaround
            string dbFileName = endPoint.ConnectionString.Split('=')[1];
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbFileName);

            using (SqliteConnection db =new SqliteConnection("Filename=" + dbPath))
            {
                try
                {
                    db.Open();
                }
                catch (Exception)
                {
                    result = false;
                }
            }
            return result;
        }
    }
}
