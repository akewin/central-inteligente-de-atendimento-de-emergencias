import type { Dispatch, SetStateAction } from 'react';
import type {
  OccurrenceFilters,
  OccurrenceList,
  OccurrencesPagination,
} from '../index';

export interface OccurrencesContextProps {
  occurrences: OccurrenceList[];
  pagination: Omit<OccurrencesPagination, 'data'>;
  filters: OccurrenceFilters;
  loading: boolean;
  error: string | null;
  fetchOccurrences: (
    page: number,
    pageSize: number,
    filters: OccurrenceFilters,
  ) => Promise<void>;
  setPagination: Dispatch<
    SetStateAction<Omit<OccurrencesPagination, 'data'>>
  >;
  setFilters: Dispatch<SetStateAction<OccurrenceFilters>>;
}
