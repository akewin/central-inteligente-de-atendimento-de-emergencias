import { useState } from 'react';
import { Alert, Box, Button, Paper, TextField, Typography } from '@mui/material';
import type { OccurrenceCreatePayload } from '../../interfaces';
import { lookupPostalCode } from '../../services/addressServices';
import { getErrorMessage } from '../../helpers';

interface Props { loading: boolean; onSubmit: (payload: OccurrenceCreatePayload) => Promise<void>; }

const initialState = {
  description: '', postalCode: '', street: '', number: '', complement: '',
  neighborhood: '', city: '', state: '', reference: '',
};

export default function OccurrenceForm({ loading, onSubmit }: Props) {
  const [form, setForm] = useState(initialState);
  const [error, setError] = useState<string | null>(null);
  const [postalCodeLoading, setPostalCodeLoading] = useState(false);
  const [postalCodeMessage, setPostalCodeMessage] = useState<string | null>(null);
  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name } = event.target;
    let value = event.target.value;
    if (name === 'postalCode') {
      const digits = value.replace(/\D/g, '').slice(0, 8);
      value = digits.length > 5 ? `${digits.slice(0, 5)}-${digits.slice(5)}` : digits;
    }
    if (name === 'state') value = value.replace(/[^a-z]/gi, '').slice(0, 2).toUpperCase();
    setForm((current) => ({ ...current, [name]: value }));
  };

  const handlePostalCodeLookup = async () => {
    if (form.postalCode.replace(/\D/g, '').length !== 8) return;
    setPostalCodeLoading(true); setPostalCodeMessage(null);
    try {
      const address = await lookupPostalCode(form.postalCode);
      setForm((current) => ({ ...current, street: address.street, neighborhood: address.neighborhood, city: address.city, state: address.state }));
      setPostalCodeMessage('Endereço localizado. Confira os dados antes de registrar.');
    } catch (requestError) {
      setPostalCodeMessage(getErrorMessage(requestError));
    } finally { setPostalCodeLoading(false); }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const required = [form.description, form.postalCode, form.street, form.number, form.neighborhood, form.city, form.state];
    if (required.some((value) => !value.trim())) return setError('Preencha todos os campos obrigatórios.');
    if (form.postalCode.replace(/\D/g, '').length !== 8) return setError('Informe um CEP válido com 8 dígitos.');
    if (form.state.length !== 2) return setError('Informe a UF com duas letras.');
    if (form.description.length > 4000) return setError('A descrição deve possuir no máximo 4000 caracteres.');
    setError(null);
    await onSubmit({
      description: form.description.trim(), postalCode: form.postalCode, street: form.street.trim(),
      number: form.number.trim(), complement: form.complement.trim() || undefined,
      neighborhood: form.neighborhood.trim(), city: form.city.trim(), state: form.state,
      reference: form.reference.trim() || undefined,
    });
  };

  return (
    <Paper component="form" onSubmit={handleSubmit} sx={{ p: { xs: 2, sm: 3 } }}>
      <Box display="flex" flexDirection="column" gap={2}>
        {error && <Alert severity="error">{error}</Alert>}
        <TextField label="Descrição da ocorrência" name="description" value={form.description} onChange={handleChange}
          required multiline minRows={4} inputProps={{ maxLength: 4000 }} helperText={`${form.description.length}/4000 caracteres`} />
        <Typography variant="h6">Endereço da ocorrência</Typography>
        <Box display="grid" gridTemplateColumns={{ xs: '1fr', sm: '1fr 2fr' }} gap={2}>
          <TextField label="CEP" name="postalCode" value={form.postalCode} onChange={handleChange} onBlur={() => void handlePostalCodeLookup()}
            required placeholder="00000-000" disabled={postalCodeLoading} helperText={postalCodeLoading ? 'Consultando CEP...' : postalCodeMessage} error={!!postalCodeMessage && !postalCodeMessage.startsWith('Endereço localizado')} />
          <TextField label="Rua ou logradouro" name="street" value={form.street} onChange={handleChange} required inputProps={{ maxLength: 200 }} />
        </Box>
        <Box display="grid" gridTemplateColumns={{ xs: '1fr', sm: '1fr 2fr' }} gap={2}>
          <TextField label="Número" name="number" value={form.number} onChange={handleChange} required inputProps={{ maxLength: 20 }} />
          <TextField label="Complemento" name="complement" value={form.complement} onChange={handleChange} inputProps={{ maxLength: 100 }} />
        </Box>
        <Box display="grid" gridTemplateColumns={{ xs: '1fr', sm: '2fr 2fr 0.7fr' }} gap={2}>
          <TextField label="Bairro" name="neighborhood" value={form.neighborhood} onChange={handleChange} required inputProps={{ maxLength: 100 }} />
          <TextField label="Cidade" name="city" value={form.city} onChange={handleChange} required inputProps={{ maxLength: 100 }} />
          <TextField label="UF" name="state" value={form.state} onChange={handleChange} required placeholder="SP" />
        </Box>
        <TextField label="Ponto de referência" name="reference" value={form.reference} onChange={handleChange}
          inputProps={{ maxLength: 200 }} helperText="Opcional" />
        <Alert severity="info">A localização geográfica é calculada internamente de forma simulada para este protótipo acadêmico.</Alert>
        <Box display="flex" justifyContent="flex-end" gap={2}>
          <Button type="button" color="secondary" onClick={() => { setForm(initialState); setError(null); }} disabled={loading}>Limpar</Button>
          <Button type="submit" variant="contained" disabled={loading}>{loading ? 'Registrando...' : 'Registrar ocorrência'}</Button>
        </Box>
      </Box>
    </Paper>
  );
}
