CREATE DATABASE volunteerapp
GO

USE volunteerapp

CREATE TABLE ProjectType (
	Id INT IDENTITY PRIMARY KEY,
	Name NVARCHAR(70)  NOT NULL,
	Description NVARCHAR(MAX) NULL
)

CREATE TABLE Skill (
	Id INT IDENTITY PRIMARY KEY,
	Name NVARCHAR(70)  NOT NULL,
	Description NVARCHAR(MAX) NULL
)

CREATE TABLE AppUser (
  Id INT IDENTITY(1,1) NOT NULL,
  Username NVARCHAR(50) NOT NULL,
  PswdHash NVARCHAR(256) NOT NULL,
  PswdSalt NVARCHAR(256) NOT NULL,
  FirstName NVARCHAR(256) NOT NULL,
  LastName NVARCHAR(256) NOT NULL,
  Email NVARCHAR(256) NOT NULL,
  IsAdmin BIT NOT NULL,
  CONSTRAINT PK_User PRIMARY KEY CLUSTERED (
    Id ASC
  )
)

CREATE TABLE Project (
	Id INT IDENTITY PRIMARY KEY,
	Title NVARCHAR(100)  NOT NULL,
	Description NVARCHAR(MAX) NULL,
	CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
	ProjectTypeId INT,
	CONSTRAINT FK_ProjectType FOREIGN KEY (ProjectTypeId) REFERENCES ProjectType(id)
)

CREATE TABLE UserProject (
    AppuserId INT NOT NULL,
    ProjectId INT NOT NULL,
    PRIMARY KEY (AppuserId, ProjectId),
    CONSTRAINT FK_User_UserProject FOREIGN KEY (AppuserId) REFERENCES AppUser(id),
    CONSTRAINT FK_Project_UserProject FOREIGN KEY (ProjectId) REFERENCES Project(id)
)

CREATE TABLE ProjectSkill (
    ProjectId INT NOT NULL,
    SkillId INT NOT NULL,
    PRIMARY KEY (ProjectId, SkillId),
    CONSTRAINT FK_Project_ProjectSkill FOREIGN KEY (ProjectId) REFERENCES Project(id),
    CONSTRAINT FK_Skill_ProjectSkill FOREIGN KEY (SkillId) REFERENCES Skill(id)
)

CREATE TABLE LogEntry (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Level NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
);

/* RESET DB */
USE master
GO
DROP DATABASE volunteerapp

/* DATA INSERTION */
USE volunteerapp
GO
INSERT INTO Skill(Name, Description) VALUES ('Dobra komunikacija','Volonter je sposoban ispravno komunicirati')
INSERT INTO Skill(Name, Description) VALUES ('Odlučan','Spreman dati ispravne odluke u kratkom vremenskom periodu')
INSERT INTO Skill(Name, Description) VALUES ('Prosječno znanje informatike','Volonter ima prosječno znanje informatičkih sustava')
SELECT * FROM Skill

INSERT INTO ProjectType(Name, Description) VALUES ('IT', 'Information Technology')
INSERT INTO ProjectType(Name, Description) VALUES ('Agriculture', 'Agriculture')
INSERT INTO ProjectType(Name, Description) VALUES ('Tourism', 'Tourists')
SELECT * FROM ProjectType

INSERT INTO Project(Title, Description, ProjectTypeId) VALUES ('Help make an AI chatbot','Help us make an AI chatbot',1)
INSERT INTO Project(Title, Description, ProjectTypeId) VALUES ('Provide IT help to customers','Provide IT help to customers',1)
SELECT * FROM Project

INSERT INTO ProjectSkill(ProjectId, SkillId) VALUES (1, 1)

/* CHECK ALL DATA */
SELECT * FROM Skill
SELECT * FROM ProjectType
SELECT * FROM Project
INNER JOIN ProjectType ON ProjectType.Id = Project.ProjectTypeId

SELECT * FROM LogEntry