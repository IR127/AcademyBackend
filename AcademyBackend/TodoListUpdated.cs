using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend
{
    public class TodoListUpdated
    {
        public string Id { get; set; } // A unique id of the event
        public DateTimeOffset DateModified { get; set; } // The date the event occurred, not when it was published
    }
}
