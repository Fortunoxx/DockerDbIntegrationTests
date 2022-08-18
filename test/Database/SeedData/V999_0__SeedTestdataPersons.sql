set identity_insert USERS on;

INSERT INTO USERS (Id, FirstName, LastName) VALUES (1, 'Hocus', 'Pocus')
INSERT INTO USERS (Id, FirstName, LastName) VALUES (2, 'Horst', 'Schlämmer')
INSERT INTO USERS (Id, FirstName, LastName) VALUES (3, 'Clark', 'Kent')
INSERT INTO USERS (Id, FirstName, LastName) VALUES (4, 'Mary Jane', 'Green')

set identity_insert USERS off;
