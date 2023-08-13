/*
 * db_drop.sql
 * Schema Version: 1.0.0
 *
 * WARNING: Dropping tables removes all data in that table. Save your data if needed
 * Run this file by uncommenting the "COMMIT TRAN" and commenting "ROLLBACK TRAN"
 * This file is intended to "uninstall" your database to allow for schema change udpates.
 */

BEGIN TRAN

USE MusicLibrary

IF (SELECT [dbo].[MusicTableExists] (N'Mood')) = 1
BEGIN
    DROP TABLE Mood
END
IF (SELECT [dbo].[MusicTableExists] (N'MoodTracks')) = 1
BEGIN
    DROP TABLE MoodTracks
END
IF (SELECT [dbo].[MusicTableExists] (N'Playlist')) = 1
BEGIN
    DROP TABLE Playlist
END
IF (SELECT [dbo].[MusicTableExists] (N'PlaylistTracks')) = 1
BEGIN
    DROP TABLE PlaylistTracks
END
IF (SELECT [dbo].[MusicTableExists] (N'Artist')) = 1
BEGIN
    DROP TABLE Artist
END
IF (SELECT [dbo].[MusicTableExists] (N'ArtistTracks')) = 1
BEGIN
    DROP TABLE ArtistTracks
END
IF (SELECT [dbo].[MusicTableExists] (N'ArtistPersons')) = 1
BEGIN
    DROP TABLE ArtistPersons
END
IF (SELECT [dbo].[MusicTableExists] (N'Genre')) = 1
BEGIN
    DROP TABLE Genre
END
IF (SELECT [dbo].[MusicTableExists] (N'GenreTracks')) = 1
BEGIN
    DROP TABLE GenreTracks
END
IF (SELECT [dbo].[MusicTableExists] (N'Album')) = 1
BEGIN
    DROP TABLE Album
END
IF (SELECT [dbo].[MusicTableExists] (N'AlbumTracks')) = 1
BEGIN
    DROP TABLE AlbumTracks
END
IF (SELECT [dbo].[MusicTableExists] (N'AlbumArt')) = 1
BEGIN
    DROP TABLE AlbumArt
END
IF (SELECT [dbo].[MusicTableExists] (N'LinkedTracks')) = 1
BEGIN
    DROP TABLE LinkedTracks
END
IF (SELECT [dbo].[MusicTableExists] (N'PlayLogs')) = 1
BEGIN
    DROP TABLE PlayLogs
END
IF (SELECT [dbo].[MusicTableExists] (N'Main')) = 1
BEGIN
    DROP TABLE Main
END
IF (SELECT [dbo].[MusicTableExists] (N'__MigrationHistory')) = 1
BEGIN
    DROP TABLE __MigrationHistory
END

IF (SELECT [dbo].[MusicViewExists] ('MainDataJoined')) = 1
BEGIN
    DROP VIEW [MainDataJoined]
END
IF (SELECT [dbo].[MusicViewExists] ('LeadArtists')) = 1
BEGIN
    DROP VIEW [LeadArtists]
END

--ROLLBACK TRAN
COMMIT TRAN