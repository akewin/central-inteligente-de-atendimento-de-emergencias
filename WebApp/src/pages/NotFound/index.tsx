import { useEffect, useState } from 'react';
import { Container, Typography, Button, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faCompass,
  faMapSigns,
  faMap,
} from '@fortawesome/free-solid-svg-icons';

export default function NotFound() {
  const navigate = useNavigate();
  const [countdown, setCountdown] = useState(5);

  useEffect(() => {
    if (countdown <= 0) {
      navigate('/', { replace: true });
      return;
    }

    const timer = setTimeout(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    return () => clearTimeout(timer);
  }, [countdown, navigate]);

  const handleGoHome = () => {
    navigate('/', { replace: true });
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
        icon={faMapSigns}
        size="4x"
        style={{ marginBottom: '20px', color: '#6b4f2c' }}
      />

      <Box display="flex" alignItems="center" justifyContent="center" gap={2}>
        <FontAwesomeIcon
          icon={faCompass}
          size="2x"
          style={{ color: '#9b853cff' }}
        />
        <Typography variant="h3" fontWeight="bold">
          Página não encontrada
        </Typography>
        <FontAwesomeIcon
          icon={faMap}
          size="2x"
          style={{ color: '#616161ff' }}
        />
      </Box>

      <Typography variant="body1" sx={{ mt: 3 }}>
        Redirecionando em {countdown} segundo
        {countdown !== 1 ? 's' : ''}...
      </Typography>

      <Button variant="contained" onClick={handleGoHome} sx={{ mt: 3 }}>
        Voltar
      </Button>
    </Container>
  );
}
