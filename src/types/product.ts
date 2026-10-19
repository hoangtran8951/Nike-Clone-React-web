export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  sku: string;
  stockQuantity: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface PaginatedProducts {
  items: Product[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateProductInput {
  name: string;
  description: string;
  price: number;
  sku: string;
  stockQuantity: number;
}

export interface UpdateProductInput {
  name: string;
  description: string;
  price: number;
  sku: string;
  stockQuantity: number;
  isActive: boolean;
}