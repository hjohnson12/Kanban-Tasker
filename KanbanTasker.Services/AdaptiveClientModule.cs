using System;
using System.Collections.Generic;
using System.Text;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.Utilities;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using KanbanTasker.Model;

namespace KanbanTasker.Services
{
    public class AdaptiveClientModule : IAdaptiveClientModule
    {
        public void Register(RegistrationHelper registrationHelper)
        {
            registrationHelper

            // Services - MSSQL
            .RegisterService<MSSQL.BoardServices, IBoardServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MSSQL)
            .RegisterService<MSSQL.TaskServices, ITaskServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MSSQL)
            .RegisterService<MSSQL.TagServices, ITaskServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MSSQL)

            // Services - MySQL
            .RegisterService<MySQL.BoardServices, IBoardServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MySQL)
            .RegisterService<MySQL.TaskServices, ITaskServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MySQL)
            .RegisterService<MySQL.TagServices, ITaskServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MySQL)

            // Services - SQLite
            .RegisterService<SQLite.BoardServices, IBoardServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.SQLite)
            .RegisterService<SQLite.TaskServices, ITaskServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.SQLite)
            .RegisterService<SQLite.TagServices, ITaskServices>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.SQLite)

            // Services - WebAPI
            .RegisterService<WebAPI.BoardServices, IBoardServices>(EndPointType.HTTP, API_Name.Kanban, DatabaseProvider.WebAPI)
            .RegisterService<WebAPI.TaskServices, ITaskServices>(EndPointType.HTTP, API_Name.Kanban, DatabaseProvider.WebAPI)

            // DbContexts
            .RegisterDbContext<Database.Db>(API_Name.Kanban)

            // Migration Contexts
            .RegisterMigrationContext<Database.Components.MSSQL.Db_MSSQL>(API_Name.Kanban, DatabaseProvider.MSSQL)
            .RegisterMigrationContext<Database.Components.MySQL.Db_MySQL>(API_Name.Kanban, DatabaseProvider.MySQL)
            .RegisterMigrationContext<Database.Components.SQLite.Db_SQLite>(API_Name.Kanban, DatabaseProvider.SQLite)

            // Database Initializers
            .RegisterDatabaseInitializer<Database.Components.MSSQL.DatabaseInitializer>(API_Name.Kanban, DatabaseProvider.MSSQL)
            .RegisterDatabaseInitializer<Database.Components.MySQL.DatabaseInitializer>(API_Name.Kanban, DatabaseProvider.MySQL)
            .RegisterDatabaseInitializer<Database.Components.SQLite.DatabaseInitializer>(API_Name.Kanban, DatabaseProvider.SQLite)

            // Service Manifests
            .RegisterServiceManifest<ServiceManifest, IServiceManifest>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MSSQL)
            .RegisterServiceManifest<ServiceManifest, IServiceManifest>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.MySQL)
            .RegisterServiceManifest<ServiceManifest, IServiceManifest>(EndPointType.DBMS, API_Name.Kanban, DatabaseProvider.SQLite)
            .RegisterServiceManifest<ServiceManifest, IServiceManifest>(EndPointType.HTTP, API_Name.Kanban, DatabaseProvider.WebAPI)

            // EndPoint Validator
            .RegisterEndPointValidator<MSSQL_EndPointValidator>(EndPointType.DBMS, DatabaseProvider.MSSQL)
            .RegisterEndPointValidator<MySQL_EndPointValidator>(EndPointType.DBMS, DatabaseProvider.MySQL)
            .RegisterEndPointValidator<Database.Components.SQLite.EndPointValidator>(EndPointType.DBMS, DatabaseProvider.SQLite)
            .RegisterEndPointValidator<Http_EndPointValidator>(EndPointType.HTTP, DatabaseProvider.WebAPI)

            // DbContextOptions
            .RegisterDbContextOptions<Database.Components.MSSQL.DbContextOptions_MSSQL>(DatabaseProvider.MSSQL)
            .RegisterDbContextOptions<Database.Components.MySQL.DbContextOptions_MySQL>(DatabaseProvider.MySQL)
            .RegisterDbContextOptions<Database.Components.SQLite.DbContextOptions_SQLite>(DatabaseProvider.SQLite);
        }
    }
}
