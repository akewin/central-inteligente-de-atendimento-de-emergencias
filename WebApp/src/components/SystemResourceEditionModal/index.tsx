import { Modal, Box, Typography, Button } from '@mui/material';
import type { SystemResource } from '../../interfaces';
import SystemResourceForm from '../SystemResourceForm';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faClose } from '@fortawesome/free-solid-svg-icons';

interface Props {
  open: boolean;
  resource: SystemResource | null;
  onClose: () => void;
  onSubmit: (resource: SystemResource) => void;
}

export default function SystemResourceEditionModal({
  open,
  resource,
  onClose,
  onSubmit,
}: Props) {
  if (!resource) return null;

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
          Editar Recurso
        </Typography>

        <SystemResourceForm resource={resource} onSubmit={onSubmit} />
      </Box>
    </Modal>
  );
}
