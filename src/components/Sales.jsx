/* eslint-disable react/prop-types */
import Title from "./utils/Title";
import Item from "./utils/Item"


// eslint-disable-next-line react/prop-types
const Sales = ( {ifExists, endpoint}) => {
  return (
    <>
      {/* <div className='nike-container relative h-auto w-auto flex flex-col mt-44 lg:mt-40 md:mt-28 sm:mt-24 xsm:mt-20'> */}
      <div className='nike-container mt-44 lg:mt-40 md:mt-28 sm:mt-24 xsm:mt-20'>
        <Title title = {endpoint.title}/>
        <br/>
        <div className={`grid items-center justify-items-center gap-7 lg:gap-5 mt-7 ${ifExists ? 'grid-cols-3 xl:grid-cols-2 sm:grid-cols-1' : 'grid-cols-4 xl:grid-cols-3 md:grid-cols-2 sm:grid-cols-1'}`}>
          {endpoint.items?.map((item, i) => (
            <Item {...item} key={i} ifExists={ifExists} />
          ))}
        </div>
      </div>
    </>
  )
}

export default Sales