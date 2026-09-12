import type { AlertColor } from '@mui/material';

export interface NotificationContextProps {
  showNotification: (message: string, severity: AlertColor) => void;
}
