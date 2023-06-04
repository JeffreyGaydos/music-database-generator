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
IF (SELECT [dbo].[MusicTableExists] (N'ListMood')) = 0
BEGIN
	CREATE TABLE ListMood (MoodID INT, MoodDesc NVARCHAR(100))
END

--This table can be matched with the main table to determine the extension type
IF (SELECT [dbo].[MusicTableExists] (N'ListExtension')) = 0
BEGIN
	CREATE TABLE ListExtension (ExtensionID INT, FileType VARCHAR(100))
END

--This table can be matched with the main table to deremine the artist
IF (SELECT [dbo].[MusicTableExists] (N'ListArtist')) = 0
BEGIN
	CREATE TABLE ListArtist (ArtistID INT, ArtistName NVARCHAR(100))
END

--This table can be matched with the main table to determine the owner of the track
IF (SELECT [dbo].[MusicTableExists] (N'ListOwner')) = 0
BEGIN
	CREATE TABLE ListOwner (OwnerID INT, OwnerName NVARCHAR(1000))
END

--"Album" is used loosely here, and represents any collection of songs that may or may
--not have track numbers associated with them. The AlbumID can matched with on the Main
--table to determine all the songs within 1 album.
IF (SELECT [dbo].[MusicTableExists] (N'Album')) = 0
BEGIN
	CREATE TABLE Album (AlbumID INT, AlbumTracks INT, AlbumName NVARCHAR(1000))
END

--The main table has a list of all your songs, some metadata, and IDs that link to other
--tables with additional meatadata
IF (SELECT [dbo].[MusicTableExists] (N'Main')) = 0
BEGIN
	CREATE TABLE Main (TrackID INT, Title NVARCHAR(4000), Duration DECIMAL, ExtensionID INT, AverageDecibels DECIMAL, Moods INT, AlbumID INT, AlbumTrackNumber INT, OwnerID INT)
END
GO