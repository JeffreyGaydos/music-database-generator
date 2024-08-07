/*
 * db_drop.sql
 * Schema Version: 1.0.0
 *
 * WARNING: Dropping tables removes all data in that table. Save your data if needed
 * Run this file by uncommenting the "COMMIT" and commenting "ROLLBACK"
 * This file is intended to "uninstall" your database to allow for schema change udpates.
 */

BEGIN

DROP TABLE IF EXISTS Playlist
DROP TABLE IF EXISTS PlaylistTracks
DROP TABLE IF EXISTS Artist
DROP TABLE IF EXISTS ArtistTracks
DROP TABLE IF EXISTS ArtistPersons
DROP TABLE IF EXISTS TrackPersons
DROP TABLE IF EXISTS Genre
DROP TABLE IF EXISTS GenreTracks
DROP TABLE IF EXISTS Album
DROP TABLE IF EXISTS AlbumTracks
DROP TABLE IF EXISTS AlbumArt
DROP TABLE IF EXISTS PlayLogs
DROP TABLE IF EXISTS Main
DROP TABLE IF EXISTS __MigrationHistory
DROP VIEW IF EXISTS [MainDataJoined]
DROP VIEW IF EXISTS [LeadArtists]

ROLLBACK
--COMMIT