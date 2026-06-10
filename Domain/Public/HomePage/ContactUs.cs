using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Public.HomePage
{
    public class ContactUs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(250)]
        public string Name { get; set; } = "";

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(250)]
        public string Email { get; set; } = "";

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(500)]
        public string Subject { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(1500)]
        public string Message { get; set; } = "";

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(500)]
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
