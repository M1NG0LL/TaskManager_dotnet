using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagerAPI.Models
{
    public class Task
    {
        [Key]
        public Guid Uuid { get; set; } = Guid.NewGuid();
        [ForeignKey("Account")]
        public Guid AccountId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public Account Account { get; set; }
    }
}