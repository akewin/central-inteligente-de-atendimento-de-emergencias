const labels: Record<string, string> = {
  GUARDA_CIVIL: 'Guarda Civil',
  SAMU: 'SAMU',
  DEFESA_CIVIL: 'Defesa Civil',
};

export function emergencyServiceLabel(service: string): string {
  return labels[service] ?? service;
}
