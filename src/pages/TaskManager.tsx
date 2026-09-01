import React, { useEffect, useState } from 'react';
import {
  CheckSquare,
  Plus,
  RefreshCw,
  Clock,
  User,
  AlertCircle,
  CheckCircle2,
  Filter,
} from 'lucide-react';
import { api, PersianMessages } from '../services/api';
import { TaskDto } from '../types';

export const TaskManager: React.FC = () => {
  const [tasks, setTasks] = useState<TaskDto[]>([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [statusFilter, setStatusFilter] = useState('ALL');
  const [message, setMessage] = useState<string | null>(null);
  const [isError, setIsError] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadTasks();
  }, []);

  const loadTasks = async () => {
    setLoading(true);
    const result = await api.get<TaskDto[]>('/api/tasks');
    setLoading(false);

    if (result.isSuccess && result.value) {
      setTasks(result.value);
      setIsError(false);
    } else {
      setIsError(true);
      setMessage(PersianMessages.error(result.error));
    }
  };

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title) {
      setIsError(true);
      setMessage('عنوان تسک الزامی است.');
      return;
    }

    setLoading(true);
    const result = await api.post<TaskDto>('/api/tasks', {
      title,
      description,
      priority,
    });
    setLoading(false);

    if (result.isSuccess) {
      setIsError(false);
      setMessage('تسک جدید با موفقیت ایجاد شد.');
      setTitle('');
      setDescription('');
      loadTasks();
    } else {
      setIsError(true);
      setMessage(PersianMessages.error(result.error));
    }
  };

  const handleUpdateStatus = async (task: TaskDto, newStatus: string) => {
    const result = await api.put(`/api/tasks/${task.id}`, {
      status: newStatus,
    });

    if (result.isSuccess) {
      loadTasks();
    } else {
      setIsError(true);
      setMessage(PersianMessages.error(result.error));
    }
  };

  const filteredTasks = tasks.filter((t) => {
    if (statusFilter === 'ALL') return true;
    return t.status === statusFilter;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-2 border-b border-slate-200">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-amber-500/10 text-amber-600 rounded-xl">
            <CheckSquare className="w-6 h-6" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-slate-900">مدیریت تسک‌ها و وظایف (Task Manager)</h1>
            <p className="text-xs text-slate-500">برنامه‌ریزی، تخصیص و پیگیری وظایف تیم‌های توسعه و عملیات</p>
          </div>
        </div>
        <button onClick={loadTasks} disabled={loading} className="btn-secondary-nexus text-xs">
          <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
          <span>به‌روزرسانی</span>
        </button>
      </div>

      {/* Alert */}
      {message && (
        <div
          className={`p-3.5 rounded-lg text-xs font-medium flex items-center justify-between gap-2 ${
            isError
              ? 'bg-rose-50 border border-rose-200 text-rose-800'
              : 'bg-emerald-50 border border-emerald-200 text-emerald-800'
          }`}
        >
          <div className="flex items-center gap-2">
            {isError ? (
              <AlertCircle className="w-4 h-4 text-rose-600 shrink-0" />
            ) : (
              <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />
            )}
            <span>{message}</span>
          </div>
          <button onClick={() => setMessage(null)} className="text-slate-400 hover:text-slate-600">
            ×
          </button>
        </div>
      )}

      {/* Create Task Panel */}
      <div className="form-panel">
        <div className="flex items-center gap-2 font-bold text-sm text-slate-800 mb-4 pb-2 border-b border-slate-100 w-full">
          <Plus className="w-4 h-4 text-amber-600" />
          <span>تعریف تسک جدید</span>
        </div>
        <form
          onSubmit={handleCreateTask}
          className="w-full grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 items-end"
        >
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1">عنوان تسک</label>
            <input
              type="text"
              required
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="مثال: به‌روزرسانی قوانین دسترسی"
              className="input-field"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1">توضیحات</label>
            <input
              type="text"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="توضیح مختصر در مورد کار..."
              className="input-field"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1">اولویت</label>
            <select
              value={priority}
              onChange={(e) => setPriority(e.target.value)}
              className="input-field bg-white"
            >
              <option value="Low">پایین (Low)</option>
              <option value="Medium">متوسط (Medium)</option>
              <option value="High">بالا (High)</option>
              <option value="Critical">بحرانی (Critical)</option>
            </select>
          </div>

          <div>
            <button
              type="submit"
              disabled={loading}
              className="w-full btn-primary-nexus py-2 text-xs bg-amber-600 hover:bg-amber-700"
            >
              <Plus className="w-3.5 h-3.5" />
              <span>ثبت تسک</span>
            </button>
          </div>
        </form>
      </div>

      {/* Task Filters & Board List */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-4">
        <div className="flex flex-wrap items-center justify-between gap-3 pb-3 mb-4 border-b border-slate-100">
          <div className="font-bold text-sm text-slate-800">
            لیست تسک‌ها ({filteredTasks.length} مورد)
          </div>
          <div className="flex items-center gap-1.5 text-xs">
            <span className="text-slate-500 font-semibold ml-2">فیلتر وضعیت:</span>
            {['ALL', 'Todo', 'InProgress', 'Done', 'Blocked'].map((status) => (
              <button
                key={status}
                onClick={() => setStatusFilter(status)}
                className={`px-3 py-1 rounded-lg font-medium transition-all ${
                  statusFilter === status
                    ? 'bg-amber-600 text-white shadow-xs'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                {status === 'ALL'
                  ? 'همه'
                  : status === 'Todo'
                  ? 'برای انجام'
                  : status === 'InProgress'
                  ? 'در حال انجام'
                  : status === 'Done'
                  ? 'انجام شد'
                  : 'مسدود'}
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredTasks.map((task) => {
            const priorityColors: Record<string, string> = {
              Low: 'bg-slate-100 text-slate-700 border-slate-200',
              Medium: 'bg-blue-50 text-blue-700 border-blue-200',
              High: 'bg-amber-50 text-amber-800 border-amber-200',
              Critical: 'bg-rose-50 text-rose-800 border-rose-200',
            };

            return (
              <div
                key={task.id}
                className="p-4 rounded-xl border border-slate-200 bg-slate-50/50 hover:bg-white hover:shadow-md transition-all flex flex-col justify-between"
              >
                <div>
                  <div className="flex items-start justify-between gap-2 mb-2">
                    <h3 className="font-bold text-sm text-slate-900 leading-snug">{task.title}</h3>
                    <span
                      className={`text-[10px] px-2 py-0.5 rounded-full font-bold border uppercase shrink-0 ${
                        priorityColors[task.priority] || 'bg-slate-100 text-slate-600'
                      }`}
                    >
                      {task.priority}
                    </span>
                  </div>
                  {task.description && (
                    <p className="text-xs text-slate-600 leading-relaxed mb-3">
                      {task.description}
                    </p>
                  )}
                </div>

                <div className="pt-3 border-t border-slate-200/60 flex items-center justify-between gap-2 text-xs">
                  <div className="flex items-center gap-1 text-slate-500">
                    <User className="w-3.5 h-3.5" />
                    <span>{task.assignedUserName || 'واگذار نشده'}</span>
                  </div>

                  <select
                    value={task.status}
                    onChange={(e) => handleUpdateStatus(task, e.target.value)}
                    className="text-xs bg-white border border-slate-300 rounded-md px-2 py-1 focus:outline-none focus:border-blue-500 font-semibold"
                  >
                    <option value="Todo">برای انجام</option>
                    <option value="InProgress">در حال انجام</option>
                    <option value="Done">انجام شد</option>
                    <option value="Blocked">مسدود شده</option>
                  </select>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
