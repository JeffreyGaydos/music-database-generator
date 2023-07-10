/*
 * db_delete.sql
 * Schema Version: 1.0.0
 *
 * This script is intended to delete all data in the
 * database without affecting the schema. This way, the
 * tables do not need to be re-built if you only want
 * to regenerate the data in the database.
 */

BEGIN TRAN

USE MusicLibrary

DELETE FROM Mood
DBCC CHECKIDENT ('dbo.Mood', RESEED, 0)
DELETE FROM MoodTracks
DELETE FROM Playlist
--DBCC CHECKIDENT ('dbo.Playlist', RESEED, 0)--Identity column not yet implemented on this table
DELETE FROM PlaylistTracks
DELETE FROM Artist
DBCC CHECKIDENT ('dbo.Artist', RESEED, 0)
DELETE FROM ArtistTracks
DELETE FROM ArtistPersons
DBCC CHECKIDENT ('dbo.ArtistPersons', RESEED, 0)
DELETE FROM Owner
DELETE FROM Genre
DBCC CHECKIDENT ('dbo.Genre', RESEED, 0)
DELETE FROM GenreTracks
DELETE FROM Album
DBCC CHECKIDENT ('dbo.Album', RESEED, 0)
DELETE FROM AlbumTracks
DELETE FROM LinkedTracks
DELETE FROM PlayLogs
DELETE FROM Main
DBCC CHECKIDENT ('dbo.Main', RESEED, 0)

--ROLLBACK TRAN
COMMIT TRAN