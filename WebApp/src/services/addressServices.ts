import api from '../api';

export interface PostalCodeAddress {
  postalCode: string;
  street: string;
  neighborhood: string;
  city: string;
  state: string;
}

export async function lookupPostalCode(postalCode: string) {
  const digits = postalCode.replace(/\D/g, '');
  const { data } = await api.get<PostalCodeAddress>(`/addresses/postal-code/${digits}`);
  return data;
}
