import { footerAPI } from "../data/data"
import { useState, useEffect } from "react"

const Footer = () => {
    const [year, setYear] = useState(2023)
    useEffect(() => {
        setYear(new Date().getFullYear());
    },[])
  return (
    <>
        <footer className='bg-theme pt-7 pb-5'>
            <div className='nike-container text-slate-200'>
                <div className='grid items-start grid-cols-3'>
                    {footerAPI.titles?.map((val, id) => (
                        <div key={id} className='grid items-center justify-items-center md:justify-items-start'>
                            <h1 className='text-lg lg:text-base md:text-sm uppercase font-semibold'>{val.title}</h1>
                        </div>
                    ))}
                    {footerAPI.links?.map((val, id) => (
                        <ul key={id} className='grid items-center justify-items-center md:justify-items-start gap-1'>
                            {val.map((content, id) => (
                                <li key={id} className='text-sm sm:text-xs'>{content.link}</li>
                            ))}
                        </ul>
                    ))}
                </div>
            <div className='mt-5 text-center'>
                <p className='text-sm md:text-center'>Copyright<sup className='text-base font-bold'>&copy;</sup> All Reserved Rights <span className='font-semibold'>DEVELOPED IN {year}</span></p>
            </div>
            </div>
        </footer>
    </>
  )
}

export default Footer