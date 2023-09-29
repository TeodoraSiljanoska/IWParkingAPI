using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models;
using IWParkingAPI.Services.Implementation;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using NSubstitute;
using IWParkingAPI.Models.Enums;
using IWParkingAPI.CustomExceptions;
using System;
using System.Collections.Generic;

namespace IWParkingAPI.Tests
{
    public class ReservationServiceTests
    {
        private readonly ReservationService _reservationService;

        public ReservationServiceTests()
        {
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var jWTDecode = Substitute.For<IJWTDecode>();
            var localTime = Substitute.For<ILocalTimeExtension>();
            var calculateCapacityExtension = Substitute.For<ICalculateCapacityExtension>();
            var enumsExtension = Substitute.For<IEnumsExtension<Enums.VehicleTypes>>();

            _reservationService = new ReservationService(
                unitOfWork,
                jWTDecode,
                calculateCapacityExtension,
                enumsExtension,
                localTime);
        }


        [Fact]
        public void ValidateDateTimeRange_ThrowsBadRequestException_WhenInvalidDates()
        {
            // Arrange
            var parkingLot = new ParkingLot
            {
                Id = 1,
                Name = "Parking Lot",
                City = "Skopje",
                Zone = "A1",
                Address = "Ul. Partizanska",
                WorkingHourFrom = TimeSpan.FromHours(8),
                WorkingHourTo = TimeSpan.FromHours(18),
                CapacityCar = 100,
                CapacityAdaptedCar = 10
            };
            var reservationStartDateTime = DateTime.Now.AddDays(1).Date.AddHours(7);
            var reservationEndDateTime = DateTime.Now.AddDays(1).Date.AddHours(19);

            // Act & Assert
            Assert.Throws<BadRequestException>(() =>
            {
                _reservationService.ValidateDateTimeRange(parkingLot, reservationStartDateTime, reservationEndDateTime);
            });
        }

        [Fact]
        public void CheckIfVehicleIsValid_ReturnsValidVehicle_WhenValidVehicleSelected()
        {
            // Arrange
            var request = new MakeReservationRequest
            {
                PlateNumber = "SK1234VV"
            };

            var user = new AspNetUser
            {
                Vehicles = new List<Vehicle>
            {
                new Vehicle { PlateNumber = "SK1234VV" } // Valid vehicle in the user's list
            }
            };

            // Act
            var selectedVehicle = ReservationService.CheckIfVehicleIsValid(request, user);

            // Assert
            Assert.NotNull(selectedVehicle);
            Assert.Equal(request.PlateNumber, selectedVehicle.PlateNumber);
        }

        [Fact]
        public void CheckIfVehicleIsValid_ThrowsBadRequestException_WhenInvalidVehicleSelected()
        {
            // Arrange
            var request = new MakeReservationRequest
            {
                PlateNumber = "InvalidPlateNumber"
            };

            var user = new AspNetUser
            {
                Vehicles = new List<Vehicle>
            {
                new Vehicle { PlateNumber = "SK1234VV" } // Valid vehicle in the user's list
            }
            };

            // Act & Assert
            var exception = Assert.Throws<BadRequestException>(() =>
            {
                ReservationService.CheckIfVehicleIsValid(request, user);
            });

            // Assert the exception message
            Assert.Equal("Please select a valid vehicle to make a reservation", exception.Message);
        }

    }
}