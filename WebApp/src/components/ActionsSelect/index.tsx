import { TextField, MenuItem } from "@mui/material";

interface ActionOption {
  value: string;
  display: string;
}

interface ActionsSelectProps {
  value: string | undefined;
  onChange: (value: string) => void;
}

const actionOptions: ActionOption[] = [
  { value: "create", display: "Criação" },
  { value: "update", display: "Edição" },
  { value: "delete", display: "Exclusão" },
  { value: "login", display: "Login" },
  { value: "logout", display: "Logout" },
  { value: "senha", display: "Alteração de senha" },
];

export default function ActionsSelect({ value, onChange }: ActionsSelectProps) {
  return (
    <TextField
      label="Ação"
      select
      fullWidth
      value={value || ""}
      onChange={(e) => onChange(e.target.value)}
      slotProps={{
        select: { displayEmpty: true },
        inputLabel: { shrink: true },
      }}
    >
      <MenuItem value="">Todas as ações</MenuItem>
      {actionOptions.map((option) => (
        <MenuItem key={option.value} value={option.value}>
          {option.display}
        </MenuItem>
      ))}
    </TextField>
  );
}
