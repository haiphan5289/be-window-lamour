using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Supplier>           Suppliers           => Set<Supplier>();
    public DbSet<Product>            Products            => Set<Product>();
    public DbSet<Customer>           Customers           => Set<Customer>();
    public DbSet<Employee>           Employees           => Set<Employee>();
    public DbSet<CashTransaction>    CashTransactions    => Set<CashTransaction>();
    public DbSet<PaymentReceipt>     PaymentReceipts     => Set<PaymentReceipt>();
    public DbSet<PaymentReceiptLine> PaymentReceiptLines => Set<PaymentReceiptLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
