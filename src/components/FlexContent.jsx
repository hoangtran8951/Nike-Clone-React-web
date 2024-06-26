/* eslint-disable react/prop-types */

const FlexContent = ({endpoint, ifExists}) => {
  return (
    <>
        <div className={`flex items-center justify-between mt-16 lg:flex-col lg:justify-center nike-container ${ifExists ? 'flex-row-reverse' : 'flex-row'}`}>
            <div className='max-w-lg lg:max-w-none w-full grid md:text-center items-center lg:justify-items-center'>
                <h1 className='text-4xl md:text-3xl font-bold text-gradient'>{endpoint.heading}</h1>
                <h1 className='text-5xl lg:text-4xl md:text-3xl sm:text-2xl font-bold text-stone-900 filter drop-shadow-lg'>{endpoint.title}</h1>
                <p className='xl:text-sm my-4 text-stone-900'>{endpoint.text}</p>
                <a href={endpoint.url} className='flex items-center' target={"_blank"} role="button"> 
                    <button type="button" className='button-theme text-slate-200 bg-slate-900 shadow-md shadow-black'>{endpoint.btn}</button>
                </a>
                
            </div>
            <div className='flex items-center justify-center max-w-xl relative lg:max-w-none w-full'>
                <img 
                    src={endpoint.img}
                    alt="highlight/img"
                    className={`w-auto object-fill transitions-theme ${ifExists ? 'h-60 lg:h-56 md:h-52 sm:h-44 xsm:h-36 rotate-6 hover:-rotate-12' : 'h-72 lg:h-64 md:h-60 sm:h-48 xsm:h-40 rotate-[19deg] hover:rotate-12'}`}
                />
            </div>
        </div>
    </>
    
  )
}

export default FlexContent