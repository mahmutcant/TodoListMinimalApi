using System.Text.Json.Serialization;

namespace WebApplication1.Entities
{
    public class User : Entity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenTime { get; set; }
        [JsonIgnore]
        public ICollection<Todo> Todos { get; set; }
    }
}
