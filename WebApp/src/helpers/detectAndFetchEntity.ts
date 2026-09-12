import { listSystemResourceById, listUserById } from '../services';

const ENTITY_REGISTRY = {
  user: {
    signature: ['id', 'username', 'email', 'permissions'],
    fetch: listUserById,
  },
  systemResource: {
    signature: ['name', 'exhibitionName'],
    fetch: listSystemResourceById,
  },
} as const;

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function detectAndFetchEntity(payload: any) {
  for (const entry of Object.values(ENTITY_REGISTRY)) {
    if (entry.signature.every((k) => k in payload)) {
      return entry.fetch(payload.id);
    }
  }
  return null;
}
