/*
    db_initialize.sql

    This script must be run at least once to create the necessary tables that this
    generator adds to. This file will be maintained to be backwards compatible and will
    update tables with any new constraints or columns as necessary
*/

--connect to your local database
--make sure you don't have an existing "MusicLibrary" database that was not created by this generator

IF DB_ID(N'MusicLibrary') IS NULL
BEGIN
	CREATE DATABASE MusicLibrary
END
GO

USE MusicLibrary;

IF OBJECT_ID(N'MusicTableExists', N'FN') IS NOT NULL
    DROP FUNCTION MusicTableExists
GO

IF OBJECT_ID(N'MusicTableColumnExists', N'FN') IS NOT NULL
    DROP FUNCTION MusicTableColumnExists
GO

/*
 * MusicTableExists
 *
 * This helper function abstracts the INFORMATION_SCHEMA
 * check when looking up tables.
 */
CREATE FUNCTION dbo.MusicTableExists (@name NVARCHAR(100))
RETURNS BIT
AS
BEGIN
	IF EXISTS (SELECT NULL FROM INFORMATION_SCHEMA.TABLES
	WHERE TABLE_CATALOG = 'MusicLibrary' AND TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @name)
	BEGIN
		RETURN 1
	END
	RETURN 0
END
GO

/*
 * MusicTableColumnExists
 *
 * This helper function abstracts the INFORMATION_SCHEMA
 * check when looking up tables columns during table alters
 */
CREATE FUNCTION dbo.MusicTableColumnExists (@tableName VARCHAR(100), @columnName VARCHAR(100))
RETURNS BIT
AS
BEGIN
	IF EXISTS (SELECT NULL FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_CATALOG = 'MusicLibrary' AND TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName)
	BEGIN
		RETURN 1
	END
	RETURN 0
END
GO

USE MusicLibrary

--This table can be matched with the main table to detemine the mood/genre of a track
--IDs are big int to accommodate granularity. Playlists could be designated using moods.
--NOTE: "Mood group IDs" may be necessary in the future should 64 moods be restrictive
IF (SELECT [dbo].[MusicTableExists] (N'ListMood')) = 0
BEGIN
	CREATE TABLE ListMood (
		MoodID BIGINT PRIMARY KEY,
		MoodDesc NVARCHAR(100)
	)
END

--The metadata of the playlists that you create.
IF (SELECT [dbo].[MusicTableExists] (N'Playlists')) = 0
BEGIN
	CREATE TABLE Playlists (
		PlaylistID INT NOT NULL PRIMARY KEY,
		PlaylistName NVARCHAR(1000),
		PlaylistDescription NVARCHAR(4000),
		CreationDate DATETIME,
		LastEditDate DATETIME,
	)
END

--Just maps from the playlist metadata table to the tracks that are in the playlist
IF (SELECT [dbo].[MusicTableExists] (N'PlaylistTracks')) = 0
BEGIN
	CREATE TABLE PlaylistTracks (
		PlaylistID INT NOT NULL,
		TrackID INT NOT NULL,
		PRIMARY KEY (PlaylistID, TrackID)
	)
END

--This table can be matched with the main table to deremine the artist
IF (SELECT [dbo].[MusicTableExists] (N'ListArtist')) = 0
BEGIN
	CREATE TABLE ListArtist (
		ArtistID INT PRIMARY KEY,
		ArtistName NVARCHAR(100)
	)
END

--This table can be matched with the main table to determine the owner of the track
IF (SELECT [dbo].[MusicTableExists] (N'ListOwner')) = 0
BEGIN
	CREATE TABLE ListOwner (
		OwnerID INT PRIMARY KEY,
		OwnerName NVARCHAR(1000)
	)
END

--traditional genre designation, directly form the MP3 files
--may or may not be a read only table in future applications
IF (SELECT [dbo].[MusicTableExists] (N'ListGenre')) = 0
BEGIN
	CREATE TABLE ListGenre (
		GenreID INT PRIMARY KEY,
		GenreName INT
	)
END

--"Album" is used loosely here, and represents any collection of songs that may or may
--not have track numbers associated with them. The AlbumID can matched with on the Main
--table to determine all the songs within 1 album.
IF (SELECT [dbo].[MusicTableExists] (N'Album')) = 0
BEGIN
	CREATE TABLE Album (
		AlbumID INT PRIMARY KEY,
		AlbumTracks INT,
		AlbumName NVARCHAR(1000)
	)
END

--Linked Tracks are any tracks that are best when played back-to-back.
--Take the songs "Parabol" and "Porabola" by the artist "Tool". There could be an
--option during future development to force songs like this to be played one after
--the other, for a more pleasent listenning experience while shuffling.
IF (SELECT [dbo].[MusicTableExists] (N'LinkedTracks')) = 0
BEGIN
	CREATE TABLE LinkedTracks (
		TrackID1 INT PRIMARY KEY,
		TrackID2 INT
	)
END

--This table is just to prevent the main table from being bloated with a bunch of fields
--and contains data on how frequently and when tracks were played.
IF (SELECT [dbo].[MusicTableExists] (N'PlayLogs')) = 0
BEGIN
	CREATE TABLE PlayLogs (
		TrackID INT PRIMARY KEY,
		DatePlayed DATETIME
	)
END

--The main table has a list of all your songs, some metadata, and IDs that link to other
--tables with additional meatadata
IF (SELECT [dbo].[MusicTableExists] (N'Main')) = 0
BEGIN
	CREATE TABLE Main (
		TrackID INT	PRIMARY KEY,
		Title NVARCHAR(4000),
		Duration DECIMAL,
		FilePath VARCHAR(260), --windows max path length = 260 characters
		AverageDecibels DECIMAL,
		MoodIDs BIGINT,
		AlbumID INT,
		AlbumTrackNumber INT,
		OwnerID INT,
		GenreID INT,
		Linked BIT)
END
GO