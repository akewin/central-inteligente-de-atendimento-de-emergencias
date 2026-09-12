import { useEffect, useState } from 'react';
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Typography } from '@mui/material';
import * as L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import type { OperationalRoute } from '../../interfaces';

const tileUrl = import.meta.env.VITE_MAP_TILE_URL || 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';

export default function OperationalRouteDialog({ route, open, onClose }: { route: OperationalRoute | null; open: boolean; onClose: () => void }) {
  const [mapElement, setMapElement] = useState<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!open || !route || !mapElement) return;
    let map: L.Map | null = null;
    const mountTimer = window.setTimeout(() => {
      map = L.map(mapElement, { scrollWheelZoom: false });
      L.tileLayer(tileUrl, { maxZoom: 19, attribution: '&copy; OpenStreetMap contributors &copy; CARTO' }).addTo(map);
      const geometry: L.LatLngExpression[] = route.geometry.map(point => [point.latitude, point.longitude]);
      const boundsPoints: L.LatLngExpression[] = geometry.length > 0
        ? geometry
        : [[route.origin.latitude, route.origin.longitude], [route.destination.latitude, route.destination.longitude]];
      L.circleMarker([route.origin.latitude, route.origin.longitude], { radius: 9, color: '#fff', weight: 3, fillColor: '#1976d2', fillOpacity: 1 }).bindPopup(route.originLabel).addTo(map);
      L.circleMarker([route.destination.latitude, route.destination.longitude], { radius: 10, color: '#fff', weight: 3, fillColor: route.stage === 'HOSPITAL' ? '#18864b' : '#d32f2f', fillOpacity: 1 }).bindPopup(route.destinationLabel).addTo(map);
      L.polyline(geometry, { color: '#fff', weight: 10, opacity: .9 }).addTo(map);
      L.polyline(geometry, { color: '#1565c0', weight: 6, opacity: .95 }).addTo(map);
      map.invalidateSize();
      map.fitBounds(L.latLngBounds(boundsPoints), { padding: [32, 32], maxZoom: 16 });
    }, 400);
    return () => {
      window.clearTimeout(mountTimer);
      map?.remove();
    };
  }, [mapElement, open, route]);

  return <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
    <DialogTitle>{route?.stage === 'HOSPITAL' ? 'Rota da vítima ao hospital' : 'Rota até a vítima'}</DialogTitle>
    <DialogContent>
      {route && <Stack gap={2}>
        <Alert severity="info">
          <strong>{route.originLabel}</strong> → <strong>{route.destinationLabel}</strong><br />
          {route.distanceKm.toLocaleString('pt-BR')} km · aproximadamente {route.estimatedMinutes.toLocaleString('pt-BR')} min
        </Alert>
        <Box ref={setMapElement} sx={{ width: '100%', height: { xs: 360, md: 500 }, minHeight: 360, borderRadius: 2, overflow: 'hidden', bgcolor: 'action.hover' }} />
        <Typography variant="caption" color="text.secondary">A rota é uma estimativa e pode mudar conforme bloqueios, trânsito e condições da via.</Typography>
      </Stack>}
    </DialogContent>
    <DialogActions><Button onClick={onClose}>Fechar</Button></DialogActions>
  </Dialog>;
}
