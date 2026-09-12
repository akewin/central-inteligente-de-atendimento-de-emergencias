import type { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import type { SystemStats } from '../interfaces';

export function buildStatsCards(
  systemStats: SystemStats,
  cardIcon: Record<string, IconDefinition>
) {
  const {
    usersCount,
    systemResourcesCount,
    monthlyReportsCount,
    monthlyReportsCountReference,
    activeOccurrencesCount,
    dispatchedOccurrencesCount,
    finalizedOccurrencesCount,
    cancelledOccurrencesCount,
  } = systemStats;

  return [
    {
      bg: '#6dc4edff, #215fb0ff',
      icon: cardIcon.users,
      content: `${usersCount} Usuários Ativos`,
    },
    {
      bg: '#cc2b5e, #753a88',
      icon: cardIcon.systemResources,
      content: `${systemResourcesCount} Recursos de Sistema`,
    },
    {
      bg: '#fdc426ff, #e67e22',
      icon: cardIcon.reports,
      content: `${monthlyReportsCount} Ações auditadas em ${monthlyReportsCountReference}`,
    },
    { bg: '#2193b0, #6dd5ed', icon: cardIcon.occurrences, content: `${activeOccurrencesCount} Ocorrências em andamento` },
    { bg: '#f7971e, #ffd200', icon: cardIcon.occurrences, content: `${dispatchedOccurrencesCount} Ocorrências despachadas` },
    { bg: '#11998e, #38ef7d', icon: cardIcon.occurrences, content: `${finalizedOccurrencesCount} Ocorrências finalizadas` },
    { bg: '#cb2d3e, #ef473a', icon: cardIcon.occurrences, content: `${cancelledOccurrencesCount} Ocorrências canceladas` },
  ];
}
