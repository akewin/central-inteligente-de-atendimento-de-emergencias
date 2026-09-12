using Api.Dtos;

namespace Api.Realtime;

public interface IDispatchRealtimeNotifier
{
    Task DispatchAssignedAsync(DispatchRealtimeEventDto message, CancellationToken cancellationToken = default);
    Task DispatchStatusChangedAsync(DispatchRealtimeEventDto message, CancellationToken cancellationToken = default);
    Task TransportChangedAsync(PatientTransportDto message, CancellationToken cancellationToken = default);
}
