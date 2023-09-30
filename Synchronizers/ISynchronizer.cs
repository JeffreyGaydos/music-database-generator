namespace MusicDatabaseGenerator.Synchronizers
{
    public interface ISynchronizer
    {
        //Should first determine which operation to perform on the data object, then call the proper function from the abstract class
        SyncOperation Synchronize();
    }
}
