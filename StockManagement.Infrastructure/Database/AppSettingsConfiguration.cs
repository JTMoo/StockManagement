using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
	public void Configure(EntityTypeBuilder<AppSettings> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
	}
}
