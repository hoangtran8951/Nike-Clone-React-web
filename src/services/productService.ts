import { Product, PaginatedProducts, CreateProductInput, UpdateProductInput } from '../types/product';

const API_BASE_URL =
  (typeof process !== 'undefined' && process.env?.REACT_APP_API_BASE_URL) ||
  (typeof import.meta !== 'undefined' && (import.meta as any).env?.VITE_API_BASE_URL) ||
  'http://localhost:5000/api';

export class ProductService {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl.replace(/\/+$/, '');
  }

  async getProducts(
    searchTerm?: string,
    pageNumber: number = 1,
    pageSize: number = 10,
    activeOnly?: boolean
  ): Promise<PaginatedProducts> {
    const params = new URLSearchParams();
    if (searchTerm) params.append('searchTerm', searchTerm);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());
    if (activeOnly !== undefined) params.append('activeOnly', activeOnly.toString());

    const url = `${this.baseUrl}/products?${params.toString()}`;
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Accept': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch products: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  async getProductById(id: string): Promise<Product> {
    const url = `${this.baseUrl}/products/${encodeURIComponent(id)}`;
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Accept': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch product with id '${id}': ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  async createProduct(input: CreateProductInput): Promise<Product> {
    const url = `${this.baseUrl}/products`;
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: JSON.stringify(input),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => null);
      const detail = errorData?.detail || errorData?.title || response.statusText;
      throw new Error(`Failed to create product: ${detail}`);
    }

    return response.json();
  }

  async updateProduct(id: string, input: UpdateProductInput): Promise<Product> {
    const url = `${this.baseUrl}/products/${encodeURIComponent(id)}`;
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: JSON.stringify(input),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => null);
      const detail = errorData?.detail || errorData?.title || response.statusText;
      throw new Error(`Failed to update product: ${detail}`);
    }

    return response.json();
  }

  async deleteProduct(id: string): Promise<void> {
    const url = `${this.baseUrl}/products/${encodeURIComponent(id)}`;
    const response = await fetch(url, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error(`Failed to delete product: ${response.status} ${response.statusText}`);
    }
  }
}

export const productService = new ProductService();