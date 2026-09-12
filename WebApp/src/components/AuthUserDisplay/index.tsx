import { Avatar, Box, Typography } from '@mui/material';
import { useAuth } from '../../hooks';

interface AuthUserDisplayProps {
  collapsed?: boolean;
}

export default function AuthUserDisplay(collapsed: AuthUserDisplayProps) {
  const { authUser } = useAuth();
  const firstName = authUser?.fullName.split(' ')[0];

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 2,
        p: 2,
        borderBottom: '1px solid rgba(0,0,0,0.1)',
        opacity: collapsed.collapsed ? 0 : 1,
        transition: 'opacity 0.3s',
      }}
    >
      <Avatar sx={{ bgcolor: 'primary.main' }}>
        {firstName?.charAt(0).toUpperCase()}
      </Avatar>
      <Box>
        <Typography variant="subtitle1">Olá, {firstName}! 👋</Typography>
        <Typography variant="body2" color="text.secondary">
          {authUser?.username}
        </Typography>
      </Box>
    </Box>
  );
}
