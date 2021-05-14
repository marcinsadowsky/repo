using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TransakcjaFir.DAL;
using Xunit;

namespace TransakcjaFirTests
{
    public class QuantityChecks
    {
        [Fact]
        public async void Should_Find_1000_Transactions_With_Version_1()
        {
            FirContext context = new FirContext();
            var count = await context.Transactions.Where(t => t.VersionNumber == 1).CountAsync();
            count.Should().Be(1000);
        }

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
                .Include(t => t.Persons).ThenInclude(l => l.List)
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();

            transaction.Should().NotBeNull();
            transaction.Core.Should().NotBeNull();
            transaction.Aml.Should().NotBeNull();
            transaction.Stir.Should().NotBeNull();
            transaction.Persons.Should().NotBeNull();
            transaction.Persons.List.Should().NotBeNull();
            transaction.Persons.List.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public async void Should_Load_Set_Of_500_Transaction_Objects()
        {
            FirContext context = new FirContext();

            var transactions = await context
                .Transactions
                .Include(t => t.Core)
                .Include(t => t.Aml)
                .Include(t => t.Stir)
                .Include(t => t.Persons).ThenInclude(l => l.List)
                .Take(500)
                .ToListAsync();

            transactions.Count().Should().Be(500);
            transactions.Select(t => t.Core).Count().Should().Be(500);
            transactions.Select(t => t.Aml).Count().Should().Be(500);
            transactions.Select(t => t.Stir).Count().Should().Be(500);
            transactions.Select(t => t.Persons).Count().Should().Be(500);
        }
    }
}
