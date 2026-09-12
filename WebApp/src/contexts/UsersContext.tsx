import {
  createContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";

import type {
  UserFormValues,
  UsersContextProps,
  UserList,
} from "../interfaces";
import {
  listUsers,
  createUser,
  updateUser,
  deleteUser,
  listUsersForSelect,
  listUserById,
} from "../services";
import { cleanStates, getErrorMessage } from "../helpers";

const UsersContext = createContext<UsersContextProps | undefined>(undefined);
export default UsersContext;

export function UsersProvider({ children }: { children: ReactNode }) {
  const [users, setUsers] = useState<UserList[]>([]);
  const [pagination, setPagination] = useState(cleanStates.tablePagination);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = useCallback(
    async (
      page = pagination.page,
      pageSize = pagination.pageSize,
      searchKey = "",
    ) => {
      setLoading(true);
      setError(null);

      try {
        const response = await listUsers(page, pageSize, searchKey);
        setUsers(response.data);
        setPagination({
          totalItems: response.totalItems,
          page: response.page,
          pageSize: response.pageSize,
          totalPages: response.totalPages,
        });
      } catch (err) {
        setError(getErrorMessage(err));
        console.error("Erro ao listar usuários:", err);
      } finally {
        setLoading(false);
      }
    },
    [pagination.page, pagination.pageSize],
  );

  const addUser = useCallback(async (user: UserFormValues) => {
    setLoading(true);
    try {
      await createUser(user);
    } finally {
      setLoading(false);
    }
  }, []);

  const editUser = useCallback(async (user: UserFormValues) => {
    setLoading(true);
    try {
      await updateUser(user);
    } finally {
      setLoading(false);
    }
  }, []);

  const removeUser = useCallback(async (id: number) => {
    setLoading(true);
    try {
      await deleteUser(id.toString());
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchUsersForSelect = useCallback(async () => {
    try {
      return await listUsersForSelect();
    } catch (err) {
      console.error("Erro ao buscar usuários para select:", err);
      return [];
    }
  }, []);

  const fetchUserById = useCallback(async (id: number) => {
    try {
      return await listUserById(id);
    } catch (err) {
      console.error("Erro ao buscar usuário:", err);
      return null;
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  return (
    <UsersContext.Provider
      value={{
        users,
        pagination,
        loading,
        error,

        fetchUsers,
        addUser,
        editUser,
        removeUser,
        fetchUsersForSelect,
        fetchUserById,

        setPagination,
      }}
    >
      {children}
    </UsersContext.Provider>
  );
}
