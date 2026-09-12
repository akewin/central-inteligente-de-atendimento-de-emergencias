export interface OccurrenceMapPoint {
  latitude: number;
  longitude: number;
  label: string;
}

export interface OccurrenceMapUnit {
  id: number;
  name: string;
  status: string;
  address: string;
  latitude: number;
  longitude: number;
}

export interface OccurrenceMapHospital {
  id: number;
  name: string;
  address: string;
  latitude: number;
  longitude: number;
  hasEmergencyDepartment: boolean;
  straightLineDistanceKm: number;
  roadDistanceKm: number | null;
  estimatedMinutes: number | null;
}

export interface OccurrenceMapCoordinate {
  latitude: number;
  longitude: number;
}

export interface OccurrenceMapRoute {
  unitId: number;
  unitName: string;
  hospitalId: number;
  totalDistanceKm: number;
  totalEstimatedMinutes: number;
  unitToVictimDistanceKm: number;
  unitToVictimMinutes: number;
  victimToHospitalDistanceKm: number;
  victimToHospitalMinutes: number;
  geometry: OccurrenceMapCoordinate[];
}

export interface OccurrenceMapData {
  occurrenceId: number;
  generatedAt: string;
  victim: OccurrenceMapPoint;
  ambulances: OccurrenceMapUnit[];
  hospitals: OccurrenceMapHospital[];
  hospitalSearchSucceeded: boolean;
  selectedHospitalId: number | null;
  hospitalSelectionConfirmed: boolean;
  routes: OccurrenceMapRoute[];
  warnings: string[];
}
