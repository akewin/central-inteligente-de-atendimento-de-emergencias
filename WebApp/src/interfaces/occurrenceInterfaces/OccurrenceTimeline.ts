import type { OccurrenceCreator, OccurrenceStatus } from './Occurrence';

export interface OccurrenceUnitStatusChange {
  unitId: number;
  unitName: string;
  previousStatus: string;
  currentStatus: string;
}

export interface OccurrenceStatusTransitionRead {
  occurrenceId: number;
  previousStatus: OccurrenceStatus;
  currentStatus: OccurrenceStatus;
  updatedAt: string;
  changedBy: OccurrenceCreator;
  units: OccurrenceUnitStatusChange[];
}

export interface OccurrenceTimelineItem {
  id: number;
  event: string;
  previousStatus: OccurrenceStatus | null;
  currentStatus: OccurrenceStatus | null;
  reason: string | null;
  generatedBy: string;
  createdAt: string;
}
