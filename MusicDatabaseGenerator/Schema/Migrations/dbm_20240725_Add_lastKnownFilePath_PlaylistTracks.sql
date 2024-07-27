BEGIN TRAN

USE MusicLibrary

ALTER TABLE PlaylistTracks ADD LastKnownPath VARCHAR(260) NULL
ALTER TABLE MusicLibrary.dbo.PlaylistTracks ADD SurrogateKey INT IDENTITY(1,1)

UPDATE PLTS
SET LastKnownPath = MAIN.FilePath
FROM MusicLibrary.dbo.PlaylistTracks PLTS
LEFT JOIN MusicLibrary.dbo.Main MAIN ON MAIN.TrackID = PLTS.TrackID

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
CREATE UNIQUE NONCLUSTERED INDEX UQ__PlaylistID_TrackID
ON PlaylistTracks(PlaylistID, TrackID)
WHERE TrackID IS NOT NULL

--enforce no duplicate track order entries for the same playlist
--This is not a primary key or unique constraint because we want to ignore NULLs in order to allow playlists to be used for Mood or other orderless designations
CREATE UNIQUE NONCLUSTERED INDEX UQ__PlaylistID_TrackOrder
ON PlaylistTracks(PlaylistID, TrackOrder)
WHERE TrackOrder IS NOT NULL

--ROLLBACK TRAN
COMMIT TRAN