import React, { useEffect, useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
  Paper,
  TablePagination,
  IconButton,
  Typography,
  Box,
  TextField,
} from '@mui/material';
import { Edit, Delete, Search } from '@mui/icons-material';

import type { SystemResource } from '../../interfaces';
import { useSystemResources } from '../../hooks';
import NoResultsFound from '../NoResultsFound';

interface SystemResourcesTableProps {
  onEdit: (resource: SystemResource) => void;
  onDelete?: (id: number) => void;
}

export default function SystemResourcesTable({
  onEdit,
  onDelete,
}: SystemResourcesTableProps) {
  const {
    resources,
    pagination,
    loading,
    fetchSystemResources,
    setPagination,
  } = useSystemResources();
  const [searchKey, setSearchKey] = useState('');

  useEffect(() => {
    fetchSystemResources(pagination.page, pagination.pageSize, searchKey);
  }, [fetchSystemResources, pagination.page, pagination.pageSize, searchKey]);

  const handleChangePage = (_: unknown, newPage: number) => {
    setPagination((prev) => ({
      ...prev,
      page: newPage + 1,
    }));
  };

  const handleChangeRowsPerPage = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPagination((prev) => ({
      ...prev,
      page: 1,
      pageSize: parseInt(e.target.value, 10),
    }));
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPagination((prev) => ({
      ...prev,
      page: 1,
    }));
    fetchSystemResources(1, pagination.pageSize, searchKey);
  };

  return (
    <Paper sx={{ p: 2 }}>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={2}
      >
        <Typography variant="h6" mr={2}>
          Recursos do sistema
        </Typography>

        <form onSubmit={handleSearchSubmit}>
          <TextField
            size="small"
            variant="outlined"
            placeholder="Buscar recurso..."
            value={searchKey}
            onChange={(e) => setSearchKey(e.target.value)}
            slotProps={{
              input: {
                startAdornment: <Search />,
              },
            }}
          />
        </form>
      </Box>

      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Nome</TableCell>
              <TableCell>Nome de exibição</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={4} align="center">
                  Carregando...
                </TableCell>
              </TableRow>
            ) : resources.length > 0 ? (
              resources.map((resource: SystemResource) => (
                <TableRow key={resource.id} hover>
                  <TableCell>{resource.id}</TableCell>
                  <TableCell>{resource.name}</TableCell>
                  <TableCell>{resource.exhibitionName}</TableCell>
                  <TableCell align="right">
                    <IconButton
                      color="primary"
                      onClick={() => onEdit(resource)}
                      title="Editar recurso"
                    >
                      <Edit />
                    </IconButton>
                    <IconButton
                      color="error"
                      onClick={() => onDelete?.(resource.id!)}
                      title="Excluir recurso"
                    >
                      <Delete />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={4} align="center">
                  <NoResultsFound entity="recurso" />
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Box display="flex" justifyContent="flex-end">
        <TablePagination
          component="div"
          count={pagination.totalItems}
          page={pagination.page - 1}
          onPageChange={handleChangePage}
          rowsPerPage={pagination.pageSize}
          onRowsPerPageChange={handleChangeRowsPerPage}
          labelRowsPerPage="Itens por página:"
          rowsPerPageOptions={[5, 10, 25]}
        />
      </Box>
    </Paper>
  );
}
