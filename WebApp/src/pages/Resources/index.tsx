import { useState } from 'react';
import { Container } from '@mui/material';
import type { SystemResource } from '../../interfaces';
import {
  useNotification,
  usePermissions,
  useSystemResources,
} from '../../hooks';
import {
  SystemResourceForm,
  SystemResourcesTable,
  SystemResourceEditionModal,
  ConfirmDialog,
  PageTitle,
} from '../../components';
import { getErrorMessage } from '../../helpers';

export default function Resources() {
  const {
    fetchSystemResources,
    addSystemResource,
    editSystemResource,
    removeSystemResource,
    pagination,
  } = useSystemResources();
  const { showNotification } = useNotification();
  const { permissionsMap } = usePermissions();

  const [editingResource, setEditingResource] = useState<SystemResource | null>(
    null
  );
  const [open, setOpen] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState({
    open: false,
    id: 0,
  });

  async function handleCreate(resource: SystemResource) {
    try {
      await addSystemResource(resource);
      showNotification('Recurso criado com sucesso!', 'success');
      fetchSystemResources(pagination.page, pagination.pageSize); // atualiza tabela
    } catch (err) {
      console.error(err);
      showNotification(getErrorMessage(err), 'error');
    }
  }

  async function handleUpdate(resource: SystemResource) {
    if (!editingResource) return;
    try {
      await editSystemResource({ ...editingResource, ...resource });
      showNotification('Recurso atualizado com sucesso!', 'success');
      fetchSystemResources(pagination.page, pagination.pageSize);
    } catch (err) {
      console.error(err);
      showNotification(getErrorMessage(err), 'error');
    } finally {
      setOpen(false);
    }
  }

  async function handleDelete(id: number) {
    setConfirmDialog({ open: true, id });
  }

  async function confirmDelete() {
    try {
      await removeSystemResource(confirmDialog.id.toString());
      showNotification('Recurso excluído com sucesso!', 'success');
      fetchSystemResources(pagination.page, pagination.pageSize);
    } catch (err) {
      console.error(err);
      showNotification(getErrorMessage(err), 'error');
    } finally {
      setConfirmDialog({ open: false, id: 0 });
    }
  }

  function cancelDelete() {
    setConfirmDialog({ open: false, id: 0 });
  }

  function handleOpenEditionModal(resource: SystemResource) {
    setEditingResource(resource);
    setOpen(true);
  }

  return (
    <Container
      sx={{
        mt: 4,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
      }}
    >
      <PageTitle
        icon={permissionsMap.RESOURCES}
        title="Gerenciamento de Recursos"
      />

      <SystemResourceForm onSubmit={handleCreate} />

      <SystemResourcesTable
        onEdit={handleOpenEditionModal}
        onDelete={handleDelete}
      />

      <SystemResourceEditionModal
        open={open}
        resource={editingResource}
        onClose={() => setOpen(false)}
        onSubmit={handleUpdate}
      />

      <ConfirmDialog
        open={confirmDialog.open}
        title="Confirmar Exclusão"
        message="Tem certeza que deseja excluir este Recurso? Esta ação não pode ser desfeita."
        onConfirm={confirmDelete}
        onCancel={cancelDelete}
      />
    </Container>
  );
}
