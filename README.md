# PillsTracker

## Migrations

Create initial migration (if you need to regenerate):

```bash
dotnet ef migrations add InitialCreate --project src/PillsTracker.Infrastructure --startup-project src/PillsTracker.WebApi
```

Apply migrations:

```bash
dotnet ef database update --project src/PillsTracker.Infrastructure --startup-project src/PillsTracker.WebApi
```
