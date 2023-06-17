# Music Database Generator
An idea to help organize your music with extra metadata using a SQL Server Database. This repository searches through a folder and gathers metadata already in mp3 files to add to a database. Additionally, some fields are left to the user to fill in manually or for use in custom music apps.

# Setup / Installation

## Database Creation

First, connect to a local database engine using a database management program of your choice. See [this tutorial](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16) on connecting locally to SQL Server Express. After connecting to your local database engine, open the file `db_initialize.sql` in the same management program and execute it. A database called `MusicLibrary`, 2 functions, and tables will be created, but no data will be added.

Next, entity framework must be configured to connect to this database you created. The connection strings in the files `App.config` and `Properties/Settings.settings` must be updated with the connection string you used to connect to your database engine earlier. Specifically, the `Data Source` parameter should be changed. If you followed the tutorial, it should be replaced with `{Your PC Name}/TEW_SQLEXPRESS`. This process is planned to be automated in the future.

## Data Generation

Until this is more streamlined, replace line 16 of the file `program.cs` with the folder where your music data is placed. This is a relative path starting at the base of this repository, absolute paths are not supported.

```
string musicFolder = "data"; //point this to where the mp3s are located, relative to the top level folder of this repo
```

# Schema Explanation & Intent

## **Main**
This table stores metadata that can be easily mapped 1-to-1 each track
- :white_check_mark: TrackID
  - Use this ID to map songs in other tables. These values are unique across all tables that reference this ID.
- :white_check_mark: Title
  - The title of the song, directly from the mp3 metadata
- :white_check_mark: Duration
  - The duration of the song in seconds, directly from the mp3 metadata
- :white_check_mark: FilePath
  - The filepath of the song, based on where you tell the source code to look for your files
- :no_entry: AverageDecibels
  - This field subject to change. The intent is to aid in automatic volume balancing
- :clock9: MoodIDs
  - This is not a generated field. See `ListMood` table
- :clock9: OwnerID
  - Whoever originally owned the file
- :clock9: GenreID
  - The genre of the song, directly from the mp3 metadata
- :clock9: Linked
  - The song is often played before or after another song.
- :white_check_mark: ReleaseYear
  - The year that the song was released, directly from the mp3 metadata
- :white_check_mark: AddDate
  - The date when this file was first downloaded