import { useEffect, useState } from "react"
import logo from '../assets/logo.png'
import { MagnifyingGlassIcon, HeartIcon, ShoppingBagIcon } from "@heroicons/react/24/outline";
// import Cart from "./Cart";
import { useSelector, useDispatch } from 'react-redux';
import { toggleDrawer } from '../store/actions';

const Navbar = () => {
    // const productList = useSelector(state => state.products);
    const dispatch = useDispatch();
    const openDrawer = () => dispatch(toggleDrawer());
    const num_cart_pros = useSelector((state) => state.total);
    const [scroll, setScroll] = useState(false);
    const onScrolling = () => {
        if(window.scrollY > 30){
            // console.log(true);
            setScroll(true);
        }
        else{
            // console.log(false);
            setScroll(false);
        }

    }

    useEffect(() => {
        window.addEventListener('scroll', onScrolling);
        return () => {
            window.removeEventListener('scroll', onScrolling);
        }
    },[])
  return (
    <header className={!scroll ? 'absolute top-7 left-0 right-0 opacity-100 z-50' : 'fixed top-0 left-0 right-0 opacity-100 z-[200] blur-effect-theme'}>
        <nav className='flex items-center nike-container'>
            <div className='flex items-center nike-container'>
                <img
                    src={logo}
                    alt='logo/fixed'
                    className={`h-auto w-16 ${scroll && 'brightness-0 my-4'}`}
                />
            </div>
            <div className='flex grid-cols-3 gap-3'>
                <button className='active:scale-90'>
                    <MagnifyingGlassIcon className={`h-auto w-6 ${!scroll && 'text-white'}`}/>
                </button>
                <button className='active:scale-90'>
                    <HeartIcon className={`h-auto w-6 ${!scroll && 'text-white'}`}/>
                </button>
                <button className='active:scale-90 relative' onClick={() => openDrawer()}>
                    <div>
                        <ShoppingBagIcon className={`h-auto w-6 ${!scroll && 'text-white'}`}/>
                        <p className={`absolute w-4 h-4 top-4 leading-tight ml-3 text-[0.65rem] flex items-center font-medium rounded-full justify-center ${scroll ? " bg-black text-slate-200": " bg-white text-slate-900"}`}>{+num_cart_pros < 10 ? num_cart_pros : "9+"}</p>
                    </div>
                </button>
            </div>
        </nav>
    </header>
  )
}

export default Navbar