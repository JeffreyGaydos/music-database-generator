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

--"Moods" are essentially simplified playlists, but are specific to the
--track itself, and are perminant descriptors of the song. It's up to
--the user to decide what moods are associated with which tracks
IF (SELECT [dbo].[MusicTableExists] (N'Mood')) = 0
BEGIN
	CREATE TABLE Mood (
		MoodID INT IDENTITY(1,1) PRIMARY KEY,
		MoodDesc NVARCHAR(100)
	)
END

--Maps your track to its moods
IF (SELECT [dbo].[MusicTableExists] (N'MoodTracks')) = 0
BEGIN
	CREATE TABLE MoodTracks (
		MoodID INT,
		TrackID INT,
		PRIMARY KEY (MoodID, TrackID)
	)
END



--The metadata of the playlists that you create.
IF (SELECT [dbo].[MusicTableExists] (N'Playlist')) = 0
BEGIN
	CREATE TABLE Playlist (
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
IF (SELECT [dbo].[MusicTableExists] (N'Artist')) = 0
BEGIN
	CREATE TABLE Artist (
		ArtistID INT IDENTITY(1,1) PRIMARY KEY,
		PrimaryPersonID INT,
		ArtistName NVARCHAR(100)
	)
END

--Maps tracks to their artist(s)
IF (SELECT [dbo].[MusicTableExists] (N'ArtistTracks')) = 0
BEGIN
	CREATE TABLE ArtistTracks (
		ArtistID INT,
		TrackID INT,
		PRIMARY KEY (ArtistID, TrackID)
	)
END

--Maps artists to persons associated with that artist or group
IF (SELECT [dbo].[MusicTableExists] (N'ArtistPersons')) = 0
BEGIN
	CREATE TABLE ArtistPersons (
		PersonID INT IDENTITY(1,1) PRIMARY KEY,
		ArtistID INT,
		PersonName NVARCHAR(1000)
	)
END

--This table can be matched with the main table to determine the owner of the track
IF (SELECT [dbo].[MusicTableExists] (N'Owner')) = 0
BEGIN
	CREATE TABLE Owner (
		OwnerID INT IDENTITY(1,1) PRIMARY KEY,
		OwnerName NVARCHAR(1000)
	)
END

--traditional genre designation, directly form the MP3 files
--may or may not be a read only table in future applications
IF (SELECT [dbo].[MusicTableExists] (N'Genre')) = 0
BEGIN
	CREATE TABLE Genre (
		GenreID INT IDENTITY(1,1) PRIMARY KEY,
		GenreName NVARCHAR(100)
	)
END

--maps tracks to their genre(s)
IF (SELECT [dbo].[MusicTableExists] (N'GenreTracks')) = 0
BEGIN
	CREATE TABLE GenreTracks (
		GenreID INT,
		TrackID INT,
		PRIMARY KEY (GenreID, TrackID)
	)
END

--"Album" is used loosely here, and represents any collection of songs that may or may
--not have track numbers associated with them. The AlbumID can matched with on the Main
--table to determine all the songs within 1 album.
IF (SELECT [dbo].[MusicTableExists] (N'Album')) = 0
BEGIN
	CREATE TABLE Album (
		AlbumID INT IDENTITY(1,1) PRIMARY KEY,
		AlbumName NVARCHAR(1000),
		TrackCount INT,
		ReleaseYear INT
	)
END

--Maps tracks to their album(s)
IF (SELECT [dbo].[MusicTableExists] (N'AlbumTracks')) = 0
BEGIN
	CREATE TABLE AlbumTracks (
		AlbumID INT,
		TrackID INT,
		TrackOrder INT,
		PRIMARY KEY (AlbumID, TrackID)
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
		TrackID INT IDENTITY(1,1) PRIMARY KEY,
		Title NVARCHAR(4000),
		Duration DECIMAL,
		FilePath VARCHAR(260), --windows max path length = 260 characters
		AverageDecibels DECIMAL,
		OwnerID INT,
		Linked BIT,
		ReleaseYear INT,
		AddDate DATETIME,
		Lyrics NVARCHAR(4000),
		Comment NVARCHAR(4000),
		BeatsPerMin INT,
		Copyright VARCHAR(1000),
		Publisher VARCHAR(1000),
		ISRC VARCHAR(1000),
		Bitrate INT,
		Channels INT,
		SampleRate INT,
		BitsPerSample INT,
		GeneratedDate DATETIME
	)
END
GO