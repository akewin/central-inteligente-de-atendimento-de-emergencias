import * as signalR from '@microsoft/signalr';
import type { DispatchRealtimeEvent, PatientTransport } from '../interfaces';

function hubUrl() {
  const apiUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5209/api';
  return `${apiUrl.replace(/\/+$/, '').replace(/\/api$/, '')}/hubs/dispatches`;
}

export function createDispatchConnection(onEvent: (event: DispatchRealtimeEvent) => void, onReconnect: () => void, onTransport?: (event: PatientTransport) => void) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl(), { accessTokenFactory: () => localStorage.getItem('token') ?? '' })
    .withAutomaticReconnect()
    .build();
  connection.on('DispatchAssigned', onEvent);
  connection.on('DispatchUpdated', onEvent);
  if (onTransport) connection.on('transportChanged', onTransport);
  connection.onreconnected(onReconnect);
  return connection;
}
