using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolicitudServidores.Helpers
{
    public class QueryUserPaging
    {

        [Required]
        public int NumPage { get; set; }

        [JsonIgnore]
        public int NumSize { get; set; } = 20;

        public string? Role { get; set; } = null;
    }
}
