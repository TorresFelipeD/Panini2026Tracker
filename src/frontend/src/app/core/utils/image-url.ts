export function resolveImageUrl(apiBaseUrl: string, imageUrl: string | null): string | null {
  if (!imageUrl) {
    return null;
  }

  if (/^https?:\/\//i.test(imageUrl)) {
    return imageUrl;
  }

  const apiOrigin = new URL(apiBaseUrl, window.location.origin).origin;
  const resolvedUrl = new URL(imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`, apiOrigin).toString();

  if (resolvedUrl.startsWith(window.location.origin) && imageUrl.includes('/uploads/')) {
    return new URL(imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`, apiOrigin).toString();
  }

  return resolvedUrl;
}
