/*
 * WARNING: This migration will delete any existing database information already in the database. Plan to re-import your database files if you run this migration. This is required because of the primary key modifications on these tables.
 */

SET XACT_ABORT ON;

BEGIN TRAN

USE MusicLibrary

TRUNCATE TABLE MusicLibrary.dbo.PlaylistTracks
TRUNCATE TABLE MusicLibrary.dbo.Playlist

--Drop PK
DECLARE @PKName NVARCHAR(100)

SELECT TOP 1 @PKName = Col.CONSTRAINT_NAME from
INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
WHERE
Col.Constraint_Name = Tab.Constraint_Name
AND Col.Table_Name = Tab.Table_Name
AND Tab.Constraint_Type = 'PRIMARY KEY'
AND Col.Table_Name = 'PlaylistTracks'

SELECT @PKName

DECLARE @PKDrop NVARCHAR(MAX) = 'ALTER TABLE MusicLibrary.dbo.PlaylistTracks DROP CONSTRAINT [' + @PKName + ']'

EXEC sp_sqlexec @PKDrop

ALTER TABLE PlaylistTracks ADD LastKnownPath VARCHAR(260) NULL
ALTER TABLE MusicLibrary.dbo.PlaylistTracks ADD SurrogateKey INT IDENTITY(1,1) PRIMARY KEY

SELECT 1 AS [ShouldBeEmpty] from
INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
WHERE
Col.Constraint_Name = Tab.Constraint_Name
AND Col.Table_Name = Tab.Table_Name
AND Tab.Constraint_Type = 'PRIMARY KEY'
AND Col.Table_Name = 'PlaylistTracks'

--Recreate PK as Unique constraint with ignored NULLs
ALTER TABLE PlaylistTracks ALTER COLUMN TrackID INT NULL --first allow NULL
--create "PK"
CREATE UNIQUE NONCLUSTERED INDEX UC_PlaylistID_TrackID
ON PlaylistTracks(PlaylistID, TrackID)
WHERE TrackID IS NOT NULL

--enforce no duplicate track order entries for the same playlist
--This is not a primary key or unique constraint because we want to ignore NULLs in order to allow playlists to be used for Mood or other orderless designations
CREATE UNIQUE NONCLUSTERED INDEX UC_PlaylistID_TrackOrder
ON PlaylistTracks(PlaylistID, TrackOrder)
WHERE TrackOrder IS NOT NULL

ALTER TABLE PlaylistTracks ALTER COLUMN TrackID INT NULL
ALTER TABLE PlaylistTracks ALTER COLUMN TrackOrder INT NULL

ROLLBACK TRAN
--COMMIT TRAN