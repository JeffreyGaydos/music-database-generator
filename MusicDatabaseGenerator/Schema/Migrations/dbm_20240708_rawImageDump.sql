﻿BEGIN TRAN

ALTER TABLE MusicLibrary.dbo.AlbumArt
ADD RawData VARBINARY(MAX)

--ROLLBACK TRAN
COMMIT TRAN