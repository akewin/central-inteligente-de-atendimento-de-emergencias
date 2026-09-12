import { useCallback, useEffect, useState } from 'react';
import type { EmergencyService, Unit, UnitFilters, UnitPayload } from '../interfaces';
import { createUnit, listEmergencyServices, listUnits, updateUnit } from '../services';
import { getErrorMessage } from '../helpers';

const initialFilters: UnitFilters = { search: '', emergencyService: '', status: '' };

export function useUnits() {
  const [units, setUnits] = useState<Unit[]>([]);
  const [services, setServices] = useState<EmergencyService[]>([]);
  const [filters, setFilters] = useState<UnitFilters>(initialFilters);
  const [pagination, setPagination] = useState({ page: 1, pageSize: 10, totalItems: 0, totalPages: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [unitsResult, servicesResult] = await Promise.all([
        listUnits(pagination.page, pagination.pageSize, filters),
        listEmergencyServices(),
      ]);
      setUnits(unitsResult.data);
      setServices(servicesResult);
      setPagination((current) => ({ ...current, totalItems: unitsResult.totalItems, totalPages: unitsResult.totalPages }));
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [filters, pagination.page, pagination.pageSize]);

  useEffect(() => { void refresh(); }, [refresh]);

  const save = async (payload: UnitPayload, current?: Unit) => {
    if (current) await updateUnit(current.id, { ...payload, expectedUpdatedAt: current.updatedAt });
    else await createUnit(payload);
    await refresh();
  };

  return { units, services, filters, setFilters, pagination, setPagination, loading, error, refresh, save };
}
