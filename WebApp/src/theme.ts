import { createTheme } from '@mui/material/styles';

/**
 * 🎨 Paletas de cores inspiradas no projeto Catppuccin
 * https://github.com/catppuccin/catppuccin
 */

export const defineTheme = (mode: 'light' | 'dark') =>
  createTheme({
    palette: {
      mode,
      ...(mode === 'light'
        ? {
            // 🌤️ Modo claro (inspirado em Catppuccin Latte)
            primary: {
              main: '#8839ef',
              light: '#9d5ef3',
              dark: '#6c2bd9',
              contrastText: '#ffffff',
            },
            secondary: {
              main: '#179299',
              light: '#2fb9c0',
              dark: '#117377',
              contrastText: '#ffffff',
            },
            success: {
              main: '#40a02b',
              light: '#6abf5b',
              dark: '#2c7a1f',
            },
            error: {
              main: '#d20f39',
              light: '#e34b63',
              dark: '#a80c2d',
            },
            warning: {
              main: '#fe640b',
              light: '#ff8a3d',
              dark: '#c94f08',
            },
            info: {
              main: '#1e66f5',
              light: '#4c86f7',
              dark: '#184ec0',
            },
            background: {
              default: '#cecece',
              paper: '#e7e7e7',
            },
            text: {
              primary: '#303246',
              secondary: '#535560',
            },
          }
        : {
            // 🌙 Modo escuro (inspirado em Catppuccin Mocha)
            primary: {
              main: '#cba6f7',
              light: '#dbc1fa',
              dark: '#a883e6',
              contrastText: '#1e1e2e',
            },
            secondary: {
              main: '#94e2d5',
              light: '#b3f0e7',
              dark: '#6fc8bb',
              contrastText: '#1e1e2e',
            },
            success: {
              main: '#a6e3a1',
              light: '#c4f1c0',
              dark: '#7acb75',
            },
            error: {
              main: '#f38ba8',
              light: '#f6a6bd',
              dark: '#e06c8d',
            },
            warning: {
              main: '#fab387',
              light: '#fccca8',
              dark: '#e4946a',
            },
            info: {
              main: '#89b4fa',
              light: '#a6c8fc',
              dark: '#6a96e8',
            },
            background: {
              default: '#1e1e2e',
              paper: '#313244',
            },
            text: {
              primary: '#cdd6f4',
              secondary: '#bac2de',
            },
          }),
    },

    typography: {
      fontFamily:
        'Roboto, system-ui, -apple-system, "Segoe UI", Helvetica, Arial, sans-serif',
    },
  });
