import type { EntityDto } from '@abp/ng.core';

export interface CreatePantryItemDto extends UpdatePantryItemDto {
  productId: string;
}

export interface PantryItemDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  quantity?: number | null;
  unit?: string | null;
  expirationDate?: string | null;
  isActive?: boolean;
}

export interface UpdatePantryItemDto {
  quantity?: number | null;
  unit?: string | null;
  expirationDate?: string | null;
}
