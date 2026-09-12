import {
  createContext,
  useCallback,
  useState,
  type ReactNode,
} from 'react';
import type {
  OccurrenceFilters,
  OccurrenceList,
  OccurrencesContextProps,
} from '../interfaces';
import { getErrorMessage } from '../helpers';
import { listOccurrences } from '../services';

const OccurrencesContext = createContext<OccurrencesContextProps | undefined>(
  undefined,
);
export default OccurrencesContext;

export function OccurrencesProvider({ children }: { children: ReactNode }) {
  const [occurrences, setOccurrences] = useState<OccurrenceList[]>([]);
  const [pagination, setPagination] = useState({
    totalItems: 0,
    page: 1,
    pageSize: 10,
    totalPages: 0,
  });
  const [filters, setFilters] = useState<OccurrenceFilters>({
    status: '',
    search: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchOccurrences = useCallback(
    async (page: number, pageSize: number, currentFilters: OccurrenceFilters) => {
      setLoading(true);
      setError(null);
      try {
        const response = await listOccurrences(page, pageSize, currentFilters);
        setOccurrences(response.data);
        setPagination({
          totalItems: response.totalItems,
          page: response.page,
          pageSize: response.pageSize,
          totalPages: response.totalPages,
        });
      } catch (requestError) {
        setError(getErrorMessage(requestError));
      } finally {
        setLoading(false);
      }
    },
    [],
  );

  return (
    <OccurrencesContext.Provider
      value={{
        occurrences,
        pagination,
        filters,
        loading,
        error,
        fetchOccurrences,
        setPagination,
        setFilters,
      }}
    >
      {children}
    </OccurrencesContext.Provider>
  );
}
