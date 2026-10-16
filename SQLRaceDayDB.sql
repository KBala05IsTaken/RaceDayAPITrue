CREATE DATABASE RaceDayAssignmentST10474385;
USE RaceDayAssignmentST10474385;

-- =========================================================
-- USERS
-- Stores both Organisers and Participants.

CREATE TABLE Users (
    UserID INT,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'Participant',

    CONSTRAINT PK_Users
        PRIMARY KEY (UserID),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);


-- =========================================================
-- EVENTS
-- Stores races, walks and cycling events.

CREATE TABLE Events (
    EventID INT,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    EventType VARCHAR(30) NOT NULL DEFAULT 'Running',
    OrganiserID INT NOT NULL,

    CONSTRAINT PK_Events
        PRIMARY KEY (EventID),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID)
        ON UPDATE CASCADE
        ,

    CONSTRAINT CK_Events_Type
        CHECK (EventType IN ('Running', 'Walking', 'Cycling'))
);

-- =========================================================
-- CATEGORIES
-- Each event can have multiple categories.
-- Example: 5 KM, 10 KM and 21 KM.

CREATE TABLE Categories (
    CategoryID INT,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKM DECIMAL(6,2) NOT NULL,
    MaximumParticipants INT NOT NULL DEFAULT 100,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryID),

    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT UQ_Categories_Event_Name
        UNIQUE (EventID, CategoryName),

    CONSTRAINT CK_Categories_Distance
        CHECK (DistanceKM > 0),

    CONSTRAINT CK_Categories_MaxParticipants
        CHECK (MaximumParticipants > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0)
);

-- =========================================================
-- EVENT ROUTES
-- Stores route information for each event.

CREATE TABLE EventRoutes (
    RouteID INT,
    EventID INT NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    DistanceKM DECIMAL(6,2) NOT NULL,
    StartPoint VARCHAR(150) NOT NULL,
    FinishPoint VARCHAR(150) NOT NULL,
    RouteDescription VARCHAR(500),

    CONSTRAINT PK_EventRoutes
        PRIMARY KEY (RouteID),

    CONSTRAINT FK_EventRoutes_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT UQ_EventRoutes_Event
        UNIQUE (EventID),

    CONSTRAINT CK_EventRoutes_Distance
        CHECK (DistanceKM > 0)
);

-- =========================================================
-- ENROLLMENTS
-- Records participants entering event categories.

CREATE TABLE Enrollments (
    EnrollmentID INT,
    UserID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status VARCHAR(20) NOT NULL DEFAULT 'Confirmed',

    CONSTRAINT PK_Enrollments
        PRIMARY KEY (EnrollmentID),

    CONSTRAINT FK_Enrollments_User
        FOREIGN KEY (UserID)
        REFERENCES Users(UserID)
        ON DELETE CASCADE,

    CONSTRAINT FK_Enrollments_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT FK_Enrollments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),

    CONSTRAINT UQ_Enrollments_User_Event_Category
        UNIQUE (UserID, EventID, CategoryID),

    CONSTRAINT CK_Enrollments_Status
        CHECK (Status IN ('Confirmed', 'Pending', 'Cancelled'))
);

-- =========================================================
-- RESULTS
-- Stores participant performance.

CREATE TABLE Results (
    ResultID INT NOT NULL,
    EnrollmentID INT NOT NULL,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,
    Pace DECIMAL(5,2),
    ResultDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultID),

    CONSTRAINT FK_Results_Enrollment
        FOREIGN KEY (EnrollmentID)
        REFERENCES Enrollments(EnrollmentID)
        ON DELETE CASCADE,

    CONSTRAINT UQ_Results_Enrollment
        UNIQUE (EnrollmentID),

    CONSTRAINT CK_Results_Position
        CHECK (Position > 0),

    CONSTRAINT CK_Results_Pace
        CHECK (Pace IS NULL OR Pace > 0)
);


-- =========================================================
-- INSERT USERS
-- Minimum requirement:
-- 2 Organisers
-- 2 Participants

INSERT INTO Users
    (UserID, FirstName, LastName, Email, PasswordHash, Role)
VALUES
    (101, 'Thabo', 'Mokoena','thabo@raceday.co.za', 'RaceDay!2026Secure', 'Organiser'),
    (102, 'Lerato', 'Naidoo', 'lerato@raceday.co.za', 'RaceDay!2026Pass','Organiser'),
    (103, 'Sipho', 'Dlamini','sipho@example.co.za','RaceDay!2026Word','Participant'),
    (104, 'Amahle', 'Nkosi','amahle@example.co.za','RaceDay!2026Somthing','Participant');

-- =========================================================
-- INSERT EVENTS
-- Minimum requirement: 3 Events

INSERT INTO Events
    (EventID, EventName, EventDate, Location, Description, EventType, OrganiserID)
VALUES
    (201,'Cape Town Summer Run', '2026-11-15', 'Cape Town, Western Cape','Annual road running event along the Cape Town coastline.','Running',101),
    (202,'Johannesburg City Walk','2026-12-05','Johannesburg, Gauteng','Community walking event through the Johannesburg city centre.','Walking',102),
    (203,'Durban Coastal Cycle', '2027-01-10', 'Durban, KwaZulu-Natal', 'Coastal cycling event for recreational and experienced cyclists.','Cycling',101);

-- =========================================================
-- INSERT CATEGORIES
-- 3 categories for EACH event = 9 categories

-- Event 1: Cape Town Summer Run
INSERT INTO Categories
    (CategoryID, EventID, CategoryName, DistanceKM, MaximumParticipants, EntryFee)
VALUES
    (301, 201, '5 KM Fun Run', 5.00, 500, 80.00),
    (302, 201, '10 KM Road Race', 10.00, 750, 120.00),
    (303, 201, '21 KM Half Marathon', 21.10, 1000, 180.00);

-- Event 2: Johannesburg City Walk
INSERT INTO Categories
    (CategoryID, EventID, CategoryName, DistanceKM, MaximumParticipants, EntryFee)
VALUES
    (304,202, '5 KM Community Walk', 5.00, 300, 50.00),
    (305,202, '10 KM City Walk', 10.00, 400, 80.00),
    (306,202, '15 KM Challenge Walk', 15.00, 250, 100.00);

-- Event 3: Durban Coastal Cycle
INSERT INTO Categories
    (CategoryID, EventID, CategoryName, DistanceKM, MaximumParticipants, EntryFee)
VALUES
    (307,203, '20 KM Social Ride', 20.00, 200, 100.00),
    (308,203, '50 KM Coastal Ride', 50.00, 400, 180.00),
    (309,203, '100 KM Endurance Ride', 100.00, 500, 250.00);


-- =========================================================
-- INSERT EVENT ROUTES

INSERT INTO EventRoutes
    (RouteID, EventID, RouteName, DistanceKM, StartPoint, FinishPoint, RouteDescription)
VALUES
    (401,201,'Cape Town Coastal Route', 10.00, 'Green Point', 'Sea Point',
        'Scenic route following the Atlantic coastline.'),
    (402,202, 'Johannesburg City Route', 10.00, 'Nelson Mandela Square', 'Mary Fitzgerald Square',
         'Urban walking route through central Johannesburg.'),
    (403,203, 'Durban Coastal Route', 50.00, 'uShaka Marine World', 'Umhlanga',
        'Coastal cycling route following the Durban beachfront.');

-- =========================================================
-- INSERT ENROLLMENTS
-- Sample participant entries

INSERT INTO Enrollments
    (EnrollmentID,UserID, EventID, CategoryID, Status)
VALUES
    (501, 103, 201, 301, 'Confirmed'), -- Sipho: Cape Town 5 KM
    (502, 103, 201, 302, 'Confirmed'), -- Sipho: Cape Town 10 KM
    (503, 104, 202, 304, 'Confirmed'), -- Amahle: Johannesburg 5 KM
    (504, 104, 203, 307, 'Confirmed'), -- Amahle: Durban 20 KM
    (505, 103, 203, 308, 'Confirmed'), -- Sipho: Durban 50 KM
    (506, 104, 201, 303, 'Pending');   -- Amahle: Cape Town Half Marathon

-- =========================================================
-- INSERT RESULTS
-- Sample personal performance results

INSERT INTO Results
    (ResultID, EnrollmentID, FinishTime, Position, Pace)
VALUES
    (601, 501, '00:28:35', 15, 5.72),
    (602, 502, '00:56:20', 32, 5.63),
    (603, 503, '01:02:15', 20, 6.23),
    (604, 504, '01:05:40', 12, 3.28),
    (605, 505, '02:15:30', 45, 2.71);

-- =========================================================
-- TEST / DISPLAY THE DATA

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM EventRoutes;
SELECT * FROM Enrollments;
SELECT * FROM Results;