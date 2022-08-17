using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeeksForLess_Test.Models
{
    public class Folder
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Title { get; set; }

        [JsonIgnore]
        public IEnumerable<Folder> Subfolders { get; set; }
    }
}
