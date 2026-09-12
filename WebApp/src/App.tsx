import { RouterProvider } from 'react-router-dom';
import router from './routes';
import {
  AuthProvider,
  ThemeModeProvider,
  NotificationProvider,
} from './contexts';
import { SnackbarNotification } from './components';

export default function App() {
  return (
    <ThemeModeProvider>
      <NotificationProvider>
        <AuthProvider>
          <RouterProvider router={router} />
          <SnackbarNotification />
        </AuthProvider>
      </NotificationProvider>
    </ThemeModeProvider>
  );
}
