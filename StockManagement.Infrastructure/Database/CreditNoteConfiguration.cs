using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


internal sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
	public void Configure(EntityTypeBuilder<CreditNote> builder)
	{
		builder.Property<string>("Id").ValueGeneratedOnAdd();
		builder.HasKey("Id");
		builder.HasIndex(creditNote => creditNote.Number).IsUnique();
		builder.HasOne(creditNote => creditNote.Invoice).WithMany().HasForeignKey("InvoiceId").IsRequired();
		builder.HasIndex("InvoiceId").IsUnique();
		builder.Property(creditNote => creditNote.Total).HasPrecision(18, 2);
		builder.Property(creditNote => creditNote.Tax).HasPrecision(18, 2);

		// DateTime.Now (Kind=Local); Npgsql only accepts UTC for "timestamp with time zone"
		builder.Property(creditNote => creditNote.Date).HasColumnType("timestamp without time zone");

		builder.Navigation(creditNote => creditNote.Invoice).AutoInclude();
	}
}
