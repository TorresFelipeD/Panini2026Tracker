export function getCountryFlagUrl(flagCode: string): string {
  const normalizedCode = flagCode.trim().toLowerCase();
  return normalizedCode ? `assets/flags/${normalizedCode}.svg` : '';
}
