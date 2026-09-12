import { useContext } from 'react';
import { OccurrencesContext } from '../contexts';

export function useOccurrences() {
  const context = useContext(OccurrencesContext);
  if (!context) {
    throw new Error(
      'useOccurrences deve ser usado dentro de <OccurrencesProvider>',
    );
  }
  return context;
}
