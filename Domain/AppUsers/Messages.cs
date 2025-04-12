using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.AppUsers
{
    public class Messages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        // Sender Foreign Key
        [ForeignKey("Sender")]
        public long SenderId { get; set; }
        public virtual AppUser Sender { get; set; }

        // Receiver Foreign Key
        [ForeignKey("Receiver")]
        public long ReceiverId { get; set; }
        public virtual AppUser Receiver { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(2000)]
        public string Content { get; set; } = "";

        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
