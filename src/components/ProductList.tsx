import React, { useEffect, useState, useCallback } from 'react';
import { Product, PaginatedProducts } from '../types/product';
import { productService } from '../services/productService';

interface ProductListProps {
  onSelectProduct?: (product: Product) => void;
  onAddNewProduct?: () => void;
}

export const ProductList: React.FC<ProductListProps> = ({ onSelectProduct, onAddNewProduct }) => {
  const [data, setData] = useState<PaginatedProducts | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    setErrorMessage(null);
    try {
      const result = await productService.getProducts(searchTerm, pageNumber, 10);
      setData(result);
    } catch (err: any) {
      setErrorMessage(err.message || 'Error loading products from backend database.');
    } finally {
      setLoading(false);
    }
  }, [searchTerm, pageNumber]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`Are you sure you want to delete "${name}"?`)) {
      return;
    }
    try {
      await productService.deleteProduct(id);
      await fetchProducts();
    } catch (err: any) {
      setErrorMessage(err.message || 'Error deleting product.');
    }
  };

  return (
    <div className="product-list-container" style={{ padding: '1.5rem', fontFamily: 'sans-serif' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '1.75rem' }}>Product Catalog</h1>
          <p style={{ margin: '0.25rem 0 0 0', color: '#666' }}>Connected to .NET 8 Web API &amp; SQLite</p>
        </div>
        {onAddNewProduct && (
          <button
            onClick={onAddNewProduct}
            style={{
              padding: '0.5rem 1rem',
              backgroundColor: '#0284c7',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
              fontWeight: 600,
            }}
          >
            + Add Product
          </button>
        )}
      </header>

      <div style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem' }}>
        <input
          type="text"
          placeholder="Search by name, SKU, or description..."
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            setPageNumber(1);
          }}
          style={{
            flex: 1,
            padding: '0.5rem 0.75rem',
            border: '1px solid #ccc',
            borderRadius: '4px',
            fontSize: '1rem',
          }}
        />
        <button
          onClick={() => fetchProducts()}
          style={{
            padding: '0.5rem 1rem',
            backgroundColor: '#475569',
            color: '#fff',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer',
          }}
        >
          Refresh
        </button>
      </div>

      {errorMessage && (
        <div
          role="alert"
          style={{
            padding: '0.75rem 1rem',
            backgroundColor: '#fee2e2',
            color: '#b91c1c',
            border: '1px solid #f87171',
            borderRadius: '4px',
            marginBottom: '1rem',
          }}
        >
          {errorMessage}
        </div>
      )}

      {loading ? (
        <p style={{ color: '#666' }}>Loading products from SQLite database...</p>
      ) : data && data.items.length > 0 ? (
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead>
              <tr style={{ backgroundColor: '#f1f5f9', borderBottom: '2px solid #cbd5e1' }}>
                <th style={{ padding: '0.75rem' }}>SKU</th>
                <th style={{ padding: '0.75rem' }}>Name</th>
                <th style={{ padding: '0.75rem' }}>Price</th>
                <th style={{ padding: '0.75rem' }}>Stock</th>
                <th style={{ padding: '0.75rem' }}>Status</th>
                <th style={{ padding: '0.75rem', textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((prod) => (
                <tr key={prod.id} style={{ borderBottom: '1px solid #e2e8f0' }}>
                  <td style={{ padding: '0.75rem', fontFamily: 'monospace' }}>{prod.sku}</td>
                  <td style={{ padding: '0.75rem' }}>
                    <div style={{ fontWeight: 600 }}>{prod.name}</div>
                    <small style={{ color: '#64748b' }}>{prod.description}</small>
                  </td>
                  <td style={{ padding: '0.75rem', fontWeight: 600 }}>${prod.price.toFixed(2)}</td>
                  <td style={{ padding: '0.75rem' }}>{prod.stockQuantity}</td>
                  <td style={{ padding: '0.75rem' }}>
                    <span
                      style={{
                        padding: '0.2rem 0.5rem',
                        borderRadius: '9999px',
                        fontSize: '0.75rem',
                        backgroundColor: prod.isActive ? '#dcfce7' : '#f3f4f6',
                        color: prod.isActive ? '#15803d' : '#4b5563',
                      }}
                    >
                      {prod.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td style={{ padding: '0.75rem', textAlign: 'right' }}>
                    {onSelectProduct && (
                      <button
                        onClick={() => onSelectProduct(prod)}
                        style={{
                          marginRight: '0.5rem',
                          padding: '0.25rem 0.6rem',
                          backgroundColor: '#e2e8f0',
                          border: 'none',
                          borderRadius: '4px',
                          cursor: 'pointer',
                        }}
                      >
                        Edit
                      </button>
                    )}
                    <button
                      onClick={() => handleDelete(prod.id, prod.name)}
                      style={{
                        padding: '0.25rem 0.6rem',
                        backgroundColor: '#ef4444',
                        color: '#fff',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer',
                      }}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginTop: '1rem',
            }}
          >
            <span style={{ color: '#64748b', fontSize: '0.875rem' }}>
              Showing {data.items.length} of {data.totalCount} products (Page {data.pageNumber} of{' '}
              {data.totalPages || 1})
            </span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button
                disabled={!data.hasPreviousPage}
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                style={{
                  padding: '0.4rem 0.8rem',
                  borderRadius: '4px',
                  border: '1px solid #cbd5e1',
                  cursor: data.hasPreviousPage ? 'pointer' : 'not-allowed',
                  opacity: data.hasPreviousPage ? 1 : 0.5,
                }}
              >
                Previous
              </button>
              <button
                disabled={!data.hasNextPage}
                onClick={() => setPageNumber((p) => p + 1)}
                style={{
                  padding: '0.4rem 0.8rem',
                  borderRadius: '4px',
                  border: '1px solid #cbd5e1',
                  cursor: data.hasNextPage ? 'pointer' : 'not-allowed',
                  opacity: data.hasNextPage ? 1 : 0.5,
                }}
              >
                Next
              </button>
            </div>
          </div>
        </div>
      ) : (
        <p style={{ color: '#64748b' }}>No products found.</p>
      )}
    </div>
  );
};