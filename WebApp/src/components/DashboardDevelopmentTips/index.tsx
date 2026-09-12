import { Box, Paper, Typography } from '@mui/material';
import { useAuth } from '../../hooks';

export default function DashboardDevelopmentTips() {
  const { authUser } = useAuth();
  const isRootUser = authUser?.permissions.map((p) => p.name).includes('root');

  if (!isRootUser) {
    return null;
  }

  return (
    <Box width="100%" display="flex" flexDirection="column" gap={2} mt={4}>
      <Typography>
        Caro Dev, utilize esse espaço para adicionar indicadores do sistema.
      </Typography>
      <Typography>Para isso, altere a resposta do endpoint:</Typography>
      <Paper
        elevation={12}
        sx={{ width: 'fit-content', padding: '8px' }}
        variant="outlined"
      >
        /api/stats
      </Paper>
      <Typography>editando o service em:</Typography>
      <Paper
        elevation={12}
        sx={{ width: 'fit-content', padding: '8px' }}
        variant="outlined"
      >
        Api/Services/SystemStatsServices/GetSystemStats.cs
      </Paper>
      <Typography>e o DTO em:</Typography>
      <Paper
        elevation={12}
        sx={{ width: 'fit-content', padding: '8px' }}
        variant="outlined"
      >
        Api/Dtos/SystemStatsDtos
      </Paper>
    </Box>
  );
}
