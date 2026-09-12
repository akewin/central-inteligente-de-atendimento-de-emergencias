import { useEffect, useState } from 'react';
import {
  Button, Dialog, DialogActions, DialogContent, DialogTitle, Grid,
  MenuItem, TextField,
} from '@mui/material';
import type { EmergencyService, Unit, UnitPayload } from '../../interfaces';
import { lookupPostalCode } from '../../services/addressServices';
import { getErrorMessage } from '../../helpers';

interface Props {
  open: boolean;
  unit: Unit | null;
  services: EmergencyService[];
  saving: boolean;
  onClose: () => void;
  onSubmit: (payload: UnitPayload) => Promise<void>;
}

export default function UnitForm({ open, unit, services, saving, onClose, onSubmit }: Props) {
  const [name, setName] = useState('');
  const [service, setService] = useState<UnitPayload['service']>('POLICIA');
  const [postalCode, setPostalCode] = useState('');
  const [street, setStreet] = useState('');
  const [number, setNumber] = useState('');
  const [complement, setComplement] = useState('');
  const [neighborhood, setNeighborhood] = useState('');
  const [city, setCity] = useState('');
  const [state, setState] = useState('SP');
  const [reference, setReference] = useState('');
  const [validation, setValidation] = useState('');
  const [postalCodeLoading, setPostalCodeLoading] = useState(false);
  const [postalCodeMessage, setPostalCodeMessage] = useState('');

  useEffect(() => {
    setName(unit?.name ?? '');
    setService(unit?.service.type ?? services[0]?.type ?? 'POLICIA');
    setPostalCode(unit?.postalCode ?? ''); setStreet(unit?.street ?? ''); setNumber(unit?.number ?? '');
    setComplement(unit?.complement ?? ''); setNeighborhood(unit?.neighborhood ?? ''); setCity(unit?.city ?? '');
    setState(unit?.state ?? 'SP'); setReference(unit?.reference ?? '');
    setValidation('');
    setPostalCodeMessage('');
  }, [open, services, unit]);

  const handlePostalCodeLookup = async () => {
    if (postalCode.replace(/\D/g, '').length !== 8) return;
    setPostalCodeLoading(true); setPostalCodeMessage('');
    try {
      const address = await lookupPostalCode(postalCode);
      setStreet(address.street); setNeighborhood(address.neighborhood); setCity(address.city); setState(address.state);
      setPostalCodeMessage('Endereço localizado. Confira os dados antes de salvar.');
    } catch (requestError) { setPostalCodeMessage(getErrorMessage(requestError)); }
    finally { setPostalCodeLoading(false); }
  };

  const submit = async () => {
    if (name.trim().length < 2 || name.trim().length > 50) return setValidation('Informe um nome entre 2 e 50 caracteres.');
    if (postalCode.replace(/\D/g, '').length !== 8) return setValidation('Informe um CEP com 8 dígitos.');
    if (!street.trim() || !number.trim() || !neighborhood.trim() || !city.trim()) return setValidation('Preencha rua, número, bairro e cidade.');
    if (!/^[A-Za-z]{2}$/.test(state.trim())) return setValidation('Informe a UF com duas letras.');
    setValidation('');
    await onSubmit({ name: name.trim(), service, postalCode, street: street.trim(), number: number.trim(), complement: complement.trim() || undefined, neighborhood: neighborhood.trim(), city: city.trim(), state: state.trim().toUpperCase(), reference: reference.trim() || undefined, status: unit?.status ?? 'DISPONIVEL' });
  };

  return (
    <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm">
      <DialogTitle>{unit ? 'Editar equipe' : 'Nova equipe simulada'}</DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          <Grid size={{ xs: 12 }}><TextField fullWidth label="Nome" value={name} onChange={(e) => setName(e.target.value)} error={!!validation && name.trim().length < 2} /></Grid>
          <Grid size={{ xs: 12 }}><TextField select fullWidth label="Serviço" value={service} onChange={(e) => setService(e.target.value as UnitPayload['service'])}>{services.map((item) => <MenuItem key={item.id} value={item.type}>{item.type} — {item.emergencyNumber}</MenuItem>)}</TextField></Grid>
          <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth required label="CEP" value={postalCode}
            onChange={(e) => { const digits = e.target.value.replace(/\D/g, '').slice(0, 8); setPostalCode(digits.length > 5 ? `${digits.slice(0, 5)}-${digits.slice(5)}` : digits); }}
            onBlur={() => void handlePostalCodeLookup()} placeholder="00000-000" disabled={postalCodeLoading}
            helperText={postalCodeLoading ? 'Consultando CEP...' : postalCodeMessage} error={!!postalCodeMessage && !postalCodeMessage.startsWith('Endereço localizado')} /></Grid>
          <Grid size={{ xs: 12, sm: 8 }}><TextField fullWidth required label="Rua" value={street} onChange={(e) => setStreet(e.target.value)} /></Grid>
          <Grid size={{ xs: 12, sm: 4 }}><TextField fullWidth required label="Número" value={number} onChange={(e) => setNumber(e.target.value)} /></Grid>
          <Grid size={{ xs: 12, sm: 8 }}><TextField fullWidth label="Complemento" value={complement} onChange={(e) => setComplement(e.target.value)} /></Grid>
          <Grid size={{ xs: 12, sm: 5 }}><TextField fullWidth required label="Bairro" value={neighborhood} onChange={(e) => setNeighborhood(e.target.value)} /></Grid>
          <Grid size={{ xs: 12, sm: 5 }}><TextField fullWidth required label="Cidade" value={city} onChange={(e) => setCity(e.target.value)} /></Grid>
          <Grid size={{ xs: 12, sm: 2 }}><TextField fullWidth required label="UF" value={state} onChange={(e) => setState(e.target.value.slice(0, 2))} /></Grid>
          <Grid size={{ xs: 12 }}><TextField fullWidth label="Ponto de referência" value={reference} onChange={(e) => setReference(e.target.value)} /></Grid>
          {validation && <Grid size={{ xs: 12 }}><TextField fullWidth error value={validation} slotProps={{ input: { readOnly: true } }} /></Grid>}
        </Grid>
      </DialogContent>
      <DialogActions><Button onClick={onClose} disabled={saving}>Cancelar</Button><Button variant="contained" onClick={() => void submit()} disabled={saving}>{saving ? 'Salvando...' : 'Salvar'}</Button></DialogActions>
    </Dialog>
  );
}
