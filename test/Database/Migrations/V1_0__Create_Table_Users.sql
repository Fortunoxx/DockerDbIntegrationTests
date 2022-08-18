create table [dbo].[USERS]
(
    Id int identity not null primary key,
    FirstName sysname not null,
    LastName sysname not null
)