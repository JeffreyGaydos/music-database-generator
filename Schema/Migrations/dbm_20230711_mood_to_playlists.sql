BEGIN TRAN

ALTER TABLE MusicLibrary.dbo.PlaylistTracks
ALTER COLUMN TrackOrder INT NULL

SELECT * FROM MusicLibrary.dbo.Mood

SELECT * FROM MusicLibrary.dbo.MoodTracks

INSERT INTO MusicLibrary.dbo.Playlist
(PlaylistName, PlaylistDescription, CreationDate, LastEditDate)
SELECT DISTINCT MoodDesc, 'SYS: Migrated from Mood Tables', GETDATE(), GETDATE() FROM MusicLibrary.dbo.Mood

INSERT INTO MusicLibrary.dbo.PlaylistTracks
(PlaylistID, TrackID, TrackOrder)
SELECT PL.PlaylistID, MT.TrackID, NULL /*NULL denotes system-generated playlist*/ FROM MusicLibrary.dbo.MoodTracks MT
JOIN MusicLibrary.dbo.Mood MOOD ON MOOD.MoodID = MT.MoodID
JOIN MusicLibrary.dbo.Playlist PL ON PL.PlaylistName = MOOD.MoodDesc

SELECT * FROM MusicLibrary.dbo.Playlist

SELECT * FROM MusicLibrary.dbo.PlaylistTracks

DROP TABLE MusicLibrary.dbo.Mood

DROP TABLE MusicLibrary.dbo.MoodTracks

ROLLBACK TRAN
--COMMIT TRAN