import { useContext } from 'react';
import { Snackbar, Alert, Slide } from '@mui/material';
import type { SlideProps } from '@mui/material';
import NotificationContext from '../../contexts/NotificationContext';

function SlideTransition(props: SlideProps) {
  return <Slide {...props} direction="left" />;
}

export default function SnackbarNotification() {
  const context = useContext(NotificationContext);
  const TOAST_DURATION = 3000;

  if (!context) return null;

  const { notification, handleClose } = context;

  return (
    <Snackbar
      open={notification.open}
      autoHideDuration={TOAST_DURATION}
      onClose={handleClose}
      anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      slots={{ transition: SlideTransition }}
    >
      <Alert
        onClose={handleClose}
        severity={notification.severity}
        sx={{ width: '100%' }}
      >
        {notification.message}
      </Alert>
    </Snackbar>
  );
}
