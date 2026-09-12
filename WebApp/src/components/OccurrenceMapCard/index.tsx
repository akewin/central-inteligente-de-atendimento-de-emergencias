import { useCallback, useEffect, useRef, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Stack,
  Typography,
} from '@mui/material';
import { CheckCircle, LocalHospital, Map, Refresh, Route } from '@mui/icons-material';
import * as L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import type { OccurrenceMapData, OccurrenceMapHospital } from '../../interfaces';
import { getErrorMessage } from '../../helpers';
import { getOccurrenceMap } from '../../services';

interface OccurrenceMapCardProps {
  occurrenceId: number;
  refreshKey: string;
  compact?: boolean;
}

const tileUrl = import.meta.env.VITE_MAP_TILE_URL || 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';

function formatKm(value: number) {
  return `${value.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 2 })} km`;
}

function formatMinutes(value: number) {
  return `${value.toLocaleString('pt-BR', { maximumFractionDigits: 1 })} min`;
}

function popup(title: string, details: string[]) {
  const container = document.createElement('div');
  const heading = document.createElement('strong');
  heading.textContent = title;
  container.appendChild(heading);
  details.filter(Boolean).forEach((detail) => {
    const line = document.createElement('div');
    line.textContent = detail;
    container.appendChild(line);
  });
  return container;
}

function formatDistance(hospital: OccurrenceMapHospital) {
  return hospital.roadDistanceKm !== null
    ? `${formatKm(hospital.roadDistanceKm)} por via`
    : `${formatKm(hospital.straightLineDistanceKm)} em linha reta`;
}

export default function OccurrenceMapCard({ occurrenceId, refreshKey, compact = false }: OccurrenceMapCardProps) {
  const mapElementRef = useRef<HTMLDivElement | null>(null);
  const [data, setData] = useState<OccurrenceMapData | null>(null);
  const [loading, setLoading] = useState(true);
  const [routeLoading, setRouteLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadMap = useCallback(async (hospitalId?: number) => {
    if (hospitalId) setRouteLoading(true);
    else setLoading(true);
    setError(null);
    try {
      setData(await getOccurrenceMap(occurrenceId, hospitalId));
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
      setRouteLoading(false);
    }
  }, [occurrenceId]);

  useEffect(() => {
    void loadMap();
  }, [loadMap, refreshKey]);

  useEffect(() => {
    if (!data || !mapElementRef.current) return;

    const map = L.map(mapElementRef.current, { scrollWheelZoom: false, zoomControl: true });
    L.tileLayer(tileUrl, {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors &copy; CARTO',
    }).addTo(map);

    const bounds: L.LatLngExpression[] = [];
    const victimPosition: L.LatLngExpression = [data.victim.latitude, data.victim.longitude];
    bounds.push(victimPosition);
    L.circleMarker(victimPosition, {
      radius: 10,
      color: '#ffffff',
      weight: 3,
      fillColor: '#d32f2f',
      fillOpacity: 1,
    }).bindPopup(popup('Vítima / ocorrência', [data.victim.label])).addTo(map);

    data.ambulances.forEach((ambulance) => {
      const position: L.LatLngExpression = [ambulance.latitude, ambulance.longitude];
      bounds.push(position);
      L.circleMarker(position, {
        radius: 9,
        color: '#ffffff',
        weight: 3,
        fillColor: '#1976d2',
        fillOpacity: 1,
      }).bindPopup(popup(`Ambulância ${ambulance.name}`, [ambulance.address, ambulance.status])).addTo(map);
    });

    data.hospitals.forEach((hospital, index) => {
      const selected = hospital.id === data.selectedHospitalId;
      const position: L.LatLngExpression = [hospital.latitude, hospital.longitude];
      bounds.push(position);
      const color = hospital.hasEmergencyDepartment ? '#18864b' : '#ed6c02';
      L.marker(position, {
        icon: L.divIcon({
          className: '',
          iconSize: [selected ? 38 : 32, selected ? 38 : 32],
          iconAnchor: [selected ? 19 : 16, selected ? 19 : 16],
          html: `<span style="display:flex;align-items:center;justify-content:center;width:100%;height:100%;border-radius:50%;background:${color};color:#fff;border:${selected ? 4 : 3}px solid #fff;box-shadow:0 3px 12px rgba(0,0,0,.35);font:700 13px Arial">${index + 1}</span>`,
        }),
        zIndexOffset: selected ? 500 : 100,
      }).bindPopup(popup(hospital.name, [hospital.address, formatDistance(hospital)])).addTo(map);
    });

    data.routes.forEach((route) => {
      const geometry: L.LatLngExpression[] = route.geometry.map((point) => [point.latitude, point.longitude]);
      geometry.forEach((position) => bounds.push(position));
      L.polyline(geometry, { color: '#ffffff', weight: 10, opacity: 0.88 }).addTo(map);
      L.polyline(geometry, { color: '#1565c0', weight: 6, opacity: 0.9 })
        .bindPopup(popup(`Rota de ${route.unitName}`, [
          `${formatMinutes(route.unitToVictimMinutes)} até a vítima`,
          `${formatMinutes(route.victimToHospitalMinutes)} até o hospital`,
        ]))
        .addTo(map);
    });

    if (bounds.length === 1) map.setView(bounds[0], 14);
    else map.fitBounds(L.latLngBounds(bounds), { padding: [32, 32], maxZoom: 15 });

    return () => {
      map.remove();
    };
  }, [data]);

  const selectedRoute = data?.routes[0];
  const selectedHospital = data?.hospitals.find((hospital) => hospital.id === data.selectedHospitalId);

  return (
    <Card sx={{ mt: compact ? 0 : 2, overflow: 'hidden', height: compact ? '100%' : 'auto' }}>
      <CardContent sx={{ height: compact ? '100%' : 'auto', boxSizing: 'border-box', overflow: compact ? 'auto' : 'visible' }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={2}>
          <Box>
            <Stack direction="row" alignItems="center" gap={1}>
              <Map color="primary" />
              <Typography variant="h5">Mapa operacional</Typography>
            </Stack>
            <Typography color="text.secondary" mt={0.5}>
              Ambulância → vítima → hospital selecionado
            </Typography>
          </Box>
          <Button startIcon={<Refresh />} onClick={() => void loadMap(data?.selectedHospitalId || undefined)} disabled={loading || routeLoading}>
            Atualizar mapa
          </Button>
        </Stack>

        {error && (
          <Alert severity="error" sx={{ mt: 2 }} action={<Button color="inherit" onClick={() => void loadMap()}>Tentar novamente</Button>}>
            {error}
          </Alert>
        )}

        {loading && !data ? (
          <Box display="flex" alignItems="center" justifyContent="center" gap={2} py={8}>
            <CircularProgress size={28} />
            <Typography>Localizando hospitais e calculando rotas...</Typography>
          </Box>
        ) : data ? (
          <>
            {data.warnings.map((warning) => <Alert key={warning} severity="warning" sx={{ mt: 2 }}>{warning}</Alert>)}
            <Box
              sx={{
                mt: 2,
                display: 'grid',
                gridTemplateColumns: compact ? { xs: '1fr', md: 'minmax(0, 1.45fr) minmax(310px, .75fr)' } : 'minmax(0, 1fr)',
                gap: 2,
              }}
            >
              <Box position="relative">
                <Box
                  ref={mapElementRef}
                  aria-label="Mapa da rota da ambulância, da vítima e dos hospitais próximos"
                  sx={{ height: compact ? { xs: 390, md: 'clamp(280px, calc(100dvh - 355px), 480px)' } : { xs: 390, md: 520 }, width: '100%', borderRadius: 3, overflow: 'hidden', bgcolor: 'action.hover', boxShadow: 'inset 0 0 0 1px rgba(0,0,0,.08)' }}
                />
                {routeLoading && (
                  <Box position="absolute" display="flex" alignItems="center" justifyContent="center" bgcolor="rgba(0,0,0,.28)" zIndex={1000} borderRadius={2} sx={{ inset: 0 }}>
                    <CircularProgress sx={{ color: 'white' }} />
                  </Box>
                )}
                <Stack direction="row" gap={1} flexWrap="wrap" mt={1}>
                  <Chip size="small" label="Vítima" sx={{ bgcolor: '#d32f2f', color: 'white' }} />
                  <Chip size="small" label="Ambulância" sx={{ bgcolor: '#1976d2', color: 'white' }} />
                  <Chip size="small" label="Hospital com emergência mapeada" sx={{ bgcolor: '#2e7d32', color: 'white' }} />
                  <Chip size="small" label="Emergência não informada" sx={{ bgcolor: '#ed6c02', color: 'white' }} />
                </Stack>
              </Box>

              <Box sx={{ bgcolor: 'background.default', borderRadius: 3, p: { xs: 1.5, md: 2 } }}>
                <Stack direction="row" alignItems="center" gap={1} mb={1}>
                  <LocalHospital color="success" />
                  <Typography variant="h6">Hospitais próximos</Typography>
                </Stack>
                <Typography variant="body2" color="text.secondary" mb={1.5}>
                  Emergência mapeada primeiro; depois, menor tempo de deslocamento.
                </Typography>
                {!data.hospitalSearchSucceeded ? (
                  <Alert severity="warning">
                    Não foi possível atualizar os hospitais agora. Tente novamente em instantes.
                  </Alert>
                ) : data.hospitals.length === 0 ? (
                  <Alert severity="info">Nenhum hospital foi localizado no raio de 15 km.</Alert>
                ) : (
                  <Box sx={{ display: 'grid', gridTemplateColumns: compact ? '1fr' : { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' }, gap: compact ? .75 : 1.25 }}>
                    {data.hospitals.map((hospital, index) => {
                      const selected = hospital.id === data.selectedHospitalId;
                      return (
                        <Button
                          key={hospital.id}
                          variant={selected ? 'contained' : 'text'}
                          color={selected ? 'primary' : 'inherit'}
                          disabled={routeLoading}
                          onClick={() => void loadMap(hospital.id)}
                          sx={{ justifyContent: 'flex-start', alignItems: 'stretch', textAlign: 'left', textTransform: 'none', px: compact ? 1 : 1.5, py: compact ? .65 : 1.25, borderRadius: 2, border: 1, borderColor: selected ? 'primary.main' : 'divider', bgcolor: selected ? 'primary.main' : 'background.paper' }}
                        >
                          <Box width="100%">
                            <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
                              <Typography fontWeight={700}>{index + 1}. {hospital.name}</Typography>
                              {index === 0 && <Chip size="small" color="success" label="Recomendado" />}
                              {hospital.hasEmergencyDepartment && index !== 0 && (
                                <Chip size="small" color="success" variant="outlined" label="Emergência mapeada" />
                              )}
                            </Stack>
                            <Typography variant="body2" color={selected ? 'inherit' : 'text.secondary'}>{hospital.address}</Typography>
                            <Typography variant="caption" display="block" mt={0.5}>
                              {hospital.estimatedMinutes !== null
                                ? `${hospital.estimatedMinutes.toLocaleString('pt-BR')} min • ${formatDistance(hospital)}`
                                : formatDistance(hospital)}
                            </Typography>
                          </Box>
                        </Button>
                      );
                    })}
                  </Box>
                )}
                {selectedHospital && (
                  <Alert
                    severity={data.hospitalSelectionConfirmed ? 'success' : 'warning'}
                    sx={{ mt: 2 }}
                    icon={data.hospitalSelectionConfirmed ? <CheckCircle /> : undefined}
                  >
                    {data.hospitalSelectionConfirmed
                      ? `${selectedHospital.name} confirmado como destino.`
                      : `${selectedHospital.name} é apenas uma prévia de rota. A equipe SAMU define o destino e a ala no local.`}
                  </Alert>
                )}
              </Box>
            </Box>

            {selectedRoute && (
              <Alert icon={<Route />} severity="info" sx={{ mt: 2 }}>
                <strong>{selectedRoute.unitName}:</strong>{' '}
                {formatMinutes(selectedRoute.unitToVictimMinutes)} até a vítima ({formatKm(selectedRoute.unitToVictimDistanceKm)})
                {' • '}{formatMinutes(selectedRoute.victimToHospitalMinutes)} da vítima ao hospital ({formatKm(selectedRoute.victimToHospitalDistanceKm)}).
              </Alert>
            )}
            <Alert severity="info" sx={{ mt: 2 }}>
              A reserva de vaga ocorre quando o SAMU confirma o hospital e a ala de destino.
            </Alert>
          </>
        ) : null}
      </CardContent>
    </Card>
  );
}
