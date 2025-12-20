import React from 'react';
import { Search, Bell, Sun } from 'lucide-react';

export const Header: React.FC = () => {
  return (
    <header className="h-16 bg-slate-900/80 backdrop-blur-md border-b border-slate-800 flex items-center justify-between px-8 sticky top-0 z-10">
      <div className="flex items-center bg-slate-800/50 border border-slate-700 px-3 py-1.5 rounded-xl w-full max-w-md focus-within:border-blue-500 transition-all">
        <Search size={18} className="text-slate-500 mr-2" />
        <input 
          type="text" 
          placeholder="Tìm kiếm hệ thống..." 
          className="bg-transparent border-none focus:outline-none text-sm w-full text-slate-200 placeholder:text-slate-600" 
        />
      </div>
      
      <div className="flex items-center space-x-4 ml-4">
        <button className="p-2 text-slate-400 hover:bg-slate-800 hover:text-amber-400 rounded-full transition-all">
          <Sun size={20} />
        </button>
        
        <button className="relative p-2 text-slate-400 hover:bg-slate-800 hover:text-white rounded-full transition-all">
          <Bell size={20} />
          <span className="absolute top-2 right-2 w-2 h-2 bg-blue-500 rounded-full border-2 border-slate-900"></span>
        </button>
        
        <div className="h-8 w-px bg-slate-800 mx-2 hidden md:block"></div>
        
        <button className="hidden md:block bg-slate-800 text-slate-200 border border-slate-700 px-4 py-2 rounded-lg text-sm font-medium hover:bg-slate-700 transition-colors">
          Hệ thống
        </button>
      </div>
    </header>
  );
};