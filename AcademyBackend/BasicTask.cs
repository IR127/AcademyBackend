using System;
using System.Collections.Generic;
using System.Text;

namespace AcademyBackend
{
        using System;
        using System.ComponentModel.DataAnnotations;
        using Newtonsoft.Json;

        public class BasicTask
        {
            [Required]
            public string UserId { get; set; }

            [JsonProperty(PropertyName = "id")]
            public Guid TaskId { get; set; }

            [Required]
            [MinLength(5)]
            public string Description { get; set; }

            public DateTime DueBy { get; set; }

            public bool IsComplete { get; set; }

            public DateTime Added { get; set; }
    }
}
