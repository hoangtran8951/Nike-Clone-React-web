/* eslint-disable react/prop-types */
import {TrashIcon} from "@heroicons/react/24/outline";
import { useDispatch } from 'react-redux';
import { setSize, setQuantity, removeItem} from '../../store/actions';
import '../styles/CartItems.css'

const CartItems = (props) => {
    const { id, title, text, img, color, shadow, price, size, quantity } = props;
    
    const dispatch = useDispatch();
    const dispatchSetSize = (item) => dispatch(setSize(item));
    const dispatchSetQuantity = (item) => dispatch(setQuantity(item));
    const dispatchRemoveItem = (id) => dispatch(removeItem(id));

  return (
    <>
      <div className='px-5 py-4 md:py-2 md:px-4 flex items-center justify-between '>
        <div className='flex items-center'>
          <div className={`relative bg-gradient-to-b ${color} ${shadow} grid items-center justify-items-center rounded-xl py-4 px-5 transition-all duration-700 ease-in-out w-52 lg:w-40 sm:w-28 hover:scale-105`}>
            <img
                  src={`${img}`}
                  alt={`img/item-img/${id}`}
                  className='h-auto w-48 '
            />
            <div className='absolute right-1 top-1 blur-theme-effect bg-white/80 text-black text-xs px-1 rounded'>${price}</div>
          </div>
          <div className='grid items-center ml-3 mb-6 lg:mb-1 md:mb-0'>
            <div className='text-lg md:text-sm sm:text-xs font-bold'>{title}</div>
            <div className='text-lg md:text-sm sm:text-xs'>{text}</div>
            <div className='flex items-center justify-between'>
              <div className="w-14 relative">
                <div className='text-md md:text-xs'>Size</div>
                <select id={size} size="1" onChange={e => { const options = [...e.target.selectedOptions]; const values = options.map(option => option.value); dispatchSetSize({id: id, size: parseFloat(values)})}}>
                  <option hidden value="">{size}</option>
                  {Array.from({length: 23}, (_, index) => {
                    const size = 34 + index * 0.5;
                    return (
                      <option key={index} value={size}>{size % 1 === 0 ? size.toFixed(0) : size.toFixed(1)}</option>
                    );
                  })}
                </select>
              </div>
              <div className="w-28">
                <div className='text-md md:text-xs'>Quantity</div>
                <select name='selectedQuantity' id={quantity} onChange={e => { const options = [...e.target.selectedOptions]; const values = options.map(option => option.value); dispatchSetQuantity({id: id, quantity: parseInt(values)})}}>
                  <option hidden value="">{quantity}</option>
                  {Array.from({length: 10}, (_, index) => {
                    const q = index+1;
                    return (
                      <option key={index} value={q}>{q}</option>
                    );
                  })}
                </select>
              </div>
            </div>
          </div>
        </div>
        <div className="grid items-center gap-5">
          <div className="grid items-center justify-center">
            <h1 className="text-lg lg:text-base text-slate-900 font-medium">${price * quantity}</h1>
          </div>
          <div className="grid items-center justify-center">
            <button type="button" className="bg-theme-cart rounded p-1 lg:p-0.5 grid items-center justify-items-center cursor-pointer" onClick={() => dispatchRemoveItem(id)}>
              <TrashIcon className="w-5 h-5 text-white" />
            </button>
          </div>
        </div>
      </div>
    </>
  )
}

export default CartItems