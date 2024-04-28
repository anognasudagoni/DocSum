import React from 'react'
import Home from './components/Home'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Summarise from './components/Summarise'

const App = () => {
  return (
    <div>
      <BrowserRouter >
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/summarise" element={<Summarise />} />
          </Routes>
      </BrowserRouter>
      
    </div>
  )
}

export default App