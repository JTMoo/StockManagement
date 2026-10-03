using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class PaymentLinkConfiguration : IEntityTypeConfiguration<PaymentLink>
{
	public void Configure(EntityTypeBuilder<PaymentLink> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.HasIndex(link => link.ExternalId).IsUnique();
		builder.HasOne(link => link.Invoice).WithMany().IsRequired();
		builder.Navigation(link => link.Invoice).AutoInclude();
		builder.Property(link => link.Amount).HasPrecision(18, 2);

		// DateTime.Now (Kind=Local); Npgsql only accepts UTC for "timestamp with time zone"
		builder.Property(link => link.CreatedAt).HasColumnType("timestamp without time zone");
		builder.Property(link => link.ExpiresAt).HasColumnType("timestamp without time zone");
		builder.Property(link => link.PaidAt).HasColumnType("timestamp without time zone");
	}
}
