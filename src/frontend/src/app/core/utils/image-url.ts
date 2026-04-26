export function resolveImageUrl(apiBaseUrl: string, imageUrl: string | null): string | null {
  if (!imageUrl) {
    return null;
  }

  if (/^https?:\/\//i.test(imageUrl)) {
    return imageUrl;
  }

  const apiOrigin = new URL(apiBaseUrl).origin;
  return new URL(imageUrl, apiOrigin).toString();
}
