namespace AcademyBackend.Concrete_Types
{
    using System;
    using AcademyBackend.Interfaces;
    using System.IO;
    using System.Linq;
    class TextFileDataStore : IDataStore
    {
        readonly string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, @"Data\MessagesCompleted.txt");
        public bool Read(string taskId)
        {
            
            string[] messagesCompleted = File.ReadAllLines(this.path);

            if (messagesCompleted.Contains(taskId))
            {
                return true;
            }

            return false;
        }

        public void Write(string taskId)
        {
            File.AppendAllTextAsync(this.path, taskId + Environment.NewLine);
        }
    }
}
