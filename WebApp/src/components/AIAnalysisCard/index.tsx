import { Alert, Box, Chip, Paper, Stack, Typography } from '@mui/material';
import type { AIAnalysisRead } from '../../interfaces';

export default function AIAnalysisCard({ analysis }: { analysis: AIAnalysisRead }) {
  return (
    <Paper sx={{ mt: 3, p: { xs: 2, sm: 3 } }}>
      <Typography variant="h6" gutterBottom>Recomendação da IA</Typography>
      <Alert severity="warning" sx={{ mb: 2 }}>
        Esta análise é apenas uma recomendação. A decisão final pertence ao atendente.
      </Alert>
      <Box display="grid" gridTemplateColumns={{ xs: '1fr', sm: '1fr 2fr' }} gap={1.5}>
        <Typography fontWeight={600}>Tipo recomendado</Typography>
        <Typography>{analysis.recommendedType.replaceAll('_', ' ')}</Typography>
        <Typography fontWeight={600}>Prioridade recomendada</Typography>
        <Typography>{analysis.recommendedPriority}</Typography>
        <Typography fontWeight={600}>Órgãos recomendados</Typography>
        <Stack direction="row" gap={1} flexWrap="wrap">
          {analysis.recommendedServices.map((service) => (
            <Chip key={service} size="small" label={service} />
          ))}
        </Stack>
        <Typography fontWeight={600}>Justificativa</Typography>
        <Typography sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>
          {analysis.reason}
        </Typography>
        <Typography fontWeight={600}>Referência técnica</Typography>
        <Typography>{analysis.provider} · {analysis.model}</Typography>
        <Typography fontWeight={600}>Analisada em</Typography>
        <Typography>{new Date(analysis.createdAt).toLocaleString('pt-BR')}</Typography>
      </Box>
    </Paper>
  );
}
