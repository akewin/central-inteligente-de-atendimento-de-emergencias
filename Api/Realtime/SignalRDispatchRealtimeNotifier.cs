using Api.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace Api.Realtime;

public sealed class SignalRDispatchRealtimeNotifier : IDispatchRealtimeNotifier
{
    private readonly IHubContext<DispatchHub> _hub;
    public SignalRDispatchRealtimeNotifier(IHubContext<DispatchHub> hub) => _hub = hub;

    public Task DispatchAssignedAsync(DispatchRealtimeEventDto message, CancellationToken cancellationToken = default)
        => Task.WhenAll(
            _hub.Clients.Group(DispatchHub.UnitGroup(message.UnitId)).SendAsync("dispatchAssigned", message, cancellationToken),
            _hub.Clients.Group(DispatchHub.OccurrenceGroup(message.OccurrenceId)).SendAsync("dispatchStatusChanged", message, cancellationToken));

    public Task DispatchStatusChangedAsync(DispatchRealtimeEventDto message, CancellationToken cancellationToken = default)
        => Task.WhenAll(
            _hub.Clients.Group(DispatchHub.UnitGroup(message.UnitId)).SendAsync("dispatchStatusChanged", message, cancellationToken),
            _hub.Clients.Group(DispatchHub.OccurrenceGroup(message.OccurrenceId)).SendAsync("dispatchStatusChanged", message, cancellationToken));

    public Task TransportChangedAsync(PatientTransportDto message, CancellationToken cancellationToken = default)
        => Task.WhenAll(
            message.HospitalId.HasValue ? _hub.Clients.Group(DispatchHub.HospitalGroup(message.HospitalId.Value)).SendAsync("transportChanged", message, cancellationToken) : Task.CompletedTask,
            message.SamuUnitId.HasValue ? _hub.Clients.Group(DispatchHub.UnitGroup(message.SamuUnitId.Value)).SendAsync("transportChanged", message, cancellationToken) : Task.CompletedTask,
            _hub.Clients.Group(DispatchHub.OccurrenceGroup(message.OccurrenceId)).SendAsync("transportChanged", message, cancellationToken));
}
