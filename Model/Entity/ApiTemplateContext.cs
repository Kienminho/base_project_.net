using Entity.Entities.Account;
using Entity.Entities.PermissionManagement;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Entity;

public partial class ApiTemplateContext : DbContext
{
    public ApiTemplateContext()
    {
    }

    public ApiTemplateContext(DbContextOptions<ApiTemplateContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SysAccount> SysAccounts { get; set; }
    public virtual DbSet<SysDevice> SysDevices { get; set; }
    public virtual DbSet<SysRole> SysRoles { get; set; }
    public virtual DbSet<SysUserRole> SysUserRoles { get; set; }
    public virtual DbSet<SysActivity> SysUsers { get; set; }
    public virtual DbSet<SysRoleActivity> SysRoleActivities { get; set; }
    public virtual DbSet<SysUserActivity> SysUserActivities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       => base.OnConfiguring(optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<BaseEntity>();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
