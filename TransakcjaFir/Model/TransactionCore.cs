using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace TransakcjaFir.Model
{
    public class TransactionCore : ITransactionPartVersion
    {
        public long Id { get; set; }
        public List<Transaction> Transaction { get; set; }
        public string TransactionReference { get; set; }
        public int VersionNumber { get; set; }
        public bool IsLastVersion { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentDetails1 { get; set; }
        public string PaymentDetails2 { get; set; }
        public string PaymentDetails3 { get; set; }
        public string PaymentDetails4 { get; set; }
    }

    internal class TransactionCoreEntityTypeConfiguration : IEntityTypeConfiguration<TransactionCore>
    {
        public void Configure(EntityTypeBuilder<TransactionCore> builder)
        {
            builder.ToTable("TransactionCore", "REP");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("TransactionCoreId");
        }
    }
}
