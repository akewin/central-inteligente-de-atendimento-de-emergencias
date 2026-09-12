import { useState } from 'react';
import { Alert, Box, Button, Container, Typography } from '@mui/material';
import { Add, Groups } from '@mui/icons-material';
import { UnitForm, UnitsTable } from '../../components';
import { useAuth, useNotification, useUnits } from '../../hooks';
import { hasPermission } from '../../permissions/Rules';
import { PERMISSIONS } from '../../permissions';
import type { Unit, UnitPayload } from '../../interfaces';
import { getErrorMessage } from '../../helpers';

export default function Units() {
  const state = useUnits();
  const { authUser } = useAuth();
  const { showNotification } = useNotification();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Unit | null>(null);
  const [saving, setSaving] = useState(false);
  const canManage = !!authUser && hasPermission(authUser, PERMISSIONS.UNITS_MANAGE);

  const submit = async (payload: UnitPayload) => {
    setSaving(true);
    try {
      await state.save(payload, editing ?? undefined);
      showNotification(editing ? 'Equipe atualizada com sucesso.' : 'Equipe criada com sucesso.', 'success');
      setOpen(false);
      setEditing(null);
    } catch (err) {
      showNotification(getErrorMessage(err), 'error');
      if ((err as { response?: { status?: number } }).response?.status === 409) await state.refresh();
    } finally { setSaving(false); }
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box display="flex" flexDirection={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={2} mb={3}>
        <Box><Box display="flex" alignItems="center" gap={1}><Groups color="primary" /><Typography variant="h4">Gerenciamento de equipes</Typography></Box><Typography color="text.secondary">Equipes fictícias para uso exclusivo neste protótipo acadêmico.</Typography></Box>
        {canManage && <Button variant="contained" startIcon={<Add />} onClick={() => { setEditing(null); setOpen(true); }}>Nova equipe</Button>}
      </Box>
      {!canManage && <Alert severity="info" sx={{ mb: 2 }}>Modo de consulta: alterações exigem permissão administrativa.</Alert>}
      <UnitsTable {...state} canManage={canManage} page={state.pagination.page} pageSize={state.pagination.pageSize} totalItems={state.pagination.totalItems}
        onFiltersChange={(filters) => { state.setFilters(filters); state.setPagination((p) => ({ ...p, page: 1 })); }}
        onPageChange={(page) => state.setPagination((p) => ({ ...p, page }))}
        onPageSizeChange={(pageSize) => state.setPagination((p) => ({ ...p, page: 1, pageSize }))}
        onEdit={(unit) => { setEditing(unit); setOpen(true); }} />
      <UnitForm open={open} unit={editing} services={state.services} saving={saving} onClose={() => setOpen(false)} onSubmit={submit} />
    </Container>
  );
}
