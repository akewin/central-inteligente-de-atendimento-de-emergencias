import { useContext } from 'react';
import { PermissionsContext } from '../contexts';

export function usePermissions() {
  const context = useContext(PermissionsContext);
  if (context === undefined) {
    throw new Error(
      'usePermissions deve ser usado dentro de <PermissionsProvider>'
    );
  }
  return context;
}
