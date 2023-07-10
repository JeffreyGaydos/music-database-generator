SELECT M.*, ATR.TrackOrder, A.AlbumName, A.ReleaseDate, G.GenreName, ART.ArtistName FROM MusicLibrary.dbo.Main M
JOIN MusicLibrary.dbo.AlbumTracks ATR ON M.TrackID = ATR.TrackID
JOIN MusicLibrary.dbo.Album A ON A.AlbumID = ATR.AlbumID
JOIN MusicLibrary.dbo.GenreTracks GTR ON GTR.TrackID = M.TrackID
JOIN MusicLibrary.dbo.Genre G ON G.GenreID = GTR.GenreID
JOIN MusicLibrary.dbo.ArtistTracks ARTT ON ARTT.TrackID = M.TrackID
JOIN MusicLibrary.dbo.Artist ART ON ART.ArtistID = ARTT.ArtistID
JOIN