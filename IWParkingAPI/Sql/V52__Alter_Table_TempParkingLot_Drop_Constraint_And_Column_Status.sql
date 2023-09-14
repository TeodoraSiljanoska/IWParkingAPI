USE [ParkingDB]
GO

ALTER TABLE [dbo].[TempParkingLot] DROP CONSTRAINT [DF__TempParki__Statu__7A672E12]
GO

ALTER TABLE [dbo].[TempParkingLot] DROP COLUMN [Status]
GO