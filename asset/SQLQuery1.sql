CREATE DATABASE secondhome;

USE secondhome;

CREATE TABLE [user] (
    ID_user INT IDENTITY(1,1) PRIMARY KEY ,
    username NVARCHAR(50) NOT NULL,
    password NVARCHAR(255) NOT NULL,
    fullname NVARCHAR(100),
    MotelID INT,
    PhoneNumber NVARCHAR(15),
    Email NVARCHAR(100),
    userrole NVARCHAR(50),
	gender NVARCHAR(10),
	blance INT DEFAULT 0
);

CREATE TABLE Motel (
    Motel_ID INT IDENTITY(1,1)  PRIMARY KEY ,
    ID_user INT,
    Name_motel NVARCHAR(100),
    location NVARCHAR(255),
    price DECIMAL(10, 2),
    is_available NVARCHAR(50),
	Details NVARCHAR(MAX),
	rooms int,
	CreatedDate date,
	Debt decimal(10, 2),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)

);
UPDATE Motel
SET debt = debt + DATEDIFF(MONTH, CreatedDate, GETDATE()) * price;



CREATE TABLE Rooms(
    room_ID INT IDENTITY(1,1) PRIMARY KEY,
    Motel_ID INT,
    ID_user INT,
    Date_of_Issue DATE,
    Electricity_Meter DECIMAL(10, 2),
    Water_Meter DECIMAL(10, 2),
    Electricity_Unit_Price DECIMAL(10, 2),
    Water_Unit_Price DECIMAL(10, 2),
    Previous_Water_Meter DECIMAL(10, 2),
    Previous_Electricity_Usage DECIMAL(10, 2),
    Electricity_Bill DECIMAL(10, 2),
    Water_Bill DECIMAL(10, 2),
    Room_Status NVARCHAR(50),
	Room_Rent DECIMAL(20, 2),
    Additional_Charges DECIMAL(10, 2),
    FOREIGN KEY (Motel_ID) REFERENCES Motel(Motel_ID),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);

CREATE TABLE Video (
    video_ID int IDENTITY(1,1) PRIMARY KEY,
    Motel_ID INT,
    updatedAt DATETIME,
    createdAt DATETIME,
    Link NVARCHAR(255),
    FOREIGN KEY (Motel_ID) REFERENCES [Motel](Motel_ID)
);

CREATE TABLE img (
    img_ID INT IDENTITY(1,1) PRIMARY KEY ,
    Motel_ID INT,
    updatedAt DATETIME,
    createdAt DATETIME,
    Link NVARCHAR(255),
    FOREIGN KEY (Motel_ID) REFERENCES [Motel](Motel_ID)
);
CREATE TABLE Mail (
    Mail_ID INT IDENTITY(1,1) PRIMARY KEY,  
    Sender INT,
	ID_user INT,
    Recipient INT,      
    SendDate DATETIME NOT NULL DEFAULT GETDATE(), 
    Content NVARCHAR(MAX) NOT NULL,        
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user) 
);

CREATE TABLE RoomBills (
    Bill_ID INT PRIMARY KEY IDENTITY(1,1), 
    Room_ID INT NOT NULL,                  
    Previous_Electricity_Usage DECIMAL(10, 2) NOT NULL, 
    Electricity_Meter DECIMAL(10, 2) NOT NULL, 
    Previous_Water_Meter DECIMAL(10, 2) NOT NULL, 
    Water_Meter DECIMAL(10, 2) NOT NULL, 
    Additional_Charges DECIMAL(10, 2) NOT NULL, 
    Price DECIMAL(10, 2) NOT NULL,
    Electricity_Bill DECIMAL(10, 2) NOT NULL, 
    Water_Bill DECIMAL(10, 2) NOT NULL, 
    Total_Amount_Due DECIMAL(10, 2) NOT NULL, 
    Room_Status VARCHAR(50), 
    Date_of_Issue DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (Room_ID) REFERENCES Rooms(room_ID) 
);

SELECT * FROM Motel
SELECT * FROM img
SELECT * FROM Video
SELECT * FROM [user]
SELECT * FROM Mail
SELECT * FROM RoomBills
SELECT * FROM Rooms
SELECT TOP 1 * 
FROM RoomBills 
ORDER BY Date_of_Issue DESC;
