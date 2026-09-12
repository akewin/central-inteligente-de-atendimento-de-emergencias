import { useContext } from 'react';
import { SystemResourcesContext } from '../contexts';

export function useSystemResources() {
  const context = useContext(SystemResourcesContext);

  if (!context) {
    throw new Error(
      'useSystemResources deve ser usado dentro de <SystemResourcesProvider>'
    );
  }

  return context;
}
