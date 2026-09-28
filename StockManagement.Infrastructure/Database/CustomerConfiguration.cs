using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
	public const string IdentificationNumberIndexName = "IX_Customers_IdentificationNumber";


	public void Configure(EntityTypeBuilder<Customer> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.HasIndex(customer => customer.CustomerId).IsUnique();
		builder.HasIndex(customer => customer.IdentificationNumber)
			.IsUnique()
			.HasDatabaseName(IdentificationNumberIndexName)
			.HasFilter("\"IdentificationNumber\" <> ''");
	}
}
