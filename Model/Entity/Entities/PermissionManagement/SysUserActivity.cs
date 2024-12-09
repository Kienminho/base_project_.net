using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities.PermissionManagement;

[Table("Template_Sys_User_Activity")]
public class SysUserActivity : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ActivityId { get; set; }
    public bool C { get; set; }
    public bool R { get; set; }
    public bool U { get; set; }
    public bool D { get; set; }
    public string? Description { get; set; }
}

public class SysUserActivityConfiguration : IEntityTypeConfiguration<SysUserActivity>
{
    public void Configure(EntityTypeBuilder<SysUserActivity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
        builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
        builder.Property(a => a.ActivityId).HasColumnType("uniqueidentifier").IsRequired();
        builder.Property(a => a.C).HasColumnType("bit").IsRequired();
        builder.Property(a => a.R).HasColumnType("bit").IsRequired();
        builder.Property(a => a.U).HasColumnType("bit").IsRequired();
        builder.Property(a => a.D).HasColumnType("bit").IsRequired();
        builder.Property(a => a.Description).HasColumnType("nvarchar(255)");
        builder.HasIndex(x => x.CreatedDate);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
        builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
        builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
    }
}