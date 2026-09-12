import {
  Alert, Box, Chip, CircularProgress, IconButton, MenuItem, Paper, Stack,
  Table, TableBody, TableCell, TableContainer, TableHead, TablePagination,
  TableRow, TextField, Typography,
} from '@mui/material';
import { Edit } from '@mui/icons-material';
import type { EmergencyService, Unit, UnitFilters, UnitStatus } from '../../interfaces';
import NoResultsFound from '../NoResultsFound';

interface Props {
  units: Unit[];
  services: EmergencyService[];
  filters: UnitFilters;
  onFiltersChange: (filters: UnitFilters) => void;
  loading: boolean;
  error: string | null;
  canManage: boolean;
  page: number;
  pageSize: number;
  totalItems: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  onEdit: (unit: Unit) => void;
}

const statuses: UnitStatus[] = ['DISPONIVEL', 'RESERVADA', 'DESLOCAMENTO', 'EM_ATENDIMENTO', 'INDISPONIVEL'];
const statusColor: Record<UnitStatus, 'success' | 'info' | 'warning' | 'default'> = {
  DISPONIVEL: 'success', RESERVADA: 'warning', DESLOCAMENTO: 'info', EM_ATENDIMENTO: 'warning', INDISPONIVEL: 'default',
};

export default function UnitsTable(props: Props) {
  const { units, services, filters, loading, error, canManage } = props;
  return (
    <Paper sx={{ width: '100%', p: { xs: 1.5, sm: 2 } }}>
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} mb={2}>
        <TextField size="small" label="Buscar por nome" value={filters.search} onChange={(e) => props.onFiltersChange({ ...filters, search: e.target.value })} sx={{ flex: 1 }} />
        <TextField select size="small" label="Serviço" value={filters.emergencyService} onChange={(e) => props.onFiltersChange({ ...filters, emergencyService: e.target.value as UnitFilters['emergencyService'] })} sx={{ minWidth: 190 }}>
          <MenuItem value="">Todos</MenuItem>{services.map((item) => <MenuItem key={item.id} value={item.type}>{item.type} — {item.emergencyNumber}</MenuItem>)}
        </TextField>
        <TextField select size="small" label="Status" value={filters.status} onChange={(e) => props.onFiltersChange({ ...filters, status: e.target.value as UnitFilters['status'] })} sx={{ minWidth: 190 }}>
          <MenuItem value="">Todos</MenuItem>{statuses.map((item) => <MenuItem key={item} value={item}>{item.replaceAll('_', ' ')}</MenuItem>)}
        </TextField>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      <TableContainer>
        <Table size="small">
          <TableHead><TableRow><TableCell>Equipe</TableCell><TableCell>Serviço</TableCell><TableCell>Status</TableCell><TableCell>Localização</TableCell>{canManage && <TableCell align="right">Ações</TableCell>}</TableRow></TableHead>
          <TableBody>
            {loading ? <TableRow><TableCell colSpan={canManage ? 5 : 4} align="center"><CircularProgress size={28} sx={{ my: 3 }} /><Typography>Carregando equipes...</Typography></TableCell></TableRow>
              : units.length === 0 ? <TableRow><TableCell colSpan={canManage ? 5 : 4}><NoResultsFound entity="equipe" /></TableCell></TableRow>
              : units.map((unit) => <TableRow key={unit.id} hover>
                <TableCell sx={{ fontWeight: 600 }}>{unit.name}</TableCell>
                <TableCell>{unit.service.type} ({unit.service.emergencyNumber})</TableCell>
                <TableCell><Chip size="small" label={unit.status.replaceAll('_', ' ')} color={statusColor[unit.status]} /></TableCell>
                <TableCell>{unit.address}</TableCell>
                {canManage && <TableCell align="right"><IconButton aria-label={`Editar ${unit.name}`} onClick={() => props.onEdit(unit)}><Edit /></IconButton></TableCell>}
              </TableRow>)}
          </TableBody>
        </Table>
      </TableContainer>
      <Box display="flex" justifyContent="flex-end">
        <TablePagination component="div" count={props.totalItems} page={props.page - 1} rowsPerPage={props.pageSize}
          onPageChange={(_, page) => props.onPageChange(page + 1)} onRowsPerPageChange={(e) => props.onPageSizeChange(Number(e.target.value))}
          labelRowsPerPage="Itens por página:" rowsPerPageOptions={[5, 10, 25]} />
      </Box>
    </Paper>
  );
}
