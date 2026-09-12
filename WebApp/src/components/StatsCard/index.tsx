import type { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Paper, Typography, Box } from '@mui/material';

interface StatsCardProps {
  background: string;
  icon: IconDefinition;
  content: string;
}

export default function StatsCard({
  background,
  icon,
  content,
}: StatsCardProps) {
  return (
    <Paper
      elevation={3}
      sx={{
        alignItems: 'center',
        background: `linear-gradient(135deg, ${background})`,
        color: 'white',
        display: 'flex',
        justifyContent: 'flex-start',
        p: 2,
        maxWidth: '30%',
        minWidth: '240px',
        width: '100%',
      }}
    >
      <Box sx={{ fontSize: '2rem', mr: 2 }}>
        <FontAwesomeIcon icon={icon} />
      </Box>
      <Box>
        <Typography>{content}</Typography>
      </Box>
    </Paper>
  );
}
