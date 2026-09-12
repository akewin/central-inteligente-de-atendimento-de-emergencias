import { Outlet } from 'react-router-dom';
import { Box } from '@mui/material';

export default function CleanLayout() {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        p: 3,
      }}
    >
      <Outlet />
    </Box>
  );
}
