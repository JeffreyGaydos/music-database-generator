/*
    sqlite_initialize.sql

    This script must be run at least once to create the necessary tables that this
    generator adds to. This file will be maintained to be backwards compatible and will
    update tables with any new constraints or columns as necessary
*/

--The metadata of the playlists that you create.
CREATE TABLE IF NOT EXISTS Playlist (
	PlaylistID INTEGER PRIMARY KEY,
	PlaylistName NVARCHAR(450),
	PlaylistDescription NVARCHAR(4000),
	CreationDate DATETIME,
	LastEditDate DATETIME,
	CONSTRAINT UC_PlaylistName UNIQUE (PlaylistName)
)

--Just maps from the playlist metadata table to the tracks that are in the playlist
CREATE TABLE IF NOT EXISTS PlaylistTracks (
	PlaylistID INT NOT NULL,
	TrackID INT NOT NULL,
	TrackOrder INT NULL, --NULL denotes system generated playlists
	LastKnownPath VARCHAR(260) PRIMARY KEY,
	CONSTRAINT UC_PlaylistID_TrackID UNIQUE (PlaylistID, TrackID),
	CONSTRAINT UC_PlaylistID_TrackOrder UNIQUE (PlaylistID, TrackOrder)
)

--This table can be matched with the main table to deremine the artist
CREATE TABLE IF NOT EXISTS Artist (
	ArtistID INTEGER PRIMARY KEY,
	PrimaryPersonID INT,
	ArtistName NVARCHAR(100),
	CONSTRAINT UC_Artist UNIQUE (PrimaryPersonID, ArtistName)
)

--Maps tracks to their artist(s)
CREATE TABLE IF NOT EXISTS ArtistTracks (
	ArtistID INT,
	TrackID INT,
	PRIMARY KEY (ArtistID, TrackID)
)

--Maps artists to persons associated with that artist or group
CREATE TABLE IF NOT EXISTS ArtistPersons (
		PersonID INTEGER PRIMARY KEY,
		ArtistID INT,
		PersonName NVARCHAR(200),
		CONSTRAINT UC_ArtistPerson UNIQUE (ArtistID, PersonName)
)

--Maps tracks to persons associated with that track specifically. Useful for bands with temporary members
CREATE TABLE IF NOT EXISTS TrackPersons (
	TrackID INT,
	PersonID INT,
	PRIMARY KEY (TrackID, PersonID)
)

--traditional genre designation, directly form the MP3 files
--may or may not be a read only table in future applications
CREATE TABLE IF NOT EXISTS Genre (
	GenreID INTEGER PRIMARY KEY,
	GenreName NVARCHAR(100),
	CONSTRAINT UC_GenreName UNIQUE (GenreName)
)

--maps tracks to their genre(s)
CREATE TABLE IF NOT EXISTS GenreTracks (
	GenreID INT,
	TrackID INT,
	PRIMARY KEY (GenreID, TrackID)
)

--"Album" is used loosely here, and represents any collection of songs that may or may
--not have track numbers associated with them. The AlbumID can matched with on the Main
--table to determine all the songs within 1 album.
CREATE TABLE IF NOT EXISTS Album (
	AlbumID INTEGER PRIMARY KEY,
	AlbumName NVARCHAR(446),
	TrackCount INT,
	ReleaseYear INT,
	CONSTRAINT UC_Album UNIQUE (AlbumName, TrackCount, ReleaseYear)
)

--Maps tracks to their album(s)
CREATE TABLE IF NOT EXISTS AlbumTracks (
	AlbumID INT,
	TrackID INT,
	TrackOrder INT,
	PRIMARY KEY (AlbumID, TrackID)
)

--The fll path to the album art associated with an Album (if present); Maps to Album table by ID
CREATE TABLE IF NOT EXISTS AlbumArt (
	AlbumArtPath VARCHAR(260) PRIMARY KEY, --windows max path length
	PrimaryColor VARCHAR(7), --mode color in hex format (#000000)
	AlbumID INT
)

--This table is just to prevent the main table from being bloated with a bunch of fields
--and contains data on how frequently and when tracks were played.
CREATE TABLE IF NOT EXISTS PlayLogs (
	TrackID INT PRIMARY KEY,
	DatePlayed DATETIME
)

--The main table has a list of all your songs, some metadata, and IDs that link to other
--tables with additional meatadata
CREATE TABLE IF NOT EXISTS Main (
	TrackID INTEGER PRIMARY KEY,
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

CREATE VIEW IF NOT EXISTS [MainDataJoined] AS
SELECT M.*, ATR.TrackOrder, A.AlbumName, A.ReleaseYear AS [AlbumReleaseYear], G.GenreName, ART.ArtistName FROM Main M
LEFT JOIN AlbumTracks ATR ON M.TrackID = ATR.TrackID
LEFT JOIN Album A ON A.AlbumID = ATR.AlbumID
LEFT JOIN GenreTracks GTR ON GTR.TrackID = M.TrackID
LEFT JOIN Genre G ON G.GenreID = GTR.GenreID
LEFT JOIN ArtistTracks ARTT ON ARTT.TrackID = M.TrackID
LEFT JOIN Artist ART ON ART.ArtistID = ARTT.ArtistID

CREATE VIEW IF NOT EXISTS [LeadArtists] AS
SELECT ART.*, AP.PersonName, PRP.PersonName AS PrimaryPerson FROM Artist ART
LEFT JOIN ArtistPersons AP ON ART.ArtistID = AP.ArtistID
LEFT JOIN ArtistPersons PRP ON PRP.PersonID = ART.PrimaryPersonID