# Playlist Transfer Tool

This tool takes playlist files from various providers, inserts them into the database generated by the Music Database Generator tool, and then exports those playlists for one of those supported providers. This can essentially convert playlists from 1 music app to another, cross platform. This tool also supports merging playlists that are named the same. Note that the music database is required for this tool to work due to the need to match music by its metadata when paths differ across devices.

## Supported Playlist Types (for both Importing and Exporting)

- Groove Music Playlists
  - Typically found in the `C:\Users\YourUserName\Music\Playlists\Playlists from Groove Music` directory as `.zpl` files
  - Note that these `.zpl` files are an old format that Groove no longer creates by default when creating a new playlist. See m3u8 files section below.  
- m3u files (Typically Samsung playlists)
  - Found directly in the `Music` folder of your Android device, but only after exporting those playlists, as `.m3u` files
    - Export your Samsung Music playlists by pressing the 3 dots in the corner, going to `Settings > Manage playlists > Export my playlists`
- m3u8 files
  - Found in newer versions of Groove Music at `C:\Users\YourUserName\Music\Playlists`
    - m3u8 files are essentially the same as m3u files and can work in place of each other with an extension rename, but Groove uses absolute paths whereas other parties (like Samsung Music playlists) use relative paths from the `Music` folder

> [!NOTE]
> `.m3u` and `.m3u8` files are largly interoperable, but depending on which device the file was created on, they may use a different path that will not work between devices. That being said, if you need to convert between `m3u` and `m3u8` files form different devices, this tool will work but may not be worth your time as you can just rename the extension to the desired format and fix any paths that aren't found.

## Configuring and Running the Tool

First, you must run the Music Database Generator tool to create a database so that this tool can match music data between playlist providers. See that tool's [Readme](https://github.com/JeffreyGaydos/music-database-generator/blob/main/README.md) for details. Of course, you need to use the same database provider in this tool as you did to generate the database (SQLite or MSSQL).

After you have a music database or have updated the database with the most recent music from your playlists, you can use this tool regardless of if the file structure between devices is the same. Additionally, if you used a custom connection string in the `MusicDatabaseGenerator` make sure you use the same connection string in this tool as you did when generating your music database.

This tool supports the following configuration:
- `playlistImportPath`: The path where the tool will look for supported playlist files, including a mixture of different file types
- `playlistExportPath`: The path where the tool will export playlist files after inserting them into the database
- `playlistExportType`: A keyword that designates which kind of playlist file you want this tool to export to
  - Supported values are `M3U`, `M3U8` or `Groove`
- `mergePlaylistWithSameName`: When set to `true`, this tool will look for playlists already in the database with the same name and udpate existing tracks or insert new tracks. When set to `false` this tool will replace the existing playlist with the one you are importing at `playlistImportPath`.
- `deleteExistingPlaylists`: When set to `true`, the tool will first delete any and all playlists already in the database, then parse, insert and convert your playlist files. If you are using this tool strictly for playlist conversion or always provide the tool with all your playlist files, set this value to `true`.

Below is an example config and the default for this repo:
```js
{
  "Settings": {
    "playlistImportPath": "..\\..\\input",
    "playlistExportPath": "..\\..\\output",
    "playlistExportType": "M3U",
    "mergePlaylistWithSameName": true,
    "deleteExistingPlaylists": false
  }
}
```

Additional Behaviors of this Tool:
- Export Only
  - If the tool is provided with no input files at the `playlistImportPath`, it will export any playlists already in the database as the type specified in `PlaylistExportType`
- Exports Imported Playlist Files Only
  - If the tool is provided with an input file at the `playlistImportPath`, it will import that playlist file (if not skipped, see below) and then export only that imported playlist file. It will not export any other playlists already in the database.
  - See above if you need to export playlists already in the database
- Skipped Processing
  - If you have already imported a playlist to the database and the playlist file remains without modification in the import path at `playlistImportPath`, this tool will skip importing this file.
  - If for whatever reason you need to re-import a playlist file, simply make a change to the file, save it, undo that change, and save it again to make it look like changes were made
- Playlist Track Order is Maintained

## Running The Tool

Once you've configured the tool, place any and all playlsits you intend to transfer into the folder you specified in the `playlistImportPath` config (thie `input` folder by default). They can be any mixture of supported playlist files. Run the executable called `PlaylistTransferTool`. You should see logging of the configuration you've entered as well as updates on which playlist file it is parsing and any issues it runs into. A copy of the logs can be found in the `PlaylistTransferTool/transfer_tool_log.txt` file. Note that the tool will overwrite any logs previously in this file, so save a copy if you intend to refer to previous runs' logs. Also note that the tool will process playlist files in alphabetical order. If the tool doesn't crash (or crashes after finishing exporting some playlists), you should find playlist files in the folder you specified in the `playlistExportPath` config (the `output` folder by default). You can then import these files according to each provider's instructions.

After the tool completes, a list of tracks that we could not find in the database will be displayed. If files are found at the paths specified, make sure you re-run the `MusicDatabaseGenerator` so that the most recent path is updated in the database. Alternatively, you can manually add these values to the exported playlists if you know where those files are on the destination device.

## Migrations

If you have previously used this tool to generate a database and see any files in the Migrations folder of the kind of database you are using (`MusicDatabaseGenerator/Schema/MSSQL/Migrations` or `MusicDatabaseGenerator/Schema/SQLite/Migrations`), read the Migrations section on the `MusicDatabaseGenerator`'s readme [here](https://github.com/JeffreyGaydos/music-database-generator/blob/main/README.md#migrations).