using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public ConfiguratorValues(string pathToSearch, bool generateAlbumArtData, bool generateMusicMetadata, bool deleteExistingData, bool runMigrations)
        {
            this.pathToSearch = pathToSearch;
            this.generateAlbumArtData = generateAlbumArtData;
            this.generateMusicMetadata = generateMusicMetadata;
            this.deleteExistingData = deleteExistingData;
            this.runMigrations = runMigrations;
        }
    }

    public class Configurator
    {
        private MusicLibraryContext _context;
        private LoggingUtils _logger;
        private Regex _goSplitter = new Regex(@"GO", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex _stripInitialCatalog = new Regex(@"(?<=;)initial catalog=MusicLibrary;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
                settings["RunMigrations"] == "True"
                );

            _logger.GenerationLogWriteData("_CONFIGURATION:__________________________________________________________________");

            _logger.GenerationLogWriteData($"Connecting to database via connection string \"{_connectionString}\"...");
            _logger.GenerationLogWriteData($"{(values.deleteExistingData ? "Deleting existing data and resetting IDs..." : "Existing database persisted...")}");
            _logger.GenerationLogWriteData($"{(values.runMigrations ? "Running migrations..." : "Skipped running migrations.")}");

            if (values.runMigrations)
            {
                RunMigrations();
            }

            //InitializeDatabaseIfNeeded();

            if (values.deleteExistingData)
            {
                DeleteExistingDatabase();
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

        private void DeleteExistingDatabase()
        {
            ExecuteNonQueryUsingConnection(File.ReadAllText("../../Schema/db_delete.sql").Replace("\\r\\n", @"
").Replace("\\t", "  "));
        }

        private void InitializeDatabaseIfNeeded()
        {
            string initSQL = File.ReadAllText("../../Schema/db_initialize.sql");
            List<string> splitInit = _goSplitter.Split(initSQL).Where(sql => !string.IsNullOrEmpty(sql)).ToList();

            foreach(string statement in splitInit)
            {
                if(statement.Contains("IF (SELECT [dbo].[MusicViewExists] ('MainDataJoined')) = 0 AND (SELECT [dbo].[MusicViewExists] ('LeadArtists')) = 0"))
                {
                    //This statement specifically determines if we should continue to attempt to create views
                    if (ExecuteQueryIntUsingConnection(statement) == 0)
                    {
                        break;
                    }
                } else
                {
                    ExecuteNonQueryUsingConnection(statement);
                }

                if(statement.Contains("CREATE DATABASE MusicLibrary"))
                {
                    _databasePresent = true;
                }
            }
        }

        private void RunMigrations()
        {
            ExecuteNonQueryUsingConnection(File.ReadAllText("../../Schema/Migrations/dbm_20231107_moodToPlaylists.sql").Replace("\\r\\n", @"
").Replace("\\t", "  "));
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
