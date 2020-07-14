using System.ComponentModel.DataAnnotations;

namespace RequestLogger.Models
{
    public class MyViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 5)]
        public string Name { get; set; }
    }
}
