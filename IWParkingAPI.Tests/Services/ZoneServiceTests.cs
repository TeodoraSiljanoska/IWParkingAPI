using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models;
using IWParkingAPI.Services.Implementation;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace IWParkingAPI.Tests
{
    public class ZoneServiceTests
    {
        //TESTS FOR GETALLZONES
        [Fact]
        public void GetAllZones_ReturnsEmptyResponse_WhenNoZones()
        {
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            zoneRepository.GetAsQueryable(Arg.Any<Expression<Func<Zone, bool>>>(),
                                          Arg.Any<Func<IQueryable<Zone>, IOrderedQueryable<Zone>>>(),
                                          Arg.Any<Func<IQueryable<Zone>, IIncludableQueryable<Zone, object>>>(),
                                          Arg.Any<Expression<Func<Zone, object>>>())
                          .Returns(new List<Zone>().AsQueryable());

            // Act
            var result = zoneService.GetAllZones();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            Assert.Equal("There aren't any zones.", result.Message);
            Assert.Empty(result.Zones);
        }

        [Fact]
        public void GetAllZones_ReturnsZones_WhenZonesExist()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            var zones = new List<Zone>
        {
            new Zone { Id = 1, Name = "Zone 1" },
            new Zone { Id = 2, Name = "Zone 2" },
        };

            zoneRepository.GetAsQueryable(null, Arg.Any<Func<IQueryable<Zone>, IOrderedQueryable<Zone>>>(),
                                          null, Arg.Any<Expression<Func<Zone, object>>>())
                          .Returns(zones.AsQueryable());

            // Act
            var result = zoneService.GetAllZones();

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Zones returned successfully", result.Message);
            Assert.Equal(zones, result.Zones);
        }

        [Fact]
        public void GetAllZones_ThrowsInternalErrorException_OnException()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            zoneRepository.When(repo => repo.GetAsQueryable(null, Arg.Any<Func<IQueryable<Zone>, IOrderedQueryable<Zone>>>(),
                                                            null, Arg.Any<Expression<Func<Zone, object>>>()))
                          .Throw(new Exception("Simulated error"));

            // Act & Assert
            Assert.Throws<InternalErrorException>(() => zoneService.GetAllZones());
        }

        //TESTS FOR GETZONEBYID
        [Fact]
        public void GetZoneById_ReturnsZone_WhenZoneExists()
        {
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            var expectedZone = new Zone { Id = 1, Name = "Zone 1" };
            zoneRepository.GetById(1).Returns(expectedZone);

            // Act
            var result = zoneService.GetZoneById(1);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Zone returned successfully", result.Message);
            Assert.Equal(expectedZone, result.Zone);
        }

        [Fact]
        public void GetZoneById_ThrowsNotFoundException_WhenZoneDoesNotExist()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            zoneRepository.GetById(1).Returns((Zone)null);

            // Act & Assert
            var exception = Assert.Throws<NotFoundException>(() => zoneService.GetZoneById(300));
            Assert.Equal("Zone not found", exception.Message);
        }

        [Fact]
        public void GetZoneById_ThrowsBadRequestException_OnInvalidId()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            // Act & Assert
            var exception = Assert.Throws<BadRequestException>(() => zoneService.GetZoneById(0));
            Assert.Equal("Zone Id is required", exception.Message);
        }

        [Fact]
        public void GetZoneById_ThrowsInternalErrorException_OnException()
        {
            // Arrange
            var unitOfWork = Substitute.For<IUnitOfWork<ParkingDbContext>>();
            var zoneRepository = Substitute.For<IGenericRepository<Zone>>();
            var mapper = Substitute.For<IMapper>();

            var zoneService = new ZoneService(unitOfWork);

            // Use reflection to set private readonly fields
            SetPrivateField(zoneService, "_zoneRepository", zoneRepository);
            SetPrivateField(zoneService, "_mapper", mapper);

            zoneRepository.GetById(1).Throws(new Exception("Simulated error"));

            // Act & Assert
            Assert.Throws<InternalErrorException>(() => zoneService.GetZoneById(1));
        }
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                throw new ArgumentException($"Field with name '{fieldName}' not found in type '{type.FullName}'.");
            }
        }
    }
}