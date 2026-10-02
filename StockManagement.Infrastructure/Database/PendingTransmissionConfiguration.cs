using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class PendingTransmissionConfiguration : IEntityTypeConfiguration<PendingTransmission>
{
	public void Configure(EntityTypeBuilder<PendingTransmission> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.HasOne(transmission => transmission.Invoice).WithMany().IsRequired();
		builder.Navigation(transmission => transmission.Invoice).AutoInclude();

		// DateTime.Now (Kind=Local); Npgsql only accepts UTC for "timestamp with time zone"
		builder.Property(transmission => transmission.NextAttemptAt).HasColumnType("timestamp without time zone");
	}
}
