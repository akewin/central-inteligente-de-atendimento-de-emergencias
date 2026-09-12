import { useContext } from 'react';
import { ThemeContext } from '../contexts';

export const useThemeMode = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error(
      'useThemeMode deve ser usado dentro de <ThemeModeProvider>'
    );
  }
  return context;
};
