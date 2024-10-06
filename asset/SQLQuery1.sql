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
	gender NVARCHAR(10)
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
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);



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
INSERT INTO [user] (username, password, fullname, MotelID, PhoneNumber, Email, userrole)
VALUES 
( N'john_doe', N'password123', N'John Doe', NULL, N'1234567890', N'john@example.com', N'Admin'),
( N'jane_smith', N'password123', N'Jane Smith', NULL, N'0987654321', N'jane@example.com', N'User'),
(N'admin_user', N'password123', N'Admin User', NULL, N'1122334455', N'admin@example.com', N'Admin');
	INSERT INTO img (ID_user, updatedAt, createdAt, Link)
VALUES 
(1,'2024-09-08 12:00:00', '2024-09-08 12:00:00', N'/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg'),
(2, '2024-09-08 13:00:00', '2024-09-08 13:00:00', N'/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg'),
(3,'2024-09-08 14:00:00', '2024-09-08 14:00:00', N'/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg');
INSERT INTO Video ( ID_user, updatedAt, createdAt, Link)
VALUES 
(1,'2024-09-08 15:00:00', '2024-09-08 15:00:00', N'/asset/videos/video.mp4'),
(2, '2024-09-08 16:00:00', '2024-09-08 16:00:00', N'/asset/videos/video.mp4')
ALTER TABLE Motel
ADD Details NVARCHAR(MAX);
SELECT
    img.Link AS ImgLink,
    motel.Name_motel AS MotelName,
    motel.location AS Location,
    motel.price AS Price,
    motel.is_available AS is_available,
    motel.ID_user AS ID_user
FROM
    img AS img
JOIN
    Motel AS motel
ON
    img.ID_user = motel.ID_user;
	SELECT * FROM Motel
	INSERT INTO Motel (ID_user, Name_motel, Postal_Code, location, price, is_available)
VALUES
(1, N'Green Field Motel', N'12345', N'123 Green Street', 1000.00, N'Còn tr?ng'),
(2, N'Ocean View Motel', N'54321', N'456 Ocean Road', 1500.00, N'Còn tr?ng'),
(3, N'Mountain Lodge Motel', N'67890', N'789 Mountain Drive', 1200.00, N'Còn tr?ng');
SELECT * FROM [user]