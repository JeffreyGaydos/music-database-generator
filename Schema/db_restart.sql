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

IF OBJECT_ID(N'MusicViewExists', N'FN') IS NOT NULL
    DROP FUNCTION MusicViewExists
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

/*
 * MusicViewExists
 *
 * This helper function determines if the view already exists
 * before creating it
 */
 CREATE FUNCTION dbo.MusicViewExists (@viewName VARCHAR(100))
 RETURNS BIT
 AS
 BEGIN
	IF EXISTS(SELECT NULL FROM sys.views WHERE name = @viewName)
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
		CONSTRAINT UC_MoodDesc UNIQUE (MoodDesc)
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
		PlaylistID INT IDENTITY(1,1) PRIMARY KEY,
		PlaylistName NVARCHAR(450),
		PlaylistDescription NVARCHAR(4000),
		CreationDate DATETIME,
		LastEditDate DATETIME,
		CONSTRAINT UC_PlaylistName UNIQUE (PlaylistName)
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
		CONSTRAINT UC_Artist UNIQUE (PrimaryPersonID, ArtistName)
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
		PersonName NVARCHAR(200)
		CONSTRAINT UC_ArtistPerson UNIQUE (ArtistID, PersonName)
	)
END

--traditional genre designation, directly form the MP3 files
--may or may not be a read only table in future applications
IF (SELECT [dbo].[MusicTableExists] (N'Genre')) = 0
BEGIN
	CREATE TABLE Genre (
		GenreID INT IDENTITY(1,1) PRIMARY KEY,
		GenreName NVARCHAR(100)
		CONSTRAINT UC_GenreName UNIQUE (GenreName)
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
		AlbumName NVARCHAR(446),
		TrackCount INT,
		ReleaseYear INT
		CONSTRAINT UC_Album UNIQUE (AlbumName, TrackCount, ReleaseYear)
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

--The fll path to the album art associated with an Album (if present); Maps to Album table by ID
IF (SELECT [dbo].[MusicTableExists] (N'AlbumArt')) = 0
BEGIN
	CREATE TABLE AlbumArt (
		AlbumArtPath VARCHAR(260) PRIMARY KEY, --windows max path length
		PrimaryColor VARCHAR(7), --mode color in hex format (#000000)
		AlbumID INT
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
		Title NVARCHAR(435), --The max key length is 900, this takes the rest of the bits
		Duration DECIMAL,
		FilePath VARCHAR(260), --windows max path length = 260 characters
		AverageDecibels DECIMAL,
		[Owner] NVARCHAR(1000), --defaults to the computer username, but can be used for however
		Linked BIT,
		ReleaseYear INT,
		AddDate DATETIME, --will attempt to persist this value during update operations
		LastModifiedDate DATETIME,
		Lyrics NVARCHAR(4000),
		Comment NVARCHAR(4000),
		BeatsPerMin INT,
		Copyright VARCHAR(1000),
		Publisher VARCHAR(1000),
		ISRC VARCHAR(12), --ISRC codes are explicitly 12 characters long
		Bitrate INT,
		Channels INT,
		SampleRate INT,
		BitsPerSample INT,
		GeneratedDate DATETIME --used to determine if updates are needed
		CONSTRAINT UC_Main UNIQUE (
			Title,
			ISRC,
			Duration
		)
	)
END
GO

IF (SELECT [dbo].[MusicViewExists] ('MainDataJoined')) = 0 AND (SELECT [dbo].[MusicViewExists] ('LeadArtists')) = 0
BEGIN
	RETURN --Views must be the only statement in the batch, end early since we can't wrap them
END
GO

CREATE VIEW [MainDataJoined] AS
SELECT M.*, ATR.TrackOrder, A.AlbumName, A.ReleaseYear AS [AlbumReleaseYear], G.GenreName, ART.ArtistName FROM MusicLibrary.dbo.Main M
JOIN MusicLibrary.dbo.AlbumTracks ATR ON M.TrackID = ATR.TrackID
JOIN MusicLibrary.dbo.Album A ON A.AlbumID = ATR.AlbumID
JOIN MusicLibrary.dbo.GenreTracks GTR ON GTR.TrackID = M.TrackID
JOIN MusicLibrary.dbo.Genre G ON G.GenreID = GTR.GenreID
JOIN MusicLibrary.dbo.ArtistTracks ARTT ON ARTT.TrackID = M.TrackID
JOIN MusicLibrary.dbo.Artist ART ON ART.ArtistID = ARTT.ArtistID
GO

CREATE VIEW [LeadArtists] AS
SELECT ART.*, AP.PersonName, PRP.PersonName AS PrimaryPerson FROM MusicLibrary.dbo.Artist ART
LEFT JOIN MusicLibrary.dbo.ArtistPersons AP ON ART.ArtistID = AP.ArtistID
LEFT JOIN MusicLibrary.dbo.ArtistPersons PRP ON PRP.PersonID = ART.PrimaryPersonID
GO