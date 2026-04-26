export function getCountryFlagUrl(flagCode: string | null | undefined): string {
  const normalizedCode = flagCode?.trim().toLowerCase() ?? '';
  return normalizedCode ? `assets/flags/${normalizedCode}.svg` : '';
}
