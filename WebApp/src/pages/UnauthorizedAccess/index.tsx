import { useEffect, useState } from 'react';
import { Container, Typography, Button, Box } from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faHandSparkles,
  faHatWizard,
  faWandMagicSparkles,
} from '@fortawesome/free-solid-svg-icons';

export default function UnauthorizedAccess() {
  const navigate = useNavigate();
  const location = useLocation();
  const previousPath = location.state?.from?.pathname || '/';

  const [countdown, setCountdown] = useState(5);

  useEffect(() => {
    if (countdown <= 0) {
      navigate(previousPath, { replace: true });
      return;
    }

    const timer = setTimeout(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    return () => clearTimeout(timer);
  }, [countdown, navigate, previousPath]);

  const handleGoBack = () => {
    navigate(previousPath, { replace: true });
  };

  return (
    <Container
      sx={{
        mt: 10,
        textAlign: 'center',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
      }}
    >
      <FontAwesomeIcon
        icon={faHatWizard}
        size="4x"
        style={{ marginBottom: '20px', color: '#4b0082' }}
      />

      <Box display="flex" alignItems="center" justifyContent="center" gap={2}>
        <FontAwesomeIcon
          icon={faWandMagicSparkles}
          size="2x"
          style={{ color: '#6b4f2c' }}
        />
        <Typography variant="h3" fontWeight="bold">
          You shall not pass!
        </Typography>
        <FontAwesomeIcon
          icon={faHandSparkles}
          size="2x"
          style={{ color: '#555' }}
        />
      </Box>

      <Typography variant="body1" sx={{ mt: 3 }}>
        Redirecionando em {countdown} segundo{countdown !== 1 ? 's' : ''}...
      </Typography>

      <Button variant="contained" onClick={handleGoBack} sx={{ mt: 3 }}>
        Voltar
      </Button>
    </Container>
  );
}
