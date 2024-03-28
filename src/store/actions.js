export const addToCart = (product) => {
    return {
      type: 'ADD_TO_CART',
      payload: product,
    };
  };
  
export const toggleDrawer = () => {
  return {
    type: 'TOGGLE_DRAWER',
  };
};

export const setSize = (item) => {
  return {
    type: 'SET_SIZE',
    payload: item,
  };
};
export const setQuantity = (item) => {
  return {
    type: 'SET_QUANTITY',
    payload: item,
  };
};

export const removeItem = (id) => {
  return {
    type: 'REMOVE_ITEM',
    payload: id,
  }
}

export const clearCart = () => {
  return {
    type: 'CLEAR_CART'
  }
}
