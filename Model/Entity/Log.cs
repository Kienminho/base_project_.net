using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entity
{
    [Table("Template_Log")]
    public class Log
    {
        public int Id { get; set; }
        public required string LogType { get; set; }
        public required string UserId { get; set; }
        public required string EndPoint { get; set; }
        public string? ParamsOrBody { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => a.CreatedDate);
            builder.Property(a => a.UserId).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.LogType).HasColumnType("varchar(10)").IsRequired();
            builder.Property(a => a.EndPoint).HasColumnType("varchar(50)").IsRequired();
            builder.Property(a => a.ParamsOrBody).HasColumnType("nvarchar(MAX)");
            builder.Property(a => a.Message).HasColumnType("nvarchar(MAX)");
            builder.Property(a => a.CreatedDate).HasColumnType("datetime").IsRequired();
        }
    }
}
