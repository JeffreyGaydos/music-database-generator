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

DROP TABLE Mood
DROP TABLE MoodTracks
DROP TABLE Playlist
DROP TABLE PlaylistTracks
DROP TABLE Artist
DROP TABLE ArtistTracks
DROP TABLE ArtistPersons
DROP TABLE Owner
DROP TABLE Genre
DROP TABLE GenreTracks
DROP TABLE Album
DROP TABLE AlbumTracks
DROP TABLE LinkedTracks
DROP TABLE PlayLogs
DROP TABLE Main

--ROLLBACK TRAN
COMMIT TRAN