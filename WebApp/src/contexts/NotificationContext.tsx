import { createContext, useState, type ReactNode } from 'react';

import type { NotificationContextProps } from '../interfaces';

interface NotificationState {
  open: boolean;
  message: string;
  severity: 'success' | 'info' | 'warning' | 'error';
}

export interface NotificationContextPropsExtended
  extends NotificationContextProps {
  notification: NotificationState;
  handleClose: () => void;
}

const NotificationContext = createContext<
  NotificationContextPropsExtended | undefined
>(undefined);
export default NotificationContext;

export function NotificationProvider({ children }: { children: ReactNode }) {
  const [notification, setNotification] = useState<NotificationState>({
    open: false,
    message: '',
    severity: 'info',
  });

  const showNotification = (
    message: string,
    severity: 'success' | 'info' | 'warning' | 'error' = 'info'
  ) => {
    setNotification({
      open: true,
      message,
      severity,
    });
  };

  const handleClose = () => {
    setNotification((prev) => ({ ...prev, open: false }));
  };

  return (
    <NotificationContext.Provider
      value={{ showNotification, notification, handleClose }}
    >
      {children}
    </NotificationContext.Provider>
  );
}
