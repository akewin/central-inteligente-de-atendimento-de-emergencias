using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Helpers;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Security.Policies;
using Api.Services.AccessPermissions;
using Api.Services.Users;
using Microsoft.EntityFrameworkCore;
using Api.Security.Permissions;
using System.Net;

namespace Api.Services.Users.Orchestrators;

public class UpdateUserWithAccessGranted
{
    private readonly ApiDbContext _context;
    private readonly UpdateUser _updateUser;
    private readonly IUserRepository _userRepository;
    private readonly IAccessPermissionRepository _accessPermissionRepository;
    private readonly CreateAccessPermissions _createAccessPermissions;
    private readonly CreateSystemLog _createSystemLog;
    private readonly AccessPermissionPolicy _accessPermissionPolicy;

    public UpdateUserWithAccessGranted(
        ApiDbContext context,
        UpdateUser updateUser,
        IUserRepository userRepository,
        IAccessPermissionRepository accessPermissionRepository,
        CreateAccessPermissions createAccessPermissions,
        CreateSystemLog createSystemLog,
        AccessPermissionPolicy accessPermissionPolicy)
    {
        _context = context;
        _updateUser = updateUser;
        _userRepository = userRepository;
        _accessPermissionRepository = accessPermissionRepository;
        _createAccessPermissions = createAccessPermissions;
        _createSystemLog = createSystemLog;
        _accessPermissionPolicy = accessPermissionPolicy;
    }

    public async Task<UserReadDto> ExecuteAsync(UserUpdateDto dto)
    {
        Guard.AgainstNonPositiveInt(dto.Id);

        object prevState;
        object currState;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _userRepository.GetByIdAsync(dto.Id)
                ?? throw new AppException("Usuário não encontrado.");

            await NormalizeOperationalAssignmentAsync(dto, user);
            _accessPermissionPolicy.EnsureCanGrant(dto.PermissionIds!);

            prevState = new
            {
                user.Username,
                user.Email,
                user.FullName,
                user.UnitId,
                user.HospitalId,
                Permissions = user.AccessPermissions
                    .Select(p => p.SystemResourceId)
                    .ToList()
            };

            var updatedUser = await _updateUser.ExecuteAsync(dto);
            await _context.SaveChangesAsync();

            if (dto.PermissionIds != null)
            {
                await _accessPermissionRepository.RemoveByUserIdAsync(updatedUser.Id);

                if (dto.PermissionIds.Any())
                {
                    await _createAccessPermissions.ExecuteAsync(
                        updatedUser.Id,
                        dto.PermissionIds
                    );
                }

                await _context.SaveChangesAsync();
            }

            currState = new
            {
                updatedUser.Username,
                updatedUser.Email,
                updatedUser.FullName,
                updatedUser.UnitId,
                updatedUser.HospitalId,
                Permissions = dto.PermissionIds
            };

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Update("User", dto.Id),
            data: new SystemLogDataDto
            {
                Type = "update",
                PrevState = prevState,
                CurrState = currState
            }
        );

        var resultUser = await _userRepository.GetByIdAsync(dto.Id)
            ?? throw new AppException("Usuário atualizado não encontrado.");

        return UserMapper.MapToUserReadDto(resultUser);
    }

    private async Task NormalizeOperationalAssignmentAsync(UserUpdateDto dto, Api.Models.User user)
    {
        var finalUnitId = dto.HasUnitId ? dto.UnitId : user.UnitId;
        var finalHospitalId = dto.HasHospitalId ? dto.HospitalId : user.HospitalId;
        var permissionIds = dto.PermissionIds?.Distinct().ToList()
            ?? user.AccessPermissions.Select(permission => permission.SystemResourceId).ToList();

        if (finalUnitId.HasValue && finalHospitalId.HasValue)
            throw new AppException("O usuário não pode estar vinculado a uma equipe e a um hospital ao mesmo tempo.");
        if (finalHospitalId.HasValue)
        {
            if (!await _context.Hospitals.AnyAsync(h => h.Id == finalHospitalId && h.Active))
                throw new AppException("Hospital vinculado não encontrado.", (int)HttpStatusCode.NotFound);
            permissionIds.RemoveAll(id => id == BasePermissions.UNIT_OPERATIONS);
            if (!permissionIds.Contains(BasePermissions.HOSPITAL_OPERATIONS)) permissionIds.Add(BasePermissions.HOSPITAL_OPERATIONS);
        }
        else permissionIds.RemoveAll(id => id == BasePermissions.HOSPITAL_OPERATIONS);

        if (finalUnitId.HasValue)
        {
            if (!await _context.Units.AnyAsync(unit => unit.Id == finalUnitId.Value))
                throw new AppException("Equipe vinculada não encontrada.", (int)HttpStatusCode.NotFound);
            if (await _context.Users.AnyAsync(other => other.UnitId == finalUnitId.Value && other.Id != user.Id))
                throw new AppException("A equipe já possui uma conta operacional.", (int)HttpStatusCode.Conflict);
            if (!permissionIds.Contains(BasePermissions.UNIT_OPERATIONS))
                permissionIds.Add(BasePermissions.UNIT_OPERATIONS);
        }
        else
        {
            permissionIds.RemoveAll(id => id == BasePermissions.UNIT_OPERATIONS);
        }

        dto.PermissionIds = permissionIds;
    }
}
