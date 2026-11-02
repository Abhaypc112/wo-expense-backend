# WoExpense Backend

ASP.NET Core Web API (.NET 10) with PostgreSQL, EF Core and JWT authentication.

## Quick start

Run everything from this folder (`backend/`).

```bash
docker compose up -d                                                        # 1. database
dotnet ef database update -p src/WoExpense.Infrastructure -s src/WoExpense.Api   # 2. tables
dotnet run --project src/WoExpense.Api --launch-profile http                # 3. API
```

Requirements: .NET SDK 10, Docker Desktop, and `dotnet tool install -g dotnet-ef`.

| URL | Purpose |
|---|---|
| http://localhost:5107 | API (http profile) |
| https://localhost:7223 | API (https profile) |
| http://localhost:5107/swagger | Swagger UI (Development only) |

## Common commands

Run from `backend/` unless noted.

### API (.NET)

| Task | Command |
|---|---|
| Restore packages | `dotnet restore` |
| Build everything | `dotnet build` |
| Run (http, port 5107) | `dotnet run --project src/WoExpense.Api --launch-profile http` |
| Run (https, port 7223) | `dotnet run --project src/WoExpense.Api --launch-profile https` |
| Run with auto-reload on file changes | `dotnet watch --project src/WoExpense.Api run --launch-profile http` |
| Release build | `dotnet build -c Release` |
| Publish for deployment | `dotnet publish src/WoExpense.Api -c Release -o ./publish` |
| Clean build output | `dotnet clean` |
| Add a NuGet package | `dotnet add src/WoExpense.Api package <PackageName>` |
| Trust the https dev certificate (once) | `dotnet dev-certs https --trust` |
| Stop the API | `Ctrl+C` in its terminal |
| Stop it if it's stuck (Windows) | `taskkill /F /IM WoExpense.Api.exe` |

### Database migrations (EF Core)

| Task | Command |
|---|---|
| Create a migration | `dotnet ef migrations add <Name> -p src/WoExpense.Infrastructure -s src/WoExpense.Api` |
| Apply all migrations | `dotnet ef database update -p src/WoExpense.Infrastructure -s src/WoExpense.Api` |
| Roll back to a migration | `dotnet ef database update <PreviousName> -p src/WoExpense.Infrastructure -s src/WoExpense.Api` |
| Remove the last migration (not yet applied) | `dotnet ef migrations remove -p src/WoExpense.Infrastructure -s src/WoExpense.Api` |
| List migrations | `dotnet ef migrations list -p src/WoExpense.Infrastructure -s src/WoExpense.Api` |
| Generate SQL script | `dotnet ef migrations script -p src/WoExpense.Infrastructure -s src/WoExpense.Api -o migrate.sql` |
| Drop the database | `dotnet ef database drop -p src/WoExpense.Infrastructure -s src/WoExpense.Api` |

### Docker / PostgreSQL

| Task | Command |
|---|---|
| Start the database | `docker compose up -d` |
| Stop (keep data) | `docker compose stop` |
| Start again after a stop | `docker compose start` |
| Restart | `docker compose restart` |
| Remove container (keep data) | `docker compose down` |
| Remove container and data | `docker compose down -v` |
| Check status | `docker compose ps` |
| View logs | `docker compose logs -f postgres` |
| Open a SQL shell | `docker exec -it wo-pg psql -U postgres -d wo_expense_db` |
| Run one SQL query | `docker exec wo-pg psql -U postgres -d wo_expense_db -c "select count(*) from \"Users\""` |
| Backup | `docker exec wo-pg pg_dump -U postgres wo_expense_db > backup.sql` |
| Restore | `docker exec -i wo-pg psql -U postgres -d wo_expense_db < backup.sql` |

Useful `psql` commands inside the shell: `\dt` list tables, `\d "Users"` describe a table, `\q` quit.

### Try the API from the command line

```bash
# Register
curl -X POST http://localhost:5107/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Test User","email":"test@example.com","password":"Passw0rd!"}'

# Login
curl -X POST http://localhost:5107/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Passw0rd!"}'

# Current user (paste the accessToken from the response)
curl http://localhost:5107/api/auth/me -H "Authorization: Bearer <accessToken>"
```

### Typical daily workflow

1. Start Docker Desktop.
2. `docker compose up -d`
3. `dotnet watch --project src/WoExpense.Api run --launch-profile http`
4. Open http://localhost:5107/swagger
5. When finished: `Ctrl+C`, then `docker compose stop`.

## Structure

```
backend/
├── WoExpense.slnx
├── docker-compose.yml
└── src/
    ├── WoExpense.Domain/            Entities (BaseEntity, User, RefreshToken)
    ├── WoExpense.Application/       DTOs and interfaces (IAuthService, ICurrentUser)
    ├── WoExpense.Infrastructure/    AppDbContext, Migrations, Services (AuthService)
    └── WoExpense.Api/               Controllers, Program.cs, appsettings, CurrentUser
```

Dependencies: `Api → Infrastructure → Application → Domain` (Api also references Application). Domain depends on nothing.

| Project | Holds |
|---|---|
| Domain | The data model only. |
| Application | Contracts: service interfaces and request/response DTOs. |
| Infrastructure | Implementations: EF Core, migrations, business logic. |
| Api | Controllers, startup, configuration. |

## Configuration

`src/WoExpense.Api/appsettings.json`:

| Key | Value |
|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;Port=5434;Database=wo_expense_db;Username=postgres;Password=wo_expense_db_011` |
| `Jwt:Key` | Signing key, at least 32 bytes |
| `Jwt:Issuer` / `Jwt:Audience` | `WoExpense.Api` / `WoExpense.Client` |

These are local dev values. Before deploying, change the DB password and `Jwt:Key` and keep them out of source control (user-secrets or environment variables).

### Database (Docker)

Defined in `docker-compose.yml`: image `postgres:17`, container `wo-pg`, host port **5434**, database `wo_expense_db`, user `postgres`, data in the named volume `wo_pgdata`.

```bash
docker compose up -d       # start
docker compose stop        # stop, keep data
docker compose down        # remove container, keep data
docker compose down -v     # remove container AND data
```

The compose file adds an `Asia/Calcutta` alias for `Asia/Kolkata`, because Postgres 17 images dropped the legacy name that Java clients (DBeaver) send.

DB client settings: host `localhost`, port `5434`, database `wo_expense_db`, user `postgres`.

## API

| Method | Path | Auth | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | no | Create account, returns tokens |
| POST | `/api/auth/login` | no | Log in, returns tokens |
| POST | `/api/auth/refresh` | no | New tokens for a refresh token (old one revoked) |
| GET | `/api/auth/me` | Bearer | Current user from the JWT |

Access token: 30 minutes. Refresh token: 7 days. Header: `Authorization: Bearer <accessToken>`.

`WeatherForecastController` is leftover template code and can be deleted.

## Conventions

Every entity inherits `BaseEntity`: `Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `IsDeleted`. `AppDbContext` manages them; don't set them by hand.

- **Audit:** insert sets all four audit fields; update changes only `UpdatedAt`/`UpdatedBy`. The user comes from `ICurrentUser` (the JWT); for anonymous register/login it is the owning user.
- **Soft delete:** `Remove()` sets `IsDeleted = true` and records `UpdatedBy`/`UpdatedAt`. A global filter hides deleted rows; use `.IgnoreQueryFilters()` to include them. Deleting a user does not cascade to their refresh tokens.
- **Unique email:** the index only covers non-deleted users, so a deleted user's email can be registered again.

## Adding a new API

Example: `Expense`.

1. **Domain:** `Entities/Expense.cs` inheriting `BaseEntity` (include `UserId` for the owner).
2. **Infrastructure:** add `DbSet<Expense>` and its mapping in `Data/AppDbContext.cs`. If it can be created in an anonymous flow, add a fallback actor in `ApplyAuditFields`.
3. **Migration:**
   ```bash
   dotnet ef migrations add AddExpenses -p src/WoExpense.Infrastructure -s src/WoExpense.Api
   dotnet ef database update -p src/WoExpense.Infrastructure -s src/WoExpense.Api
   ```
4. **Application:** `DTOs/Expenses/ExpenseDtos.cs` (records) and `Interfaces/IExpenseService.cs`.
5. **Infrastructure:** `Services/ExpenseService.cs`. Inject `AppDbContext` and `ICurrentUser`, always filter by `UserId == _currentUser.UserId`, and call `SaveChangesAsync()`.
6. **Api:** register in `Program.cs`: `builder.Services.AddScoped<IExpenseService, ExpenseService>();`
7. **Api:** `Controllers/ExpensesController.cs` with `[ApiController]`, `[Authorize]`, `[Route("api/[controller]")]`. Return 404 for missing or other users' records, 400 for validation errors.
8. **Test:** open Swagger, log in, click Authorize, paste the token, try each endpoint, then check `CreatedBy`/`UpdatedBy`/`IsDeleted` in the database.
9. **Docs:** add the endpoints to the table above.

## Migrations

```bash
dotnet ef migrations add <Name> -p src/WoExpense.Infrastructure -s src/WoExpense.Api
dotnet ef database update      -p src/WoExpense.Infrastructure -s src/WoExpense.Api
```

Existing: `Init`, `AddAuditFields`, `AddIsDeleted`.

## Troubleshooting

| Symptom | Fix |
|---|---|
| `password authentication failed for user "postgres"` | Another Postgres owns the port (e.g. WSL on 5432). Use port 5434 and the password above. |
| `invalid value for parameter "TimeZone": "Asia/Calcutta"` | Container wasn't created from `docker-compose.yml`. Run `docker compose up -d`. |
| `AddSwaggerGen` / `UseSwagger` not found | Add package `Swashbuckle.AspNetCore` to `WoExpense.Api.csproj`. |
| `failed to connect to the docker API` | Start Docker Desktop. |
