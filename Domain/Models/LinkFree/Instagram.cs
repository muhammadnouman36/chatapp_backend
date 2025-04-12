using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.LinkFree
{
    public class Instagram
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(500)]
        public string OrgLink { get; set; } = "";

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(500)]
        public string NewLink { get; set; } = "";

      
    }
}
