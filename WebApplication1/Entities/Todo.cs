using System.Text.Json.Serialization;

namespace WebApplication1.Entities
{
    public class Todo:Entity
    {
        public string? Location { get; set; }
        public string? job { get; set; }
        public bool IsComplete { get; set; }

        [JsonIgnore]
        public User Owner { get; set; }
    }
}
