import type { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faSearch } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Box, Paper, Typography } from '@mui/material';

interface NoResultsFoundProps {
  icon?: IconDefinition;
  entity: string;
  isSearchResult?: boolean;
}

export default function NoResultsFound({
  icon = faSearch,
  entity,
  isSearchResult = true,
}: NoResultsFoundProps) {
  const message = `Nenhum ${entity} encontrado${
    isSearchResult ? ' nessa busca' : ''
  }.`;

  return (
    <Box
      sx={{ margin: 'auto', p: 2 }}
      component={isSearchResult ? 'div' : Paper}
    >
      <Typography
        sx={{
          alignItems: 'center',
          display: 'flex',
          fontSize: '1.3rem',
          gap: 2,
          justifyContent: 'center',
        }}
      >
        <FontAwesomeIcon icon={icon} />
        {message}
      </Typography>
    </Box>
  );
}
