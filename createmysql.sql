CREATE USER IF NOT EXISTS 
	'SoScienceExecuter'@'localhost'
	IDENTIFIED BY 'k6UwAf4K*puBTEb^';

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
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Completed BIT,
    LastEdited DATETIME,
    DateToDeletion DATETIME,
    ProjectThemeID INT NOT NULL,
	FOREIGN KEY(ProjectThemeID) REFERENCES ProjectTheme(ID)
);
CREATE TABLE ProjectMember(
    ProjectID INT NOT NULL,
    Username VARCHAR(255) NOT NULL,
    PRIMARY KEY(ProjectID, Username),
	FOREIGN KEY(ProjectID) REFERENCES Project(ID)
);
CREATE TABLE Document(
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ProjectID INT NOT NULL,
    Title VARCHAR(255),
    Data TEXT,
	FOREIGN KEY(ProjectID) REFERENCES Project(ID)
);
CREATE TABLE RemoteFile(
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ProjectID INT NOT NULL,
    Title VARCHAR(255),
    Type VARCHAR(255),
	FOREIGN KEY(ProjectID) REFERENCES Project(ID)
);
CREATE TABLE DocumentPart(
    ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255)
);
CREATE TABLE CompletedPart(
    DocumentID INT NOT NULL,
    PartID INT NOT NULL,
	PRIMARY KEY (DocumentID, PartID),
	FOREIGN KEY(DocumentID) REFERENCES Document(ID),
	FOREIGN KEY(PartID) REFERENCES DocumentPart(ID)
);
 
-- Teacher


DELIMITER $$
CREATE PROCEDURE SPDeleteTeacher (_id int, _username VARCHAR(255))
BEGIN
	IF EXISTS(SELECT * FROM teacher WHERE Username = _username AND ID = id)
	THEN
		DELETE FROM CoTeacher WHERE TeacherID = _id;
		IF (SELECT COUNT(*) FROM ProjectTheme WHERE TeacherID = _id) > 0
        THEN
            UPDATE ProjectTheme PT
				INNER JOIN CoTeacher CT ON CT.ProjectThemeID = PT.ID AND CT.TeacherID != _id
				SET PT.TeacherID = CT.TeacherID
                WHERE PT.TeacherID = _id;
        END IF;
		DELETE FROM CoTeacher WHERE TeacherID = _id;
		DELETE FROM teacher WHERE ID = _id;
	END IF;
	select _id as id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPCheckTeacher (_username VARCHAR(255))
BEGIN
	SELECT Count(username) FROM Teacher WHERE Username = _username;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPInsertTeacher (_username VARCHAR(255), _schoolname VARCHAR(255))
BEGIN
	INSERT INTO Teacher (Username, SchoolName) VALUES (_username, _schoolName);
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPUpdateTeacher (_id int, _username VARCHAR(255), _schoolname VARCHAR(255))
BEGIN
	UPDATE teacher SET SchoolName = _schoolname, Username = _username WHERE ID = _id;
    SELECT _id;
END$$
DELIMITER ;
 
-- Project
DELIMITER $$
CREATE PROCEDURE SPDeleteProject (id int, username VARCHAR(255))
BEGIN
	IF EXISTS(select * FROM ProjectMember WHERE ProjectID = id AND Username = username)
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
CREATE PROCEDURE SPGetProject (id int, username VARCHAR(255))
BEGIN
	SELECT Project.ID,Project.Name,Completed,Project.LastEdited,Enddate,DateToDeletion FROM Project 
	JOIN ProjectTheme ON ProjectThemeID = ProjectTheme.ID	
	WHERE Project.ID = id and Project.ID in (SELECT ProjectID FROM ProjectMember WHERE Username = username);
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPInsertProject (username VARCHAR(255), name VARCHAR(255), completed BIT, ProjectThemeID INT)
BEGIN
	INSERT INTO Project (Name,Completed,LastEdited,ProjectThemeID) VALUES (name, completed, (SELECT NOW()), ProjectThemeID);
	SET @id = (SELECT @@IDENTITY);
	INSERT INTO ProjectMember (ProjectID, Username) VALUES (@id, username);
	SELECT id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPUpdateProject (id int, name VARCHAR(255), completed BIT)
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

DELIMITER $$
CREATE PROCEDURE SPGetDocument (id INT)
BEGIN
	SELECT ProjectID, ID,Title,Data FROM Document WHERE ID = id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPInsertDocument (id INT, title VARCHAR(255), data text)
BEGIN
	INSERT INTO Document (ProjectID, Title, Data) VALUES (id, title, data);
	SELECT @@IDENTITY as ID;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPUpdateDocument (id INT, title VARCHAR(255), data text)
BEGIN
	IF (CHAR_LENGTH(data) > 1)
	THEN
		UPDATE Document SET Title = title, Data = data WHERE ID = id;
        SELECT id;
	ELSE
		UPDATE Document SET Title = title WHERE ID = id;
        SELECT id;
	END IF;
END$$
DELIMITER ;

-- RFile
DELIMITER $$
CREATE PROCEDURE SPDeleteRFile (id INT, pid INT)
BEGIN
	DELETE FROM RemoteFile WHERE ID = id AND ProjectID = pid;
    SELECT id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPGetRFile (id INT)
BEGIN
	SELECT ID, Title, ProjectID, Path, Type FROM RemoteFile WHERE ID = id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPInsertRFile (title VARCHAR(255), pid INT, path VARCHAR(255), type VARCHAR(255))
BEGIN
	INSERT INTO RemoteFile (Title, ProjectID, Path, Type) VALUES (title, pid, path, Type);
	SELECT @@IDENTITY as ID;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPUpdateRFile (id INT, title VARCHAR(255))
BEGIN
	Update RemoteFile Set Title = @title WHERE ID = id;
	SELECT id;
END$$
DELIMITER ;

-- Completed parts
DELIMITER $$
CREATE PROCEDURE SPClearCompleted (did INT)
BEGIN
	DELETE FROM CompletedPart WHERE DocumentID = did;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPInsertCompleted (did INT, title VARCHAR(255))
BEGIN
	INSERT INTO CompletedPart (DocumentID, PartID) VALUES (did, (SELECT id FROM DocumentPart WHERE Title = title));
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPGetCompletedParts (id INT)
BEGIN
	SELECT Title FROM DocumentPart WHERE ID in (SELECT PartID FROM CompletedPart WHERE DocumentID = id );
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPGetMissingPart (id INT)
BEGIN
	SELECT title FROM DocumentPart WHERE ID NOT IN (SELECT PartID FROM CompletedPart WHERE DocumentID = id);
END$$
DELIMITER ;

-- Lists
DELIMITER $$
CREATE PROCEDURE SPGetDocumentsSimple (id INT)
BEGIN
	SELECT ProjectID, ID,Title, (SELECT COUNT(PartID) FROM CompletedPart WHERE DocumentID = Document.ID) as completed FROM Document WHERE ProjectID = id;
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPGetProjects (username VARCHAR(255))
BEGIN
	SELECT ID, name, completed, lastEdited, EndDate FROM Project WHERE ID IN (SELECT ProjectID FROM ProjectMember WHERE username = username);
END$$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE SPGetRFiles (pid INT)
BEGIN
	SELECT ID, Title, ProjectID, Type FROM RemoteFile WHERE ProjectID = pid;
END$$
DELIMITER ;

-- permission 
GRANT EXECUTE ON soscience.* TO 'SoScienceExecuter'@'localhost';


-- Populate
INSERT INTO DocumentPart (Title) VALUES ('Forside');
INSERT INTO DocumentPart (Title) VALUES ('Formaal');
INSERT INTO DocumentPart (Title) VALUES ('Materiale');
INSERT INTO DocumentPart (Title) VALUES ('Forsoegsopstilling');
INSERT INTO DocumentPart (Title) VALUES ('Sikkerhed');
INSERT INTO DocumentPart (Title) VALUES ('Teori');
INSERT INTO DocumentPart (Title) VALUES ('Resultater');
INSERT INTO DocumentPart (Title) VALUES ('Diskussion');
INSERT INTO DocumentPart (Title) VALUES ('Fejlkilder');
INSERT INTO DocumentPart (Title) VALUES ('Konklusion');
INSERT INTO DocumentPart (Title) VALUES ('Kilder');

INSERT INTO School (Name) Values ('ZBC Slagelse');
