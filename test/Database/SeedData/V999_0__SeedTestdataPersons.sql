set identity_insert USERS on;

-- INSERT INTO USERS (Id, FirstName, LastName) VALUES (1, 'Hocus', 'Pocus')
-- INSERT INTO USERS (Id, FirstName, LastName) VALUES (2, 'Horst', 'Schlämmer')
-- INSERT INTO USERS (Id, FirstName, LastName) VALUES (3, 'Clark', 'Kent')
-- INSERT INTO USERS (Id, FirstName, LastName) VALUES (4, 'Mary Jane', 'Green')

INSERT INTO USERS (Id, CreationDate, DisplayName, DownVotes, LastAccessDate, Reputation, UpVotes, Views)
    VALUES
        (1, CURRENT_TIMESTAMP, 'Hocus Pocus', 1, CURRENT_TIMESTAMP, 10, 10, 100),
        (2, CURRENT_TIMESTAMP, 'Clark Kent', 2, CURRENT_TIMESTAMP, 20, 20, 200),
        (3, CURRENT_TIMESTAMP, 'Mary Jane Green', 3, CURRENT_TIMESTAMP, 30, 30, 300)

set identity_insert USERS off;
