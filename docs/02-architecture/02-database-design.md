# Database Strategy

**Version:** 1.0
**Last Updated:** 2025-10-18

---

## 1. Overview

The application supports multiple database providers to accommodate different deployment scenarios:
- **SQLite** for local development and testing
- **MySQL/MariaDB** for production deployments (cost-effective, widely supported)
- **PostgreSQL** for production deployments (advanced features, robust)
- **SQL Server** for enterprise deployments (optional)

---

## 2. Database Provider Support

### Development
- **Primary**: SQLite
- **Location**: Local file (`DiscordBot.db`)
- **Benefits**: Zero setup, portable, fast for dev/test

### Production Options
- **MySQL/MariaDB** - Recommended for most deployments
  - Cost-effective hosting
  - Wide hosting provider support
  - Good performance for this use case

- **PostgreSQL** - Recommended for advanced scenarios
  - Superior JSON support
  - Better concurrency handling
  - Advanced indexing options

- **SQL Server** - Optional for enterprise
  - Already configured in base template
  - Higher hosting costs
  - Good tooling support

---

## 3. EF Core Configuration

### 3.1 Multi-Provider Setup

The `ApplicationDbContext` should be provider-agnostic. Provider selection happens at DI registration.

```csharp
// Program.cs
var dbProvider = builder.Configuration.GetValue<string>("Database:Provider") ?? "sqlite";

switch (dbProvider.ToLowerInvariant())
{
    case "sqlite":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("SqliteConnection"),
                x => x.MigrationsAssembly("DiscordBot.Blazor")));
        break;

    case "mysql":
    case "mariadb":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("MySqlConnection"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection")),
                x => x.MigrationsAssembly("DiscordBot.Blazor")));
        break;

    case "postgresql":
    case "postgres":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PostgreSqlConnection"),
                x => x.MigrationsAssembly("DiscordBot.Blazor")));
        break;

    case "sqlserver":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SqlServerConnection"),
                x => x.MigrationsAssembly("DiscordBot.Blazor")));
        break;

    default:
        throw new InvalidOperationException($"Unsupported database provider: {dbProvider}");
}
```

### 3.2 Configuration (appsettings.json)

```json
{
  "Database": {
    "Provider": "sqlite"
  },
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=DiscordBot.db",
    "MySqlConnection": "Server=localhost;Database=discordbot;User=root;Password=password;",
    "PostgreSqlConnection": "Host=localhost;Database=discordbot;Username=postgres;Password=password;",
    "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=DiscordBot;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3.3 Environment-Based Configuration

```json
// appsettings.Development.json
{
  "Database": {
    "Provider": "sqlite"
  }
}

// appsettings.Production.json
{
  "Database": {
    "Provider": "mysql"
  }
}
```

---

## 4. Cross-Database Compatibility

### 4.1 Known Issues & Solutions

#### Issue 1: Text/String Field Types

**Problem**: SQL Server uses `nvarchar(MAX)`, SQLite uses `TEXT`, MySQL needs explicit lengths.

**Solution**: Always specify `MaxLength` in entity configuration:

```csharp
// ApplicationDbContext.cs - OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<InviteCode>(entity =>
    {
        entity.Property(e => e.Code)
            .HasMaxLength(14)      // XXXX-XXXX-XXXX
            .IsRequired();

        entity.Property(e => e.DiscordUsername)
            .HasMaxLength(100)     // Discord max username length
            .IsRequired();

        // Indexes
        entity.HasIndex(e => e.Code).IsUnique();
        entity.HasIndex(e => e.DiscordUserId);
        entity.HasIndex(e => new { e.IsUsed, e.ExpiresAt });
    });

    modelBuilder.Entity<ApplicationUser>(entity =>
    {
        entity.Property(e => e.DiscordUsername)
            .HasMaxLength(100);

        entity.HasIndex(e => e.DiscordUserId).IsUnique();
    });
}
```

#### Issue 2: DateTime Handling

**Problem**: Different providers handle DateTime precision differently.

**Solution**: Use UTC consistently and avoid sub-millisecond precision:

```csharp
public class InviteCode
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}

// In service
var code = new InviteCode
{
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddHours(24)
};
```

#### Issue 3: ulong (Discord ID) Storage

**Problem**: SQLite doesn't have native BIGINT unsigned support.

**Solution**: Use value converters for consistent handling:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Convert ulong to long for database storage
    var ulongConverter = new ValueConverter<ulong, long>(
        v => (long)v,
        v => (ulong)v);

    modelBuilder.Entity<InviteCode>(entity =>
    {
        entity.Property(e => e.DiscordUserId)
            .HasConversion(ulongConverter);
    });

    modelBuilder.Entity<ApplicationUser>(entity =>
    {
        entity.Property(e => e.DiscordUserId)
            .HasConversion(ulongConverter);
    });
}
```

#### Issue 4: GUID/UUID Storage

**Problem**: Different databases store GUIDs differently.

**Solution**: Let EF Core handle it automatically, but be consistent:

```csharp
public class InviteCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // EF Core handles conversion automatically
}
```

#### Issue 5: Boolean Storage

**Problem**: SQLite uses INTEGER (0/1), others use BOOLEAN.

**Solution**: EF Core handles this automatically, no action needed.

---

## 5. Migration Strategy

### 5.1 Initial Migration Fix

The current migration was scaffolded for SQL Server. Need to:

1. **Delete existing migration** (if not applied to production)
2. **Update `ApplicationDbContext.OnModelCreating()`** with cross-database configs
3. **Regenerate migration** with SQLite provider active
4. **Test migration** against all target providers

### 5.2 Migration Commands

```bash
# Delete old migrations (if safe)
Remove-Migration

# Switch to SQLite in appsettings.json
# "Database:Provider": "sqlite"

# Create new migration
dotnet ef migrations add InitialCreate --project DiscordBot.Blazor

# Test against SQLite
dotnet ef database update --project DiscordBot.Blazor

# Test against MySQL (change provider in config)
dotnet ef database update --project DiscordBot.Blazor

# Test against PostgreSQL (change provider in config)
dotnet ef database update --project DiscordBot.Blazor
```

### 5.3 Provider-Specific Migrations (If Needed)

For complex scenarios, maintain separate migration paths:

```
DiscordBot.Blazor/
└── Data/
    └── Migrations/
        ├── Sqlite/
        ├── MySql/
        └── PostgreSql/
```

Configure in `UseSqlite()` call:
```csharp
options.UseSqlite(connection, x => x.MigrationsAssembly("DiscordBot.Blazor")
                                    .MigrationsHistoryTable("__EFMigrationsHistory_SQLite"));
```

**Note**: Start with single migration path. Only split if absolutely necessary.

---

## 6. NuGet Package Requirements

### Required Packages

```xml
<ItemGroup>
  <!-- Existing -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.20" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.20" />

  <!-- Add for multi-database support -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.20" />
  <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.10" />
</ItemGroup>
```

---

## 7. Testing Strategy

### 7.1 Unit Tests
Use **SQLite in-memory** for fast, isolated tests:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite("Data Source=:memory:")
    .Options;

using var context = new ApplicationDbContext(options);
context.Database.OpenConnection(); // Keep in-memory DB alive
context.Database.EnsureCreated();

// Run tests
```

### 7.2 Integration Tests
Test against **real database instances** in CI/CD:

```yaml
# GitHub Actions example
- name: Setup MySQL
  run: |
    docker run -d -p 3306:3306 -e MYSQL_ROOT_PASSWORD=test mysql:8

- name: Setup PostgreSQL
  run: |
    docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=test postgres:16

- name: Run Integration Tests
  run: dotnet test --filter Category=Integration
```

---

## 8. Deployment Considerations

### 8.1 Database Initialization

```csharp
// Program.cs
var app = builder.Build();

// Auto-migrate on startup (dev/staging only)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Production: Run migrations manually or via CI/CD
```

### 8.2 Connection Pooling

All providers support connection pooling by default. No special configuration needed.

### 8.3 Backup Strategy

- **SQLite**: File-based, copy `DiscordBot.db`
- **MySQL**: `mysqldump`
- **PostgreSQL**: `pg_dump`
- **SQL Server**: Built-in backup tools

---

## 9. Performance Considerations

### Indexes
Already covered in `OnModelCreating` - same indexes work across all providers.

### Query Performance
Most queries are simple lookups. All providers perform well for this use case.

### Connection Limits
- SQLite: Single writer (fine for this app)
- MySQL/PostgreSQL: Hundreds of connections (more than sufficient)

---

## 10. Recommended Approach

### Phase 1 (Now - Development)
1. Switch to **SQLite** for development
2. Fix `ApplicationDbContext.OnModelCreating()` for cross-database compatibility
3. Delete and recreate migrations
4. Add required NuGet packages
5. Test migrations with SQLite

### Phase 2 (Pre-Production)
1. Add MySQL/PostgreSQL packages
2. Test migrations against all target providers
3. Document provider-specific quirks if any
4. Update deployment docs

### Phase 3 (Production)
1. Deploy with **MySQL** or **PostgreSQL**
2. Monitor performance
3. Optimize if needed

---

## 11. Example: Fixed Entity Configuration

```csharp
// DiscordBot.Blazor/Data/ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Value converter for ulong (Discord IDs)
    var ulongConverter = new ValueConverter<ulong, long>(
        v => unchecked((long)v),
        v => unchecked((ulong)v));

    var nullableUlongConverter = new ValueConverter<ulong?, long?>(
        v => v.HasValue ? unchecked((long)v.Value) : null,
        v => v.HasValue ? unchecked((ulong)v.Value) : null);

    // InviteCode configuration
    modelBuilder.Entity<InviteCode>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Code)
            .HasMaxLength(14)
            .IsRequired();

        entity.Property(e => e.DiscordUserId)
            .HasConversion(ulongConverter)
            .IsRequired();

        entity.Property(e => e.DiscordUsername)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .IsRequired();

        entity.Property(e => e.ExpiresAt)
            .IsRequired();

        entity.Property(e => e.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(e => e.UsedAt)
            .IsRequired(false);

        entity.Property(e => e.UsedByApplicationUserId)
            .HasMaxLength(450) // ASP.NET Identity default
            .IsRequired(false);

        // Indexes
        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.HasIndex(e => e.DiscordUserId);

        entity.HasIndex(e => new { e.IsUsed, e.ExpiresAt });

        // Foreign key
        entity.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UsedByApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
    });

    // ApplicationUser extensions
    modelBuilder.Entity<ApplicationUser>(entity =>
    {
        entity.Property(e => e.DiscordUserId)
            .HasConversion(nullableUlongConverter);

        entity.Property(e => e.DiscordUsername)
            .HasMaxLength(100);

        entity.Property(e => e.DiscordLinkedAt)
            .IsRequired(false);

        entity.HasIndex(e => e.DiscordUserId)
            .IsUnique()
            .HasFilter("[DiscordUserId] IS NOT NULL"); // SQL Server style, adjust for others
    });
}
```

**Note**: The filter on the unique index may need provider-specific handling. Consider removing if issues arise.

---

## 12. Common Pitfalls to Avoid

❌ **Don't** use provider-specific SQL in migrations
❌ **Don't** use MAX without explicit length on strings (breaks MySQL)
❌ **Don't** rely on default GUID generation (be explicit)
❌ **Don't** use `DateTime.Now` (always use `DateTime.UtcNow`)
❌ **Don't** assume case-sensitive string comparisons

✅ **Do** specify string max lengths
✅ **Do** use UTC for all timestamps
✅ **Do** test migrations on all target providers
✅ **Do** use value converters for special types (ulong)
✅ **Do** keep migrations simple and data-agnostic
