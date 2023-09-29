ALTER TABLE [Parking Lot]
ADD IsDeactivated BIT DEFAULT 'False' NOT NULL,  
	TimeCreated DATETIME NOT NULL,
	TimeModified DATETIME,
	IsFavourite BIT DEFAULT 'False' NOT NULL