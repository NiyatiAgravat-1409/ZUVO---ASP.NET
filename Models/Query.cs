using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZUVO_MVC_.Models
{
    public class Query
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string QueryText { get; set; }
        
        public DateTime SubmissionDate { get; set; }
        
        public QueryStatus Status { get; set; }
        
        public string? Response { get; set; }
        
        public DateTime? ResponseDate { get; set; }

        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }
    }

    public enum QueryStatus
    {
        Pending,
        InProgress,
        Resolved
    }
} 