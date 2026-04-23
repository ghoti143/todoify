# todoify

# todo
* should we add roles to different users - read, write, etc.
* should we add docker to the stack
* do we need slnx file?
* test error handling in api
* unit tests and integration tests
* is the directory structure good? .NET likes TitleCase
* is the .net project name ok? Api?
* We keep a placeholder JWT key in appsettings.json; a real demo key is stored in appsettings.Development.json (git‑ignored) for easy local setup.
* enable light/dark mode

## Setup & Running
(two commands to get each running — make it foolproof)

## Architecture Decisions
- Why I chose SQLLite over EF Core because it will make the upgrade easier when moving to a more robust db engine
- used DateOnly instead of DateTime for the due date column. b/c this way we dont get caught in weird edge case around midnight
- Why Repository pattern even over a simple ORM
- Why React Query for state management

## Assumptions
- Auth is out of scope; in production would add JWT + ASP.NET Identity
- Single user; multi-tenancy would require user IDs on tasks
- In-memory DB resets on restart; production would use PostgreSQL/SQL Server

## What I'd Add for Production
- Authentication & authorization
- Pagination on the task list
- Real database with migrations
- Unit + integration tests (xUnit, bUnit for React)
- CI/CD pipeline (GitHub Actions)
- Rate limiting
- Soft deletes instead of hard deletes

## Trade-offs
- Chose in-memory EF over SQLite for simplicity — SQLite would be a one-line change in DI config
- No tests in this submission due to time, but architecture is designed to be testable (DI, interfaces, no static state)
