namespace AcademyBackend.Interfaces
{
    using System;

    public interface IDataStore
    {
        bool Read(string taskId);
    }
}