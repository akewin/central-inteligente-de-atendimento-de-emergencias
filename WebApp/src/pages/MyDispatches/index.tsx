import { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Badge, Box, Button, Card, CardActions, CardContent, Chip, CircularProgress, Container, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, Tab, Tabs, TextField, Typography } from '@mui/material';
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive';
import RouteIcon from '@mui/icons-material/Route';
import { getErrorMessage } from '../../helpers';
import type { DispatchStatus, Hospital, OperationalDispatch, OperationalRoute } from '../../interfaces';
import { assignTransportDestination, completeDispatchOnSite, getMyDispatchRoute, listHospitals, listMyDispatches, requestPatientTransport, startPatientTransport, transitionMyDispatch } from '../../services';
import { createDispatchConnection } from '../../services/dispatchRealtimeService';
import OperationalRouteDialog from '../../components/OperationalRouteDialog';

const nextStatus: Partial<Record<DispatchStatus, { status: DispatchStatus; label: string }>> = {
  ATRIBUIDO: { status: 'ACEITO', label: 'Aceitar chamado' },
  ACEITO: { status: 'NO_LOCAL', label: 'Informar chegada' },
  NO_LOCAL: { status: 'EM_ATENDIMENTO', label: 'Iniciar atendimento' },
};

export default function MyDispatches() {
  const [scope, setScope] = useState<'active' | 'history'>('active');
  const [items, setItems] = useState<OperationalDispatch[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState<number | null>(null);
  const [soundEnabled, setSoundEnabled] = useState(false);
  const [route, setRoute] = useState<OperationalRoute | null>(null);
  const [hospitals, setHospitals] = useState<Hospital[]>([]);
  const [destinationDispatch, setDestinationDispatch] = useState<OperationalDispatch | null>(null);
  const [hospitalId, setHospitalId] = useState<number | ''>(''); const [wardId, setWardId] = useState<number | ''>(''); const [notes, setNotes] = useState('');
  const audioContext = useRef<AudioContext | null>(null);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try { setItems((await listMyDispatches(scope)).data); }
    catch (requestError) { setError(getErrorMessage(requestError)); }
    finally { setLoading(false); }
  }, [scope]);

  useEffect(() => { void load(); }, [load]);
  useEffect(() => { void listHospitals().then(setHospitals).catch(() => setHospitals([])); }, []);
  useEffect(() => {
    const connection = createDispatchConnection(() => {
      void load();
      if (soundEnabled && audioContext.current) {
        const oscillator = audioContext.current.createOscillator();
        oscillator.connect(audioContext.current.destination); oscillator.frequency.value = 880;
        oscillator.start(); oscillator.stop(audioContext.current.currentTime + 0.22);
      }
    }, () => void load(), () => void load());
    void connection.start().catch(() => setError('Tempo real indisponível. Use Atualizar para sincronizar.'));
    return () => { void connection.stop(); };
  }, [load, soundEnabled]);

  const enableSound = async () => {
    audioContext.current ??= new AudioContext();
    await audioContext.current.resume(); setSoundEnabled(true);
  };
  const transition = async (dispatch: OperationalDispatch, target: DispatchStatus) => {
    setBusy(dispatch.id); setError(null);
    try { await transitionMyDispatch(dispatch.id, target, dispatch.updatedAt); await load(); }
    catch (requestError) { setError(getErrorMessage(requestError)); }
    finally { setBusy(null); }
  };
  const showRoute = async (dispatch: OperationalDispatch) => {
    setBusy(dispatch.id); setError(null);
    try { setRoute(await getMyDispatchRoute(dispatch.id)); }
    catch (requestError) { setError(getErrorMessage(requestError)); }
    finally { setBusy(null); }
  };
  const transportAction = async (dispatch: OperationalDispatch, action: 'request' | 'start' | 'complete') => {
    setBusy(dispatch.id); setError(null); try {
      if (action === 'request') await requestPatientTransport(dispatch.id, window.prompt('Observação para o encaminhamento (opcional):') ?? undefined);
      if (action === 'start') await startPatientTransport(dispatch.id);
      if (action === 'complete') await completeDispatchOnSite(dispatch.id);
      await load();
    } catch (e) { setError(getErrorMessage(e)); } finally { setBusy(null); }
  };
  const confirmDestination = async () => { if (!destinationDispatch || !hospitalId || !wardId) return; setBusy(destinationDispatch.id); try { const hospital = hospitals.find(h => h.id === hospitalId)!; await assignTransportDestination(destinationDispatch.id, hospital, Number(wardId), notes); setDestinationDispatch(null); await load(); } catch (e) { setError(getErrorMessage(e)); } finally { setBusy(null); } };
  const selectedHospital = hospitals.find(h => h.id === hospitalId);

  return <Container maxWidth="lg" sx={{ py: 3 }}>
    <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" gap={2} mb={2}>
      <Box><Typography variant="h4">Meus chamados</Typography><Typography color="text.secondary">{items[0]?.unitName ?? 'Equipe operacional'}</Typography></Box>
      <Stack direction="row" gap={1}><Button onClick={load}>Atualizar</Button><Button variant={soundEnabled ? 'outlined' : 'contained'} onClick={enableSound} startIcon={<NotificationsActiveIcon />}>{soundEnabled ? 'Som ativado' : 'Ativar alertas sonoros'}</Button></Stack>
    </Stack>
    <Tabs value={scope} onChange={(_, value) => setScope(value)}><Tab value="active" label={<Badge color="error" badgeContent={scope === 'active' ? items.length : 0}>Ativos</Badge>} /><Tab value="history" label="Histórico" /></Tabs>
    {error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}
    {loading ? <Box textAlign="center" py={8}><CircularProgress /></Box> : items.length === 0 ? <Alert severity="info" sx={{ mt: 2 }}>Nenhum chamado nesta lista.</Alert> :
      <Stack gap={2} mt={2}>{items.map(item => <Card key={item.id} variant="outlined">
        <CardContent><Stack direction="row" justifyContent="space-between" gap={1}><Typography variant="h6">Ocorrência #{item.occurrenceId}</Typography><Stack direction="row" gap={1} alignItems="center"><Chip color={item.status === 'ATRIBUIDO' ? 'warning' : 'primary'} label={item.status.replaceAll('_', ' ')} />{item.status !== 'ATRIBUIDO' && item.status !== 'CONCLUIDO' && item.status !== 'CANCELADO' && <Button size="small" variant="outlined" startIcon={<RouteIcon />} disabled={busy === item.id} onClick={() => void showRoute(item)}>Ver rota</Button>}</Stack></Stack>
          <Typography fontWeight={600} mt={1}>{item.priority?.replaceAll('_', ' ') ?? 'Prioridade não informada'} · {item.type?.replaceAll('_', ' ') ?? 'Tipo não informado'}</Typography>
          <Typography mt={1}>{item.description}</Typography><Typography color="text.secondary" mt={1}>{item.address}</Typography>
          <Typography variant="body2" mt={1}>Serviços: {item.services.join(', ')}</Typography>
        </CardContent>
        {nextStatus[item.status] && <CardActions sx={{ flexWrap: 'wrap' }}><Button disabled={busy === item.id} variant="contained" onClick={() => void transition(item, nextStatus[item.status]!.status)}>{busy === item.id ? 'Registrando...' : nextStatus[item.status]!.label}</Button>
          {['NO_LOCAL', 'EM_ATENDIMENTO'].includes(item.status) && !item.transport && <Button onClick={() => void transportAction(item, 'request')}>Solicitar encaminhamento</Button>}
          {['NO_LOCAL', 'EM_ATENDIMENTO'].includes(item.status) && item.unitService === 'SAMU' && (!item.transport || ['AGUARDANDO_CENTRAL', 'AGUARDANDO_DESTINO'].includes(item.transport.status)) && <Button color="secondary" onClick={() => { setDestinationDispatch(item); setHospitalId(''); setWardId(''); }}>Definir hospital</Button>}
          {item.unitService === 'SAMU' && item.transport?.status === 'HOSPITAL_AVISADO' && <Button color="success" onClick={() => void transportAction(item, 'start')}>Iniciar transporte</Button>}
          {['NO_LOCAL', 'EM_ATENDIMENTO'].includes(item.status) && <Button color="warning" onClick={() => void transportAction(item, 'complete')}>Encerrar minha atuação</Button>}
        </CardActions>}
      </Card>)}</Stack>}
    <OperationalRouteDialog route={route} open={!!route} onClose={() => setRoute(null)} />
    <Dialog open={!!destinationDispatch} onClose={() => setDestinationDispatch(null)} fullWidth><DialogTitle>Definir destino hospitalar</DialogTitle><DialogContent><Stack gap={2} mt={1}><TextField select label="Hospital" value={hospitalId} onChange={e => { setHospitalId(Number(e.target.value)); setWardId(''); }}>{hospitals.filter(h => h.active && h.wards.some(w => w.active && w.availableBeds > 0)).map(h => <MenuItem key={h.id} value={h.id}>{h.name}</MenuItem>)}</TextField><TextField select label="Ala" value={wardId} onChange={e => setWardId(Number(e.target.value))} disabled={!selectedHospital}>{selectedHospital?.wards.filter(w => w.active && w.availableBeds > 0).map(w => <MenuItem key={w.id} value={w.id}>{w.name} — {w.availableBeds} vagas</MenuItem>)}</TextField><TextField multiline minRows={3} label="Observações operacionais" value={notes} onChange={e => setNotes(e.target.value)} /></Stack></DialogContent><DialogActions><Button onClick={() => setDestinationDispatch(null)}>Cancelar</Button><Button variant="contained" disabled={!hospitalId || !wardId} onClick={() => void confirmDestination()}>Reservar e avisar</Button></DialogActions></Dialog>
  </Container>;
}
