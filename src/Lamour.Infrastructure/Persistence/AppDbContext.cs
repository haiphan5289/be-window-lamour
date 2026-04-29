using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Supplier>              Suppliers              => Set<Supplier>();
    public DbSet<Product>               Products               => Set<Product>();
    public DbSet<Customer>              Customers              => Set<Customer>();
    public DbSet<Employee>              Employees              => Set<Employee>();
    public DbSet<CashTransaction>       CashTransactions       => Set<CashTransaction>();
    public DbSet<Receipt>               Receipts               => Set<Receipt>();
    public DbSet<ReceiptEntry>          ReceiptEntries         => Set<ReceiptEntry>();
    public DbSet<Warehouse>             Warehouses             => Set<Warehouse>();
    public DbSet<WarehouseReceipt>      WarehouseReceipts      => Set<WarehouseReceipt>();
    public DbSet<WarehouseReceiptLine>  WarehouseReceiptLines  => Set<WarehouseReceiptLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
