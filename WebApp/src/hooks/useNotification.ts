import { useContext } from 'react';
import { NotificationContext } from '../contexts';

export function useNotification() {
  const context = useContext(NotificationContext);

  if (!context) {
    throw new Error(
      'useNotification deve ser usado dentro de <NotificationProvider>'
    );
  }

  return context;
}
