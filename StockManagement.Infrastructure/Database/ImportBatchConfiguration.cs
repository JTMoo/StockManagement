using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
	public void Configure(EntityTypeBuilder<ImportBatch> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.Ignore(batch => batch.ReadyRows);

		builder.OwnsMany(batch => batch.Rows, row =>
		{
			row.WithOwner().HasForeignKey("ImportBatchId");
			row.Property<Guid>("Id").ValueGeneratedOnAdd();
			row.HasKey("Id");
			row.ToTable("ImportBatchRows");
		});
		builder.Navigation(batch => batch.Rows).AutoInclude();
	}
}
