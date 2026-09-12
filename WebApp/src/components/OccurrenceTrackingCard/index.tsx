import { Alert, Button, Paper, Stack, Typography } from '@mui/material';
import type { OccurrenceStatus } from '../../interfaces';

type Target = Extract<OccurrenceStatus, 'EM_ATENDIMENTO' | 'FINALIZADA' | 'CANCELADA'>;

interface Props {
  status: OccurrenceStatus;
  loading: boolean;
  onAction: (target: Target) => void;
  onRefresh: () => void;
}

export default function OccurrenceTrackingCard({ status, loading, onAction, onRefresh }: Props) {
  const canStart = status === 'DESPACHADA';
  const canFinish = status === 'EM_ATENDIMENTO';
  const canCancel = ['ABERTA', 'AGUARDANDO_CONFIRMACAO', 'DESPACHADA', 'EM_ATENDIMENTO'].includes(status);
  const terminal = status === 'FINALIZADA' || status === 'CANCELADA';

  return (
    <Paper sx={{ p: { xs: 2, sm: 3 }, mt: 3 }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={2}>
        <div>
          <Typography variant="h5">Acompanhamento</Typography>
          <Typography color="text.secondary">Status atual: {status.replaceAll('_', ' ')}</Typography>
        </div>
        <Button onClick={onRefresh} disabled={loading}>Atualizar dados</Button>
      </Stack>
      {terminal && <Alert severity="success" sx={{ mt: 2 }}>Esta ocorrência está encerrada e não aceita novas transições.</Alert>}
      <Stack direction={{ xs: 'column', sm: 'row' }} gap={1.5} mt={2}>
        {canStart && <Button variant="contained" onClick={() => onAction('EM_ATENDIMENTO')} disabled={loading}>Iniciar atendimento</Button>}
        {canFinish && <Button variant="contained" color="success" onClick={() => onAction('FINALIZADA')} disabled={loading}>Finalizar ocorrência</Button>}
        {canCancel && <Button variant="outlined" color="error" onClick={() => onAction('CANCELADA')} disabled={loading}>Cancelar ocorrência</Button>}
      </Stack>
    </Paper>
  );
}
