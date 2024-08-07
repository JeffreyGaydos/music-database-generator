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
BEGIN
    DROP FUNCTION MusicTableExists
END
GO

USE MusicLibrary;

IF OBJECT_ID(N'MusicTableColumnExists', N'FN') IS NOT NULL
BEGIN
    DROP FUNCTION MusicTableColumnExists
END
GO

USE MusicLibrary;

IF OBJECT_ID(N'MusicViewExists', N'FN') IS NOT NULL
BEGIN
    DROP FUNCTION MusicViewExists
END
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
		TrackOrder INT NULL, --NULL denotes system generated playlists
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

--Maps tracks to persons associated with that track specifically. Useful for bands with temporary members
IF (SELECT [dbo].[MusicTableExists] (N'TrackPersons')) = 0
BEGIN
	CREATE TABLE TrackPersons (
		TrackID INT,
		PersonID INT,
		PRIMARY KEY (TrackID, PersonID)
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
		Volume INT, --A user defined value to help when manually matching volume across multiple files
		[Owner] NVARCHAR(1000), --defaults to the computer username, but can be used for however
		ReleaseYear INT,
		Lyrics NVARCHAR(4000),
		Comment NVARCHAR(4000),
		BeatsPerMin INT,
		Copyright NVARCHAR(1000),
		Publisher NVARCHAR(1000),
		ISRC VARCHAR(12), --ISRC codes are explicitly 12 characters long
		Bitrate INT,
		Channels INT,
		SampleRate INT,
		BitsPerSample INT,
		LinkedTrackPlaylistID INT, --Linked Tracks are any tracks that are best when played back-to-back. This mini-playlist defines the order
		Rating INT, --User defined only; Not generated because ratings "on" mp3 files are specific to the Windows OS (might consider adding support)
		AddDate DATETIME, --will attempt to persist this value during update operations
		LastModifiedDate DATETIME,
		GeneratedDate DATETIME, --used to determine if updates are needed
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
	SELECT 1
END
ELSE
BEGIN
	SELECT 0
	RETURN --Views must be the only statement in the batch, end early since we can't wrap them
END
GO

CREATE VIEW [MainDataJoined] AS
SELECT M.*, ATR.TrackOrder, A.AlbumName, A.ReleaseYear AS [AlbumReleaseYear], G.GenreName, ART.ArtistName FROM MusicLibrary.dbo.Main M
LEFT JOIN MusicLibrary.dbo.AlbumTracks ATR ON M.TrackID = ATR.TrackID
LEFT JOIN MusicLibrary.dbo.Album A ON A.AlbumID = ATR.AlbumID
LEFT JOIN MusicLibrary.dbo.GenreTracks GTR ON GTR.TrackID = M.TrackID
LEFT JOIN MusicLibrary.dbo.Genre G ON G.GenreID = GTR.GenreID
LEFT JOIN MusicLibrary.dbo.ArtistTracks ARTT ON ARTT.TrackID = M.TrackID
LEFT JOIN MusicLibrary.dbo.Artist ART ON ART.ArtistID = ARTT.ArtistID
GO

CREATE VIEW [LeadArtists] AS
SELECT ART.*, AP.PersonName, PRP.PersonName AS PrimaryPerson FROM MusicLibrary.dbo.Artist ART
LEFT JOIN MusicLibrary.dbo.ArtistPersons AP ON ART.ArtistID = AP.ArtistID
LEFT JOIN MusicLibrary.dbo.ArtistPersons PRP ON PRP.PersonID = ART.PrimaryPersonID
GO