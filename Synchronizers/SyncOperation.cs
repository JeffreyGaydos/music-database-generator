using System;

namespace MusicDatabaseGenerator.Synchronizers
{
    [Flags]
    public enum SyncOperation
    {
        None = 0,
        Insert = 1 << 0,
        Update = 1 << 1,
        Delete = 1 << 2,
        Skip = 1 << 3,
    }
}
