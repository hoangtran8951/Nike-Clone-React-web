/* eslint-disable no-unused-vars */

import { current } from '@reduxjs/toolkit';
import _ from 'lodash'
import toast from 'react-hot-toast';

const initState = {
    OpenCart: false,
    total: 0,
    totalAmount: 0,
    products: [
    ],
}

const AddToCart = (currentList, newItem) => {
    const testlist = {...newItem, quantity: 1 , size: 40};
    let CloneList = _.cloneDeep(currentList);
    let findpro = CloneList.findIndex(item => (item.id == newItem.id));
    if(findpro !== -1){
        toast.success(`${newItem.title} QTY Increase`)
        CloneList[findpro].quantity++;
        
    }
    else{
        toast.success(`${newItem.title} Added to Cart`)
        CloneList.push(testlist);
    }
    return CloneList;
}
const removeItem = (currentList, id) => {
    let findpro = currentList.findIndex(item => (item.id == id));
    if(findpro !== -1)
        toast.success(`${currentList[findpro].title} removed`)
    let CloneList = _.cloneDeep(currentList.filter((item) => item.id !== id));
    return CloneList;
}

const setSizeOrQuantity = (currentList, id, size, quantity) => {
    if(size === 0)
        currentList = currentList?.map((product) => product.id === id 
            ? { ...product, quantity: quantity}
            : product)
    else
        currentList = currentList?.map((product) => product.id === id 
            ? { ...product, size: size}
            : product)
    
    return currentList;
}

export const rootReducer = (state = initState, action) => {
    console.log(state.products);
    switch(action.type){
        case 'ADD_TO_CART':{
            const idx = state.products.findIndex(item => item.id == action.payload.id); 
            if(idx !== -1){
                let Quantity = state.products[idx].quantity;
                if(Quantity >= 10){
                    toast.error(`${state.products[idx].title} QTY reached max (10 items/product/bill)`)
                    return {
                        ...state
                    }
                }
            }
            return {
                ...state,
                total: +state.total+1,
                totalAmount: +state.totalAmount+parseInt(action.payload.price),
                products: AddToCart(state.products, action.payload)
            }
        }
        case 'TOGGLE_DRAWER':
            return {
                ...state,
                OpenCart: !state.OpenCart,
            };
        case 'SET_QUANTITY':{
            const idx = state.products.findIndex(item => item.id == action.payload.id); 
            let Remove_quantity = state.products[idx].quantity;
            let Price = state.products[idx].price;
            return {
                ...state,
                total: +state.total-Remove_quantity+action.payload.quantity,
                totalAmount: +state.totalAmount+ (action.payload.quantity-Remove_quantity)*parseInt(Price),
                products: setSizeOrQuantity(state.products, action.payload.id, 0, action.payload.quantity),
            }
        }
        case 'SET_SIZE':
            return {
                ...state,
                products: setSizeOrQuantity(state.products, action.payload.id, action.payload.size, 0),
            }
        case 'REMOVE_ITEM': { 
            const idx = state.products.findIndex(item => item.id == action.payload); 
            let Remove_quantity = state.products[idx].quantity;
            let Price = state.products[idx].price;
            return {
                ...state,
                total: +state.total-Remove_quantity,
                totalAmount: +state.totalAmount-(parseInt(Price)*Remove_quantity),
                products: removeItem(state.products, action.payload)
            }
        }
        case 'CLEAR_CART':{ 
            toast.success(`Clean Cart`) 
            return {
                ...state,
                total: 0,
                totalAmount: 0,
                products: []
            }}
        default:
            return state;
    }
}