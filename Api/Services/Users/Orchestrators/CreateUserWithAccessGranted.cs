using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Models;
using Api.Security.Policies;
using Api.Services.AccessPermissions;
using Api.Services.Users;
using Microsoft.EntityFrameworkCore;
using Api.Security.Permissions;
using Api.Middlewares;
using System.Net;

namespace Api.Services.Users.Orchestrators;

public class CreateUserWithAccessGranted
{
    private readonly ApiDbContext _context;
    private readonly CreateUser _createUser;
    private readonly CreateAccessPermissions _createAccessPermissions;
    private readonly IUserRepository _userRepository;
    private readonly CreateSystemLog _createSystemLog;
    private readonly AccessPermissionPolicy _accessPermissionPolicy;

    public CreateUserWithAccessGranted(
        ApiDbContext context,
        CreateUser createUser,
        CreateAccessPermissions createAccessPermissions,
        IUserRepository userRepository,
        CreateSystemLog createSystemLog,
        AccessPermissionPolicy accessPermissionPolicy)
    {
        _context = context;
        _createUser = createUser;
        _createAccessPermissions = createAccessPermissions;
        _userRepository = userRepository;
        _createSystemLog = createSystemLog;
        _accessPermissionPolicy = accessPermissionPolicy;
    }

    public async Task<UserReadDto> ExecuteAsync(UserCreateDto dto)
    {
        await NormalizeOperationalAssignmentAsync(dto);
        _accessPermissionPolicy.EnsureCanGrant(dto.PermissionIds);

        User user;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            user = await _createUser.ExecuteAsync(dto);
            await _context.SaveChangesAsync();

            if (dto.PermissionIds != null && dto.PermissionIds.Any())
            {
                await _createAccessPermissions.ExecuteAsync(
                    user.Id,
                    dto.PermissionIds
                );

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Create("User", user.Id),
            data: new SystemLogDataDto
            {
                Type = "create",
                Created = new
                {
                    dto.Username,
                    dto.Email,
                    dto.FullName,
                    dto.UnitId,
                    dto.HospitalId,
                    Permissions = dto.PermissionIds
                }
            }
        );

        var createdUser = await _userRepository.GetByIdAsync(user.Id)
            ?? throw new Exception("Usuário não encontrado.");

        return UserMapper.MapToUserReadDto(createdUser);
    }

    private async Task NormalizeOperationalAssignmentAsync(UserCreateDto dto)
    {
        dto.PermissionIds ??= [];
        if (dto.UnitId.HasValue && dto.HospitalId.HasValue)
            throw new AppException("O usuário não pode estar vinculado a uma equipe e a um hospital ao mesmo tempo.");
        if (dto.HospitalId.HasValue)
        {
            if (!await _context.Hospitals.AnyAsync(h => h.Id == dto.HospitalId && h.Active))
                throw new AppException("Hospital vinculado não encontrado.", (int)HttpStatusCode.NotFound);
            dto.PermissionIds.RemoveAll(id => id == BasePermissions.UNIT_OPERATIONS);
            if (!dto.PermissionIds.Contains(BasePermissions.HOSPITAL_OPERATIONS)) dto.PermissionIds.Add(BasePermissions.HOSPITAL_OPERATIONS);
            return;
        }
        dto.PermissionIds.RemoveAll(id => id == BasePermissions.HOSPITAL_OPERATIONS);
        if (!dto.UnitId.HasValue)
        {
            dto.PermissionIds.RemoveAll(id => id == BasePermissions.UNIT_OPERATIONS);
            return;
        }

        if (!await _context.Units.AnyAsync(unit => unit.Id == dto.UnitId.Value))
            throw new AppException("Equipe vinculada não encontrada.", (int)HttpStatusCode.NotFound);
        if (await _context.Users.AnyAsync(user => user.UnitId == dto.UnitId.Value))
            throw new AppException("A equipe já possui uma conta operacional.", (int)HttpStatusCode.Conflict);
        if (!dto.PermissionIds.Contains(BasePermissions.UNIT_OPERATIONS))
            dto.PermissionIds.Add(BasePermissions.UNIT_OPERATIONS);
    }
}
