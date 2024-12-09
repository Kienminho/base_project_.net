using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entity.Entities.Account
{
    [Table("Template_Sys_Device")]
    public class SysDevice : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UDID { get; set; } = null!;
        public string? OSVersion { get; set; }
        public string? OSName { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceDescription { get; set; }
        public bool IsActive { get; set; } = false;
        public string RefreshToken { get; set; } = null!;
        public DateTime RfTokenCreateTime { get; set; }
        public DateTime RfTokenExpiryTime { get; set; }
        public DateTime? RfTokenRevokedTime { get; set; }
        public string RfTokenCreatedByIp { get; set; } = null!;
        public string? RfTokenRevokedByIp { get; set; }
    }

    public class SysDeviceConfiguration : IEntityTypeConfiguration<SysDevice>
    {
        public void Configure(EntityTypeBuilder<SysDevice> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => new { a.UserId, a.UDID }).IsUnique();
            builder.Property(a => a.UDID).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.OSVersion).HasColumnType("varchar(50)");
            builder.Property(a => a.OSName).HasColumnType("varchar(50)");
            builder.Property(a => a.DeviceType).HasColumnType("varchar(50)");
            builder.Property(a => a.DeviceName).HasColumnType("varchar(50)");
            builder.Property(a => a.DeviceDescription).HasColumnType("nvarchar(255)");
            builder.Property(a => a.RefreshToken).HasColumnType("varchar(1000)").IsRequired();
            builder.Property(a => a.RfTokenCreateTime).HasColumnType("datetime").IsRequired();
            builder.Property(a => a.RfTokenExpiryTime).HasColumnType("datetime").IsRequired();
            builder.Property(a => a.RfTokenRevokedTime).HasColumnType("datetime");
            builder.Property(a => a.RfTokenCreatedByIp).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.RfTokenRevokedByIp).HasColumnType("varchar(50)");
            
            builder.HasIndex(x => x.CreatedDate);
            builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
            builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
        }
    }
}
