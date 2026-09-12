import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Paper,
  Stack,
  Typography,
} from '@mui/material';
import type { EmergencyServiceType, OccurrenceRead } from '../../interfaces';
import { confirmOccurrenceServices } from '../../services';
import { getErrorMessage } from '../../helpers';

const services: EmergencyServiceType[] = ['POLICIA', 'SAMU', 'BOMBEIROS'];

export default function ServiceConfirmationCard({
  occurrence,
  onConfirmed,
}: {
  occurrence: OccurrenceRead;
  onConfirmed: () => Promise<void>;
}) {
  const [selected, setSelected] = useState<EmergencyServiceType[]>([]);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setSelected(occurrence.latestAIAnalysis?.recommendedServices ?? []);
  }, [occurrence.latestAIAnalysis]);

  if (occurrence.serviceConfirmation) {
    const confirmation = occurrence.serviceConfirmation;
    return (
      <Paper sx={{ mt: 3, p: { xs: 2, sm: 3 }, borderLeft: 5, borderColor: 'success.main' }}>
        <Typography variant="h6" gutterBottom>Confirmado pelo atendente</Typography>
        <Alert severity="success" sx={{ mb: 2 }}>
          Esta é a decisão humana registrada. Nenhuma equipe foi despachada nesta etapa.
        </Alert>
        <Stack direction="row" gap={1} flexWrap="wrap" mb={2}>
          {confirmation.confirmedServices.map((service) => (
            <Chip key={service} color="success" label={service} />
          ))}
        </Stack>
        <Typography>
          Confirmado por {confirmation.confirmedBy.fullName} ({confirmation.confirmedBy.username}) em{' '}
          {new Date(confirmation.confirmedAt).toLocaleString('pt-BR')}.
        </Typography>
      </Paper>
    );
  }

  if (!occurrence.latestAIAnalysis) return null;

  const toggleService = (service: EmergencyServiceType) => {
    setSelected((current) => current.includes(service)
      ? current.filter((item) => item !== service)
      : [...current, service]);
    setError(null);
  };

  const confirm = async () => {
    if (selected.length === 0) {
      setError('Selecione pelo menos um órgão.');
      setDialogOpen(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      await confirmOccurrenceServices(occurrence.id, selected);
      setDialogOpen(false);
      await onConfirmed();
    } catch (requestError) {
      setDialogOpen(false);
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper sx={{ mt: 3, p: { xs: 2, sm: 3 }, borderLeft: 5, borderColor: 'info.main' }}>
      <Typography variant="h6">Decisão do atendente</Typography>
      <Typography color="text.secondary" sx={{ mb: 1 }}>
        Revise a recomendação da IA e marque os órgãos que você considera necessários.
      </Typography>
      <Box display="flex" flexDirection={{ xs: 'column', sm: 'row' }} gap={{ sm: 2 }}>
        {services.map((service) => (
          <FormControlLabel
            key={service}
            control={(
              <Checkbox
                checked={selected.includes(service)}
                onChange={() => toggleService(service)}
              />
            )}
            label={service}
          />
        ))}
      </Box>
      {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
      <Button
        variant="contained"
        disabled={selected.length === 0 || loading}
        onClick={() => setDialogOpen(true)}
      >
        Revisar e confirmar órgãos
      </Button>

      <Dialog open={dialogOpen} onClose={() => !loading && setDialogOpen(false)}>
        <DialogTitle>Confirmar decisão do atendente?</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            Esta confirmação é uma decisão humana e ainda não despacha nenhuma equipe.
          </Alert>
          <Typography gutterBottom>Órgãos selecionados:</Typography>
          <Stack direction="row" gap={1} flexWrap="wrap">
            {selected.map((service) => <Chip key={service} label={service} />)}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button disabled={loading} onClick={() => setDialogOpen(false)}>Cancelar</Button>
          <Button variant="contained" disabled={loading} onClick={confirm}>
            {loading ? <CircularProgress size={22} /> : 'Confirmar órgãos'}
          </Button>
        </DialogActions>
      </Dialog>
    </Paper>
  );
}
