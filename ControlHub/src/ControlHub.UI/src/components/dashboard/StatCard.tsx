import React from 'react';
import { type LucideIcon } from 'lucide-react';

interface StatCardProps {
  title: string;
  value: string;
  change: string;
  icon: LucideIcon;
  color: string;
}

export const StatCard: React.FC<StatCardProps> = ({ title, value, change, icon: Icon, color }) => (
  <div className="bg-slate-900 p-6 rounded-2xl border border-slate-800 shadow-xl hover:border-slate-700 transition-all group">
    <div className="flex justify-between items-start">
      <div>
        <p className="text-sm font-medium text-slate-400 mb-1 group-hover:text-slate-300 transition-colors">{title}</p>
        <h3 className="text-2xl font-bold text-white tracking-tight">{value}</h3>
        <p className={`text-xs mt-2 font-semibold ${change.startsWith('+') ? 'text-emerald-400' : 'text-rose-400'}`}>
          {change} <span className="text-slate-600 font-normal ml-1 italic">vs last month</span>
        </p>
      </div>
      <div className={`p-3 rounded-xl ${color} text-white shadow-lg`}>
        <Icon size={24} />
      </div>
    </div>
  </div>
);