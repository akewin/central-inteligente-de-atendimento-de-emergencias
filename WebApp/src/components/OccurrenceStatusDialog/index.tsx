import { useEffect, useState } from 'react';
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from '@mui/material';
import type { OccurrenceStatus } from '../../interfaces';

type Target = Extract<OccurrenceStatus, 'EM_ATENDIMENTO' | 'FINALIZADA' | 'CANCELADA'>;

interface Props {
  target: Target | null;
  loading: boolean;
  onClose: () => void;
  onConfirm: (reason?: string) => void;
}

const labels: Record<Target, string> = {
  EM_ATENDIMENTO: 'iniciar o atendimento', FINALIZADA: 'finalizar a ocorrência', CANCELADA: 'cancelar a ocorrência',
};

export default function OccurrenceStatusDialog({ target, loading, onClose, onConfirm }: Props) {
  const [reason, setReason] = useState('');
  useEffect(() => setReason(''), [target]);
  const invalidCancellation = target === 'CANCELADA' && reason.trim().length < 5;

  return (
    <Dialog open={target !== null} onClose={loading ? undefined : onClose} fullWidth maxWidth="sm">
      <DialogTitle>Confirmar alteração de status</DialogTitle>
      <DialogContent>
        {target && <Alert severity={target === 'CANCELADA' ? 'warning' : 'info'} sx={{ mb: 2 }}>Você confirma que deseja {labels[target]}?</Alert>}
        {target === 'CANCELADA' && (
          <TextField autoFocus fullWidth multiline minRows={3} label="Justificativa do cancelamento" value={reason}
            onChange={(event) => setReason(event.target.value)} error={reason.length > 0 && invalidCancellation}
            helperText="Obrigatória, entre 5 e 500 caracteres." inputProps={{ maxLength: 500 }} />
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Voltar</Button>
        <Button variant="contained" color={target === 'CANCELADA' ? 'error' : 'primary'}
          disabled={!target || loading || invalidCancellation} onClick={() => onConfirm(reason.trim() || undefined)}>
          {loading ? 'Registrando...' : 'Confirmar alteração'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
