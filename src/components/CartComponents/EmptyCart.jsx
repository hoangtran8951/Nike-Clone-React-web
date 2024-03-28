import { ChevronLeftIcon } from '@heroicons/react/24/outline';
import emptybar from '../../assets/emptybag.png';
import {useDispatch} from 'react-redux';
import { toggleDrawer } from '../../store/actions';

const EmptyCart = () => {
    const dispatch = useDispatch();
    const dispatchCloseDrawer = () => dispatch(toggleDrawer());
  return (
  <div>
    <img
        src={emptybar}
        alt={`img/empty`}
        className='scale-50 hover:scale-75 transition-all duration-1000'
    />
    <div className='grid items-center px-24'>
        <button className='bg-orange-500 rounded-lg py-2 px-10 text-black font-bold transition-all duration-500 hover:scale-110' onClick={() => dispatchCloseDrawer()}>
            <div className='flex items-center'>
                <ChevronLeftIcon className='w-6 h-auto'/> 
                <div>Back to the store</div>
            </div>
        </button>
    </div>
  </div>
  )
}

export default EmptyCart