import { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import { Box, IconButton, Toolbar, Tooltip } from '@mui/material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBars } from '@fortawesome/free-solid-svg-icons';
import { SidePanel } from '../../components';
import { useThemeMode } from '../../hooks';
import { Bedtime, Sunny } from '@mui/icons-material';

export default function DefaultLayout() {
  const [open, setOpen] = useState(true);
  const { mode, toggleTheme } = useThemeMode();
  const navigate = useNavigate();

  const handleNavigate = (route: string) => {
    navigate(route);
  };

  return (
    <Box sx={{ display: 'flex' }}>
      <SidePanel open={open} onNavigate={handleNavigate} />

      <Box component="main" sx={{ flexGrow: 1 }}>
        <Toolbar
          sx={{
            justifyContent: 'space-between',
            position: 'sticky',
            top: 0,
            zIndex: 7,
            bgcolor: 'background.paper',
          }}
        >
          <IconButton onClick={() => setOpen(!open)}>
            <FontAwesomeIcon icon={faBars} />
          </IconButton>

          <Tooltip
            title={
              mode === 'light' ? 'Ativar modo escuro' : 'Ativar modo claro'
            }
            arrow
          >
            <IconButton onClick={toggleTheme} color="inherit">
              {mode === 'light' ? <Bedtime /> : <Sunny />}
            </IconButton>
          </Tooltip>
        </Toolbar>

        <Outlet />
      </Box>
    </Box>
  );
}
