import { useState, useEffect } from 'react';
import {
  TextField,
  Button,
  Box,
  Alert,
  FormControlLabel,
  Checkbox,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks';
import { getErrorMessage } from '../../helpers';
import type { LoginPayload } from '../../interfaces';

export default function LoginForm() {
  const [form, setForm] = useState<LoginPayload>({
    identifier: '',
    password: '',
  });

  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const navigate = useNavigate();
  const { handleLogin } = useAuth();

  useEffect(() => {
    const savedIdentifier = localStorage.getItem('identifier');
    if (savedIdentifier) {
      setForm((prev) => ({ ...prev, identifier: savedIdentifier }));
      setRememberMe(true);
    }
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRememberMe(e.target.checked);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const authenticatedUser = await handleLogin(form);

      if (rememberMe) {
        localStorage.setItem('identifier', form.identifier);
      } else {
        localStorage.removeItem('identifier');
      }

      navigate(authenticatedUser.hospitalId ? '/hospital-receptions' : '/dashboard');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit}
      sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
    >
      {error && <Alert severity="error">{error}</Alert>}

      <TextField
        label="Username ou Email"
        name="identifier"
        value={form.identifier}
        onChange={handleChange}
        required
      />

      <TextField
        label="Senha"
        type="password"
        name="password"
        value={form.password}
        onChange={handleChange}
        required
      />

      <FormControlLabel
        control={
          <Checkbox
            checked={rememberMe}
            onChange={handleCheckboxChange}
            color="primary"
          />
        }
        label="Lembre de mim 🎶"
      />

      <Button
        type="submit"
        variant="contained"
        color="primary"
        disabled={loading}
      >
        {loading ? 'Entrando...' : 'Entrar'}
      </Button>
    </Box>
  );
}
