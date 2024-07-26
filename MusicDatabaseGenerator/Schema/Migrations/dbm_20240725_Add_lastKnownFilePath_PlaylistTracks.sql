BEGIN TRAN

USE MusicLibrary

ALTER TABLE PlaylistTracks ADD LastKnownPath VARCHAR(260) NULL

UPDATE PLTS
SET LastKnownPath = MAIN.FilePath
FROM MusicLibrary.dbo.PlaylistTracks PLTS
LEFT JOIN MusicLibrary.dbo.Main MAIN ON MAIN.TrackID = PLTS.TrackID

--ROLLBACK TRAN
COMMIT TRAN