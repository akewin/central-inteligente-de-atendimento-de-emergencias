import { Box, Button, Container } from '@mui/material';
import { Add } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { OccurrencesTable, PageTitle } from '../../components';
import { usePermissions } from '../../hooks';

export default function Occurrences() {
  const navigate = useNavigate();
  const { permissionsMap } = usePermissions();
  return (
    <Container maxWidth="xl" sx={{ py: 3 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" gap={2} flexWrap="wrap" mb={2}>
        <PageTitle icon={permissionsMap.OCCURRENCES} title="Ocorrências" />
        <Button variant="contained" startIcon={<Add />} onClick={() => navigate('/occurrences/new')}>
          Nova ocorrência
        </Button>
      </Box>
      <OccurrencesTable />
    </Container>
  );
}
