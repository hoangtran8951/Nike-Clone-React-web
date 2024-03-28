import { FlexContent, Hero, Sales, TopStories, Footer, NavBar, Cart } from './components'
import { highlight, popularsales, sneaker, toprateslaes } from './data/data'
import 'rsuite/dist/rsuite.min.css';
import { Toaster } from 'react-hot-toast';

function App() {

  return (
    <>
      <Cart/>
      <NavBar/>
      <main className='flex flex-col gap-16 relative'>
        <Hero/>
        <Sales endpoint={popularsales} ifExists/>
        <FlexContent endpoint={highlight} ifExists/>
        <Sales endpoint={toprateslaes}/>
        <FlexContent endpoint={sneaker}/>
        <TopStories/>
        <Footer/>
      </main>
      <Toaster/>
    </>
    
  )
}

export default App
