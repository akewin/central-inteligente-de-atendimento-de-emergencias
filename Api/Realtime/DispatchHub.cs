using Api.Security.Jwt;
using Api.Security.Permissions;
using Microsoft.AspNetCore.SignalR;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Realtime;

public sealed class DispatchHub : Hub
{
    private readonly ApiDbContext _context;
    public DispatchHub(ApiDbContext context) => _context = context;

    public override async Task OnConnectedAsync()
    {
        var principal = Context.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            Context.Abort();
            return;
        }
        var unitId = principal.GetUnitId();
        var userId = principal.GetUserId();
        if (unitId.HasValue && principal.HasPermission(BasePermissions.UNIT_OPERATIONS))
        {
            var validAssignment = await _context.Users.AsNoTracking().AnyAsync(user =>
                user.Id == userId && user.Active && user.UnitId == unitId.Value &&
                user.AccessPermissions.Any(permission => permission.SystemResourceId == BasePermissions.UNIT_OPERATIONS));
            if (!validAssignment) { Context.Abort(); return; }
            await Groups.AddToGroupAsync(Context.ConnectionId, UnitGroup(unitId.Value));
        }
        var hospitalId = principal.GetHospitalId();
        if (hospitalId.HasValue && principal.HasPermission(BasePermissions.HOSPITAL_OPERATIONS))
        {
            var validHospital = await _context.Users.AsNoTracking().AnyAsync(user => user.Id == userId && user.Active
                && user.HospitalId == hospitalId.Value && user.AccessPermissions.Any(permission => permission.SystemResourceId == BasePermissions.HOSPITAL_OPERATIONS));
            if (validHospital) await Groups.AddToGroupAsync(Context.ConnectionId, HospitalGroup(hospitalId.Value));
        }
        await base.OnConnectedAsync();
    }

    public Task WatchOccurrence(int occurrenceId)
    {
        if (occurrenceId <= 0 || Context.User is null ||
            (!Context.User.IsRoot() && !Context.User.HasPermission(BasePermissions.OCCURRENCES)))
            throw new HubException("Acesso negado à ocorrência.");
        return Groups.AddToGroupAsync(Context.ConnectionId, OccurrenceGroup(occurrenceId));
    }

    public static string UnitGroup(int unitId) => $"unit:{unitId}";
    public static string OccurrenceGroup(int occurrenceId) => $"occurrence:{occurrenceId}";
    public static string HospitalGroup(int hospitalId) => $"hospital:{hospitalId}";
}
