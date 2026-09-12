import type { UserFormValues } from '../userInterfaces/UserFormValues';
import type { UserList } from '../userInterfaces/UserList';
import type { UserOption } from '../userInterfaces/UserOption';
import type { UserRead } from '../userInterfaces/UserRead';
import type { UsersPagination } from '../userInterfaces/UsersPagination';

export interface UsersContextProps {
  users: UserList[];
  pagination: Omit<UsersPagination, 'data'>;
  loading: boolean;
  error: string | null;
  fetchUsers: (
    page?: number,
    pageSize?: number,
    searchKey?: string
  ) => Promise<void>;
  addUser: (user: UserFormValues) => Promise<void>;
  editUser: (user: UserFormValues) => Promise<void>;
  removeUser: (id: number) => Promise<void>;
  fetchUsersForSelect: () => Promise<UserOption[]>;
  fetchUserById: (id: number) => Promise<UserRead | null>;
  setPagination: React.Dispatch<
    React.SetStateAction<Omit<UsersPagination, 'data'>>
  >;
}
