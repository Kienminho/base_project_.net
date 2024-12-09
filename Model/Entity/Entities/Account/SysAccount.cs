using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities.Account
{
    [Table("Template_Sys_Account")]
    public class SysAccount : BaseEntity
    {
        public Guid Id { get; set; }
        public required string Code { get; set; } // Mã nhân viên
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Group { get; set; }
        public string? Base { get; set; }
        public string? Note { get; set; }
        public string? FullNameNoAccent { get; set; }
        public string? Avatar { get; set; }

        [NotMapped]
        public string? FullName => $"{LastName} {FirstName}";
    }

    public class SysAccountConfiguration : IEntityTypeConfiguration<SysAccount>
    {
        public void Configure(EntityTypeBuilder<SysAccount> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => new { a.Code, a.UserName }).IsUnique();
            builder.Property(a => a.Code).HasColumnType("varchar(15)").IsRequired();
            builder.Property(a => a.UserName).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.Email).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.Base).HasColumnType("varchar(15)");
            builder.Property(a => a.Group).HasColumnType("varchar(50)");
            builder.Property(a => a.Note).HasColumnType("nvarchar(255)");
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
