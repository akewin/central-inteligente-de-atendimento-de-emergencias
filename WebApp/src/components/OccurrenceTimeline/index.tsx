import { Alert, Box, CircularProgress, Paper, Stack, Typography } from '@mui/material';
import type { OccurrenceTimelineItem } from '../../interfaces';

interface Props { items: OccurrenceTimelineItem[]; loading: boolean; error: string | null; }

export default function OccurrenceTimeline({ items, loading, error }: Props) {
  return (
    <Paper sx={{ p: { xs: 2, sm: 3 }, mt: 3 }}>
      <Typography variant="h5" mb={2}>Histórico da ocorrência</Typography>
      {loading && <Box display="flex" gap={2}><CircularProgress size={24} /><Typography>Carregando histórico...</Typography></Box>}
      {error && <Alert severity="error">{error}</Alert>}
      {!loading && !error && items.length === 0 && <Alert severity="info">Nenhum evento registrado.</Alert>}
      <Stack spacing={0}>
        {items.map((item) => (
          <Box key={item.id} sx={{ borderLeft: 3, borderColor: 'primary.main', pl: 2, py: 1.25 }}>
            <Typography fontWeight={600}>{item.event}</Typography>
            {item.previousStatus && item.currentStatus && (
              <Typography variant="body2">{item.previousStatus.replaceAll('_', ' ')} → {item.currentStatus.replaceAll('_', ' ')}</Typography>
            )}
            {item.reason && <Typography variant="body2">Justificativa: {item.reason}</Typography>}
            <Typography variant="caption" color="text.secondary">
              {new Date(item.createdAt).toLocaleString('pt-BR')} · {item.generatedBy}
            </Typography>
          </Box>
        ))}
      </Stack>
    </Paper>
  );
}
