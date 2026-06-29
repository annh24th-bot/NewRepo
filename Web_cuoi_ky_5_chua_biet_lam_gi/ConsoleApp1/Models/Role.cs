using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Database.Models
{
    [Table ("Role")]
    public class Role
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength (150)]

        public string Name { get; set; }
        [Required]
        [MaxLength(150)]
        public string? Code { get; set; }
        [ForeignKey("CategoryId")]
        public Guid? CategoryId { get; set; } 
        public Category? Category { get; set; }
        public ICollection<Authorized> Authorized { get; set; } = new List<Authorized> ();

    }
}
