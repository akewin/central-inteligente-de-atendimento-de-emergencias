using Api.Helpers;
using Api.Models;
using Api.Security.Passwords;
using Api.Security.Permissions;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public static class DbInitializer
{
    public static async Task SeedSystemResourcesAsync(ApiDbContext context)
    {
        var resources = new List<SystemResource>
            {
                new SystemResource { Name = "root", ExhibitionName = "Administrador" },
                new SystemResource { Name = "users", ExhibitionName = "Gerenciamento de Usuários" },
                new SystemResource { Name = "resources", ExhibitionName = "Recursos do Sistema" },
                new SystemResource { Name = "reports", ExhibitionName = "Auditoria do Sistema" },
                new SystemResource { Name = "occurrences", ExhibitionName = "Gerenciamento de Ocorrências" },
                new SystemResource { Id = 6, Name = "units-view", ExhibitionName = "Consulta Operacional de Equipes" },
                new SystemResource { Id = 7, Name = "units-manage", ExhibitionName = "Gerenciamento Administrativo de Equipes" },
                new SystemResource { Id = 8, Name = "unit-operations", ExhibitionName = "Operações da Equipe" },
                new SystemResource { Id = 9, Name = "hospitals-view", ExhibitionName = "Consulta de Hospitais" },
                new SystemResource { Id = 10, Name = "hospitals-manage", ExhibitionName = "Gerenciamento de Hospitais" },
                new SystemResource { Id = 11, Name = "hospital-operations", ExhibitionName = "Recebimentos Hospitalares" },
            };

        foreach (var reserved in resources.Where(resource => resource.Id is >= 6 and <= 11))
        {
            var conflict = await context.SystemResources.AsNoTracking()
                .FirstOrDefaultAsync(resource => resource.Id == reserved.Id && resource.Name != reserved.Name);
            if (conflict is not null)
                throw new InvalidOperationException(
                    $"O recurso '{conflict.Name}' ocupa o ID reservado {reserved.Id} de '{reserved.Name}'.");

            var existingWithName = await context.SystemResources.AsNoTracking()
                .FirstOrDefaultAsync(resource => resource.Name == reserved.Name);
            if (existingWithName is not null && existingWithName.Id != reserved.Id)
                throw new InvalidOperationException(
                    $"O recurso '{reserved.Name}' existe com ID {existingWithName.Id}, mas o RBAC exige o ID {reserved.Id}.");
        }

        var existingNames = await context.SystemResources
            .Select(resource => resource.Name)
            .ToListAsync();

        var missingResources = resources
            .Where(resource => !existingNames.Contains(resource.Name))
            .ToList();

        if (missingResources.Count == 0)
            return;

        foreach (var resource in missingResources)
        {
            resource.CreatedAt = DateTime.UtcNow;
            resource.UpdatedAt = DateTime.UtcNow;
        }

        await context.SystemResources.AddRangeAsync(missingResources);
        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlRawAsync(
            "SELECT setval(pg_get_serial_sequence('system_resources', 'id'), (SELECT MAX(id) FROM system_resources), true)");

        Console.WriteLine("Seed de system resources executada.");
    }

    public static async Task SeedEmergencyServicesAsync(ApiDbContext context)
    {
        var definitions = new[]
        {
            new EmergencyService { Type = EmergencyServiceType.POLICIA, EmergencyNumber = "190", DisplayName = "Polícia Militar" },
            new EmergencyService { Type = EmergencyServiceType.SAMU, EmergencyNumber = "192", DisplayName = "Serviço de Atendimento Móvel de Urgência" },
            new EmergencyService { Type = EmergencyServiceType.BOMBEIROS, EmergencyNumber = "193", DisplayName = "Corpo de Bombeiros" }
        };

        var existingTypes = await context.EmergencyServices.Select(service => service.Type).ToListAsync();
        var missing = definitions.Where(service => !existingTypes.Contains(service.Type)).ToList();
        if (missing.Count == 0) return;

        await context.EmergencyServices.AddRangeAsync(missing);
        await context.SaveChangesAsync();
        Console.WriteLine("Seed de serviços de emergência executada.");
    }

    public static async Task SeedEmergencyUnitsAsync(ApiDbContext context)
    {
        var runSeed = Environment.GetEnvironmentVariable("RUN_EMERGENCY_UNITS_SEED");
        if (!string.Equals(runSeed, "true", StringComparison.OrdinalIgnoreCase)) return;

        var services = await context.EmergencyServices.ToDictionaryAsync(service => service.Type);
        var definitions = new[]
        {
            ("PM-01", EmergencyServiceType.POLICIA, -23.550520m, -46.633308m),
            ("PM-02", EmergencyServiceType.POLICIA, -23.561414m, -46.655881m),
            ("SAMU-01", EmergencyServiceType.SAMU, -23.557790m, -46.639557m),
            ("SAMU-02", EmergencyServiceType.SAMU, -23.574300m, -46.623100m),
            ("BM-01", EmergencyServiceType.BOMBEIROS, -23.543100m, -46.642500m),
            ("BM-02", EmergencyServiceType.BOMBEIROS, -23.568900m, -46.649200m)
        };

        var existingNames = await context.Units.Select(unit => unit.NormalizedName).ToListAsync();
        var missing = definitions
            .Where(item => !existingNames.Contains(item.Item1.ToUpperInvariant()))
            .Select(item => new Unit
            {
                Name = item.Item1,
                NormalizedName = item.Item1.ToUpperInvariant(),
                EmergencyServiceId = services[item.Item2].Id,
                Latitude = item.Item3,
                Longitude = item.Item4,
                Status = UnitStatus.DISPONIVEL
            }).ToList();

        if (missing.Count == 0) return;
        await context.Units.AddRangeAsync(missing);
        await context.SaveChangesAsync();
        Console.WriteLine("Seed de equipes simuladas executada.");
    }

    public static async Task SeedHospitalsAsync(ApiDbContext context)
    {
        var definitions = new[]
        {
            new Hospital { Name = "Unidade de Pronto Atendimento Elisa Sbrissa Franchozza", PostalCode = "13606414", Street = "Avenida Irineu Carroci", Number = "400", Neighborhood = "Jardim José Ometto II", Latitude = -22.357520m, Longitude = -47.385150m, HasEmergencyDepartment = true },
            new Hospital { Name = "Hospital Pró Saúde", PostalCode = "13600000", Street = "Avenida Augusta Viola da Costa", Number = "805", Neighborhood = "Jardim Celina", Latitude = -22.354300m, Longitude = -47.376900m, HasEmergencyDepartment = true },
            new Hospital { Name = "Hospital São Leopoldo Mandic", PostalCode = "13600000", Street = "Avenida Padre Alarico Zacharias", Number = "1253", Neighborhood = "Jardim Belvedere", Latitude = -22.365100m, Longitude = -47.389700m, HasEmergencyDepartment = true },
            new Hospital { Name = "Hospital Unimed Anhanguera - Unidade Araras", PostalCode = "13607213", Street = "Avenida José Marques da Silva", Number = "1410", Neighborhood = "Jardim Nova Suíça", Latitude = -22.362000m, Longitude = -47.374200m, HasEmergencyDepartment = true },
            new Hospital { Name = "Santa Casa de Misericórdia de Araras - Hospital São Luiz", PostalCode = "13600730", Street = "Praça Doutor Narciso Gomes", Number = "49", Neighborhood = "Centro", Latitude = -22.356600m, Longitude = -47.383600m, HasEmergencyDepartment = true }
        };
        var existing = await context.Hospitals.Select(h => h.Name).ToListAsync();
        var missing = definitions.Where(h => !existing.Contains(h.Name)).ToList();
        foreach (var hospital in missing)
        {
            hospital.City = "Araras"; hospital.State = "SP"; hospital.Active = true;
            hospital.Wards.Add(new HospitalWard { Name = "Pronto atendimento", TotalBeds = 10, Active = true });
            hospital.Wards.Add(new HospitalWard { Name = "Observação", TotalBeds = 8, Active = true });
            hospital.Wards.Add(new HospitalWard { Name = "Emergência", TotalBeds = 4, Active = true });
        }
        if (missing.Count > 0) { context.Hospitals.AddRange(missing); await context.SaveChangesAsync(); }
    }

    public static async Task SeedHospitalUsersAsync(ApiDbContext context)
    {
        var defaultPassword = Environment.GetEnvironmentVariable("HOSPITAL_USERS_DEFAULT_PASSWORD");
        if (string.IsNullOrWhiteSpace(defaultPassword))
            defaultPassword = "Hospital@123";

        var hospitalOperations = await context.SystemResources
            .SingleAsync(resource => resource.Id == BasePermissions.HOSPITAL_OPERATIONS);
        var hospitals = await context.Hospitals.ToDictionaryAsync(hospital => hospital.Name);
        var definitions = new[]
        {
            (Hospital: "Unidade de Pronto Atendimento Elisa Sbrissa Franchozza", Username: "hospital.upa", Email: "upa@hospital.araras.local", FullName: "Recepção UPA Elisa Sbrissa"),
            (Hospital: "Hospital Pró Saúde", Username: "hospital.prosaude", Email: "prosaude@hospital.araras.local", FullName: "Recepção Hospital Pró Saúde"),
            (Hospital: "Hospital São Leopoldo Mandic", Username: "hospital.mandic", Email: "mandic@hospital.araras.local", FullName: "Recepção Hospital São Leopoldo Mandic"),
            (Hospital: "Hospital Unimed Anhanguera - Unidade Araras", Username: "hospital.unimed", Email: "unimed@hospital.araras.local", FullName: "Recepção Hospital Unimed Araras"),
            (Hospital: "Santa Casa de Misericórdia de Araras - Hospital São Luiz", Username: "hospital.santacasa", Email: "santacasa@hospital.araras.local", FullName: "Recepção Santa Casa de Araras")
        };

        foreach (var definition in definitions)
        {
            var hospital = hospitals[definition.Hospital];
            var user = await context.Users
                .Include(existing => existing.AccessPermissions)
                .SingleOrDefaultAsync(existing => existing.Username == definition.Username);

            if (user is null)
            {
                user = new User
                {
                    Username = definition.Username,
                    Email = definition.Email,
                    Password = PasswordHash.Generate(defaultPassword),
                    FullName = definition.FullName,
                    HospitalId = hospital.Id,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
            }
            else
            {
                if (user.UnitId.HasValue)
                    throw new InvalidOperationException($"O usuário reservado '{user.Username}' já está vinculado a uma equipe.");

                user.HospitalId = hospital.Id;
                user.Active = true;
                user.UpdatedAt = DateTime.UtcNow;
            }

            if (user.AccessPermissions.All(permission => permission.SystemResourceId != hospitalOperations.Id))
            {
                user.AccessPermissions.Add(new AccessPermission
                {
                    SystemResourceId = hospitalOperations.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Contas operacionais dos hospitais verificadas.");
    }

    public static async Task CreateRootAsync(ApiDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Username == "root"))
            return;

        var rootResource = await context.SystemResources.FirstOrDefaultAsync(r => r.Name == "root");
        if (rootResource == null)
        {
            rootResource = new SystemResource
            {
                Name = "root",
                ExhibitionName = "Administrador",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await context.SystemResources.AddAsync(rootResource);
            await context.SaveChangesAsync();
        }

        var rootUser = new User
        {
            Username = "root",
            Email = "root@admin.com",
            Password = PasswordHash.Generate("root1234"), // trocar para senha segura em produção
            FullName = "Administrador",
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(rootUser);
        await context.SaveChangesAsync();

        var accessPermission = new AccessPermission
        {
            UserId = rootUser.Id,
            SystemResourceId = rootResource.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.AccessPermissions.AddAsync(accessPermission);
        await context.SaveChangesAsync();

        Console.WriteLine("Usuário root criado com sucesso.");
    }

    public static async Task SeedUsersAsync(ApiDbContext context)
    {
        var runSeed = Environment.GetEnvironmentVariable("RUN_USERS_SEED");
        if (!string.Equals(runSeed, "true", StringComparison.OrdinalIgnoreCase))
            return;

        if (await context.Users.AnyAsync(u => u.Username != "root"))
            return;

        var users = new List<User>
            {
                new User { Username = "alice", Email = "alice@test.com", Password = "123456", FullName = "Alice Wonderland" },
                new User { Username = "bob", Email = "bob@test.com", Password = "123456", FullName = "Bob Builder" },
                new User { Username = "carol", Email = "carol@test.com", Password = "123456", FullName = "Carol Singer" },
                new User { Username = "dave", Email = "dave@test.com", Password = "123456", FullName = "Dave Grohl" },
                new User { Username = "eve", Email = "eve@test.com", Password = "123456", FullName = "Eve Online" },
                new User { Username = "frank", Email = "frank@test.com", Password = "123456", FullName = "Frank Ocean" },
                new User { Username = "grace", Email = "grace@test.com", Password = "123456", FullName = "Grace Hopper" },
                new User { Username = "heidi", Email = "heidi@test.com", Password = "123456", FullName = "Heidi Klum" },
                new User { Username = "ivan", Email = "ivan@test.com", Password = "123456", FullName = "Ivan Drago" },
                new User { Username = "judy", Email = "judy@test.com", Password = "123456", FullName = "Judy Hopps" },
            };

        foreach (var user in users)
        {
            user.Password = PasswordHash.Generate(user.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        Console.WriteLine("Seed de usuários de teste executada.");
    }

    public static async Task SeedAllAsync(ApiDbContext context)
    {
        await SeedSystemResourcesAsync(context);
        await CreateRootAsync(context);
        await SeedUsersAsync(context);
        await SeedEmergencyServicesAsync(context);
        await SeedHospitalsAsync(context);
        await SeedHospitalUsersAsync(context);
        await SeedEmergencyUnitsAsync(context);
    }
}

public static class ApplicationBuilderExtensions
{
    public static async Task UseDbInitializerAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        await context.Database.MigrateAsync();

        await DbInitializer.SeedAllAsync(context);
    }
}
