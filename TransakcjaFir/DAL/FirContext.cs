using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace TransakcjaFir.DAL
{
    public class FirContext : DbContext
    {
        public DbSet<Model.Transaction> Transactions { get; set; }
        public DbSet<Model.TransactionCore> TransactionCores { get; set; }
        public DbSet<Model.TransactionAml> TransactionAmls { get; set; }
        public DbSet<Model.TransactionStir> TransactionStirs { get; set; }
        public DbSet<Model.TransactionDisposersList> TransactionPersonsLists { get; set; }
        public DbSet<Model.TransactionDisposer> TransactionPersons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=localhost;Database=TransactionFirDb;Trusted_Connection=True;"
                , options => options.EnableRetryOnFailure(20, TimeSpan.FromSeconds(3), null)
                );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ApplyEntitiesConfiguration(modelBuilder);
        }

        protected void ApplyEntitiesConfiguration(ModelBuilder modelBuilder)
        {
            var applyGenericMethod = typeof(ModelBuilder)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == "ApplyConfiguration")
                .Where(m => m.GetGenericArguments().FirstOrDefault(a => a.Name == "TEntity") != null)
                .FirstOrDefault();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
                        applyConcreteMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });
                        break;
                    }
                }
            }
        }
    }
}
