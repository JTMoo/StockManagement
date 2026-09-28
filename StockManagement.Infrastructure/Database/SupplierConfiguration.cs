using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
	public void Configure(EntityTypeBuilder<Supplier> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.HasIndex(supplier => supplier.Name).IsUnique();
	}
}
