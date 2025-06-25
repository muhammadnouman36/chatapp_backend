using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.AppUsers;

namespace Domain.Chat
{
    public class Friends
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public long UserId { get; set; }

        public virtual AppUser appuserid { get; set; }

        // Foreign Key for FriendId
        [ForeignKey("FriendId")]
        public long FriendId { get; set; }

        public virtual AppUser appuserid2 { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

            public bool isBloacked { get; set; } = false;

            public bool isUnfriend { get; set; } = false;




    }
}
