CREATE DATABASE secondhome;

USE secondhome;

CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(255) NOT NULL,
    password NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(255),
    email NVARCHAR(255) NOT NULL UNIQUE,
    phone_number NVARCHAR(20),
    user_role NVARCHAR(255) NOT NULL,
);

CREATE TABLE Motel (
    motel_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
	motel_name NVARCHAR(255),
	number int,
	location NVARCHAR(255),
	price decimal(18, 0),
    is_available NVARCHAR(25),

);

CREATE TABLE Rooms (
    room_id INT IDENTITY(1,1) PRIMARY KEY,
    room_type NVARCHAR(25) NOT NULL,
    rent INT NOT NULL,
	is_rent_pay int,
    is_available NVARCHAR(25),
	motel_id int,
	FOREIGN KEY (motel_id) REFERENCES Motel(motel_id) ON DELETE CASCADE,
);
INSERT INTO Users (username, password, full_name, email, phone_number, user_role)
VALUES 
('john_doe', 'password123', 'John Doe', 'john.doe@example.com', '123-456-7890', 'admin'),
('jane_smith', 'password456', 'Jane Smith', 'jane.smith@example.com', '098-765-4321', 'user');

INSERT INTO Motel (user_id, motel_name, number, location, price, is_available)
VALUES 
(1, 'Sunny Hotel', 101, '123 Main St', 100.00, 'Yes'),
(2, 'Cozy Inn', 102, '456 Elm St', 75.00, 'No');
INSERT INTO Rooms (room_type, rent, is_rent_pay, is_available, motel_id)
VALUES 
('Single', 50, 0, 'Yes', 1),
('Double', 80, 1, 'No', 1),
('Suite', 120, 0, 'Yes', 2);

