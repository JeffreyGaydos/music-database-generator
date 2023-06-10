/*
 * db_drop.sql
 * Schema Version: 1.0.0
 *
 * WARNING: Dropping tables removes all data in that table. Save your data if needed
 * Run this file by uncommenting the "COMMIT TRAN" and commenting "ROLLBACK TRAN"
 * This file is intended to "uninstall" your database to allow for schema change udpates.
 */

BEGIN TRAN

DROP TABLE Album
DROP TABLE LinkedTracks
DROP TABLE ListArtist
DROP TABLE ListExtension
DROP TABLE ListGenre
DROP TABLE ListMood
DROP TABLE ListOwner
DROP TABLE Main

ROLLBACK TRAN
--COMMIT TRAN