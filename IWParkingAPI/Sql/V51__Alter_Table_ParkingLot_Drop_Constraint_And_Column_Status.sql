USE [ParkingDB]
GO

ALTER TABLE [dbo].[ParkingLot] DROP  CONSTRAINT [DF_ParkingLot_Status] 
GO

ALTER TABLE [dbo].[ParkingLot] DROP COLUMN [Status]
GO