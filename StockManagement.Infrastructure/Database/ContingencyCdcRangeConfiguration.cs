using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class ContingencyCdcRangeConfiguration : IEntityTypeConfiguration<ContingencyCdcRange>
{
	public void Configure(EntityTypeBuilder<ContingencyCdcRange> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
	}
}
