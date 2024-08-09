using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator
{
    public class ConfiguratorValues
    {
        public string pathToSearch { get; private set; }
        public bool generateAlbumArtData { get; private set; }
        public bool generateMusicMetadata { get; private set; }
        public bool deleteExistingData { get; private set; }
        public bool runMigrations { get; private set; }
        public DatabaseProvider databaseProvider { get; private set; }

        public ConfiguratorValues(string pathToSearch, bool generateAlbumArtData, bool generateMusicMetadata, bool deleteExistingData, bool runMigrations, DatabaseProvider dbProvider)
        {
            this.pathToSearch = pathToSearch;
            this.generateAlbumArtData = generateAlbumArtData;
            this.generateMusicMetadata = generateMusicMetadata;
            this.deleteExistingData = deleteExistingData;
            this.runMigrations = runMigrations;
            databaseProvider = dbProvider;
        }
    }

    public class Configurator
    {
        private MusicLibraryContext _context;
        private LoggingUtils _logger;
        private Regex _goSplitter = new Regex(@"GO", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex _stripInitialCatalog = new Regex(@"(?<=;)initial catalog=MusicLibrary;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex _removeCommentsSQLite = new Regex(@"(\/\*[^\/]+\*\/)|(--[^\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private string _connectionString;
        private string _initialConnectionString;
        private bool _databasePresent = false;

        public Configurator(MusicLibraryContext context, LoggingUtils logger) {
            _context = context;
            _logger = logger;
        }

        public ConfiguratorValues HandleConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile(Directory.GetParent("./") + "../../../appsettings.json").Build();

            var settings = config.GetSection("Settings").GetChildren().ToDictionary(r => r.Key, r => r.Value);

            //when creating the database, there is no initial catalog we can connect to, but when
            //we need to create a function, we have to connect specifically to this new database
            _connectionString = _context.Database.Connection.ConnectionString;
            _initialConnectionString = _stripInitialCatalog.Replace(_connectionString, ""); 
            ConfiguratorValues values = new ConfiguratorValues(
                settings["MusicFolderPathAbsolute"],
                settings["GenerateAlbumArtData"] == "True",
                settings["GenerateMusicMetadata"] == "True",
                settings["DeleteDataOnGeneration"] == "True",
                settings["RunMigrations"] == "True",
                /*BUILD_PROCESS_SQLite: INACTIVE
                 DatabaseProvider.SQLite
                 /**/
                ///*BUILD_PROCESS_MSSQL: ACTIVE
                 DatabaseProvider.MSSQL
                 /**/
                );

            _logger.GenerationLogWriteData("_CONFIGURATION:__________________________________________________________________");

            _logger.GenerationLogWriteData($"Connecting to database via connection string \"{_connectionString}\"...");
            _logger.GenerationLogWriteData($"{(values.deleteExistingData ? "Deleting existing data and resetting IDs..." : "Existing database persisted...")}");
            _logger.GenerationLogWriteData($"{(values.runMigrations ? "Running migrations..." : "Skipped running migrations.")}");
            _logger.GenerationLogWriteData($"Generating a {values.databaseProvider} Database");

            if (values.runMigrations)
            {
                RunMigrations(values.databaseProvider);
            }

            InitializeDatabaseIfNeeded(values.databaseProvider);

            if (values.deleteExistingData)
            {
                DeleteExistingDatabase(values.databaseProvider);
            }

            _logger.GenerationLogWriteData($"Searching for data at location \"{values.pathToSearch}\"");
            _logger.GenerationLogWriteData($@"{(values.generateAlbumArtData ?
                values.generateMusicMetadata ?
                    "Will generate music metadata and album art metadata"
                    : "Will generate album art metadata ONLY"
                : values.generateMusicMetadata ?
                    "Will generate music metadata ONLY"
                    : "Config was set to generate no data. Will still check for files with limited meatadata.")}");

            _logger.GenerationLogWriteData("_________________________________________________________________________________");

            return values;
        }

        private void DeleteExistingDatabase(DatabaseProvider provider)
        {
            switch(provider)
            {
                case DatabaseProvider.SQLite:
                    string deleteSql = File.ReadAllText("../../Schema/SQLite/sqlite_delete.sql").Replace("\\r\\n", @"
").Replace("\\t", "  ");
                    deleteSql = _removeCommentsSQLite.Replace(deleteSql, "");
                    var deletes = deleteSql.Split(new string[] { "DELETE" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach(var sql in deletes)
                    {
                        if (sql[0] == '\r') continue;
                        ExecuteNonQueryUsingSQLiteConnection($"DELETE{sql}");
                    }
                    break;
                case DatabaseProvider.MSSQL:
                default:
                    ExecuteNonQueryUsingConnection(File.ReadAllText("../../Schema/db_delete.sql").Replace("\\r\\n", @"
").Replace("\\t", "  "));
                    break;
            }
        }

        private void InitializeDatabaseIfNeeded(DatabaseProvider provider)
        {
            switch (provider)
            {
                case DatabaseProvider.SQLite:
                    string initSQLite = _removeCommentsSQLite.Replace(File.ReadAllText("../../Schema/SQLite/sqlite_initialize.sql"), "");
                    initSQLite = initSQLite.Replace("\\r\\n", @"
").Replace("\\t", "  ");
                    var SQLiteToExecute = initSQLite.Split(new string[] { "CREATE" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach(var sql in SQLiteToExecute)
                    {
                        if (sql[0] == '\r') continue;
                        ExecuteNonQueryUsingSQLiteConnection($"CREATE{sql}");
                    }
                    _databasePresent = true;
                    break;
                case DatabaseProvider.MSSQL:
                default:
                    string initSQL = File.ReadAllText("../../Schema/db_initialize.sql");
                    List<string> splitInit = _goSplitter.Split(initSQL).Where(sql => !string.IsNullOrEmpty(sql)).ToList();

                    foreach (string statement in splitInit)
                    {
                        if (statement.Contains("IF (SELECT [dbo].[MusicViewExists] ('MainDataJoined')) = 0 AND (SELECT [dbo].[MusicViewExists] ('LeadArtists')) = 0"))
                        {
                            //This statement specifically determines if we should continue to attempt to create views
                            if (ExecuteQueryIntUsingConnection(statement) == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            ExecuteNonQueryUsingConnection(statement);
                        }

                        if (statement.Contains("CREATE DATABASE MusicLibrary"))
                        {
                            _databasePresent = true;
                        }
                    }
                    break;
            }
        }

        private void RunMigrations(DatabaseProvider provider)
        {
            if(provider != DatabaseProvider.SQLite)
            {
                ExecuteNonQueryUsingConnection(File.ReadAllText("../../Schema/Migrations/dbm_20231107_moodToPlaylists.sql")
                .Replace("\\r\\n", @"
").Replace("\\t", "  "));
            }
            //SQLite implemented after all current migrations were made. Future migrations will be implemented for SQLite and MSSQL
        }

        private void ExecuteNonQueryUsingConnection(string sql)
        {
            using (SqlConnection connection = new SqlConnection(_databasePresent ? _connectionString : _initialConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }            
        }

        private void ExecuteNonQueryUsingSQLiteConnection(string sql)
        {
            using (SQLiteConnection conn = new SQLiteConnection("data source=../../../MusicLibrary.db"))
            {
                conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private int ExecuteQueryIntUsingConnection(string sql)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    return (int)command.ExecuteScalar();
                }
            }
        }
    }
}
