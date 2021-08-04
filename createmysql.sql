CREATE TABLE School (
    Name VARCHAR(255) NOT NULL PRIMARY KEY
);
CREATE TABLE Subject(
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL
);
CREATE TABLE Teacher(
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(255) NOT NULL,
    SchoolName VARCHAR(255) NOT NULL,
	FOREIGN KEY(SchoolName) REFERENCES School(Name)
);
CREATE TABLE ProjectTheme(
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Enddate DATETIME,
    LastEdited DATETIME,
    TeacherID INT NOT NULL,
    SubjectID INT NOT NULL,
	FOREIGN KEY(TeacherID) REFERENCES Teacher(ID),
	FOREIGN KEY(SubjectID) REFERENCES Subject(ID)
);
CREATE TABLE CoTeacher(
    ProjectThemeID INT NOT NULL,
    TeacherID INT NOT NULL,
	FOREIGN KEY(ProjectThemeID) REFERENCES ProjectTheme(ID),
	FOREIGN KEY(TeacherID) REFERENCES Teacher(ID),
    PRIMARY KEY(ProjectThemeID, TeacherID)
);
CREATE TABLE Project(
    ID INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Completed BIT,
    LastEdited DATETIME,
    DateToDeletion DATETIME,
    ProjectThemeID INT NOT NULL FOREIGN KEY REFERENCES ProjectTheme(ID)
)
CREATE TABLE ProjectMember(
    ProjectID INT NOT NULL FOREIGN KEY REFERENCES Project(ID),
    Username NVARCHAR(255) NOT NULL,
    PRIMARY KEY(ProjectID, Username)
)
CREATE TABLE Document(
    ID INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    ProjectID INT NOT NULL FOREIGN KEY REFERENCES Project(ID),
    Title NVARCHAR(255),
    Data TEXT
)
CREATE TABLE RemoteFile(
    ID INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    ProjectID INT NOT NULL FOREIGN KEY REFERENCES Project(ID),
    Title NVARCHAR(255),
    Type NVARCHAR(255)
)
CREATE TABLE DocumentPart(
    ID INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    Title NVARCHAR(255)
)
CREATE TABLE CompletedPart(
    DocumentID INT NOT NULL FOREIGN KEY REFERENCES Document(ID),
    PartID INT NOT NULL FOREIGN KEY REFERENCES DocumentPart(ID),
	PRIMARY KEY (DocumentID, PartID)
)

-- Project
DELIMITER $$
CREATE PROCEDURE SPDeleteProject (id int, username nvarchar(255))
BEGIN
	IF EXISTS(select * FROM ProjectMember WHERE ProjectID = id AND Username = @username)
	THEN
		DELETE FROM CompletedPart WHERE DocumentID IN (SELECT ID FROM Document WHERE ProjectID = id);
		DELETE FROM Document WHERE ProjectID = id;
		DELETE FROM RemoteFile WHERE ProjectID = id;
		DELETE FROM ProjectMember WHERE ProjectID = id;
		DELETE FROM Project WHERE id = id;
	END IF;
	select id as id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPGetProject (id int, username nvarchar(255))
BEGIN
	SELECT Project.ID,Project.Name,Completed,Project.LastEdited,Enddate,DateToDeletion FROM Project 
	JOIN ProjectTheme ON ProjectThemeID = ProjectTheme.ID	
	WHERE Project.ID = id and Project.ID in (SELECT ProjectID FROM ProjectMember WHERE Username = username);
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPInsertProject (username nvarchar(255), name nvarchar(255), completed BIT, ProjectThemeID INT)
BEGIN
	INSERT INTO Project (Name,Completed,LastEdited,ProjectThemeID) VALUES (name, completed, (SELECT NOW()), ProjectThemeID);
	SET id = (SELECT @@IDENTITY);
	INSERT INTO ProjectMember (ProjectID, Username) VALUES (id, @username);
	SELECT id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPUpdateProject (id int, name nvarchar(255), completed BIT)
BEGIN
	UPDATE Project SET Name = name, completed = completed WHERE ID = id;
    SELECT id;
END$$
DELIMITER ;

-- Document
DELIMITER $$
CREATE PROCEDURE SPDeleteDocument (id INT, pid INT)
BEGIN
	IF EXISTS(select * FROM Document WHERE ID = id AND ProjectID = pid)
	THEN
		DELETE FROM CompletedPart WHERE DocumentID = id;
		DELETE FROM Document WHERE id = id;
        SELECT id;
	END IF;
END$$
DELIMITER ;