import type { EmergencyServiceType } from './Occurrence';

export interface UnitRecommendation {
  id: number;
  name: string;
  status: 'DISPONIVEL';
  address: string;
  distanceKm: number;
}

export interface UnitRecommendationGroup {
  service: EmergencyServiceType;
  emergencyNumber: string;
  displayName: string;
  hasAvailableUnits: boolean;
  units: UnitRecommendation[];
}

export interface OccurrenceUnitRecommendations {
  occurrenceId: number;
  generatedAt: string;
  groups: UnitRecommendationGroup[];
}
