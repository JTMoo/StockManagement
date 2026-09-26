using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
	public void Configure(EntityTypeBuilder<StockItem> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.HasIndex(item => item.Code).IsUnique();
		builder.Property(item => item.Price).HasPrecision(18, 2);
		builder.Property(item => item.Factor).HasPrecision(18, 2);
	}
}
