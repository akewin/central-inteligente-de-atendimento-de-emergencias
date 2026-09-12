import {
  Alert, Box, Button, Card, CardContent, Chip, CircularProgress, FormControlLabel,
  Paper, Radio, Stack, Typography,
} from '@mui/material';
import { Refresh } from '@mui/icons-material';
import type { EmergencyServiceType, OccurrenceUnitRecommendations } from '../../interfaces';

interface Props {
  recommendations: OccurrenceUnitRecommendations | null;
  loading: boolean;
  error: string | null;
  onRefresh: () => void;
  selectedByService: Partial<Record<EmergencyServiceType, number>>;
  onSelect: (service: EmergencyServiceType, unitId: number) => void;
  onConfirm: () => void;
  confirming: boolean;
}

export default function UnitRecommendationsCard({
  recommendations, loading, error, onRefresh, selectedByService, onSelect, onConfirm, confirming,
}: Props) {
  const formatDistance = (distanceKm: number) => distanceKm.toLocaleString('pt-BR', {
    minimumFractionDigits: 1,
    maximumFractionDigits: 2,
  });
  const allServicesSelected = !!recommendations && recommendations.groups.length > 0
    && recommendations.groups.every((group) => group.hasAvailableUnits && !!selectedByService[group.service]);

  return (
    <Paper sx={{ p: { xs: 2, sm: 3 }, mt: 3 }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={2} mb={2}>
        <Box>
          <Typography variant="h5">Equipes recomendadas</Typography>
          <Typography color="text.secondary">
            Distância geográfica em linha reta. Não considera trânsito, rota ou ETA e não reserva equipes.
          </Typography>
        </Box>
        <Button startIcon={<Refresh />} onClick={onRefresh} disabled={loading}>
          Atualizar
        </Button>
      </Stack>

      {loading && <Box display="flex" alignItems="center" gap={2} py={3}><CircularProgress size={26} /><Typography>Localizando equipes disponíveis...</Typography></Box>}
      {error && <Alert severity="error" action={<Button color="inherit" onClick={onRefresh}>Tentar novamente</Button>}>{error}</Alert>}

      {!loading && !error && recommendations && (
        <Stack spacing={2}>
          {recommendations.groups.map((group) => (
            <Card key={group.service} variant="outlined">
              <CardContent>
                <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={1} mb={1.5}>
                  <Box>
                    <Typography variant="h6">{group.service} — {group.emergencyNumber}</Typography>
                    <Typography variant="body2" color="text.secondary">{group.displayName}</Typography>
                  </Box>
                  <Chip label={`${group.units.length} disponível${group.units.length === 1 ? '' : 'is'}`} color={group.hasAvailableUnits ? 'success' : 'default'} />
                </Stack>

                {!group.hasAvailableUnits ? (
                  <Alert severity="info">Nenhuma equipe disponível para este órgão no momento.</Alert>
                ) : group.units.map((unit) => (
                  <Box key={unit.id} sx={{ borderTop: 1, borderColor: 'divider', py: 1 }}>
                    <FormControlLabel
                      value={unit.id}
                      control={<Radio checked={selectedByService[group.service] === unit.id} disabled={confirming} />}
                      onChange={() => onSelect(group.service, unit.id)}
                      label={
                        <Box>
                          <Typography fontWeight={600}>{unit.name} · {formatDistance(unit.distanceKm)} km</Typography>
                          <Typography variant="body2" color="text.secondary">
                            {unit.address} · {unit.status}
                          </Typography>
                        </Box>
                      }
                    />
                  </Box>
                ))}
              </CardContent>
            </Card>
          ))}

          <Alert severity="warning">
            A seleção é provisória, existe apenas nesta tela e ainda não confirma nem cria despacho.
          </Alert>
          <Button
            variant="contained"
            color="warning"
            disabled={!allServicesSelected || confirming}
            onClick={onConfirm}
          >
            {confirming ? 'Registrando despacho...' : 'Revisar e confirmar despacho'}
          </Button>
        </Stack>
      )}
    </Paper>
  );
}
