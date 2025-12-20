import React from 'react';
import { 
  LayoutDashboard, Settings, Users, Activity, X, Menu, 
  type LucideIcon 
} from 'lucide-react';

interface SidebarProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  activeTab: string;
  setActiveTab: (tab: string) => void;
}

interface SidebarItemProps {
  icon: LucideIcon;
  label: string;
  active: boolean;
  onClick: () => void;
  isOpen: boolean;
}

const SidebarItem: React.FC<SidebarItemProps> = ({ icon: Icon, label, active, onClick, isOpen }) => (
  <button
    onClick={onClick}
    className={`w-full flex items-center space-x-3 px-4 py-3 rounded-lg transition-all ${
      active 
        ? 'bg-blue-600 text-white shadow-lg shadow-blue-500/50' 
        : 'text-slate-400 hover:bg-slate-800 hover:text-white'
    }`}
  >
    <Icon size={20} />
    {isOpen && <span className="font-medium">{label}</span>}
  </button>
);

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, setIsOpen, activeTab, setActiveTab }) => {
  const menuItems = [
    { id: 'Dashboard', label: 'Bảng Điều Khiển', icon: LayoutDashboard },
    { id: 'Users', label: 'Người Dùng', icon: Users },
    { id: 'Performance', label: 'Hiệu Suất', icon: Activity },
    { id: 'Settings', label: 'Cài Đặt', icon: Settings },
  ];

  return (
    <aside className={`${isOpen ? 'w-64' : 'w-20'} bg-slate-900 border-r border-slate-800 transition-all duration-300 flex flex-col fixed h-full z-20`}>
      <div className="p-6 flex items-center justify-between">
        {isOpen && <h1 className="text-xl font-bold bg-gradient-to-r from-blue-400 to-indigo-400 bg-clip-text text-transparent">ControlHub</h1>}
        <button onClick={() => setIsOpen(!isOpen)} className="p-1 hover:bg-slate-800 text-slate-400 rounded-md transition-colors">
          {isOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </div>

      <nav className="flex-1 px-4 space-y-2 mt-4">
        {menuItems.map((item) => (
          <SidebarItem 
            key={item.id}
            icon={item.icon}
            label={item.label}
            active={activeTab === item.id}
            onClick={() => setActiveTab(item.id)}
            isOpen={isOpen}
          />
        ))}
      </nav>
      
      <div className="p-4 border-t border-slate-800">
        <div className="flex items-center space-x-3 p-2">
          <div className="w-8 h-8 rounded-full bg-slate-800 flex items-center justify-center font-bold text-blue-400">AD</div>
          {isOpen && (
            <div className="overflow-hidden">
              <p className="text-sm font-bold text-slate-200 truncate">Admin</p>
              <p className="text-xs text-slate-500 truncate">Online</p>
            </div>
          )}
        </div>
      </div>
    </aside>
  );
};