import { Box, Chip, Divider, Paper, Typography } from '@mui/material';
import type { OccurrenceRead } from '../../interfaces';

export default function OccurrenceDetailsCard({ occurrence }: { occurrence: OccurrenceRead }) {
  const rows = [
    ['Status', occurrence.status.replaceAll('_', ' ')],
    ['Tipo confirmado', occurrence.confirmedType?.replaceAll('_', ' ') ?? 'Ainda não confirmado'],
    ['Prioridade confirmada', occurrence.confirmedPriority ?? 'Ainda não confirmada'],
    ['Localização', occurrence.locationDescription],
    ['Ponto de referência', occurrence.reference ?? 'Não informado'],
    ['Registrada por', `${occurrence.createdBy.fullName} (${occurrence.createdBy.username})`],
    ['Criada em', new Date(occurrence.createdAt).toLocaleString('pt-BR')],
    ['Atualizada em', new Date(occurrence.updatedAt).toLocaleString('pt-BR')],
  ];

  return (
    <Paper sx={{ p: { xs: 2, sm: 3 } }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" gap={2} mb={2}>
        <Typography variant="h5">Ocorrência #{occurrence.id}</Typography>
        <Chip color="primary" label={occurrence.status.replaceAll('_', ' ')} />
      </Box>
      <Typography variant="overline" color="text.secondary">Descrição informada pelo atendente</Typography>
      <Typography sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere', mb: 3 }}>
        {occurrence.description}
      </Typography>
      <Divider sx={{ mb: 2 }} />
      <Box display="grid" gridTemplateColumns={{ xs: '1fr', sm: '1fr 2fr' }} gap={1.5}>
        {rows.map(([label, value]) => (
          <Box key={label} sx={{ display: 'contents' }}>
            <Typography fontWeight={600}>{label}</Typography>
            <Typography>{value}</Typography>
          </Box>
        ))}
      </Box>
    </Paper>
  );
}
