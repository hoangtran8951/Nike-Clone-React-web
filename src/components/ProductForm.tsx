import React, { useState } from 'react';
import { Product, CreateProductInput, UpdateProductInput } from '../types/product';
import { productService } from '../services/productService';

interface ProductFormProps {
  product?: Product | null;
  onSuccess: () => void;
  onCancel: () => void;
}

export const ProductForm: React.FC<ProductFormProps> = ({ product, onSuccess, onCancel }) => {
  const isEditing = Boolean(product);
  const [name, setName] = useState(product?.name || '');
  const [sku, setSku] = useState(product?.sku || '');
  const [description, setDescription] = useState(product?.description || '');
  const [price, setPrice] = useState(product?.price?.toString() || '0.00');
  const [stockQuantity, setStockQuantity] = useState(product?.stockQuantity?.toString() || '0');
  const [isActive, setIsActive] = useState(product?.isActive ?? true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);

    const parsedPrice = parseFloat(price);
    const parsedStock = parseInt(stockQuantity, 10);

    if (isNaN(parsedPrice) || parsedPrice <= 0) {
      setError('Price must be greater than zero.');
      setSubmitting(false);
      return;
    }

    if (isNaN(parsedStock) || parsedStock < 0) {
      setError('Stock quantity cannot be negative.');
      setSubmitting(false);
      return;
    }

    try {
      if (isEditing && product) {
        const updateInput: UpdateProductInput = {
          name,
          description,
          price: parsedPrice,
          sku,
          stockQuantity: parsedStock,
          isActive,
        };
        await productService.updateProduct(product.id, updateInput);
      } else {
        const createInput: CreateProductInput = {
          name,
          description,
          price: parsedPrice,
          sku,
          stockQuantity: parsedStock,
        };
        await productService.createProduct(createInput);
      }
      onSuccess();
    } catch (err: any) {
      setError(err.message || 'An unexpected error occurred while saving the product.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div style={{ maxWidth: '600px', margin: '1rem auto', padding: '1.5rem', border: '1px solid #cbd5e1', borderRadius: '8px' }}>
      <h2 style={{ marginTop: 0 }}>{isEditing ? 'Edit Product' : 'Add New Product'}</h2>
      {error && (
        <div style={{ padding: '0.75rem', backgroundColor: '#fee2e2', color: '#b91c1c', borderRadius: '4px', marginBottom: '1rem' }}>
          {error}
        </div>
      )}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '1rem' }}>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.25rem' }}>Name</label>
          <input
            required
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }}
          />
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.25rem' }}>SKU</label>
          <input
            required
            type="text"
            value={sku}
            onChange={(e) => setSku(e.target.value)}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }}
          />
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.25rem' }}>Description</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }}
          />
        </div>

        <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{ flex: 1 }}>
            <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.25rem' }}>Price ($)</label>
            <input
              required
              type="number"
              step="0.01"
              min="0.01"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }}
            />
          </div>
          <div style={{ flex: 1 }}>
            <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.25rem' }}>Stock Quantity</label>
            <input
              required
              type="number"
              min="0"
              value={stockQuantity}
              onChange={(e) => setStockQuantity(e.target.value)}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }}
            />
          </div>
        </div>

        {isEditing && (
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
              <input
                type="checkbox"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
              />
              <span style={{ fontWeight: 600 }}>Active in catalog</span>
            </label>
          </div>
        )}

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem' }}>
          <button
            type="button"
            onClick={onCancel}
            disabled={submitting}
            style={{ padding: '0.5rem 1rem', borderRadius: '4px', border: '1px solid #ccc', cursor: 'pointer' }}
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={submitting}
            style={{
              padding: '0.5rem 1rem',
              backgroundColor: '#0284c7',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              cursor: submitting ? 'not-allowed' : 'pointer',
              fontWeight: 600,
            }}
          >
            {submitting ? 'Saving...' : 'Save Product'}
          </button>
        </div>
      </form>
    </div>
  );
};