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

import type { UserList } from '../../interfaces';
import { useUsers } from '../../hooks';
import NoResultsFound from '../NoResultsFound';

interface UsersTableProps {
  onEdit: (id: number) => void;
  onDelete?: (id: number) => void;
}

export default function UsersTable({ onEdit, onDelete }: UsersTableProps) {
  const { users, pagination, loading, fetchUsers, setPagination } = useUsers();
  const [searchKey, setSearchKey] = useState('');

  useEffect(() => {
    fetchUsers(pagination.page, pagination.pageSize, searchKey);
  }, [fetchUsers, pagination.page, pagination.pageSize, searchKey]);

  const handleChangePage = (_: unknown, newPage: number) => {
    setPagination((prev) => ({ ...prev, page: newPage + 1 }));
  };

  const handleChangeRowsPerPage = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPagination({
      ...pagination,
      page: 1,
      pageSize: parseInt(e.target.value, 10),
    });
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPagination((prev) => ({ ...prev, page: 1 }));
    fetchUsers(1, pagination.pageSize, searchKey);
  };

  return (
    <Paper sx={{ p: 2 }}>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={2}
      >
        <Typography variant="h6">Usuários cadastrados</Typography>

        <form onSubmit={handleSearchSubmit}>
          <TextField
            size="small"
            variant="outlined"
            placeholder="Buscar usuário..."
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
              <TableCell>Usuário</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Nome completo</TableCell>
              <TableCell>Ativo desde</TableCell>
              <TableCell sx={{ minWidth: 112 }} align="center">
                Ações
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  Carregando...
                </TableCell>
              </TableRow>
            ) : users.length > 0 ? (
              users.map((user: UserList) => (
                <TableRow key={user.id} hover>
                  <TableCell>{user.username}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>{user.fullName}</TableCell>
                  <TableCell>
                    {new Date(user.createdAt).toLocaleString()}
                  </TableCell>
                  <TableCell sx={{ minWidth: 112 }} align="center">
                    <IconButton
                      color="primary"
                      onClick={() => onEdit(user.id)}
                      title="Editar usuário"
                    >
                      <Edit />
                    </IconButton>
                    <IconButton
                      color="error"
                      onClick={() => onDelete?.(user.id)}
                      title="Excluir usuário"
                    >
                      <Delete />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <NoResultsFound entity="usuário" />
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
