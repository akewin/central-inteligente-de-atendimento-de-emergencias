import { useEffect, useState } from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
  Paper,
  TablePagination,
  Typography,
  IconButton,
} from "@mui/material";
import { Visibility } from "@mui/icons-material";

import type { SystemLogFiltersPayload } from "../../interfaces";
import { useReports } from "../../hooks/useReports";
import LogDetailsModal from "../LogDetailsModal";
import NoResultsFound from "../NoResultsFound";

interface ReportsTableProps {
  filters: SystemLogFiltersPayload;
}

export default function ReportsTable({ filters }: ReportsTableProps) {
  const { logs, pagination, setPagination, setReportFilters } = useReports();
  const [selectedLogId, setSelectedLogId] = useState<number | null>(null);

  useEffect(() => {
    setReportFilters({
      ...filters,
      page: 1,
      pageSize: pagination.pageSize,
    });
  }, [filters, pagination.pageSize, setReportFilters]);

  return (
    <Paper sx={{ p: 2 }}>
      <Typography variant="h6" mb={2}>
        Logs do Sistema
      </Typography>

      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Gerado por</TableCell>
              <TableCell>Ação</TableCell>
              <TableCell>Data</TableCell>
              <TableCell align="right">Ações</TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {logs.length > 0 ? (
              logs.map((log) => (
                <TableRow key={log.id} hover>
                  <TableCell>{log.id}</TableCell>
                  <TableCell>{log.generatedBy}</TableCell>
                  <TableCell>{log.action}</TableCell>
                  <TableCell>
                    {new Date(log.createdAt).toLocaleString()}
                  </TableCell>
                  {log.action.includes("create") ||
                  log.action.includes("update") ? (
                    <TableCell align="right">
                      <IconButton
                        title="Ver detalhes"
                        onClick={() => setSelectedLogId(log.id)}
                      >
                        <Visibility />
                      </IconButton>
                    </TableCell>
                  ) : (
                    <TableCell />
                  )}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  <NoResultsFound entity="log" />
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={pagination.totalItems}
        page={pagination.page - 1}
        rowsPerPage={pagination.pageSize}
        rowsPerPageOptions={[5, 10, 25]}
        labelRowsPerPage="Itens por página:"
        onPageChange={(_, newPage) =>
          setPagination((prev) => ({ ...prev, page: newPage + 1 }))
        }
        onRowsPerPageChange={(e) =>
          setPagination({
            ...pagination,
            page: 1,
            pageSize: Number(e.target.value),
          })
        }
      />

      <LogDetailsModal
        logId={selectedLogId}
        open={!!selectedLogId}
        onClose={() => setSelectedLogId(null)}
      />
    </Paper>
  );
}
