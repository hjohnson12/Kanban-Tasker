using LeaderAnalytics.AdaptiveClient;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace KanbanTasker.Services.Database.Components.SQLite
{
    public class EndPointValidator : IEndPointValidator
    {

        public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint)
        {
            bool result = true;

            using (SqliteConnection db =new SqliteConnection(endPoint.ConnectionString))
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
