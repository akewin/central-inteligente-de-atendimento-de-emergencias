import {
  Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, List, ListItem, ListItemText,
} from '@mui/material';
import type { UnitRecommendation } from '../../interfaces';

interface Props {
  open: boolean;
  units: UnitRecommendation[];
  loading: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}

export default function DispatchConfirmationDialog({ open, units, loading, onCancel, onConfirm }: Props) {
  return (
    <Dialog open={open} onClose={loading ? undefined : onCancel} fullWidth maxWidth="sm">
      <DialogTitle>Confirmar despacho de equipes</DialogTitle>
      <DialogContent>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Esta confirmação registrará o despacho, alterará a ocorrência para DESPACHADA e as equipes para DESLOCAMENTO.
        </Alert>
        <List dense>
          {units.map((unit) => (
            <ListItem key={unit.id} divider>
              <ListItemText primary={unit.name} secondary={`${unit.distanceKm.toFixed(3)} km · ${unit.status}`} />
            </ListItem>
          ))}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} disabled={loading}>Voltar e revisar</Button>
        <Button variant="contained" color="error" onClick={onConfirm} disabled={loading}>
          {loading ? 'Confirmando...' : 'Confirmar despacho'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
