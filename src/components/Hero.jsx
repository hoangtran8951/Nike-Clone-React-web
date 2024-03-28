import { heroapi } from "../data/data"
import Clips from "./utils/Clips"
import SocialLink from "./utils/SocialLink"

const Hero = () => {
  return (
    <div className='relative h-auto w-auto flex flex-col'>
        <div className='bg-theme clip-path h-[85vh] lg:h-[75vh] md:h-[65vh] sm:h-[55vh] w-auto absolute top-0 left-0 right-0 opacity-100 z-10'></div>
        <div className='relative opacity-100 z-20 grid items-center justify-items-center nike-container'>
            <div className='grid items-center justify-items-center mt-28 md:mt-24'>
                <h1 className='text-6xl lg:text-5xl md:text-4xl sm:text-3xl xsm:text-2xl font-extrabold drop-shadow-sm text-slate-200'>{heroapi.title}</h1>
                <h1 className='text-6xl lg:text-5xl md:text-4xl sm:text-3xl xsm:text-2xl font-extrabold drop-shadow-sm text-slate-200'>{heroapi.subtitle}</h1>
                <button type="button" className="button-theme bg-slate-200 shadow-slate-200 rounded-xl my-5">{heroapi.btntext}</button>
                <div className='grid items-center gap-5 md:gap-3 absolute top-[38vh] lg:top-[33vh] md:top-[30vh] left-[11%] xl:left-0 w-auto h-auto sm:gap-2 sm:top-[27vh] xsm:gap-1 xsm:top-[33vh]'>
                    {heroapi.videos?.map((val,i) => (<Clips key={i} imgsrc={val.imgsrc} clip={val.clip}/>))}
                </div>
                <div className='grid items-center absolute w-auto h-auto right-0 top-[40vh] lg:top-[35vh] md:top-[33vh] sm:top-[30vh] xsm:top-[33vh] gap-5 lg:gap-4 md:gap-3 sm:gap-2 xsm:gap-1'>
                    {heroapi.sociallinks?.map((val,i) => (<SocialLink key={i} icon={val.icon}/>))}
                </div>
            </div>
            <div className='flex items-center'>
                <img
                    src={heroapi.img}
                    alt="hero-img/img"
                    className='w-auto h-[45vh] lg:h-[35vh] md:h-[31vh] sm:h-[21vh] xsm:h-[19vh] transitions-theme -rotate-[25deg] hover:rotate-0 cursor-pointer object-fill'
                />
            </div>
        </div>
        <button></button>
    </div>
  )
}

export default Hero