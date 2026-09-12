import { useState } from "react";
import { Container } from "@mui/material";
import {
  PageTitle,
  UserEditionModal,
  UserForm,
  UsersTable,
  ConfirmDialog,
} from "../../components";
import type { UserFormValues, UserRead } from "../../interfaces";
import { useUsers, useNotification } from "../../hooks";
import { usePermissions } from "../../hooks";
import { getErrorMessage } from "../../helpers";

export default function Users() {
  const { fetchUsers, fetchUserById, addUser, editUser, removeUser } =
    useUsers();
  const { showNotification } = useNotification();
  const { permissionsMap } = usePermissions();
  const [editingUser, setEditingUser] = useState<UserRead | null>(null);
  const [open, setOpen] = useState(false);
  const [confirmDialog, setConfirmDialog] = useState({
    open: false,
    id: 0,
  });

  async function handleCreate(user: UserFormValues) {
    try {
      await addUser(user);
      showNotification("Usuário cadastrado com sucesso!", "success");
      await fetchUsers();
    } catch (err) {
      showNotification(getErrorMessage(err), "error");
    }
  }

  async function handleUpdate(user: UserFormValues) {
    if (!editingUser) return;
    try {
      await editUser({ ...editingUser, ...user });
      showNotification("Usuário atualizado com sucesso!", "success");
      await fetchUsers();
    } catch (err) {
      showNotification(getErrorMessage(err), "error");
    } finally {
      setOpen(false);
    }
  }

  async function handleDelete(id: number) {
    setConfirmDialog({ open: true, id });
  }

  async function confirmDelete() {
    try {
      await removeUser(confirmDialog.id);
      showNotification("Usuário excluído com sucesso!", "success");
      await fetchUsers();
    } catch (err) {
      showNotification(getErrorMessage(err), "error");
    } finally {
      setConfirmDialog({ open: false, id: 0 });
    }
  }

  function cancelDelete() {
    setConfirmDialog({ open: false, id: 0 });
  }

  async function handleOpenEditionModal(userId: number) {
    const user = await fetchUserById(userId);
    if (!user) return;

    setEditingUser(user);
    setOpen(true);
  }

  return (
    <Container
      sx={{
        mt: 4,
        alignItems: "center",
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        textAlign: "center",
      }}
    >
      <PageTitle
        icon={permissionsMap.USERS}
        title="Gerenciamento de Usuários"
      />

      <UserForm onSubmit={handleCreate} />

      <UsersTable onEdit={handleOpenEditionModal} onDelete={handleDelete} />

      <UserEditionModal
        open={open}
        user={editingUser}
        onClose={() => setOpen(false)}
        onSubmit={handleUpdate}
      />

      <ConfirmDialog
        open={confirmDialog.open}
        title="Confirmar Exclusão"
        message="Tem certeza que deseja excluir este usuário? Esta ação não pode ser desfeita."
        onConfirm={confirmDelete}
        onCancel={cancelDelete}
      />
    </Container>
  );
}
