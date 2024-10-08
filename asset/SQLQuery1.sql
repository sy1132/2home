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
    Postal_Code NVARCHAR(20),
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
    room_ID INT IDENTITY(1,1) PRIMARY KEY ,
	Motel_ID INT,
    ID_user INT ,
    Date_of_Issue DATE,
    Electricity_Meter DECIMAL(10, 2),
    Water_Meter DECIMAL(10, 2),
    Electricity_Usage DECIMAL(10, 2),
    Water_Usage DECIMAL(10, 2),
    Electricity_Bill DECIMAL(10, 2),
    Water_Bill DECIMAL(10, 2),
    Room_Status NVARCHAR(50),
    Room_Rent DECIMAL(10, 2),
    Additional_Charges DECIMAL(10, 2),
    Total_Amount_Due DECIMAL(15, 2),
	FOREIGN KEY (Motel_ID) REFERENCES Motel(Motel_ID),
	FOREIGN KEY (ID_user) REFERENCES [user](ID_user)

);

CREATE TABLE Video (
    video_ID int IDENTITY(1,1) PRIMARY KEY,
    ID_user INT,
    updatedAt DATETIME,
    createdAt DATETIME,
    Link NVARCHAR(255),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);

CREATE TABLE img (
    img_ID INT IDENTITY(1,1) PRIMARY KEY ,
    ID_user INT,
    updatedAt DATETIME,
    createdAt DATETIME,
    Link NVARCHAR(255),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);
CREATE TABLE Mail (
    Mail_ID INT IDENTITY(1,1) PRIMARY KEY,  
    Sender NVARCHAR(100) NOT NULL,
	ID_user INT,
    Recipient NVARCHAR(100) NOT NULL,      
    SendDate DATETIME NOT NULL DEFAULT GETDATE(), 
    Content NVARCHAR(MAX) NOT NULL,        
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user) 
);



SELECT * FROM Rooms
SELECT * FROM Motel
SELECT * FROM [user]
SELECT * FROM Mail
