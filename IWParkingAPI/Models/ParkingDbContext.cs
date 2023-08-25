﻿using System;
using System.Collections.Generic;
using IWParkingAPI.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace IWParkingAPI.Models;

public partial class ParkingDbContext : DbContext
{
    public ParkingDbContext()
    {
    }

    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<ParkingLot> ParkingLots { get; set; }

    public virtual DbSet<ParkingLotRequest> ParkingLotRequests { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<TempParkingLot> TempParkingLots { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<Zone> Zones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsDeactivated)
                .IsRequired()
                .HasDefaultValueSql("('False')");
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.Surname).HasMaxLength(256);
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.ParkingLotsNavigation).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UsersFavouriteParkingLot",
                    r => r.HasOne<ParkingLot>().WithMany().HasForeignKey("ParkingLotId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "ParkingLotId");
                        j.ToTable("UsersFavouriteParkingLots");
                    });

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__City__3214EC07E968CC48");

            entity.ToTable("City");

            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Parking __3214EC071BD0950E");

            entity.ToTable("ParkingLot");

            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.CapacityAdaptedCar).HasColumnName("Capacity_Adapted_Car");
            entity.Property(e => e.CapacityCar).HasColumnName("Capacity_Car");
            entity.Property(e => e.CityId).HasColumnName("City_Id");
            entity.Property(e => e.IsDeactivated)
                .IsRequired()
                .HasDefaultValueSql("('False')");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.WorkingHourFrom).HasColumnName("Working_Hour_From");
            entity.Property(e => e.WorkingHourTo).HasColumnName("Working_Hour_To");
            entity.Property(e => e.ZoneId).HasColumnName("Zone_Id");

            entity.HasOne(d => d.City).WithMany(p => p.ParkingLots)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParkingLot.City_Id");

            entity.HasOne(d => d.User).WithMany(p => p.ParkingLots)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parking Lot.User_Id");

            entity.HasOne(d => d.Zone).WithMany(p => p.ParkingLots)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParkingLot.Zone_Id");
        });

        modelBuilder.Entity<ParkingLotRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Request__3214EC07DC5C8037");

            entity.ToTable("ParkingLotRequest");

            entity.Property(e => e.ParkingLotId).HasColumnName("Parking_Lot_Id");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.Type).HasDefaultValueSql("((1))");
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.User).WithMany(p => p.ParkingLotRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Request.User_Id");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3214EC07A67200A6");

            entity.ToTable("Payment");

            entity.Property(e => e.MadeOn)
                .HasColumnType("datetime")
                .HasColumnName("Made_On");
            entity.Property(e => e.ReservationId).HasColumnName("Reservation_Id");
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.Reservation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment.Reservation_Id");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reservat__3214EC07E6E4DA8B");

            entity.ToTable("Reservation");

            entity.Property(e => e.EndDate)
                .HasColumnType("date")
                .HasColumnName("End_Date");
            entity.Property(e => e.EndTime).HasColumnName("End_Time");
            entity.Property(e => e.IsPaid).HasColumnName("Is_Paid");
            entity.Property(e => e.ParkingLotId).HasColumnName("Parking_Lot_Id");
            entity.Property(e => e.StartDate)
                .HasColumnType("date")
                .HasColumnName("Start_Date");
            entity.Property(e => e.StartTime).HasColumnName("Start_Time");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.ParkingLot).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.ParkingLotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation.Parking_Lot_Id");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation.User_Id");
        });

        modelBuilder.Entity<TempParkingLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TempPark__3214EC07C2EAA1CD");

            entity.ToTable("TempParkingLot");

            entity.Property(e => e.Address).HasMaxLength(20);
            entity.Property(e => e.CapacityAdaptedCar).HasColumnName("Capacity_Adapted_Car");
            entity.Property(e => e.CapacityCar).HasColumnName("Capacity_Car");
            entity.Property(e => e.CityId).HasColumnName("City_Id");
            entity.Property(e => e.IsDeactivated)
                .IsRequired()
                .HasDefaultValueSql("('False')");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.ParkingLotId).HasColumnName("ParkingLot_Id");
            entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.WorkingHourFrom).HasColumnName("Working_Hour_From");
            entity.Property(e => e.WorkingHourTo).HasColumnName("Working_Hour_To");
            entity.Property(e => e.ZoneId).HasColumnName("Zone_Id");

            entity.HasOne(d => d.City).WithMany(p => p.TempParkingLots)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TempParkingLot.City_Id");

            entity.HasOne(d => d.User).WithMany(p => p.TempParkingLots)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TempParking Lot.User_Id");

            entity.HasOne(d => d.Zone).WithMany(p => p.TempParkingLots)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TempParkingLot.Zone_Id");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vehicle__3214EC07CE7B2E42");

            entity.ToTable("Vehicle");

            entity.Property(e => e.IsPrimary)
                .IsRequired()
                .HasDefaultValueSql("('False')");
            entity.Property(e => e.PlateNumber)
                .HasMaxLength(8)
                .HasColumnName("Plate_Number");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.UserId).HasColumnName("User_Id");

            entity.HasOne(d => d.User).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehicle.User_Id");
        });

        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Zone__3214EC07435A2BC6");

            entity.ToTable("Zone");

            entity.Property(e => e.Name).HasMaxLength(256);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
