import './App.css'

function App() {
  return (
    <div className="relative flex items-center justify-center min-h-screen bg-slate-950 overflow-hidden">
      
      {/* Контейнер с общим размытием */}
      <div className="absolute inset-0 z-0 filter blur-[75px] opacity-60">
        <div className="absolute top-[10%] left-[10%] w-[40vw] h-[40vw] bg-indigo-600 rounded-full animate-float" />
        <div className="absolute bottom-[10%] right-[10%] w-[35vw] h-[35vw] bg-fuchsia-600 rounded-full animate-float [animation-delay:7s]" />
      </div>

      {/* Основной контент */}
      <div className="relative z-10 w-full m-[5%] min-h-[90vh] bg-white/5 backdrop-blur-xl rounded-[15px] shadow-2xl flex items-center justify-center border border-white/10">
      </div>
      
    </div>
  );
}

export default App
