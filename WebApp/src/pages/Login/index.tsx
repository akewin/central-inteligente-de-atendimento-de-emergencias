import { useState, useEffect, useRef } from 'react';
import { Box, Typography, Paper, Link, CircularProgress } from '@mui/material';
import { LoginForm, PasswordResetRequestModal } from '../../components';
import { useAuth, useNotification } from '../../hooks';
import { useLocation, useNavigate } from 'react-router-dom';
import { getErrorMessage } from '../../helpers';

export default function Login() {
  const { showNotification } = useNotification();
  const [openResetModal, setOpenResetModal] = useState(false);
  const [isCheckingToken, setIsCheckingToken] = useState(true);
  const [isExternalLoginInProgress, setIsExternalLoginInProgress] =
    useState(false);

  const externalLoginExecutedRef = useRef(false);

  const { token, authUser, handleExternalLogin } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);
    const urlToken = searchParams.get('token');

    async function processAuthentication() {
      try {
        if (urlToken && !externalLoginExecutedRef.current) {
          externalLoginExecutedRef.current = true;
          setIsExternalLoginInProgress(true);

          const authenticatedUser = await handleExternalLogin({ externalToken: urlToken });

          navigate(authenticatedUser.hospitalId ? '/hospital-receptions' : '/dashboard', { replace: true });
          return;
        }

        if (token) {
          navigate(authUser?.hospitalId ? '/hospital-receptions' : '/dashboard', { replace: true });
          return;
        }
      } catch (error) {
        showNotification(getErrorMessage(error), 'error');
      } finally {
        setIsCheckingToken(false);
      }
    }

    void processAuthentication();
  }, [location.search, handleExternalLogin, navigate, token, authUser, showNotification]);

  if (isExternalLoginInProgress || isCheckingToken) {
    return (
      <Box
        sx={{
          height: '100vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          gap: 2,
        }}
      >
        <Typography variant="h4">Redirecionando...</Typography>
        <CircularProgress />
      </Box>
    );
  }

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
        <Typography mb={2} variant="h5" align="center" gutterBottom>
          Faça login no sistema
        </Typography>

        <LoginForm />

        <Box mt={2} textAlign="center">
          <Link
            component="button"
            variant="body2"
            onClick={() => setOpenResetModal(true)}
          >
            Esqueci minha senha
          </Link>
        </Box>
      </Paper>

      <PasswordResetRequestModal
        open={openResetModal}
        onClose={() => setOpenResetModal(false)}
      />
    </Box>
  );
}
