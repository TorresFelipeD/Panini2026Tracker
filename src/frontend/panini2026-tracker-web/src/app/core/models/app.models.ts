export interface AlbumOverview {
  summary: AlbumSummary;
  countries: CountryAlbum[];
}

export interface AlbumSummary {
  total: number;
  owned: number;
  missing: number;
  completionPercentage: number;
}

export interface CountryAlbum {
  countryId: string;
  countryCode: string;
  countryName: string;
  flagCode: string;
  total: number;
  owned: number;
  missing: number;
  completionPercentage: number;
  stickers: StickerCard[];
}

export interface StickerCard {
  stickerId: string;
  stickerCode: string;
  displayName: string;
  countryCode: string;
  countryName: string;
  type: string;
  isOwned: boolean;
  hasImage: boolean;
  duplicateCount: number;
  imageUrl: string | null;
  isProvisional: boolean;
}

export interface StickerDetail {
  stickerId: string;
  stickerCode: string;
  displayName: string;
  countryCode: string;
  countryName: string;
  flagCode: string;
  type: string;
  isOwned: boolean;
  duplicateCount: number;
  notes: string | null;
  imageUrl: string | null;
  isProvisional: boolean;
  birthday: string;
  height: string;
  weight: string;
  team: string;
  additionalInfo: Record<string, string>;
  metadata: Record<string, string>;
}

export interface DuplicateItem {
  stickerId: string;
  stickerCode: string;
  displayName: string;
  countryCode: string;
  countryName: string;
  quantity: number;
  isOwned: boolean;
}

export interface SystemLogItem {
  id: string;
  category: string;
  action: string;
  details: string;
  level: string;
  createdAtUtc: string;
}

export interface StickerImageItem {
  stickerId: string;
  stickerCode: string;
  countryCode: string;
  countryName: string;
  displayName: string;
  imageUrl: string;
  uploadedAtUtc: string;
}

export interface AppMeta {
  name: string;
  version: string;
  license: string;
  backendFramework: string;
  frontendTarget: string;
}
