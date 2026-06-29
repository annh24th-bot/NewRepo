using Core.Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Database.Models
{
    [Table("Articles")]
    public class Articles : IAuditable, IMeta
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = " ";
        [MaxLength(50)]
        public string? Picture { get; set; }
        [MaxLength(500)]
        public string? Intro { get; set; }
        public string? Content { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? Createdon { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? Modifieddon { get; set; }
        public string? Keyword { get; set; }
        public string? Description { get; set; }
    }
}