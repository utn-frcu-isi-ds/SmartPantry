import type { ExternalProductLookupStatus } from './external-product-lookup-status.enum';

export interface BarcodeLookupInputDto {
  barcode: string;
}

export interface ExternalProductDto {
  code?: string;
  name?: string | null;
  brand?: string | null;
  imageUrl?: string | null;
}

export interface ExternalProductLookupDto {
  status?: ExternalProductLookupStatus;
  product?: ExternalProductDto | null;
}
