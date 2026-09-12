import { useEffect, useState } from 'react';
import { Alert, Box, Button, Card, CardContent, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, Grid, Stack, Switch, TextField, Typography } from '@mui/material';
import type { Hospital, HospitalPayload, HospitalWard } from '../../interfaces';
import { createHospital, createHospitalWard, listHospitals, updateHospital, updateHospitalWard } from '../../services';
import { lookupPostalCode } from '../../services/addressServices';
import { getErrorMessage } from '../../helpers';

const empty: HospitalPayload = { name: '', postalCode: '', street: '', number: '', neighborhood: '', city: 'Araras', state: 'SP', latitude: -22.357, longitude: -47.384, hasEmergencyDepartment: true, active: true };

export default function Hospitals() {
  const [items, setItems] = useState<Hospital[]>([]); const [editing, setEditing] = useState<Hospital | null>(null);
  const [form, setForm] = useState<HospitalPayload>(empty); const [open, setOpen] = useState(false); const [error, setError] = useState('');
  const [postalCodeLoading, setPostalCodeLoading] = useState(false); const [postalCodeMessage, setPostalCodeMessage] = useState<string | null>(null);
  const load = async () => { try { setItems(await listHospitals(true)); setError(''); } catch (e) { setError(getErrorMessage(e)); } };
  useEffect(() => { void load(); }, []);
  const show = (hospital?: Hospital) => { setEditing(hospital ?? null); setForm(hospital ? { ...hospital, expectedUpdatedAt: hospital.updatedAt } : empty); setPostalCodeMessage(null); setOpen(true); };
  const save = async () => { try { editing ? await updateHospital(editing.id, form) : await createHospital(form); setOpen(false); await load(); } catch (e) { setError(getErrorMessage(e)); } };
  const addWard = async (hospital: Hospital) => { const name = window.prompt('Nome da nova ala:'); if (!name) return; const total = Number(window.prompt('Quantidade total de vagas:', '10')); if (!Number.isInteger(total) || total < 0) return; await createHospitalWard(hospital.id, { name, totalBeds: total, occupiedBeds: 0, active: true }); await load(); };
  const editWard = async (hospital: Hospital, ward: HospitalWard) => { const occupied = Number(window.prompt(`Vagas ocupadas em ${ward.name}:`, String(ward.occupiedBeds))); if (!Number.isInteger(occupied)) return; try { await updateHospitalWard(hospital.id, { ...ward, occupiedBeds: occupied }); await load(); } catch (e) { setError(getErrorMessage(e)); } };
  const set = (key: keyof HospitalPayload, value: unknown) => setForm(current => ({ ...current, [key]: value }));
  const changePostalCode = (value: string) => {
    const digits = value.replace(/\D/g, '').slice(0, 8);
    set('postalCode', digits.length > 5 ? `${digits.slice(0, 5)}-${digits.slice(5)}` : digits);
    setPostalCodeMessage(null);
  };
  const lookupHospitalPostalCode = async () => {
    if (form.postalCode.replace(/\D/g, '').length !== 8) return;
    setPostalCodeLoading(true); setPostalCodeMessage(null);
    try {
      const address = await lookupPostalCode(form.postalCode);
      setForm(current => ({ ...current, postalCode: address.postalCode, street: address.street, neighborhood: address.neighborhood, city: address.city, state: address.state }));
      setPostalCodeMessage('Endereço localizado. Confira o número antes de salvar.');
    } catch (requestError) {
      setPostalCodeMessage(getErrorMessage(requestError));
    } finally { setPostalCodeLoading(false); }
  };
  return <Container maxWidth="xl" sx={{ py: 3 }}><Stack direction="row" justifyContent="space-between" mb={2}><Box><Typography variant="h4">Hospitais de Araras</Typography><Typography color="text.secondary">Cadastro, alas e capacidade operacional.</Typography></Box><Button variant="contained" onClick={() => show()}>Novo hospital</Button></Stack>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}<Grid container spacing={2}>{items.map(h => <Grid key={h.id} size={{ xs: 12, md: 6 }}><Card variant="outlined"><CardContent><Stack direction="row" justifyContent="space-between"><Box><Typography variant="h6">{h.name}</Typography><Typography color="text.secondary">{h.street}, {h.number} — {h.neighborhood}</Typography></Box><Chip color={h.active ? 'success' : 'default'} label={h.active ? 'ATIVO' : 'INATIVO'} /></Stack><Stack gap={1} mt={2}>{h.wards.map(w => <Stack key={w.id} direction="row" justifyContent="space-between" alignItems="center"><Typography>{w.name}</Typography><Button size="small" onClick={() => void editWard(h, w)}>{w.availableBeds} livres · {w.reservedBeds} reservadas · {w.occupiedBeds} ocupadas</Button></Stack>)}</Stack><Stack direction="row" gap={1} mt={2}><Button onClick={() => show(h)}>Editar hospital</Button><Button onClick={() => void addWard(h)}>Adicionar ala</Button></Stack></CardContent></Card></Grid>)}</Grid>
    <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="md"><DialogTitle>{editing ? 'Editar hospital' : 'Novo hospital'}</DialogTitle><DialogContent><Grid container spacing={2} sx={{ mt: .5 }}>
      <Grid size={{ xs: 12 }}><TextField fullWidth label="Nome" value={form.name} onChange={e => set('name', e.target.value)} /></Grid>
      <Grid size={{ xs: 4 }}><TextField fullWidth label="CEP" value={form.postalCode} onChange={e => changePostalCode(e.target.value)} onBlur={() => void lookupHospitalPostalCode()} disabled={postalCodeLoading} placeholder="00000-000" helperText={postalCodeLoading ? 'Consultando CEP...' : postalCodeMessage} error={!!postalCodeMessage && !postalCodeMessage.startsWith('Endereço localizado')} /></Grid><Grid size={{ xs: 6 }}><TextField fullWidth label="Rua" value={form.street} onChange={e => set('street', e.target.value)} /></Grid><Grid size={{ xs: 2 }}><TextField fullWidth label="Número" value={form.number} onChange={e => set('number', e.target.value)} /></Grid>
      <Grid size={{ xs: 6 }}><TextField fullWidth label="Bairro" value={form.neighborhood} onChange={e => set('neighborhood', e.target.value)} /></Grid><Grid size={{ xs: 4 }}><TextField fullWidth disabled label="Cidade" value={form.city} /></Grid><Grid size={{ xs: 2 }}><TextField fullWidth disabled label="UF" value={form.state} /></Grid>
      <Grid size={{ xs: 6 }}><TextField fullWidth type="number" label="Latitude" value={form.latitude} onChange={e => set('latitude', Number(e.target.value))} /></Grid><Grid size={{ xs: 6 }}><TextField fullWidth type="number" label="Longitude" value={form.longitude} onChange={e => set('longitude', Number(e.target.value))} /></Grid>
      <Grid size={{ xs: 6 }}><Typography>Pronto atendimento <Switch checked={form.hasEmergencyDepartment} onChange={e => set('hasEmergencyDepartment', e.target.checked)} /></Typography></Grid><Grid size={{ xs: 6 }}><Typography>Ativo <Switch checked={form.active} onChange={e => set('active', e.target.checked)} /></Typography></Grid>
    </Grid></DialogContent><DialogActions><Button onClick={() => setOpen(false)}>Cancelar</Button><Button variant="contained" onClick={() => void save()}>Salvar</Button></DialogActions></Dialog>
  </Container>;
}
