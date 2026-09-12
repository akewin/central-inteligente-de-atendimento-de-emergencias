using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Hospitals;

public sealed class ManageHospitals
{
    private readonly ApiDbContext _context;
    public ManageHospitals(ApiDbContext context) => _context = context;

    public async Task<IReadOnlyList<HospitalDto>> ListAsync(bool includeInactive, CancellationToken ct)
        => (await _context.Hospitals.AsNoTracking().Include(h => h.Wards)
            .Where(h => includeInactive || h.Active).OrderBy(h => h.Name).ToListAsync(ct))
            .Select(HospitalMapper.ToDto).ToList();

    public async Task<HospitalDto> SaveAsync(int? id, HospitalSaveDto dto, CancellationToken ct)
    {
        Validate(dto);
        Hospital hospital;
        if (id.HasValue)
        {
            hospital = await _context.Hospitals.Include(h => h.Wards).SingleOrDefaultAsync(h => h.Id == id, ct)
                ?? throw new AppException("Hospital não encontrado.", (int)HttpStatusCode.NotFound);
            if (!dto.ExpectedUpdatedAt.HasValue) throw new AppException("Versão do hospital não informada.");
            _context.Entry(hospital).Property(h => h.UpdatedAt).OriginalValue = dto.ExpectedUpdatedAt.Value;
        }
        else
        {
            hospital = new Hospital();
            _context.Hospitals.Add(hospital);
        }
        hospital.Name = dto.Name.Trim(); hospital.PostalCode = Digits(dto.PostalCode); hospital.Street = dto.Street.Trim();
        hospital.Number = dto.Number.Trim(); hospital.Complement = Empty(dto.Complement); hospital.Neighborhood = dto.Neighborhood.Trim();
        hospital.City = "Araras"; hospital.State = "SP"; hospital.Latitude = dto.Latitude; hospital.Longitude = dto.Longitude;
        hospital.HasEmergencyDepartment = dto.HasEmergencyDepartment; hospital.Active = dto.Active;
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AppException("O hospital foi atualizado por outro usuário.", (int)HttpStatusCode.Conflict); }
        catch (DbUpdateException) { throw new AppException("Já existe um hospital com esse nome ou os dados são inválidos.", (int)HttpStatusCode.Conflict); }
        return HospitalMapper.ToDto(hospital);
    }

    public async Task<HospitalWardDto> SaveWardAsync(int hospitalId, int? wardId, HospitalWardSaveDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.TotalBeds < 0 || dto.OccupiedBeds < 0 || dto.OccupiedBeds > dto.TotalBeds)
            throw new AppException("Informe uma ala e capacidades válidas.");
        if (!await _context.Hospitals.AnyAsync(h => h.Id == hospitalId, ct))
            throw new AppException("Hospital não encontrado.", (int)HttpStatusCode.NotFound);
        HospitalWard ward;
        if (wardId.HasValue)
        {
            ward = await _context.HospitalWards.SingleOrDefaultAsync(w => w.Id == wardId && w.HospitalId == hospitalId, ct)
                ?? throw new AppException("Ala não encontrada.", (int)HttpStatusCode.NotFound);
            if (!dto.ExpectedUpdatedAt.HasValue) throw new AppException("Versão da ala não informada.");
            if (dto.OccupiedBeds + ward.ReservedBeds > dto.TotalBeds)
                throw new AppException("O total não pode ser menor que as vagas ocupadas e reservadas.", (int)HttpStatusCode.Conflict);
            _context.Entry(ward).Property(w => w.UpdatedAt).OriginalValue = dto.ExpectedUpdatedAt.Value;
        }
        else { ward = new HospitalWard { HospitalId = hospitalId }; _context.HospitalWards.Add(ward); }
        ward.Name = dto.Name.Trim(); ward.TotalBeds = dto.TotalBeds; ward.OccupiedBeds = dto.OccupiedBeds; ward.Active = dto.Active;
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AppException("A ala foi atualizada por outro usuário.", (int)HttpStatusCode.Conflict); }
        catch (DbUpdateException) { throw new AppException("Já existe uma ala com esse nome ou a capacidade é inválida.", (int)HttpStatusCode.Conflict); }
        return HospitalMapper.ToDto(ward);
    }

    private static void Validate(HospitalSaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Street) || string.IsNullOrWhiteSpace(dto.Number)
            || string.IsNullOrWhiteSpace(dto.Neighborhood) || Digits(dto.PostalCode).Length != 8)
            throw new AppException("Preencha nome, CEP e endereço do hospital.");
        if (!dto.City.Trim().Equals("Araras", StringComparison.OrdinalIgnoreCase) || !dto.State.Trim().Equals("SP", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Somente hospitais de Araras/SP podem ser cadastrados.");
        if (dto.Latitude is < -90 or > 90 || dto.Longitude is < -180 or > 180 || dto.Latitude == 0 || dto.Longitude == 0)
            throw new AppException("Informe coordenadas válidas para o hospital.");
    }
    private static string Digits(string value) => new(value.Where(char.IsDigit).ToArray());
    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
