import { useState } from 'react';
import { Zap, Users, Activity, ShieldCheck } from 'lucide-react';
// Đảm bảo các đường dẫn này khớp chính xác với cấu trúc thư mục của bạn
import { Sidebar } from './components/dashboard/Sidebar';
import { StatCard } from './components/dashboard/StatCard';
import { Header } from './components/dashboard/Header';

function App() {
  const [activeTab, setActiveTab] = useState('Dashboard');
  const [isSidebarOpen, setSidebarOpen] = useState(true);

  // Dữ liệu mẫu cho các thẻ thống kê
  const stats = [
    { 
      title: 'Tổng Lưu Lượng', 
      value: '2.4TB', 
      change: '+12.5%', 
      icon: Zap, 
      color: 'bg-amber-500' 
    },
    { 
      title: 'Người Dùng Online', 
      value: '1,284', 
      change: '+3.2%', 
      icon: Users, 
      color: 'bg-blue-600' 
    },
    { 
      title: 'Độ Trễ Hệ Thống', 
      value: '24ms', 
      change: '-5.1%', 
      icon: Activity, 
      color: 'bg-emerald-500' 
    },
    { 
      title: 'Cảnh Báo Bảo Mật', 
      value: '0', 
      change: '0%', 
      icon: ShieldCheck, 
      color: 'bg-rose-500' 
    },
  ];

  return (
    <div className="min-h-screen bg-slate-50 flex text-slate-900 font-sans">
      {/* 1. Thanh Menu bên trái */}
      <Sidebar 
        isOpen={isSidebarOpen} 
        setIsOpen={setSidebarOpen} 
        activeTab={activeTab} 
        setActiveTab={setActiveTab} 
      />

      {/* 2. Nội dung chính bên phải */}
      <main className={`flex-1 transition-all duration-300 ${isSidebarOpen ? 'ml-64' : 'ml-20'}`}>
        {/* Thanh tìm kiếm và thông báo phía trên */}
        <Header />
        
        <div className="p-8">
          {/* Tiêu đề trang */}
          <div className="mb-8">
            <h2 className="text-2xl font-bold text-slate-900">Tổng quan hệ thống</h2>
            <p className="text-slate-500 text-sm">Chào mừng trở lại! Dưới đây là tình trạng hiện tại của ControlHub.</p>
          </div>

          {/* Hàng các thẻ thống kê nhanh */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {stats.map((stat, idx) => (
              <StatCard 
                key={idx} 
                title={stat.title}
                value={stat.value}
                change={stat.change}
                icon={stat.icon}
                color={stat.color}
              />
            ))}
          </div>

          {/* Khu vực nội dung chi tiết (Placeholder) */}
          <div className="mt-8 grid grid-cols-1 lg:grid-cols-3 gap-8">
             <div className="lg:col-span-2 bg-white h-64 rounded-2xl border border-dashed border-slate-300 flex items-center justify-center text-slate-400">
                Biểu đồ lưu lượng sẽ hiển thị ở đây
             </div>
             <div className="bg-white h-64 rounded-2xl border border-dashed border-slate-300 flex items-center justify-center text-slate-400">
                Hoạt động gần đây
             </div>
          </div>
        </div>
      </main>
    </div>
  );
}

export default App;