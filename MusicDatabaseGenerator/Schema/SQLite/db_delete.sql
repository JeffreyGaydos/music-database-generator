/*
 * sqlite_delete.sql
 * Schema Version: 1.0.0
 *
 * This script is intended to delete all data in the
 * database without affecting the schema. This way, the
 * tables do not need to be re-built if you only want
 * to regenerate the data in the database.
 */
DELETE FROM Playlist
DELETE FROM PlaylistTracks
DELETE FROM Artist
DELETE FROM ArtistTracks
DELETE FROM ArtistPersons
DELETE FROM TrackPersons
DELETE FROM Genre
DELETE FROM GenreTracks
DELETE FROM Album
DELETE FROM AlbumTracks
DELETE FROM AlbumArt
DELETE FROM PlayLogs
DELETE FROM Main