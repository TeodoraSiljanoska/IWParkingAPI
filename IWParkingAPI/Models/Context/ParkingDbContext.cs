using System;
using System.Collections.Generic;
using IWParkingAPI.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace IWParkingAPI.Models.Context;

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
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
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
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

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
            entity.HasKey(e => e.Id).HasName("PK__City__3214EC07DA85B458");

            entity.ToTable("City");

            entity.Property(e => e.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ParkingL__3214EC07682C9CC0");

            entity.ToTable("ParkingLot");

            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.CapacityAdaptedCar).HasColumnName("Capacity_Adapted_Car");
            entity.Property(e => e.CapacityCar).HasColumnName("Capacity_Car");
            entity.Property(e => e.City).HasMaxLength(20);
            entity.Property(e => e.IsDeactivated)
                .IsRequired()
                .HasDefaultValueSql("('False')");
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.WorkingHourFrom).HasColumnName("Working_Hour_From");
            entity.Property(e => e.WorkingHourTo).HasColumnName("Working_Hour_To");
            entity.Property(e => e.Zone).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.ParkingLots)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parking Lot.User_Id");
        });

        modelBuilder.Entity<ParkingLotRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ParkingL__3214EC07E2CED285");

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
            entity.HasKey(e => e.Id).HasName("PK__Payment__3214EC07B72A109C");

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
            entity.HasKey(e => e.Id).HasName("PK__Reservat__3214EC070F3FD7DC");

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
            entity.Property(e => e.VehicleId).HasColumnName("Vehicle_Id");

            entity.HasOne(d => d.ParkingLot).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.ParkingLotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation.Parking_Lot_Id");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation.User_Id");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation.Vehicle_Id");
        });

        modelBuilder.Entity<TempParkingLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TempPark__3214EC07CEB60E3F");

            entity.ToTable("TempParkingLot");

            entity.Property(e => e.Address).HasMaxLength(256);
            entity.Property(e => e.CapacityAdaptedCar).HasColumnName("Capacity_Adapted_Car");
            entity.Property(e => e.CapacityCar).HasColumnName("Capacity_Car");
            entity.Property(e => e.City).HasMaxLength(20);
            entity.Property(e => e.IsDeactivated)
                .IsRequired()
                .HasDefaultValueSql("('False')");
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.ParkingLotId).HasColumnName("ParkingLot_Id");
            entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            entity.Property(e => e.TimeCreated).HasColumnType("datetime");
            entity.Property(e => e.TimeModified).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.WorkingHourFrom).HasColumnName("Working_Hour_From");
            entity.Property(e => e.WorkingHourTo).HasColumnName("Working_Hour_To");
            entity.Property(e => e.Zone).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.TempParkingLots)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TempParking Lot.User_Id");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vehicle__3214EC079B10CFC2");

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
            entity.HasKey(e => e.Id).HasName("PK__Zone__3214EC07B5A027D1");

            entity.ToTable("Zone");

            entity.Property(e => e.Name).HasMaxLength(256);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
