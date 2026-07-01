# 15 - Standards développement

Conventions et quality gates obligatoires pour tous développeurs Kine.

## Code organisation

### Structure repo
```
Kine/
├─ src/
│  ├─ Kine.Api                          (entry point HTTP)
│  ├─ Kine.Modules.Identity             (users, roles, auth)
│  ├─ Kine.Modules.Patients
│  ├─ Kine.Modules.Clinical
│  ├─ Kine.Modules.Scheduling
│  ├─ Kine.Modules.Billing
│  ├─ Kine.Modules.Reimbursement
│  ├─ Kine.Modules.Reporting
│  ├─ Kine.Modules.Audit
│  └─ Kine.SharedKernel                 (base classes, interfaces, constants)
├─ tests/
│  ├─ Kine.UnitTests
│  └─ Kine.IntegrationTests
├─ infra/
│  ├─ terraform/                        (cloud resources)
│  ├─ docker/                           (dev environments)
│  └─ scripts/                          (automation)
└─ SPEC/                                (documentation)
```

### Module structure (Clean Architecture light)
```
Kine.Modules.Patients/
├─ Domain/
│  ├─ Aggregates/
│  │  ├─ Patient.cs                     (root entity)
│  │  └─ PatientContact.cs
│  ├─ ValueObjects/
│  │  ├─ PatientId.cs
│  │  └─ ContactInfo.cs
│  ├─ Events/
│  │  ├─ PatientCreatedEvent.cs         (domain event)
│  │  └─ PatientStatusChangedEvent.cs
│  └─ Repositories/
│     └─ IPatientRepository.cs          (interface only)
├─ Application/
│  ├─ Commands/
│  │  ├─ CreatePatientCommand.cs
│  │  └─ CreatePatientHandler.cs
│  ├─ Queries/
│  │  ├─ GetPatientByIdQuery.cs
│  │  └─ GetPatientByIdHandler.cs
│  ├─ DTOs/
│  │  └─ PatientDto.cs
│  └─ Interfaces/
│     └─ IPatientService.cs
├─ Infrastructure/
│  ├─ Persistence/
│  │  ├─ PatientRepository.cs           (implementation)
│  │  └─ PatientsDbContext.cs           (EF Core)
│  └─ Services/
│     └─ PatientService.cs              (application logic)
└─ Presentation/
   └─ Controllers/
      └─ PatientsController.cs           (HTTP endpoints)
```

## Conventions de code

### Language
- **Français pour logique métier**, anglais pour technique.
- Classes métier: `Patient`, `RendezVous`, `FactureRemboursement` (français).
- Classes tech: `Repository`, `Handler`, `Controller`, `Middleware` (anglais).
- Commentaires: français pour métier, anglais pour détails tech.

### Naming
- **Classes:** PascalCase (`Patient`, `PatientRepository`)
- **Méthodes:** PascalCase (`CreatePatient`, `GetPatientById`)
- **Variables:** camelCase (`patientId`, `createdAt`)
- **Constantes:** UPPER_SNAKE_CASE (`MAX_PATIENTS_PER_QUERY = 1000`)
- **Privé:** `_underscore` prefix (`_patientRepository`, `_logger`)

### Style
- **Async/await obligatoire** pour I/O (DB, API calls)
- **Null-coalescing:** `patient?.Name ?? "Unknown"`
- **Pattern matching** où applicable
- **Immutability:** value objects immutables
- **No magic numbers:** constantes explicites

### Example
```csharp
public class Patient : AggregateRoot
{
    public PatientId Id { get; }
    public TenantId TenantId { get; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public PatientStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public string CreatedBy { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Patient(PatientId id, TenantId tenantId, string firstName, string lastName)
    {
        Id = id;
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
        Status = PatientStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Patient Create(TenantId tenantId, string firstName, string lastName, string createdBy)
    {
        var patient = new Patient(PatientId.New(), tenantId, firstName, lastName)
        {
            CreatedBy = createdBy
        };
        patient.AddDomainEvent(new PatientCreatedEvent(patient.Id, tenantId, firstName, lastName, createdBy));
        return patient;
    }

    public void UpdateStatus(PatientStatus newStatus, string updatedBy)
    {
        if (newStatus == Status) return;
        
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        
        AddDomainEvent(new PatientStatusChangedEvent(
            Id, TenantId, previousStatus, newStatus, updatedBy));
    }
}
```

## Sécurité

### Multi-tenant (OBLIGATOIRE)
- **Chaque query doit inclure tenant context.**
- **Middleware tenant extraction:** JWT claim ou header.
- **RLS policy activé DB:** toutes tables métier.
- **Code review:** checklist "Tenant context present?"

### Exemple (API endpoint)
```csharp
[Authorize]
[HttpPost("patients")]
public async Task<ActionResult> CreatePatient(CreatePatientDto dto)
{
    var tenantId = TenantContext.Current.TenantId; // obligatoire
    if (tenantId == null)
        return Unauthorized("Tenant context missing");

    var result = await _patientService.CreateAsync(tenantId, dto.FirstName, dto.LastName, User.Id);
    // ...
}
```

### Audit trail (OBLIGATOIRE)
- **Toute opération sensible → audit_logs.**
- **Payload:** user, action, entity, entity_id, before/after, timestamp.

```csharp
public class AuditService
{
    public async Task LogAsync(
        TenantId tenantId,
        string action,
        string entity,
        string entityId,
        object? before,
        object after,
        string userId)
    {
        var auditEntry = new AuditLogEntry(
            Id: Guid.NewGuid(),
            TenantId: tenantId,
            Action: action,
            Entity: entity,
            EntityId: entityId,
            BeforePayload: JsonConvert.SerializeObject(before),
            AfterPayload: JsonConvert.SerializeObject(after),
            CreatedAt: DateTimeOffset.UtcNow,
            UserId: userId);

        _db.AuditLogs.Add(auditEntry);
        await _db.SaveChangesAsync();
    }
}
```

### Secrets
- **Aucun secret en code source.** Vault ou environment variables.
- **API keys rotation:** 90 jours max.
- **Database passwords:** Vault managé.

## Testing

### Coverage
- **Unit: 60%+ obligatoire** sur logique métier.
- **Integration: flows critiques** (auth, patient CRUD, audit).
- **E2E: UAT final** avec 2-3 cabinets réels.

### Unit test example
```csharp
[TestFixture]
public class CreatePatientHandlerTests
{
    [Test]
    public async Task CreatePatient_WithValidInput_ReturnsPatientId()
    {
        // Arrange
        var tenantId = TenantId.New();
        var handler = new CreatePatientHandler(_patientService);
        var command = new CreatePatientCommand(tenantId, "Jean", "Dupont", "user123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.EqualTo(PatientId.Empty));
    }

    [Test]
    public async Task CreatePatient_WithoutTenantContext_ReturnsFail()
    {
        // Arrange
        var handler = new CreatePatientHandler(_patientService);
        var command = new CreatePatientCommand(TenantId.Empty, "Jean", "Dupont", "user123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Contains.Substring("Tenant"));
    }
}
```

### Anti-fuite tests (CI gate)
```csharp
[TestFixture]
public class MultiTenantIsolationTests
{
    [Test]
    public async Task Query_WithoutTenantFilter_ShouldFail()
    {
        // Arrange
        var db = _testDbContext;

        // Act - Query sans tenant_id filter
        var query = db.Patients.ToList(); // ❌ SHOULD FAIL

        // Assert
        Assert.Fail("Query executed without tenant context - isolation breach!");
    }

    [Test]
    public async Task RLS_Policy_IsEnforced()
    {
        // Arrange
        var tenant1 = TenantId.New();
        var tenant2 = TenantId.New();
        
        var patient1 = Patient.Create(tenant1, "Tenant1", "Patient1", "user1");
        var patient2 = Patient.Create(tenant2, "Tenant2", "Patient2", "user2");
        
        await _db.Patients.AddAsync(patient1);
        await _db.Patients.AddAsync(patient2);
        await _db.SaveChangesAsync();

        // Act - Set session current_user_tenant
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SET app.tenant_id = {tenant1}");
        
        var result = await _db.Patients.ToListAsync();

        // Assert - Doit retourner UNIQUEMENT patient1
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(patient1.Id));
    }
}
```

## CI/CD gates

### Pre-merge checks
- [ ] Code compiles
- [ ] Unit tests pass (60%+ coverage)
- [ ] No hardcoded secrets
- [ ] Anti-fuite tests pass (tenant isolation)
- [ ] Lint/static analysis pass
- [ ] Architecture review (module boundaries)

### Sample GitHub Actions workflow
```yaml
name: CI

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      
      - run: dotnet restore
      - run: dotnet build --configuration Release
      - run: dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
      
      - name: SonarQube scan
        run: dotnet sonarscanner begin /k:"Kine" && dotnet build && dotnet sonarscanner end
      
      - name: Anti-fuite test
        run: dotnet test --filter "Category=MultiTenantIsolation"
```

## Database

### Migrations
- **Entity Framework Core CodeFirst** (C#-first, migrations en git).
- Nommage: `yyyyMMdd_HHmmss_DescriptionMigration.cs`
- Example: `20260701_100000_AddPatientTable.cs`

### Schema rules
- **Toute table métier:** `tenant_id (UUID not null)` obligatoire.
- **Index minimum:** `(tenant_id, id)`, `(tenant_id, status)`, `(tenant_id, updated_at)`.
- **Audit trail:** `audit_logs` append-only.
- **Historisation:** tables dédiées pour transitions critiques (status changes).

## Documentation

### Code comments
```csharp
/// <summary>
/// Crée un patient pour le cabinet courant.
/// </summary>
/// <param name="tenantId">Identifiant du cabinet (tenant)</param>
/// <param name="firstName">Prénom du patient</param>
/// <param name="lastName">Nom du patient</param>
/// <param name="createdBy">Utilisateur créateur (audit)</param>
/// <returns>PatientId ou erreur si validation échoue</returns>
/// <exception cref="InvalidOperationException">Si tenant_id invalide</exception>
public async Task<Result<PatientId>> CreateAsync(
    TenantId tenantId,
    string firstName,
    string lastName,
    string createdBy)
{
    // Validate tenant context
    if (tenantId == null || tenantId.Value == Guid.Empty)
        return Result.Failure<PatientId>("Invalid tenant context");

    // TODO: Validate against INAMI patient rules (Q-B11 pending)
    // ...
}
```

### Module README
Chaque module = README.md avec:
- Responsabilités domaine
- Entités principales
- Events publiés
- Dépendances internes

## Quality rules

| Rule | Standard | Gate |
|---|---|---|
| Code coverage | 60%+ (unit) | Pre-merge |
| Cyclomatic complexity | <10 per method | Lint |
| Anti-fuite test | Pass | Pre-merge |
| Architecture boundary | No circular deps | Code review |
| Tenant context | Present all queries | Code review |
| Audit trail | Sensible ops logged | Code review |
| Secrets | Zero in repo | Pre-merge |
| TLS | 1.2+ all endpoints | Deploy gate |

## Owner
**Owner:** Engineering Lead + Team
**Reviewed:** CTO, Security Engineer
**Updated:** Each sprint (evolve standards)
