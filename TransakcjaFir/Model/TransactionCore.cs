using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace TransakcjaFir.Model
{
    public class TransactionCore
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

        public static TransactionCore Create(Transaction transaction, string reference, int version) =>
            new TransactionCore()
            {
                Transaction = new List<Transaction>() { transaction },
                TransactionReference = reference,
                VersionNumber = version,
                IsLastVersion = true
            };

        public static TransactionCore CloneAsNewVersion(TransactionCore original) =>
           new TransactionCore()
           {
               TransactionReference = original.TransactionReference,
               VersionNumber = original.VersionNumber + 1,
               IsLastVersion = true,
               Amount = original.Amount,
               Currency = original.Currency,
               PaymentDetails1 = original.PaymentDetails1,
               PaymentDetails2 = original.PaymentDetails2,
               PaymentDetails3 = original.PaymentDetails3,
               PaymentDetails4 = original.PaymentDetails4,
           };
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
