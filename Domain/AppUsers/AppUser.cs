using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.AppUsers
{
    public class AppUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Column(TypeName = "NVARCHAR")]

        [MaxLength(450)]
        public string Email { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(200)]
        public string UserName { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(200)]
        public string Password { get; set; } = "";

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(25)]
        public string FirstName { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(25)]
        public string LastName { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(100)]
        public string Gender { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(100)]
        public string PhoneNumber { get; set; } = "";
        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(250)]
        public string CountryName { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(250)]
        public string StateName { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(250)]
        public string CityName { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(1000)]
        public string Address { get; set; } = "";
        [Column(TypeName = "NVARCHAR")]
        [MaxLength(1500)]
        public string BlockedReason { get; set; } = "";

        [Column(TypeName = "NVARCHAR")]
        [MaxLength(300)]
        public string ProfileImageUrl { get; set; } = "";
        public bool IsEmailVerified { get; set; } = false;
        public bool IsBlocked { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
    }
}
