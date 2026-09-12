import { Alert, List, ListItem, ListItemText, Paper, Typography } from '@mui/material';
import type { DispatchConfirmationRead } from '../../interfaces';

export default function DispatchResultCard({ result }: { result: DispatchConfirmationRead }) {
  return (
    <Paper sx={{ p: { xs: 2, sm: 3 }, mt: 3 }}>
      <Alert severity="success" sx={{ mb: 2 }}>Despacho registrado após confirmação do atendente.</Alert>
      <Typography variant="h5">Resultado do despacho</Typography>
      <Typography color="text.secondary">
        Confirmado por {result.confirmedBy.fullName} ({result.confirmedBy.username}) em{' '}
        {new Date(result.confirmedAt).toLocaleString('pt-BR')}.
      </Typography>
      <List>
        {result.dispatches.map((dispatch) => (
          <ListItem key={dispatch.id} divider>
            <ListItemText
              primary={`${dispatch.unitName} — ${dispatch.service}`}
              secondary={`Dispatch #${dispatch.id} · ${dispatch.unitStatus}`}
            />
          </ListItem>
        ))}
      </List>
    </Paper>
  );
}
