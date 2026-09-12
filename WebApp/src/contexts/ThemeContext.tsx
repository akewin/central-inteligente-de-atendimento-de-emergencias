import { createContext, useMemo, useState, type ReactNode } from 'react';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { defineTheme } from '../theme';
import type { ThemeContextProps } from '../interfaces';

const ThemeContext = createContext<ThemeContextProps | undefined>(undefined);

export function ThemeModeProvider({ children }: { children: ReactNode }) {
  const storedMode =
    (localStorage.getItem('themeMode') as 'light' | 'dark') || 'light';
  const [mode, setMode] = useState<'light' | 'dark'>(storedMode);

  const toggleTheme = () => {
    setMode((prev) => {
      const newMode = prev === 'light' ? 'dark' : 'light';
      localStorage.setItem('themeMode', newMode);
      return newMode;
    });
  };

  const theme = useMemo(() => defineTheme(mode), [mode]);

  return (
    <ThemeContext.Provider value={{ mode, toggleTheme }}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ThemeContext.Provider>
  );
}

export default ThemeContext;
