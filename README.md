## Meeting Scheduler API

A RESTful API for scheduling meetings for multiple users without conflicts. Implements an algorithm that finds the earliest available time slot for a group of users given existing schedules.
Git: https://github.com/Arsen-Grigor/MeetingScheduler

### Prerequisites

- .NET 9.0.
- Visual Studio or JetBrains Rider.
- xUnit.

### Running tests
1. Navigate to project directory `.../MeetingScheduler` and build project first:
    `dotnet build`;
2. Run the application:
    `dotnet run`;
1. Navigate to test project directory `.../MeetingScheduler.Tests` and run all test:
    `dotnet test`.

### API Endpoints

| Method | Description |
|-|-|
`GET /meetings/users` | gets all users
`GET /meetings/users/{id}`| gets user by ID
`GET /meetings/meetings`| gets all meetings
`GET /meetings/meetings/{id}`| gets meeting by ID
`GET /meetings/users/{id}/meetings` | gets all meetings for a specific user
`POST /meetings/users`| creates a new user
`POST /meetings/meetings` | creates a new meeting (auto-scheduled)

### Examples of Requests

`POST /meetings/users`
`{`
    `"id": 1,`
    `"name": "Corwin"`
`}`
`POST /meetings/meetings`
`{`
    `"participantIds": [1, 2, 3],`
   ` "durationMinutes": 60,`
    `"earliestStart": "2025-06-20T09:00:00Z",`
    `"latestEnd": "2025-06-20T17:00:00Z"`
`}`

### Known Limitations
- Not suitable for production;
- In-memory storage only;
- Time treated as UTC;
- No  limit validation;
- Exception handling is lacking.