import { Box, Paper, Typography } from '@mui/material';

interface JsonWrapperProps {
  title: string;
  jsonContent: Record<string, unknown>;
}

export default function JsonWrapper({ title, jsonContent }: JsonWrapperProps) {
  return (
    <Box flexGrow={1}>
      <Typography variant="subtitle1" mb={1}>
        {title}
      </Typography>

      <Paper
        elevation={7}
        sx={{
          backgroundColor: '#cececeff',
          color: 'black',
          fontFamily: 'monospace',
          fontSize: '0.8rem',
          overflow: 'auto',
          p: 2,
          whiteSpace: 'pre',
          maxHeight: 360,
        }}
      >
        {JSON.stringify(jsonContent, null, 2)}
      </Paper>
    </Box>
  );
}
