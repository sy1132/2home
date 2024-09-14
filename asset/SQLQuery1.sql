CREATE DATABASE secondhome;

USE secondhome;

CREATE TABLE [user] (
    ID_user INT PRIMARY KEY ,
    username NVARCHAR(50) NOT NULL,
    password NVARCHAR(255) NOT NULL,
    fullname NVARCHAR(100),
    MotelID INT,
    PhoneNumber NVARCHAR(15),
    Email NVARCHAR(100),
    userrole NVARCHAR(50)
);

CREATE TABLE Motel (
    Motel_ID INT PRIMARY KEY ,
    ID_user INT,
    Name_motel NVARCHAR(100),
    Postal_Code NVARCHAR(20),
    location NVARCHAR(255),
    price DECIMAL(10, 2),
    is_available NVARCHAR(50),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);

CREATE TABLE rooms (
    room_ID INT PRIMARY KEY ,
    Motel_ID INT,
    Electricity_Meter DECIMAL(10, 2),
    Water_Meter DECIMAL(10, 2),
    Electricity_Usage DECIMAL(10, 2),
    Water_Usage DECIMAL(10, 2),
    Room_Status NVARCHAR(50),
    Room_Rent DECIMAL(10, 2),
    FOREIGN KEY (Motel_ID) REFERENCES Motel(Motel_ID)
);

CREATE TABLE Room_Invoice (
    room_ID INT,
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
    FOREIGN KEY (room_ID) REFERENCES rooms(room_ID)
);

CREATE TABLE Video (
    video_ID INT PRIMARY KEY,
    ID_user INT,
    updatedAt DATETIME,
    createdAt DATETIME,
    Link NVARCHAR(255),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);

CREATE TABLE img (
    img_ID INT PRIMARY KEY ,
    ID_user INT,
    updatedAt DATETIME,
    createdAt DATETIME,
    Link NVARCHAR(255),
    FOREIGN KEY (ID_user) REFERENCES [user](ID_user)
);
INSERT INTO [user] (ID_user, username, password, fullname, MotelID, PhoneNumber, Email, userrole)
VALUES 
(101, N'john_doe', N'password123', N'John Doe', NULL, N'1234567890', N'john@example.com', N'Admin'),
(102, N'jane_smith', N'password123', N'Jane Smith', NULL, N'0987654321', N'jane@example.com', N'User'),
(103, N'admin_user', N'password123', N'Admin User', NULL, N'1122334455', N'admin@example.com', N'Admin');

	INSERT INTO img (img_ID, ID_user, updatedAt, createdAt, Link)
	VALUES 
	(1, 101, '2024-09-08 12:00:00', '2024-09-08 12:00:00', N'~/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg'),
	(2, 102, '2024-09-08 13:00:00', '2024-09-08 13:00:00', N'~/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg'),
	(3, 103, '2024-09-08 14:00:00', '2024-09-08 14:00:00', N'~/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg');
	INSERT INTO img (img_ID, ID_user, updatedAt, createdAt, Link)
VALUES 
(4, 101, '2024-09-08 12:00:00', '2024-09-08 12:00:00', N'/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg'),
(5, 102, '2024-09-08 13:00:00', '2024-09-08 13:00:00', N'/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg'),
(6, 103, '2024-09-08 14:00:00', '2024-09-08 14:00:00', N'/asset/images/pngtree-outline-user-icon-png-image_1727916.jpg');
INSERT INTO Video (video_ID, ID_user, updatedAt, createdAt, Link)
VALUES 
(1, 101, '2024-09-08 15:00:00', '2024-09-08 15:00:00', N'/asset/videos/video.mp4'),
(2, 102, '2024-09-08 16:00:00', '2024-09-08 16:00:00', N'/asset/videos/video.mp4')
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
	SELECT * FROM Motel WHERE ID_user = 103;