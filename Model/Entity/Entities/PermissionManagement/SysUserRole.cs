using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities.PermissionManagement;

[Table("Template_Sys_UserRole")]
public class SysUserRole : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string? Description { get; set; }
}

public class SysUserRoleConfiguration : IEntityTypeConfiguration<SysUserRole>
{
    public void Configure(EntityTypeBuilder<SysUserRole> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
        builder.Property(a => a.UserId).HasColumnType("uniqueidentifier").IsRequired();
        builder.Property(a => a.RoleId).HasColumnType("uniqueidentifier").IsRequired();
        builder.Property(a => a.Description).HasColumnType("nvarchar(255)");
        builder.HasIndex(x => x.CreatedDate);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
        builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
        builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
    }
}