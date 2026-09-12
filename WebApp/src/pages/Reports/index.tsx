import { useState } from "react";
import { Box, Paper, Button } from "@mui/material";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDateFns } from "@mui/x-date-pickers/AdapterDateFns";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import {
  ActionsSelect,
  PageTitle,
  ReportsTable,
  UsersSelect,
} from "../../components";
import { ptBR } from "date-fns/locale";
import { cleanStates } from "../../helpers";
import { usePermissions } from "../../hooks";
import { UsersProvider } from "../../contexts";

export default function Reports() {
  const [filters, setFilters] = useState(cleanStates.logsReportFilters);
  const [error, setError] = useState<string>("");
  const { permissionsMap } = usePermissions();

  const today = new Date();

  const handleResetFilters = () => {
    setFilters(cleanStates.logsReportFilters);
    setError("");
  };

  const handleDateChange = (type: "start" | "end", value: Date | null) => {
    if (value) {
      const utcDate = new Date(
        Date.UTC(value.getFullYear(), value.getMonth(), value.getDate()),
      );
      const dateString = utcDate.toISOString().split("T")[0];
      setFilters((prev) => ({
        ...prev,
        [type === "start" ? "startDate" : "endDate"]: dateString,
      }));
    } else {
      setFilters((prev) => ({
        ...prev,
        [type === "start" ? "startDate" : "endDate"]: undefined,
      }));
    }
  };

  const handleUserChange = (userId: number | undefined) => {
    setFilters((prev) => ({
      ...prev,
      userId,
    }));
  };

  const handleActionChange = (action: string) => {
    setFilters((prev) => ({
      ...prev,
      action,
    }));
  };

  const startDate = filters.startDate
    ? new Date(filters.startDate + "T00:00:00")
    : null;
  const endDate = filters.endDate
    ? new Date(filters.endDate + "T00:00:00")
    : null;

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={ptBR}>
      <Box p={3}>
        <PageTitle icon={permissionsMap.REPORTS} title="Relatórios de Logs" />

        <Paper sx={{ p: 3, margin: "auto", mb: 4, maxWidth: 540 }}>
          <Box
            component="form"
            sx={{
              display: "flex",
              flexWrap: "wrap",
              gap: 2,
              alignItems: "flex-end",
            }}
          >
            <Box sx={{ maxWidth: "48%" }}>
              <DatePicker
                label="Data inicial"
                value={startDate}
                onChange={(date) => handleDateChange("start", date)}
                maxDate={today}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!error,
                  },
                }}
              />
            </Box>

            <Box sx={{ maxWidth: "48%" }}>
              <DatePicker
                label="Data final"
                value={endDate}
                onChange={(date) => handleDateChange("end", date)}
                minDate={startDate ?? undefined}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!error,
                  },
                }}
              />
            </Box>

            <UsersProvider>
              <Box sx={{ flex: { xs: "1 1 100%", sm: "1 1 65%" } }}>
                <UsersSelect
                  value={filters.userId}
                  onChange={handleUserChange}
                />
              </Box>
            </UsersProvider>

            <Box sx={{ flex: { xs: "1 1 100%", sm: "1 1 30%" } }}>
              <ActionsSelect
                value={filters.action}
                onChange={handleActionChange}
              />
            </Box>

            <Box
              sx={{
                flex: "1 1 100%",
                display: "flex",
                justifyContent: "flex-end",
              }}
            >
              <Button variant="contained" onClick={handleResetFilters}>
                Limpar filtros
              </Button>
            </Box>
          </Box>
        </Paper>

        <ReportsTable filters={filters} />
      </Box>
    </LocalizationProvider>
  );
}
