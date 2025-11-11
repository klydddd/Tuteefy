CREATE TABLE UserTable (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    UserRole NVARCHAR(10) CHECK (UserRole IN ('Tutor', 'Tutee'))
);

CREATE TABLE TutorTable (
    TutorID INT IDENTITY PRIMARY KEY,
    FOREIGN KEY (TutorID) REFERENCES UserTable(UserID)
);

CREATE TABLE TuteeTable (
    TuteeID INT IDENTITY(1,1) PRIMARY KEY,
    TutorID INT NOT NULL,  -- FK to Users(UserID)
    FullName NVARCHAR(100) NOT NULL

    CONSTRAINT FK_Tutee_Tutor FOREIGN KEY (TutorID)
        REFERENCES UserTable(UserID)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

INSERT INTO UserTable(FullName, Email, PasswordHash, UserRole)
VALUES ('Gabriel', 'gab@mail.com', '1234', 'Tutee')
