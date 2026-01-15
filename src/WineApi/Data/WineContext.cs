using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace WineApi.Data;

public partial class WineContext : DbContext
{
    public WineContext()
    {
    }

    public WineContext(DbContextOptions<WineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bottle> Bottles { get; set; }

    public virtual DbSet<Storage> Storages { get; set; }

    public virtual DbSet<Wine> Wines { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_bin")
            .HasCharSet("latin1");

        modelBuilder.Entity<Bottle>(entity =>
        {
            entity.HasKey(e => e.Bottleid).HasName("PRIMARY");

            entity.HasIndex(e => e.Storageid, "IX_tblBottles_storageid");

            entity.HasIndex(e => e.Wineid, "IX_tblBottles_wineid");

            entity.Property(e => e.Bottleid).HasColumnName("bottleid");
            entity.Property(e => e.BinX).HasColumnName("binX");
            entity.Property(e => e.BinY).HasColumnName("binY");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.ConsumedDate)
                .HasColumnType("datetime")
                .HasColumnName("consumed_date");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Depth)
                .HasDefaultValueSql("'1'")
                .HasColumnName("depth");
            entity.Property(e => e.Storageid).HasColumnName("storageid");
            entity.Property(e => e.TsDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("ts_date");
            entity.Property(e => e.Wineid).HasColumnName("wineid");

            entity.HasOne(d => d.Storage).WithMany(p => p.Bottles)
                .HasForeignKey(d => d.Storageid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bottles_storage");

            entity.HasOne(d => d.Wine).WithMany(p => p.Bottles)
                .HasForeignKey(d => d.Wineid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bottles_wine");
        });

        modelBuilder.Entity<Storage>(entity =>
        {
            entity.ToTable("storage");

            entity.HasKey(e => e.Storageid).HasName("PRIMARY");

            entity.Property(e => e.Storageid).HasColumnName("storageid");
            entity.Property(e => e.StorageAddr1)
                .HasMaxLength(50)
                .HasColumnName("storageAddr1");
            entity.Property(e => e.StorageAddr2)
                .HasMaxLength(50)
                .HasColumnName("storageAddr2");
            entity.Property(e => e.StorageCity)
                .HasMaxLength(50)
                .HasColumnName("storageCity");
            entity.Property(e => e.StorageDescription)
                .HasMaxLength(50)
                .HasColumnName("storageDescription");
            entity.Property(e => e.StorageState)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("storageState");
            entity.Property(e => e.StorageZip).HasColumnName("storageZip");
            entity.Property(e => e.TsDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("ts_date");
        });

        modelBuilder.Entity<Wine>(entity =>
        {
            entity.HasKey(e => e.Wineid).HasName("PRIMARY");

            entity.Property(e => e.Wineid).HasColumnName("wineid");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Label)
                .HasMaxLength(50)
                .HasColumnName("label");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.TsDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("ts_date");
            entity.Property(e => e.Varietal)
                .HasMaxLength(30)
                .HasColumnName("varietal");
            entity.Property(e => e.Vineyard)
                .HasMaxLength(50)
                .HasColumnName("vineyard");
            entity.Property(e => e.Vintage).HasColumnName("vintage");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Key, "key_UNIQUE").IsUnique();

            entity.HasIndex(e => e.Username, "username_UNIQUE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key)
                .HasMaxLength(45)
                .HasColumnName("key");
            entity.Property(e => e.KeyExpires)
                .HasColumnType("datetime")
                .HasColumnName("key_expires");
            entity.Property(e => e.LastOn)
                .HasMaxLength(45)
                .HasColumnName("last_on");
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .HasColumnName("password");
            entity.Property(e => e.Salt)
                .HasMaxLength(32)
                .HasColumnName("salt");
            entity.Property(e => e.Username)
                .HasMaxLength(45)
                .HasColumnName("username");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSnakeCaseNamingConvention();
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
