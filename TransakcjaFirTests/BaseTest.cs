using FluentAssertions;
using System;
using TransakcjaFir.Model;

namespace TransakcjaFirTests
{
    public class BaseTest
    {
        public enum PersonsListOperation { Add, Modify, Delete }
        public enum AmlPersonsRole { None, MainDisposer, AdditionalDisposer, Ubo }
        public enum StirPersonsRole { None, MainDisposer, Disposer }

        internal static string GenerateReference(string prefix) => 
            $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.ffff} {prefix} {Guid.NewGuid()}".Substring(0, 40);

        protected void CheckTransactionData(Transaction data, int versionNumber, bool isLastVersion)
        {
            data.VersionNumber.Should().Be(versionNumber);
            data.IsLastVersion.Should().Be(isLastVersion);
        }

        protected void CheckTransactionCoreData(TransactionCore data, int versionNumber, bool isLastVersion)
        {
            data.VersionNumber.Should().Be(versionNumber);
            data.IsLastVersion.Should().Be(isLastVersion);
        }

        protected void CheckPersonsData(TransactionPersonsList data, int versionNumber, bool isLastVersion, int personsCount)
        {
            data.VersionNumber.Should().Be(versionNumber);
            data.IsLastVersion.Should().Be(isLastVersion);
            data.List.Count.Should().Be(personsCount);
        }

        protected void CheckAmlData(TransactionAml data, int versionNumber, bool isLastVersion, AmlExportStatusEnum status)
        {
            data.VersionNumber.Should().Be(versionNumber);
            data.IsLastVersion.Should().Be(isLastVersion);
            data.ProcessingStatus.Should().Be(status);
        }

        protected void CheckStirData(TransactionStir data, int versionNumber, bool isLastVersion, StirExportStatusEnum status)
        {
            data.VersionNumber.Should().Be(versionNumber);
            data.IsLastVersion.Should().Be(isLastVersion);
            data.ProcessingStatus.Should().Be(status);
        }
    }
}
