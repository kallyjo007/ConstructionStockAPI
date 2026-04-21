using System;
using System.Collections.Generic;
using ConstructionStockAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ConstructionStockAPI.Data;

public partial class ConstructionStockDbContext : DbContext
{
    public ConstructionStockDbContext(DbContextOptions<ConstructionStockDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<LowStockAlert> LowStockAlerts { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Items__727E838B207669EB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Site).WithMany(p => p.Items)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Items_Sites");
        });

        modelBuilder.Entity<LowStockAlert>(entity =>
        {
            entity.HasKey(e => e.AlertId).HasName("PK__LowStock__EBB16A8DAF85D4C0");

            entity.Property(e => e.AlertDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Item).WithMany(p => p.LowStockAlerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alert_Items");

            entity.HasOne(d => d.Site).WithMany(p => p.LowStockAlerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alert_Sites");
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.SiteId).HasName("PK__Sites__B9DCB963F24442C5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__StockTra__55433A6B73502EE6");

            entity.ToTable(tb => tb.HasTrigger("trg_AfterStockTransaction"));

            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Item).WithMany(p => p.StockTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stock_Items");

            entity.HasOne(d => d.RecordedByUser).WithMany(p => p.StockTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stock_Users");

            entity.HasOne(d => d.Site).WithMany(p => p.StockTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stock_Sites");

            entity.HasOne(d => d.Supplier).WithMany(p => p.StockTransactions).HasConstraintName("FK_Stock_Supplier");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666B4032F9B2A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CB450C408");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Site).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Sites");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
