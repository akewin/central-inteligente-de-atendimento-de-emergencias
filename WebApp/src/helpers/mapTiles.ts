export const mapTileUrl = import.meta.env.VITE_MAP_TILE_URL?.trim()
  || 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';

export const mapTileAttribution = import.meta.env.VITE_MAP_TILE_ATTRIBUTION?.trim()
  || '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors';
