import { useState } from 'react';
import { Container } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { OccurrenceForm, PageTitle } from '../../components';
import { useNotification, usePermissions } from '../../hooks';
import { createOccurrence } from '../../services';
import { getErrorMessage } from '../../helpers';
import type { OccurrenceCreatePayload } from '../../interfaces';

export default function NewOccurrence() {
  const navigate = useNavigate();
  const { showNotification } = useNotification();
  const { permissionsMap } = usePermissions();
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (payload: OccurrenceCreatePayload) => {
    setLoading(true);
    try {
      const occurrence = await createOccurrence(payload);
      showNotification('Ocorrência registrada com sucesso.', 'success');
      navigate(`/occurrences/${occurrence.id}`);
    } catch (error) {
      showNotification(getErrorMessage(error), 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="md" sx={{ py: 3 }}>
      <PageTitle icon={permissionsMap.OCCURRENCES} title="Nova ocorrência" />
      <OccurrenceForm loading={loading} onSubmit={handleSubmit} />
    </Container>
  );
}
