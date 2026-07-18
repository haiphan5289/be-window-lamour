---
name: revert-spm-to-cocoapods
description: Revert or roll back an EF Core migration in the BE Window Lamour project — undo a migration, drop a table, revert schema changes. BE equivalent of iOS revert-spm-to-cocoapods. Use when a migration needs to be rolled back or corrected.
argument-hint: "migrationName:[Name] or targetMigration:[PreviousMigrationName]"
---

# BE Migration Rollback — Revert EF Core Migration

> Safely rolls back EF Core database migrations in the BE Window Lamour project.

---

## Rollback to a Specific Migration

```bash
# List all migrations to find the target
dotnet ef migrations list \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

# Revert DB to a previous migration (does NOT delete the migration file)
dotnet ef database update [PreviousMigrationName] \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

---

## Remove the Latest Migration (if not applied to DB yet)

```bash
# Removes the latest migration FILE — only safe if not applied to DB
dotnet ef migrations remove \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

---

## Full Reset (Development Only)

```bash
# Drop the database entirely
dotnet ef database drop \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

# Reapply all migrations from scratch
dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

---

## Safe Rollback Checklist

- [ ] Check `dotnet ef migrations list` to confirm current migration state
- [ ] Identify the target migration to revert to
- [ ] Run `dotnet ef database update [TargetMigration]` to revert DB schema
- [ ] Run `dotnet ef migrations remove` only if the migration was never applied to production
- [ ] Update code to remove entity changes if rolling back a feature
- [ ] Run `dotnet build` to verify no compilation errors
- [ ] Run `dotnet test` to verify nothing is broken

---

## When NOT to Remove a Migration

- Migration was already applied to **production** or **staging** DB — revert with `database update [Previous]` instead, then create a new corrective migration
- Migration is referenced by another migration — resolve in order

---

## Create a Corrective Migration

If the wrong migration was applied to production, create a corrective one instead of removing:

```bash
# Fix the code/entity, then create a new migration that undoes the mistake
dotnet ef migrations add Revert_[BadMigrationName] \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api

dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```
