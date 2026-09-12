import { useState, useEffect } from "react";
import { Box, TextField, Button, FormHelperText, MenuItem, Paper } from "@mui/material";
import type { Hospital, UserFormValues, UserRead } from "../../interfaces";
import { cleanStates } from "../../helpers";
import { useAuth } from "../../hooks";
import SystemResourceSelect from "../SystemResourcesSelect";
import { canEditPassword, canEditPermissions } from "../../permissions/Rules";
import { mapUserReadToFormValues } from "../../mappers";
import { SystemResourcesProvider } from "../../contexts";
import { listHospitals, listOperationalUnitOptions } from "../../services";

interface Props {
  onSubmit: (user: UserFormValues) => void;
  user?: UserRead;
}

export default function UserForm({ onSubmit, user }: Props) {
  const [form, setForm] = useState(cleanStates.userForm);

  const [error, setError] = useState("");
  const [unitOptions, setUnitOptions] = useState<Array<{ id: number; name: string; service: string }>>([]);
  const [hospitalOptions, setHospitalOptions] = useState<Hospital[]>([]);
  const { authUser } = useAuth();

  const showPasswordField = authUser && canEditPassword(authUser, user);
  const canEditPerms = authUser && canEditPermissions(authUser, user);

  useEffect(() => {
    if (!user) return;

    setForm(mapUserReadToFormValues(user));
  }, [user]);

  useEffect(() => {
    void listOperationalUnitOptions(user?.id).then(setUnitOptions).catch(() => setUnitOptions([]));
    void listHospitals().then(setHospitalOptions).catch(() => setHospitalOptions([]));
  }, [user?.id]);

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  function handlePermissionsChange(permissionIds: number[]) {
    setForm({ ...form, permissionIds });
    if (permissionIds.length > 0) setError("");
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (form.permissionIds.length === 0 && !form.unitId) {
      setError("É necessário conceder ao menos uma permissão.");
      return;
    }
    setError("");

    if (user && (!form.password || form.password.trim() === "")) {
      delete form.password;
    }
    onSubmit(form);
    setForm(cleanStates.userForm);
  }

  return (
    <Paper
      component="form"
      onSubmit={handleSubmit}
      sx={{
        alignItems: "center",
        display: "flex",
        flexWrap: "wrap",
        gap: 2,
        justifyContent: "center",
        marginBottom: 4,
        maxWidth: 800,
        padding: 2,
      }}
    >
      <TextField
        label="Nome Completo"
        name="fullName"
        value={form.fullName}
        onChange={handleChange}
        required
        fullWidth
      />

      <TextField
        label="Usuário"
        name="username"
        value={form.username}
        onChange={handleChange}
        required
        sx={{ flexGrow: 1 }}
      />

      <TextField
        label="E-mail"
        name="email"
        value={form.email}
        onChange={handleChange}
        required
        sx={{ flexGrow: 1 }}
      />

      {showPasswordField && (
        <TextField
          label="Senha"
          name="password"
          type="password"
          value={form.password}
          onChange={handleChange}
          required={!user}
          sx={{ flexGrow: 1 }}
        />
      )}

      <TextField
        select
        fullWidth
        label="Equipe operacional (opcional)"
        value={form.unitId ?? ''}
        onChange={(event) => setForm({ ...form, unitId: event.target.value === '' ? null : Number(event.target.value), hospitalId: null })}
        helperText="Ao vincular uma equipe, a permissão operacional é concedida automaticamente."
      >
        <MenuItem value="">Sem equipe (usuário administrativo)</MenuItem>
        {unitOptions.map((unit) => <MenuItem key={unit.id} value={unit.id}>{unit.name} — {unit.service}</MenuItem>)}
      </TextField>

      <TextField select fullWidth label="Hospital (opcional)" value={form.hospitalId ?? ''}
        onChange={(event) => setForm({ ...form, hospitalId: event.target.value === '' ? null : Number(event.target.value), unitId: null })}
        helperText="O vínculo concede automaticamente acesso ao painel de recebimentos.">
        <MenuItem value="">Sem hospital</MenuItem>{hospitalOptions.map(h => <MenuItem key={h.id} value={h.id}>{h.name}</MenuItem>)}
      </TextField>

      <SystemResourcesProvider>
        <Box sx={{ width: "100%" }}>
          <SystemResourceSelect
            value={form.permissionIds}
            onChange={handlePermissionsChange}
            readOnly={!canEditPerms}
          />
          {error && <FormHelperText error>{error}</FormHelperText>}
        </Box>
      </SystemResourcesProvider>

      <Box display="flex" width="100%" gap={2} justifyContent="center">
        <Button variant="contained" type="submit">
          {user ? "Atualizar" : "Cadastrar"}
        </Button>

        <Button
          variant="contained"
          color="secondary"
          onClick={() => setForm(cleanStates.userForm)}
        >
          Limpar
        </Button>
      </Box>
    </Paper>
  );
}
