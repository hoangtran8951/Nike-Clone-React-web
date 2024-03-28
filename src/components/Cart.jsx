/* eslint-disable react/prop-types */
import {useSelector, useDispatch} from 'react-redux';
import { clearCart, toggleDrawer } from '../store/actions';
import { XMarkIcon} from '@heroicons/react/24/solid';
import CartItems from './CartComponents/CartItems';
import EmptyCart from './CartComponents/EmptyCart';

const Cart = () => {
    const openDrawer = useSelector((state) => state.OpenCart);
    const totalPros = useSelector((state) => state.total);
    const productList = useSelector((state) => state.products);
    const total = useSelector(state => state.totalAmount);
    const dispatch = useDispatch();
    const dispatchCloseDrawer = () => dispatch(toggleDrawer());
    const dispatchClearCart = () => dispatch(clearCart());
    console.log(productList);
    return (
      <>
        <div className={`fixed top-0 bottom-0 left-0 right-0 blur-effect-theme duration-100 h-screen w-full opacity-100 z-[250] ${openDrawer ? "visible translate-x-0 opacity-100" : "invisible translate-x-9 opacity-0"}`}>
          <div className={`absolute right-0 w-full max-w-xl h-screen blur-effect-theme duration-300 opacity-500 ${openDrawer ? "visible translate-x-0 opacity-100" : "invisible translate-x-8 opacity-0"}`}>
            <div className={`h-10 bg-white w-full`}>
              <div className='flex items-center nike-container'> 
                <div className='flex items-center nike-container'> 
                  <p className={`text-sl flex items-center p-0.5 font-medium rounded-lg justify-center bg-black text-slate-200`}> {totalPros} items</p>
                </div>
                <div className='flex grid-cols-2 gap-3 mt-2'>
                  <button type='button' className='active:scale-110 transition-all duration-300 relative' onClick={() => dispatchClearCart()}>
                        <i className="fa-solid fa-cart-shopping text-black scale-150"></i>
                        <i className="fa-sharp fa-solid fa-xmark text-white absolute top-1 left-0.5 shadow w-4 h-4 text-[0.65rem] leading-tight font-medium flex items-center justify-center cursor-pointer hover:scale-110 transition-all duration-300"></i>
                  </button>
                  <button className='active:scale-110 relative' onClick={() => dispatchCloseDrawer()}>
                    <div>
                        <XMarkIcon className={`h-auto w-6 rounded-sm bg-black text-white`}/>
                    </div>
                  </button>
                </div>
              </div>
            </div>
            {totalPros === 0 
            ? <div className='h-full w-full grid items-center justify-center'>
                <EmptyCart/>
              </div>
            : <>
              <div className='overflow-y-scroll h-[81vh] scroll-smooth scroll-hidden'>
                {productList?.map((val, id) => (
                  <CartItems key={id} {...val}/>
                ))}
              </div>
              <div className='fixed bottom-0 w-full py-2 px-5 bg-white grid items-center'>
                  <div className='flex items-center justify-between'>
                    <h1 className='text-lg font-bold uppercase'>SubTotal</h1>
                    <h1 className='text-sm rounded bg-theme-cart text-white px-1 py-0.5'>${total}</h1>
                  </div>
                  <div className="grid items-center gap-2">
                  <p className="text-sm font-medium text-center">Taxes and Shipping Will Calculate At Shipping</p>
                  <button type="button" className="button-theme bg-theme-cart text-white">Check Out</button>
                </div>
              </div>
              </>
            }
            
            
          </div>
        </div>
      </>
      
    );
};


export default Cart