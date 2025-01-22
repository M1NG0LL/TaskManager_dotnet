using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Models
{
    public class Task
    {
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public Guid AccountId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}