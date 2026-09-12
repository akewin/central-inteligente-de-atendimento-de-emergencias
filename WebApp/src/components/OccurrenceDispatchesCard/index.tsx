import { Alert, Card, CardContent, Chip, CircularProgress, Stack, Typography } from '@mui/material';
import type { OperationalDispatch } from '../../interfaces';

export default function OccurrenceDispatchesCard({ items, loading, error }: { items: OperationalDispatch[]; loading: boolean; error?: string | null }) {
  if (loading) return <Card sx={{ mt: 2 }}><CardContent><CircularProgress size={24} /></CardContent></Card>;
  if (error) return <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>;
  if (!items.length) return null;
  return <Card sx={{ mt: 2 }}><CardContent><Typography variant="h5" mb={2}>Andamento das equipes</Typography>
    <Stack gap={1}>{items.map(item => <Stack key={item.id} direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={1} p={1.5} border={1} borderColor="divider" borderRadius={1}>
      <div><Typography fontWeight={700}>{item.unitName} · {item.unitService}</Typography><Typography variant="body2" color="text.secondary">Atualizado em {new Date(item.updatedAt).toLocaleString('pt-BR')}</Typography></div>
      <Chip label={item.status.replaceAll('_', ' ')} color={item.status === 'CONCLUIDO' ? 'success' : item.status === 'CANCELADO' ? 'default' : 'primary'} />
    </Stack>)}</Stack>
  </CardContent></Card>;
}
