# Music Database Generator
This tool takes your existing set of local music files on your file system and transforms them into a SQL Server Database. Updates, inserts, and deletes to the generated database are determined based on changes you make to the files themselves. The database generated from this tool was designed for use in custom music apps and has some empty tables you may use or simply delete. See the readme in the `Schema` folder [here](https://github.com/JeffreyGaydos/music-database-generator/blob/main/MusicDatabaseGenerator/Schema/SCHEMA_README.md) for what the database looks like exactly.

This project uses `TagLib` to get metadata from MP3 files: https://github.com/taglib/taglib

NOTE: Only data stored within music files are used during generation. This tool does not search the internet or other 3rd parties to get additional data.

# Setup / Installation

## Database Connection

First, the tool needs to connect to a local database engine using a database management program of your choice. This tool supports MSSQL and SQLite database generation. For each release, there are 2 builds of each kind of tool, named according to the database engine that it supports.

> [!NOTE]
> If you intend to use this tool in conjunction with the [Synthia Music App](https://github.com/JeffreyGaydos/SynthiaMusicApp), a SQLite database is required.

### MSSQL 
First, you will need to [download SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16) or another way to execute a sql script on SQL Server. Next, [download SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads). Be sure you get the "Express" version, as this project was not tested using the "Developer" version. If you use the "Basic" settings during installation, this project will be set up automatically to point to your newly created local database engine.

If your database engine is not named `localhost\SQLEXPRESS`, go to the file `App.config` in this project and change the line that looks like...
```xml
<add name="MusicLibraryContext" connectionString="data source=localhost\SQLEXPRESS;initial catalog=MusicLibrary;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework" providerName="System.Data.SqlClient" />
```
so that the `connectionString` attribute has the name of your database as the `data source`. If your database name is something like `localhost\MY_LOCAL_DATABASE_NAME`, this line should look like:
```xml
<add name="MusicLibraryContext" connectionString="data source=localhost\MY_LOCAL_DATABASE_NAME;initial catalog=MusicLibrary;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework" providerName="System.Data.SqlClient" />
```

### SQLite
First [download SQLite](https://sqlite.org/download.html). Then, it is recommended that you [download the DB Browser](https://sqlitebrowser.org/dl/) for SQLite so you can easily execute SQL and view your database. The tool (once run) will then generate a `.db` file that you can open with the DB browser called `MusicLibrary.db` at the root of the built folder. It is important that the name of the database remains the same if you intend to use this tool to update the database in the future.

## Prepare Your "Raw" Music Data

It is recommended that your music is in `.mp3`, `.m4a`, `.wma` or `.flac` formats when using this tool. `.wav` files are partially supported but have far less metadata than the aforementioned file types. Most professionally published `.wav` files follow the below naming convention to define additional metadata for the file:
```
[Artist Name] - [Album Name] - [Track Order in Album] [Title].wav
```
If you must use `.wav` files, following the above naming convention will ensure that this tool adds that data to the database. A list of `.wav` files that you may want to replace can be found in `files_with_limited_data.txt` at the root of this project (which is created upon running the project). Note that files listed in this log are not checked for duplicates (i.e. if you have `.wav` and `.mp3` files for the same song, these `.wav` files will still appear in this log).

Additionally, though the fully supported file types have the _potential_ to have much more metadata in them, certain publishers of music may or may not include (many) fields. If you see a lot of `NULL` values associated with a specific track, look at the file's properties directly and add any missing data to the file itself, rather than directly inserting into the database. Additional information on how data was parsed for particular files can be found in `generation.txt` in the root of this project (which is created upon running the project).

## Data Generation

First, use the config variables found in `appsettings.json` to configure how you want this tool to run. The following are available options:
- `MusicFolderPathAbsolute`: put the absolute path to your music directory that you want this tool to read. Note that this tool was built under the expectation that the user's music would be organized in 2 structures:

      Structure 1:
       - MusicFolderPathAbsolute
          ∟ Artist name and album name (folder)
            ∟ music files
            ∟ cover art files
      
      Structure 2: 
       - MusicFolderPathAbsolute
         ∟ Artist name (folder)
            ∟ Album name (folder)
              ∟ music files
              ∟ cover art files
       
  - Any combination of these 2 structures in the same folder is supported. If you really want to add a path relative to the root of this project, prepend `../../` to this config.
- `GenerateAlbumArtData`: set to true if you want metadata from your album art files to be inserted into the database
  - Supported image file types are `.jpg`, `.png`
  - Note that many professionally produced music files have image metadata built-in to the file that is not directly visible to the file system. This tool looks for those "hidden" images as well as those images directly viewable in the file system.
- `GenerateMusicMetadata`: set to true if you want metadata from your music files to be inserted into the database
  - Supported music file types are: `.mp3`, `.m4a`, `.wma` or `.flac`, with `.wav` files being supported but not recommended (see above section)
- `DeleteDataOnGeneration`: set to true if you want any and all data existing in the database to be cleared for a full re-generation of your music metadata.
  - This is generally only needed if you need to go from a very large database to a very small one, and don't need the tool to check each file before deleting it.
  - Regardless of the value of this config, database creation & setup will be performed automatically if needed

Next, run the project by finding the executable `MusicDatabaseGenerator.exe` in the `MusicDatabaseGenerator/bin/Release` folder. The console will output logs of how many tracks/images it has processed and will close when finished. This project takes roughly 1 second to insert 1 new track or update 1 existing track on average, depending on the size of your music library. After importing your files once, this project will check the `modified at` date of your music files to determine if the file needs an update. This process is much faster and runs at about 20 milliseconds per track. When an update occurs, the `TrackID` is preserved as much as possible. If you need to delete a track, simply delete the file and this project will handle removing those references. Essentially, if you need to make an update to the data in your generated database, update the files using your file system and re-run the tool.

After each run, and if it succeeded, data should exist in the `MusicLibrary.dbo.Main` table in the database engine you connected to above (among the various other tables). Additionally, you will find the following logs at the root of this project:
- `generation.txt`: Has more details about what data was added and from where for each track
  - Most helpful for debugging why a file has unexpected or missing metadata
- `files_with_limited_data.txt`: lists each file that is a `.wav` file, because it is likely to have a limited amount of metadata

:warning: These logs are not appended to, but replaced after each run. Don't forget to copy them if you plan on referencing past runs' logs later

To add your playlists to this database, use the `PlaylistTransferTool` and see its [Readme](https://github.com/JeffreyGaydos/music-database-generator/blob/main/PlaylistTransferTool/README.md).

# Schema Explanation & Intent

For the details on each field of the table, see the readme in the `Schema` folder [here](https://github.com/JeffreyGaydos/music-database-generator/blob/main/MusicDatabaseGenerator/Schema/SCHEMA_README.md). You'll notice that many fields are described as being "non-generated" or "manual". This is referring to those fields that are either too difficult to get out of regular music files or are too opinionated to be generated directly. Again, these fields remain as a placeholder for custom music apps that may use this database.

## Migrations

Migrations are small scripts that attempt to update an existing database's schema to a newer version. If you have a version of this database already loaded with your music data and prefer not to re-import that data when the schema changes (especially for large collections of music), you can use the migrations to get your schema updated with minimal data-loss.

All migration scripts are found in the relevant database's folder: `MusicDatabaseGenerator/Schema/MSSQL/Migrations` for the Microsoft SQL Server version and `MusicDatabaseGenerator/Schema/SQLite/Migrations` for the SQLite version. All migration scripts are named according to the date at which they were written. i.e. the migration `dbm_20240725_playlistTracksTweaks.sql` was written on 7/25/2024. If you created a database before that date don't want to re-import your data, run that migration. There is no need to run a migration that is dated before you first created the database.

Alternatively, or if a migration fails, you can re-create the database and re-import all data using the `MusicDatabaseGenerator/Schema/MSSQL/db_drop.sql` to delete the schema of your database and then `MusicDatabaseGenerator/Schema/MSSQL/db_initialize.sql` to re-initialize the schema (there are also SQLite counterparts in the `SQLite` folder). Then re-import your data by running the tool.