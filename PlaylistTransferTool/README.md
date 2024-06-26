# Playlist Transfer Tool

This tool takes playlist files from various providers, inserts them into the database generated by the Music Database Generator tool, and then exports those playlists for one of those supported providers. This can essentially convert playlists from 1 music app to another, cross platform. This tool also supports merging playlists that are named the same.

## Supported Playlist Types (for both Importing and Exporting)

- Groove Music Playlists
  - Typically found in the `C:\Users\YourUserName\Music\Playlists\Playlists from Groove Music` directory as `.zpl` files
- Samsung Music Playlists
  - Found directly in the `Music` folder of your Android device, but only after exporting those playlists, as `.m3u` files
    - Export your Samsung Music playlists by pressing the 3 dots in the corner, going to `Settings > Manage playlists > Export my playlists`

## Configuring and Running the Tool

First, you must run the Music Database Generator tool to create a database so that this tool can match music data between playlist providers. See that tool's [Readme](https://github.com/JeffreyGaydos/music-database-generator/blob/main/README.md) for details. Of course, you need to use the same database provider in this tool as you did to generate the database (SQLite or MSSQL).

After you have a music database or have updated the database with the most recent music from your playlists, you need to make sure the folder structure of all your devices are exactly the same from the `Music` folder onwards (if you are not transferring playlists from different devices, this is not applicable). Additionally, make sure you use the same connection string in this tool as you did when generating your music database.

This tool supports the following configuration:
- `playlistImportPath`: The path where the tool will look for supported playlist files, including a mixture of different file types
- `playlistExportPath`: The path where the tool will export playlist files after inserting them into the database
- `playlistExportType`: A keyword that designates which kind of playlist file you want this tool to export to
  - Supported values are `Samsung` or `Groove`
- `mergePlaylistWithSameName`: When set to `true`, the tool will union the tracks found in any 2 or more playlists that have the same name, or with any playlists that already exist in the database
  - To update a playlist, you will need to first delete that playlist in the database, then reimport your playlist using this tool.
- `deleteExistingPlaylists`: When set to `true`, the tool will first delete any playlists already in the database, then parse, insert and convert your playlist files. If you are using this tool strictly for playlist conversion or always provide the tool with all your playlist files, set this value to `true`.

Below is an example config and the default for this repo:
```js
{
  "Settings": {
    "playlistImportPath": "..\\..\\input",
    "playlistExportPath": "..\\..\\output",
    "playlistExportType": "Samsung",
    "mergePlaylistWithSameName": true,
    "deleteExistingPlaylists": true
  }
}
```

Next, place any and all playlsits you intend to transfer into the folder you specified in the `playlistImportPath` config (thie `input` folder by default). They can be any mixture of supported playlist files. Run the executable called `PlaylistTransferTool`. You should see logging of the configuration you've entered as well as updates on which playlist file it is parsing and any issues it runs into. A copy of the logs can be found in the `PlaylistTransferTool/transfer_tool_log.txt` file. Note that the tool will overwrite any logs previously in this file, so save a copy if you intend to refer to previous runs' logs. Also note that the tool will process playlist files in alphabetical order. If the tool doesn't crash (or crashes after finishing exporting some playlists), you should find playlist files in the folder you specified in the `playlistExportPath` config (the `output` folder by default). You can then import these files according to each provider's instructions.