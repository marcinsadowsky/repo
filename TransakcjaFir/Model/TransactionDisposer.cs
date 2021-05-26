using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TransakcjaFir.Model
{
    public class TransactionDisposer
    {
        public long Id { get; set; }
        public TransactionDisposersList TransactionDisposersList { get; set; }
        public long TransactionDisposersListId { get; set; }
        public long PersonId { get; set; }
        public bool IsAmlMainDisposer { get; set; }
        public bool IsAmlAdditionalDisposer { get; set; }
        public bool IsStirDisposer { get; set; }
        public string PersonName { get; set; }

        public bool IsAmlPerson() => IsAmlMainDisposer || IsAmlAdditionalDisposer;
        public bool IsStirPerson() => IsStirDisposer;
      
    }

    internal class TransactionPersonEntityTypeConfiguration : IEntityTypeConfiguration<TransactionDisposer>
    {
        public void Configure(EntityTypeBuilder<TransactionDisposer> builder)
        {
            builder.ToTable("TransactionDisposer", "REP");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("TransactionDisposerId");
        }
    }
}
