# todoify

## Setup & Running
(two commands to get each running — make it foolproof)

## Architecture Decisions
- Why EF Core in-memory vs SQLite (you chose X because Y)
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
