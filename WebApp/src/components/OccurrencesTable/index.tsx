import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  IconButton,
  MenuItem,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { Search, Visibility } from '@mui/icons-material';
import { useOccurrences } from '../../hooks';
import type { OccurrenceStatus } from '../../interfaces';
import NoResultsFound from '../NoResultsFound';

const statuses: Array<{ value: OccurrenceStatus | ''; label: string }> = [
  { value: '', label: 'Todos os status' },
  { value: 'ABERTA', label: 'Aberta' },
  { value: 'EM_ANALISE', label: 'Em análise' },
  { value: 'AGUARDANDO_CONFIRMACAO', label: 'Aguardando confirmação' },
  { value: 'DESPACHADA', label: 'Despachada' },
  { value: 'EM_ATENDIMENTO', label: 'Em atendimento' },
  { value: 'FINALIZADA', label: 'Finalizada' },
  { value: 'CANCELADA', label: 'Cancelada' },
];

export default function OccurrencesTable() {
  const navigate = useNavigate();
  const {
    occurrences,
    pagination,
    filters,
    loading,
    error,
    fetchOccurrences,
    setPagination,
    setFilters,
  } = useOccurrences();
  const [search, setSearch] = useState(filters.search ?? '');

  useEffect(() => {
    fetchOccurrences(pagination.page, pagination.pageSize, filters);
  }, [fetchOccurrences, filters, pagination.page, pagination.pageSize]);

  const applySearch = (event: React.FormEvent) => {
    event.preventDefault();
    setPagination((current) => ({ ...current, page: 1 }));
    setFilters((current) => ({ ...current, search: search.trim() }));
  };

  return (
    <Paper sx={{ p: { xs: 1, sm: 2 } }}>
      <Box
        display="flex"
        flexDirection={{ xs: 'column', md: 'row' }}
        justifyContent="space-between"
        gap={2}
        mb={2}
      >
        <Typography variant="h6">Ocorrências registradas</Typography>
        <Box component="form" onSubmit={applySearch} display="flex" gap={1} flexWrap="wrap">
          <TextField
            select
            size="small"
            label="Status"
            value={filters.status ?? ''}
            onChange={(event) => {
              setPagination((current) => ({ ...current, page: 1 }));
              setFilters((current) => ({
                ...current,
                status: event.target.value as OccurrenceStatus | '',
              }));
            }}
            sx={{ minWidth: 190 }}
          >
            {statuses.map((status) => (
              <MenuItem key={status.value || 'all'} value={status.value}>
                {status.label}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            size="small"
            placeholder="Buscar descrição ou local"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            inputProps={{ maxLength: 100 }}
          />
          <Button type="submit" variant="outlined" startIcon={<Search />}>
            Buscar
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size="small"
              onClick={() => fetchOccurrences(pagination.page, pagination.pageSize, filters)}
            >
              Tentar novamente
            </Button>
          }
          sx={{ mb: 2 }}
        >
          {error}
        </Alert>
      )}

      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Descrição</TableCell>
              <TableCell>Localização</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Criada por</TableCell>
              <TableCell>Data</TableCell>
              <TableCell align="center">Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <CircularProgress size={28} />
                </TableCell>
              </TableRow>
            ) : occurrences.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7}>
                  <NoResultsFound entity="ocorrência" />
                </TableCell>
              </TableRow>
            ) : (
              occurrences.map((occurrence) => (
                <TableRow key={occurrence.id} hover>
                  <TableCell>{occurrence.id}</TableCell>
                  <TableCell sx={{ minWidth: 240 }}>{occurrence.descriptionSummary}</TableCell>
                  <TableCell sx={{ minWidth: 180 }}>{occurrence.locationDescription}</TableCell>
                  <TableCell><Chip size="small" label={occurrence.status.replaceAll('_', ' ')} /></TableCell>
                  <TableCell>{occurrence.createdBy.fullName}</TableCell>
                  <TableCell>{new Date(occurrence.createdAt).toLocaleString('pt-BR')}</TableCell>
                  <TableCell align="center">
                    <IconButton
                      color="primary"
                      title="Ver detalhes"
                      onClick={() => navigate(`/occurrences/${occurrence.id}`)}
                    >
                      <Visibility />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={pagination.totalItems}
        page={pagination.page - 1}
        rowsPerPage={pagination.pageSize}
        rowsPerPageOptions={[5, 10, 25, 50]}
        labelRowsPerPage="Itens por página:"
        onPageChange={(_, page) =>
          setPagination((current) => ({ ...current, page: page + 1 }))
        }
        onRowsPerPageChange={(event) =>
          setPagination((current) => ({
            ...current,
            page: 1,
            pageSize: Number(event.target.value),
          }))
        }
      />
    </Paper>
  );
}
