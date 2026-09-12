import { useEffect, useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Alert,
} from '@mui/material';
import { useAuth } from '../../hooks';

interface PasswordResetRequestModalProps {
  open: boolean;
  onClose: () => void;
}

export default function PasswordResetRequestModal({
  open,
  onClose,
}: PasswordResetRequestModalProps) {
  const [email, setEmail] = useState('');
  const [confirmEmail, setConfirmEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const { handlePasswordResetRequest } = useAuth();

  useEffect(() => {
    let timer: number;
    if (success) {
      timer = setTimeout(() => {
        (document.activeElement as HTMLElement)?.blur();
        onClose();
        setEmail('');
        setConfirmEmail('');
        setError(null);
        setSuccess(null);
      }, 3000);
    }
    return () => clearTimeout(timer);
  }, [success, onClose]);

  const handleSubmit = async () => {
    setError(null);
    setSuccess(null);

    if (email.trim() === '' || confirmEmail.trim() === '') {
      setError('Informe ambos os campos.');
      return;
    }

    if (email !== confirmEmail) {
      setError('Os e-mails informados não coincidem.');
      return;
    }

    setLoading(true);
    try {
      const message = await handlePasswordResetRequest(email);
      setSuccess(message);

      // Remove foco para evitar warning de aria-hidden
      (document.activeElement as HTMLElement)?.blur();
    } catch {
      setError('Erro ao enviar solicitação. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      open={open}
      onClose={() => {
        (document.activeElement as HTMLElement)?.blur(); // remove foco
        onClose();
      }}
      aria-labelledby="password-reset-title"
    >
      <DialogTitle id="password-reset-title">Redefinir senha</DialogTitle>

      <DialogContent
        sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}
      >
        {error && <Alert severity="error">{error}</Alert>}
        {success && <Alert severity="success">{success}</Alert>}

        <TextField
          label="E-mail"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          onPaste={(e) => e.preventDefault()}
          required
          autoFocus
        />
        <TextField
          label="Confirme seu e-mail"
          type="email"
          value={confirmEmail}
          onChange={(e) => setConfirmEmail(e.target.value)}
          onPaste={(e) => e.preventDefault()}
          required
        />
      </DialogContent>

      <DialogActions>
        <Button
          onClick={() => {
            (document.activeElement as HTMLElement)?.blur();
            onClose();
          }}
        >
          Cancelar
        </Button>
        <Button onClick={handleSubmit} disabled={loading} variant="contained">
          {loading ? 'Enviando...' : 'Enviar link'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
