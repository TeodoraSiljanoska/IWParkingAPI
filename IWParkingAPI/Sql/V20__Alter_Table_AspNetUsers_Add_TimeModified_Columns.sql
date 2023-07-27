ALTER TABLE AspNetUsers
ADD IsDeactived BIT DEFAULT 'False' NOT NULL,  
	TimeCreated DATETIME NOT NULL,
	TimeModified DATETIME

