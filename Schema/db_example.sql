SELECT M.*, ATR.TrackOrder, A.AlbumName, A.ReleaseYear, G.GenreName, ART.ArtistName FROM MusicLibrary.dbo.Main M
JOIN MusicLibrary.dbo.AlbumTracks ATR ON M.TrackID = ATR.TrackID
JOIN MusicLibrary.dbo.Album A ON A.AlbumID = ATR.AlbumID
JOIN MusicLibrary.dbo.GenreTracks GTR ON GTR.TrackID = M.TrackID
JOIN MusicLibrary.dbo.Genre G ON G.GenreID = GTR.GenreID
JOIN MusicLibrary.dbo.ArtistTracks ARTT ON ARTT.TrackID = M.TrackID
JOIN MusicLibrary.dbo.Artist ART ON ART.ArtistID = ARTT.ArtistID

SELECT ART.*, AP.*, PRP.PersonName AS PrimaryPerson FROM MusicLibrary.dbo.Artist ART
LEFT JOIN MusicLibrary.dbo.ArtistPersons AP ON ART.ArtistID = AP.ArtistID
LEFT JOIN MusicLibrary.dbo.ArtistPersons PRP ON PRP.PersonID = ART.PrimaryPersonID