import { useEffect, useMemo, useState } from 'react';
import { Autocomplete, CircularProgress, TextField, Box } from '@mui/material';
import { useUsers } from '../../hooks';
import type { UserOption } from '../../interfaces';

interface UsersSelectProps {
  value: number | undefined;
  onChange: (value: number | undefined) => void;
}

export default function UsersSelect({ value, onChange }: UsersSelectProps) {
  const { fetchUsersForSelect, loading } = useUsers();
  const [options, setOptions] = useState<UserOption[]>([]);

  useEffect(() => {
    async function loadOptions() {
      const data = await fetchUsersForSelect();
      setOptions(data);
    }
    loadOptions();
  }, [fetchUsersForSelect]);

  const selectedUser = useMemo(
    () => options.find((u) => u.id === value),
    [options, value]
  );

  if (loading && !options.length) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" p={2}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  return (
    <Autocomplete
      options={options}
      value={selectedUser || null}
      getOptionLabel={(option) => option.fullName}
      isOptionEqualToValue={(opt, val) => opt.id === val.id}
      onChange={(_, newValue) => onChange(newValue ? newValue.id! : undefined)}
      renderInput={(params) => (
        <TextField
          {...params}
          label="Usuário"
          placeholder="Buscar usuário..."
        />
      )}
      fullWidth
      loading={loading}
      loadingText="Carregando usuários..."
      noOptionsText="Nenhum usuário encontrado"
      slotProps={{ listbox: { style: { maxHeight: 300, overflow: 'auto' } } }}
    />
  );
}
