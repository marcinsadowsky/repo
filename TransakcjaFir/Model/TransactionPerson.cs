using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TransakcjaFir.Model
{
    public class TransactionPerson
    {
        public long TransactionPersonId { get; set; }
        public TransactionPersonsList TransactionPersonsList { get; set; }
        public long TransactionPersonsListId { get; set; }
        public long PersonId { get; set; }
        public bool IsAmlMainDisposer { get; set; }
        public bool IsAmlAdditionalDisposer { get; set; }
        public bool IsAmlUbo { get; set; }
        public bool IsStirMainDisposer { get; set; }
        public bool IsStirDisposer { get; set; }
        public string PersonName { get; set; }

        public bool IsAmlPerson() => IsAmlMainDisposer || IsAmlAdditionalDisposer || IsAmlUbo;
        public bool IsStirPerson() => IsStirMainDisposer || IsStirDisposer;

        public static TransactionPerson CloneAsNewVersion(TransactionPerson original) =>
           new TransactionPerson()
           {
               TransactionPersonId = 0,
               PersonId = original.PersonId,
               IsAmlMainDisposer = original.IsAmlMainDisposer,
               IsAmlAdditionalDisposer = original.IsAmlAdditionalDisposer,
               IsAmlUbo = original.IsAmlUbo,
               IsStirMainDisposer = original.IsStirMainDisposer,
               IsStirDisposer = original.IsStirDisposer,
               PersonName = original.PersonName,
           };
    }

    internal class TransactionPersonEntityTypeConfiguration : IEntityTypeConfiguration<TransactionPerson>
    {
        public void Configure(EntityTypeBuilder<TransactionPerson> builder)
        {
            builder.ToTable("TransactionPerson", "REP");
            builder.HasKey(c => c.TransactionPersonId);
        }
    }
}
