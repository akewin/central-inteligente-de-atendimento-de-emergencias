import { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Box, Button, Chip, CircularProgress, Container, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Paper, Stack, Tab, Tabs, TextField, Typography, useMediaQuery, useTheme } from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import {
  AIAnalysisCard,
  DispatchConfirmationDialog,
  DispatchResultCard,
  OccurrenceDetailsCard,
  OccurrenceStatusDialog,
  OccurrenceTimeline,
  OccurrenceTrackingCard,
  OccurrenceDispatchesCard,
  OccurrenceMapCard,
  PageTitle,
  ServiceConfirmationCard,
  UnitRecommendationsCard,
} from '../../components';
import { usePermissions } from '../../hooks';
import {
  confirmDispatch, getOccurrenceById, getOccurrenceTimeline, getOccurrenceUnitRecommendations, getOccurrenceDispatches,
  requestOccurrenceAnalysis, transitionOccurrenceStatus, getOccurrenceTransport, assignSamuToTransport, listUnits,
} from '../../services';
import { getErrorMessage } from '../../helpers';
import type {
  DispatchConfirmationRead, EmergencyServiceType, OccurrenceRead,
  OccurrenceStatus, OccurrenceTimelineItem, OccurrenceUnitRecommendations, UnitRecommendation, OperationalDispatch, PatientTransport, Unit,
} from '../../interfaces';
import { createDispatchConnection } from '../../services/dispatchRealtimeService';

type HumanStatusTarget = Extract<OccurrenceStatus, 'EM_ATENDIMENTO' | 'FINALIZADA' | 'CANCELADA'>;
type DesktopTab = 'map' | 'decision' | 'teams' | 'tracking';

function initialDesktopTab(status: OccurrenceStatus): DesktopTab {
  if (status === 'ABERTA' || status === 'EM_ANALISE') return 'decision';
  if (status === 'AGUARDANDO_CONFIRMACAO') return 'teams';
  if (status === 'DESPACHADA' || status === 'EM_ATENDIMENTO') return 'map';
  return 'tracking';
}

export default function OccurrenceDetails() {
  const theme = useTheme();
  const desktop = useMediaQuery(theme.breakpoints.up('md'));
  const { id } = useParams();
  const navigate = useNavigate();
  const { permissionsMap } = usePermissions();
  const [occurrence, setOccurrence] = useState<OccurrenceRead | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [analysisLoading, setAnalysisLoading] = useState(false);
  const [analysisError, setAnalysisError] = useState<string | null>(null);
  const [unitRecommendations, setUnitRecommendations] = useState<OccurrenceUnitRecommendations | null>(null);
  const [recommendationsLoading, setRecommendationsLoading] = useState(false);
  const [recommendationsError, setRecommendationsError] = useState<string | null>(null);
  const [selectedByService, setSelectedByService] = useState<Partial<Record<EmergencyServiceType, number>>>({});
  const [dispatchDialogOpen, setDispatchDialogOpen] = useState(false);
  const [dispatchLoading, setDispatchLoading] = useState(false);
  const [dispatchError, setDispatchError] = useState<string | null>(null);
  const [dispatchResult, setDispatchResult] = useState<DispatchConfirmationRead | null>(null);
  const [timeline, setTimeline] = useState<OccurrenceTimelineItem[]>([]);
  const [timelineLoading, setTimelineLoading] = useState(false);
  const [timelineError, setTimelineError] = useState<string | null>(null);
  const [statusTarget, setStatusTarget] = useState<HumanStatusTarget | null>(null);
  const [statusLoading, setStatusLoading] = useState(false);
  const [statusError, setStatusError] = useState<string | null>(null);
  const [dispatches, setDispatches] = useState<OperationalDispatch[]>([]);
  const [dispatchesLoading, setDispatchesLoading] = useState(false);
  const [dispatchesError, setDispatchesError] = useState<string | null>(null);
  const [desktopTab, setDesktopTab] = useState<DesktopTab>('decision');
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [transport, setTransport] = useState<PatientTransport | null>(null);
  const [samuOptions, setSamuOptions] = useState<Unit[]>([]); const [samuUnitId, setSamuUnitId] = useState<number | ''>('');
  const desktopTabInitialized = useRef(false);

  const loadOccurrence = useCallback(async () => {
    const occurrenceId = Number(id);
    if (!Number.isInteger(occurrenceId) || occurrenceId <= 0) {
      setError('Identificador de ocorrência inválido.');
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      setOccurrence(await getOccurrenceById(occurrenceId));
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadOccurrence();
  }, [loadOccurrence]);

  const loadTimeline = useCallback(async () => {
    const occurrenceId = Number(id);
    if (!Number.isInteger(occurrenceId) || occurrenceId <= 0) return;
    setTimelineLoading(true);
    setTimelineError(null);
    try {
      setTimeline(await getOccurrenceTimeline(occurrenceId));
    } catch (requestError) {
      setTimelineError(getErrorMessage(requestError));
    } finally {
      setTimelineLoading(false);
    }
  }, [id]);

  useEffect(() => { void loadTimeline(); }, [loadTimeline]);

  const loadDispatches = useCallback(async () => {
    const occurrenceId = Number(id); if (!Number.isInteger(occurrenceId) || occurrenceId <= 0) return;
    setDispatchesLoading(true); setDispatchesError(null);
    try { setDispatches(await getOccurrenceDispatches(occurrenceId)); }
    catch (requestError) { setDispatchesError(getErrorMessage(requestError)); }
    finally { setDispatchesLoading(false); }
  }, [id]);

  useEffect(() => { void loadDispatches(); }, [loadDispatches]);
  const loadTransport = useCallback(async () => { const occurrenceId = Number(id); if (!occurrenceId) return; try { const value = await getOccurrenceTransport(occurrenceId); setTransport(value); if (value?.status === 'AGUARDANDO_CENTRAL') setSamuOptions((await listUnits(1, 100, { search: '', emergencyService: 'SAMU', status: 'DISPONIVEL' })).data); } catch { setTransport(null); } }, [id]);
  useEffect(() => { void loadTransport(); }, [loadTransport]);
  useEffect(() => {
    const occurrenceId = Number(id); if (!Number.isInteger(occurrenceId)) return;
    const refreshRealtime = () => void Promise.all([loadDispatches(), loadOccurrence(), loadTimeline(), loadTransport()]);
    const connection = createDispatchConnection(refreshRealtime, () => void Promise.all([loadDispatches(), loadTransport()]), refreshRealtime);
    void connection.start().then(() => connection.invoke('WatchOccurrence', occurrenceId)).catch(() => undefined);
    return () => { void connection.stop(); };
  }, [id, loadDispatches, loadOccurrence, loadTimeline, loadTransport]);
  const assignSamu = async () => { if (!occurrence || !samuUnitId) return; try { await assignSamuToTransport(occurrence.id, Number(samuUnitId)); setSamuUnitId(''); await Promise.all([loadTransport(), loadDispatches(), loadOccurrence()]); } catch (e) { setDispatchError(getErrorMessage(e)); } };

  const requestAnalysis = async () => {
    if (!occurrence) return;
    setAnalysisLoading(true);
    setAnalysisError(null);
    try {
      await requestOccurrenceAnalysis(occurrence.id);
      await loadOccurrence();
    } catch (requestError) {
      setAnalysisError(getErrorMessage(requestError));
    } finally {
      setAnalysisLoading(false);
    }
  };

  const loadUnitRecommendations = useCallback(async () => {
    const occurrenceId = Number(id);
    if (!Number.isInteger(occurrenceId) || occurrenceId <= 0) return;
    setRecommendationsLoading(true);
    setRecommendationsError(null);
    try {
      const result = await getOccurrenceUnitRecommendations(occurrenceId);
      setUnitRecommendations(result);
      setSelectedByService({});
    } catch (requestError) {
      setRecommendationsError(getErrorMessage(requestError));
    } finally {
      setRecommendationsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (occurrence?.serviceConfirmation && occurrence.status === 'AGUARDANDO_CONFIRMACAO') {
      void loadUnitRecommendations();
    } else {
      setUnitRecommendations(null);
      setSelectedByService({});
    }
  }, [occurrence?.serviceConfirmation, occurrence?.status, loadUnitRecommendations]);

  const selectedUnits: UnitRecommendation[] = unitRecommendations?.groups.flatMap((group) => {
    const selectedId = selectedByService[group.service];
    return group.units.filter((unit) => unit.id === selectedId);
  }) ?? [];

  const submitDispatch = async () => {
    if (!occurrence || selectedUnits.length !== unitRecommendations?.groups.length) return;
    setDispatchLoading(true);
    setDispatchError(null);
    try {
      const result = await confirmDispatch(occurrence.id, selectedUnits.map((unit) => unit.id));
      setDispatchResult(result);
      setDispatchDialogOpen(false);
      await loadOccurrence();
      await loadTimeline();
    } catch (requestError) {
      setDispatchError(getErrorMessage(requestError));
      setDispatchDialogOpen(false);
      await loadUnitRecommendations();
    } finally {
      setDispatchLoading(false);
    }
  };

  const submitStatusTransition = async (reason?: string) => {
    if (!occurrence || !statusTarget) return;
    setStatusLoading(true);
    setStatusError(null);
    try {
      await transitionOccurrenceStatus(occurrence.id, statusTarget, occurrence.updatedAt, reason);
      setStatusTarget(null);
      await Promise.all([loadOccurrence(), loadTimeline()]);
    } catch (requestError) {
      setStatusError(getErrorMessage(requestError));
    } finally {
      setStatusLoading(false);
    }
  };

  const refreshTracking = () => void Promise.all([loadOccurrence(), loadTimeline()]);

  const canRequestAnalysis = occurrence &&
    !occurrence.serviceConfirmation &&
    ['ABERTA', 'AGUARDANDO_CONFIRMACAO'].includes(occurrence.status);

  useEffect(() => {
    if (!occurrence || desktopTabInitialized.current) return;
    setDesktopTab(initialDesktopTab(occurrence.status));
    desktopTabInitialized.current = true;
  }, [occurrence]);

  return (
    <Container
      maxWidth={desktop ? false : 'lg'}
      sx={desktop ? {
        height: 'calc(100dvh - 64px)', py: 1.5, px: 2.5,
        display: 'flex', flexDirection: 'column', overflow: 'hidden',
      } : { py: 3 }}
    >
      {!desktop && <PageTitle icon={permissionsMap.OCCURRENCES} title="Detalhes da ocorrência" />}
      {loading ? (
        <Box display="flex" justifyContent="center" py={8}><CircularProgress /></Box>
      ) : error ? (
        <Alert
          severity="error"
          action={<Button color="inherit" onClick={loadOccurrence}>Tentar novamente</Button>}
        >
          {error}
        </Alert>
      ) : occurrence && desktop ? (
        <Box display="flex" flexDirection="column" minHeight={0} flex={1}>
          <Paper variant="outlined" sx={{ px: 2, py: 1.25, mb: 1, flexShrink: 0 }}>
            <Stack direction="row" justifyContent="space-between" alignItems="center" gap={2}>
              <Box minWidth={0}>
                <Stack direction="row" alignItems="center" gap={1} mb={.5}>
                  <Typography variant="h5" fontWeight={700}>Ocorrência #{occurrence.id}</Typography>
                  <Chip size="small" color="primary" label={occurrence.status.replaceAll('_', ' ')} />
                  <Chip size="small" variant="outlined" label={occurrence.confirmedPriority ?? 'Prioridade não definida'} />
                  <Chip size="small" variant="outlined" label={occurrence.confirmedType?.replaceAll('_', ' ') ?? 'Tipo não definido'} />
                </Stack>
                <Typography fontWeight={600} noWrap>{occurrence.description}</Typography>
                <Typography variant="body2" color="text.secondary" noWrap>{occurrence.locationDescription}</Typography>
              </Box>
              <Stack direction="row" gap={1} flexShrink={0}>
                <Button onClick={() => setDetailsOpen(true)}>Ver detalhes</Button>
                <Button onClick={() => navigate('/occurrences')}>Voltar</Button>
              </Stack>
            </Stack>
          </Paper>

          <Paper variant="outlined" sx={{ flexShrink: 0, mb: 1 }}>
            <Tabs value={desktopTab} onChange={(_, value: DesktopTab) => setDesktopTab(value)} variant="fullWidth">
              <Tab value="map" label="Mapa e hospital" />
              <Tab value="decision" label="IA e decisão" />
              <Tab value="teams" label={dispatches.length ? `Equipes (${dispatches.length})` : 'Equipes'} />
              <Tab value="tracking" label="Acompanhamento" />
            </Tabs>
          </Paper>

          <Box minHeight={0} flex={1} overflow="hidden">
            {desktopTab === 'map' && (
              <OccurrenceMapCard occurrenceId={occurrence.id} refreshKey={occurrence.updatedAt} compact />
            )}
            {desktopTab === 'decision' && (
              <Box sx={{ height: '100%', overflow: 'auto', display: 'grid', gridTemplateColumns: 'minmax(0, 1.15fr) minmax(360px, .85fr)', gap: 1.5, alignItems: 'start', '& .MuiPaper-root': { mt: '0 !important' } }}>
                <Box>
                  {transport?.status === 'AGUARDANDO_CENTRAL' && <Alert severity="warning" sx={{ mb: 1 }} action={<Stack direction="row" gap={1}><TextField select size="small" label="Equipe SAMU" value={samuUnitId} onChange={e => setSamuUnitId(Number(e.target.value))} sx={{ minWidth: 220, bgcolor: 'background.paper' }}>{samuOptions.map(unit => <MenuItem key={unit.id} value={unit.id}>{unit.name}</MenuItem>)}</TextField><Button color="inherit" disabled={!samuUnitId} onClick={() => void assignSamu()}>Despachar</Button></Stack>}>Encaminhamento solicitado no local. Despache uma equipe SAMU.</Alert>}
                  <Paper variant="outlined" sx={{ p: 2, mb: 1.5 }}>
                    <Stack direction="row" alignItems="center" justifyContent="space-between" gap={2}>
                      <Box><Typography variant="h6">Análise inteligente</Typography><Typography variant="body2" color="text.secondary">Classificação e recomendação para apoiar a decisão.</Typography></Box>
                      <Button variant="contained" disabled={!canRequestAnalysis || analysisLoading} onClick={requestAnalysis}>
                        {analysisLoading ? 'Analisando...' : occurrence.latestAIAnalysis ? 'Refazer análise' : 'Solicitar análise da IA'}
                      </Button>
                    </Stack>
                    {analysisError && <Alert severity="error" sx={{ mt: 1.5 }}>{analysisError}</Alert>}
                  </Paper>
                  {occurrence.latestAIAnalysis && <AIAnalysisCard analysis={occurrence.latestAIAnalysis} />}
                </Box>
                <ServiceConfirmationCard occurrence={occurrence} onConfirmed={loadOccurrence} />
              </Box>
            )}
            {desktopTab === 'teams' && (
              <Box sx={{ height: '100%', overflow: 'auto', display: 'grid', gridTemplateColumns: occurrence.status === 'AGUARDANDO_CONFIRMACAO' ? 'minmax(0, 1.35fr) minmax(340px, .65fr)' : '1fr', gap: 1.5, alignItems: 'start', '& > .MuiPaper-root, & .MuiPaper-root': { mt: '0 !important' } }}>
                <Box>
                  {!occurrence.serviceConfirmation ? (
                    <Alert severity="info">Confirme os órgãos na aba “IA e decisão” antes de selecionar equipes.</Alert>
                  ) : occurrence.status === 'AGUARDANDO_CONFIRMACAO' ? (
                    <UnitRecommendationsCard
                      recommendations={unitRecommendations} loading={recommendationsLoading} error={recommendationsError}
                      onRefresh={() => void loadUnitRecommendations()} selectedByService={selectedByService}
                      onSelect={(service, unitId) => setSelectedByService((current) => ({ ...current, [service]: unitId }))}
                      onConfirm={() => setDispatchDialogOpen(true)} confirming={dispatchLoading}
                    />
                  ) : <OccurrenceDispatchesCard items={dispatches} loading={dispatchesLoading} error={dispatchesError} />}
                  {dispatchError && <Alert severity="error" sx={{ mt: 1 }}>{dispatchError}</Alert>}
                </Box>
                {occurrence.status === 'AGUARDANDO_CONFIRMACAO' && (
                  <Box><OccurrenceDispatchesCard items={dispatches} loading={dispatchesLoading} error={dispatchesError} />{dispatchResult && <DispatchResultCard result={dispatchResult} />}</Box>
                )}
              </Box>
            )}
            {desktopTab === 'tracking' && (
              <Box sx={{ height: '100%', overflow: 'auto', display: 'grid', gridTemplateColumns: 'minmax(360px, .7fr) minmax(0, 1.3fr)', gap: 1.5, alignItems: 'start', '& .MuiPaper-root': { mt: '0 !important' } }}>
                <Box>{statusError && <Alert severity="error" sx={{ mb: 1 }}>{statusError}</Alert>}<OccurrenceTrackingCard status={occurrence.status} loading={statusLoading || loading} onAction={setStatusTarget} onRefresh={refreshTracking} /></Box>
                <OccurrenceTimeline items={timeline} loading={timelineLoading} error={timelineError} />
              </Box>
            )}
          </Box>

          <Dialog open={detailsOpen} onClose={() => setDetailsOpen(false)} fullWidth maxWidth="md">
            <DialogTitle>Detalhes da ocorrência #{occurrence.id}</DialogTitle>
            <DialogContent><OccurrenceDetailsCard occurrence={occurrence} /></DialogContent>
            <DialogActions><Button onClick={() => setDetailsOpen(false)}>Fechar</Button></DialogActions>
          </Dialog>
          <DispatchConfirmationDialog open={dispatchDialogOpen} units={selectedUnits} loading={dispatchLoading} onCancel={() => setDispatchDialogOpen(false)} onConfirm={() => void submitDispatch()} />
          <OccurrenceStatusDialog target={statusTarget} loading={statusLoading} onClose={() => setStatusTarget(null)} onConfirm={(reason) => void submitStatusTransition(reason)} />
        </Box>
      ) : occurrence ? (
        <>
          <OccurrenceDetailsCard occurrence={occurrence} />
          <OccurrenceMapCard occurrenceId={occurrence.id} refreshKey={occurrence.updatedAt} />
          <Stack direction={{ xs: 'column', sm: 'row' }} gap={2} mt={2} alignItems={{ sm: 'center' }}>
            <Button
              variant="contained"
              disabled={!canRequestAnalysis || analysisLoading}
              onClick={requestAnalysis}
            >
              {analysisLoading ? 'Analisando...' : 'Solicitar análise da IA'}
            </Button>
            {analysisLoading && <CircularProgress size={24} />}
          </Stack>
          {analysisError && <Alert severity="error" sx={{ mt: 2 }}>{analysisError}</Alert>}
          {occurrence.latestAIAnalysis && (
            <AIAnalysisCard analysis={occurrence.latestAIAnalysis} />
          )}
          <ServiceConfirmationCard occurrence={occurrence} onConfirmed={loadOccurrence} />
          {occurrence.serviceConfirmation && (
            <>
              {dispatchError && <Alert severity="error" sx={{ mt: 2 }}>{dispatchError}</Alert>}
              {dispatchResult && <DispatchResultCard result={dispatchResult} />}
              {occurrence.status === 'AGUARDANDO_CONFIRMACAO' && (
                <UnitRecommendationsCard
                  recommendations={unitRecommendations}
                  loading={recommendationsLoading}
                  error={recommendationsError}
                  onRefresh={() => void loadUnitRecommendations()}
                  selectedByService={selectedByService}
                  onSelect={(service, unitId) => setSelectedByService((current) => ({ ...current, [service]: unitId }))}
                  onConfirm={() => setDispatchDialogOpen(true)}
                  confirming={dispatchLoading}
                />
              )}
              <DispatchConfirmationDialog
                open={dispatchDialogOpen}
                units={selectedUnits}
                loading={dispatchLoading}
                onCancel={() => setDispatchDialogOpen(false)}
                onConfirm={() => void submitDispatch()}
              />
            </>
          )}
          {statusError && <Alert severity="error" sx={{ mt: 2 }}>{statusError}</Alert>}
          <OccurrenceTrackingCard
            status={occurrence.status}
            loading={statusLoading || loading}
            onAction={setStatusTarget}
            onRefresh={refreshTracking}
          />
          <OccurrenceDispatchesCard items={dispatches} loading={dispatchesLoading} error={dispatchesError} />
          <OccurrenceTimeline items={timeline} loading={timelineLoading} error={timelineError} />
          <OccurrenceStatusDialog
            target={statusTarget}
            loading={statusLoading}
            onClose={() => setStatusTarget(null)}
            onConfirm={(reason) => void submitStatusTransition(reason)}
          />
        </>
      ) : null}
      {!desktop && <Button sx={{ mt: 2 }} onClick={() => navigate('/occurrences')}>Voltar para ocorrências</Button>}
    </Container>
  );
}
