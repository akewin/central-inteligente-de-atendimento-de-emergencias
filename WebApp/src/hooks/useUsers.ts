import { useContext } from 'react';
import { UsersContext } from '../contexts';

export function useUsers() {
  const context = useContext(UsersContext);

  if (!context) {
    throw new Error('useUsers deve ser usado dentro de <UsersProvider>');
  }

  return context;
}
