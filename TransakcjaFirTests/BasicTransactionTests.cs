using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TransakcjaFir.DAL;
using Xunit;

namespace TransakcjaFirTests
{
    public class BasicTransactionTests
    {
        [Fact]
        public async void Should_Load_Whole_Transaction_Object()
        {
            FirContext context = new FirContext();
            var id = (await context.Transactions.FirstAsync()).Id;

            var transaction = await context
                .Transactions
                .Include(t => t.Core)
                .Include(t => t.Aml)
                .Include(t => t.Stir)
                .Include(t => t.Disposers).ThenInclude(l => l.List)
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();

            transaction.Should().NotBeNull();
            transaction.Core.Should().NotBeNull();
            transaction.Aml.Should().NotBeNull();
            transaction.Stir.Should().NotBeNull();
            transaction.Disposers.Should().NotBeNull();
            transaction.Disposers.List.Should().NotBeNull();
            transaction.Disposers.List.Count().Should().BeGreaterThan(0);
        }
    }
}
