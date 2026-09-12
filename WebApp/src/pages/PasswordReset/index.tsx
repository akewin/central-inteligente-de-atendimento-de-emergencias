import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  Alert,
} from '@mui/material';
import { useAuth } from '../../hooks';
import type { PasswordResetPayload } from '../../interfaces';

export default function PasswordReset() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const navigate = useNavigate();

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const { handlePasswordReset } = useAuth();

  const handleSubmit = async () => {
    setError(null);
    setSuccess(null);

    if (!newPassword || !confirmPassword) {
      setError('Informe ambos os campos.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('As senhas informadas não coincidem.');
      return;
    }

    if (!token) {
      setError('Token inválido ou expirado.');
      return;
    }

    setLoading(true);
    try {
      const payload: PasswordResetPayload = {
        token,
        newPassword,
      };
      await handlePasswordReset(payload);
      setSuccess('Senha redefinida com sucesso! Redirecionando...');
    } catch {
      setError('Erro ao redefinir a senha. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (success) {
      const timer = window.setTimeout(() => {
        navigate('/');
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [success, navigate]);

  return (
    <Box
      sx={{
        height: '100vh',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
      }}
    >
      <Paper
        elevation={3}
        sx={{
          padding: 4,
          width: '100%',
          maxWidth: 400,
          borderRadius: 2,
        }}
      >
        <Typography variant="h5" align="center" gutterBottom>
          Redefinir senha
        </Typography>

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
          {error && <Alert severity="error">{error}</Alert>}
          {success && <Alert severity="success">{success}</Alert>}

          <TextField
            label="Nova senha"
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            onPaste={(e) => e.preventDefault()}
            required
          />
          <TextField
            label="Confirme a nova senha"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            onPaste={(e) => e.preventDefault()}
            required
          />

          <Button variant="contained" onClick={handleSubmit} disabled={loading}>
            {loading ? 'Redefinindo...' : 'Redefinir senha'}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}
