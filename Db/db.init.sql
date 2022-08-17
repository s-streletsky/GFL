BEGIN TRANSACTION;

CREATE TABLE "Folders" (

    "Id" INTEGER NOT NULL CONSTRAINT "PK_Folders" PRIMARY KEY AUTOINCREMENT,

    "ParentId" INTEGER NULL,

    "Title" TEXT NOT NULL,

    CONSTRAINT "FK_Folders_Folders_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Folders" ("Id")

);

INSERT INTO Folders (Id, ParentId, Title) VALUES (1, NULL, 'Virtual Root');
INSERT INTO Folders (Id, ParentId, Title) VALUES (2, 1, 'Creating Digital Images');
INSERT INTO Folders (Id, ParentId, Title) VALUES (3, 2, 'Resources');
INSERT INTO Folders (Id, ParentId, Title) VALUES (4, 2, 'Evidence');
INSERT INTO Folders (Id, ParentId, Title) VALUES (5, 2, 'Graphic Products');
INSERT INTO Folders (Id, ParentId, Title) VALUES (6, 3, 'Primary Sources');
INSERT INTO Folders (Id, ParentId, Title) VALUES (7, 3, 'Secondary Sources');
INSERT INTO Folders (Id, ParentId, Title) VALUES (8, 5, 'Process');
INSERT INTO Folders (Id, ParentId, Title) VALUES (9, 5, 'Final Product');

COMMIT TRANSACTION;
