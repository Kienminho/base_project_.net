using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.Entities.PermissionManagement;

[Table("Template_Sys_SysRole")]
public class SysRole : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? RoleType { get; set; }
}

public class SysRoleConfiguration : IEntityTypeConfiguration<SysRole>
{
    public void Configure(EntityTypeBuilder<SysRole> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnType("uniqueidentifier");
        builder.Property(a => a.Name).HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(a => a.Description).HasColumnType("nvarchar(255)");
        builder.Property(a => a.RoleType).HasColumnType("int");
        builder.HasIndex(x => x.CreatedDate);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime").IsRequired();
        builder.Property(x => x.UpdatedDate).HasColumnType("datetime");
        builder.Property(x => x.CreatedName).HasColumnType("nvarchar(50)");
        builder.Property(x => x.Updater).HasColumnType("nvarchar(50)");
    }
}