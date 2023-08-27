namespace MusicDatabaseGenerator.Generators
{
    public abstract class AGenerator
    {
        internal TagLib.File _file;
        internal MusicLibraryTrack _data;
        internal LoggingUtils _logger;
    }
}
