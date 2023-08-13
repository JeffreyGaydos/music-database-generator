# Music Database Generator
An idea to help organize your music into a SQL Server Database using the metadata already found on your music files. Additionally, some fields are left to the user to fill in manually or for use in custom music apps.

# Setup / Installation

## Database Creation

First, connect to a local database engine using a database management program of your choice. See [this tutorial](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16) on connecting locally to SQL Server Express. After connecting to your local database engine, open the file `db_initialize.sql` in the same management program and execute it. A database called `MusicLibrary`, 2 functions, and tables will be created, but no data will be added.

Next, entity framework must be configured to connect to this database you created. The connection strings in the files `App.config` and `Properties/Settings.settings` must be updated with the connection string you used to connect to your database engine earlier. Specifically, the `Data Source` parameter should be changed. If you followed the tutorial, it should be replaced with `{Your PC Name}/TEW_SQLEXPRESS`. This process is planned to be automated in the future.

## Prepare Your "Raw" Music Data

It is recommended that your music is in `.mp3` or `.flac` formats when using this tool. `.wav` files are partially supported, but have far less metadata than `.mp3` or `.flac` files. Most professionally published `.wav` files follow the below convention to define additional metadata for the file:
```
[Artist Name] - [Album Name] - [Track Order in Album] [Title].wav
```
If you must use `.wav` files, following the above convention will ensure that this tool adds that data to the database.

Additionally, though `.mp3` and `.flac` files have the _potential_ to have much more metadata in them, certain publishers of music may or may not include (many) fields. If you see a lot of `NULL` values associated with a specific track, look at the file's properties directly and add any missing data to the file itself, rather than directly insertting into the database.

## Data Generation

Until this is more streamlined, replace line 16 of the file `program.cs` with the folder where your music data is placed. This is a relative path starting at the base of this repository, absolute paths are not supported.

```
string musicFolder = "data"; //point this to where the mp3s are located, relative to the top level folder of this repo
```

Run the project using Viual Studio. The console will output logs of how many tracks/images it has processed and will close when finished. Afterwards, data should exist in the `MusicLibrary` table in the database engine you connected to above.

# Schema Explanation & Intent

For the details on each field of the table, see the readme in the `Schema` folder [here](). You'll notice that many fields are described as being "non-generated" or "manual". This is referring to those fields that are either too difficult to get out of regular `.mp3`/`.flac` files, or are too opinionated to be generated directly. Again, these fields remain as a placeholder for custom music apps that may use this database.