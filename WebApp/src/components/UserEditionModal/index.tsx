import { Modal, Box, Typography, Button } from '@mui/material';
import type { UserRead, UserFormValues } from '../../interfaces';
import UserForm from '../UserForm';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faClose } from '@fortawesome/free-solid-svg-icons';

interface Props {
  open: boolean;
  user: UserRead | null;
  onClose: () => void;
  onSubmit: (data: UserFormValues) => void;
}

export default function UserEditionModal({
  open,
  user,
  onClose,
  onSubmit,
}: Props) {
  if (!user) return null;

  return (
    <Modal open={open} onClose={onClose}>
      <Box
        sx={{
          bgcolor: 'background.paper',
          p: 4,
          borderRadius: 2,
          m: 'auto',
          mt: 'auto',
          width: 400,
        }}
      >
        <Box display="flex" justifyContent="flex-end">
          <Button onClick={onClose}>
            <FontAwesomeIcon icon={faClose} />
          </Button>
        </Box>

        <Typography variant="h6" gutterBottom>
          Editar Usuário
        </Typography>

        <UserForm user={user} onSubmit={onSubmit} />
      </Box>
    </Modal>
  );
}
