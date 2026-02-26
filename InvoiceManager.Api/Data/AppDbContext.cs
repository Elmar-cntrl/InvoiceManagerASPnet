using Microsoft.EntityFrameworkCore;
using InvoiceManager.Api.Entities;

namespace InvoiceManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceRow> InvoiceRows => Set<InvoiceRow>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //  User (1) → (Many) Customers
        //  Customer НЕ МОЖЕТ существовать без User
        //  UserId — обязательный
        //  Чтобы создать Customer, User уже должен быть в БД
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithMany(u => u.Customers)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        // Invoice (1) → (Many) InvoiceRows
        // InvoiceRow НЕ МОЖЕТ существовать без Invoice
        // InvoiceId — обязательный
        // Чтобы создать InvoiceRow, Invoice уже должен быть в БД
        modelBuilder.Entity<InvoiceRow>()
            .HasOne(r => r.Invoice)
            .WithMany(i => i.Rows)
            .HasForeignKey(r => r.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        
        // Customer (1) → (Many) Invoices
        // Invoice НЕ МОЖЕТ существовать без Customer
        //  CustomerId — обязательный
        //  Чтобы создать Invoice, Customer уже должен быть в БД
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
    
    //  User
    //    └── Customer
    //            └── Invoice
    //                   └── InvoiceRow
    
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Entities.User ||
                        e.Entity is Entities.Customer ||
                        e.Entity is Entities.Invoice);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTimeOffset.UtcNow;
                entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
            }
        }
    }
    /////////////////////////////////////////////////////
    
}