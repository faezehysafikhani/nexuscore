import React, { useEffect, useState } from 'react';
import {
  Ticket as TicketIcon,
  Plus,
  RefreshCw,
  MessageCircle,
  Clock,
  Send,
  AlertCircle,
  CheckCircle2,
  ChevronLeft,
} from 'lucide-react';
import { api, PersianMessages } from '../services/api';
import { TicketDto, TicketCommentDto, UserDto } from '../types';

interface TicketingProps {
  currentUser: UserDto | null;
}

export const Ticketing: React.FC<TicketingProps> = ({ currentUser }) => {
  const [tickets, setTickets] = useState<TicketDto[]>([]);
  const [selectedTicket, setSelectedTicket] = useState<TicketDto | null>(null);
  const [comments, setComments] = useState<TicketCommentDto[]>([]);
  const [newComment, setNewComment] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [message, setMessage] = useState<string | null>(null);
  const [isError, setIsError] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadTickets();
  }, []);

  useEffect(() => {
    if (selectedTicket) {
      loadComments(selectedTicket.id);
    }
  }, [selectedTicket]);

  const loadTickets = async () => {
    setLoading(true);
    const res = await api.get<TicketDto[]>('/api/tickets');
    setLoading(false);
    if (res.isSuccess && res.value) {
      setTickets(res.value);
      if (!selectedTicket && res.value.length > 0) {
        setSelectedTicket(res.value[0]);
      }
    }
  };

  const loadComments = async (ticketId: string) => {
    const res = await api.get<TicketCommentDto[]>(`/api/tickets/${ticketId}/comments`);
    if (res.isSuccess && res.value) {
      setComments(res.value);
    }
  };

  const handleCreateTicket = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title || !description) {
      setIsError(true);
      setMessage('عنوان و توضیحات تیکت الزامی است.');
      return;
    }

    setLoading(true);
    const res = await api.post<TicketDto>('/api/tickets', {
      title,
      description,
      priority,
    });
    setLoading(false);

    if (res.isSuccess && res.value) {
      setIsError(false);
      setMessage('تیکت با موفقیت ثبت شد.');
      setTitle('');
      setDescription('');
      await loadTickets();
      setSelectedTicket(res.value);
    } else {
      setIsError(true);
      setMessage(PersianMessages.error(res.error));
    }
  };

  const handleSendComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || !selectedTicket) return;

    const res = await api.post<TicketCommentDto>(
      `/api/tickets/${selectedTicket.id}/comments`,
      {
        comment: newComment.trim(),
        authorUserId: currentUser?.id || '33333333-3333-3333-3333-333333333333',
      }
    );

    if (res.isSuccess && res.value) {
      setComments((prev) => [...prev, res.value!]);
      setNewComment('');
      loadTickets();
    }
  };

  const handleStatusChange = async (newStatus: string) => {
    if (!selectedTicket) return;
    const res = await api.put<TicketDto>(`/api/tickets/${selectedTicket.id}`, {
      status: newStatus,
    });
    if (res.isSuccess && res.value) {
      setSelectedTicket(res.value);
      loadTickets();
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-2 border-b border-slate-200">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-rose-600/10 text-rose-600 rounded-xl">
            <TicketIcon className="w-6 h-6" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-slate-900">سامانه تیکتینگ و پشتیبانی (Support Desk)</h1>
            <p className="text-xs text-slate-500">ثبت درخواست‌ها، رسیدگی به تیکت‌های پشتیبانی و ثبت نظرات</p>
          </div>
        </div>
        <button onClick={loadTickets} disabled={loading} className="btn-secondary-nexus text-xs">
          <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
          <span>به‌روزرسانی تیکت‌ها</span>
        </button>
      </div>

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

      {/* Create Ticket Panel */}
      <div className="form-panel">
        <div className="flex items-center gap-2 font-bold text-sm text-slate-800 mb-4 pb-2 border-b border-slate-100 w-full">
          <Plus className="w-4 h-4 text-rose-600" />
          <span>ثبت تیکت پشتیبانی جدید</span>
        </div>
        <form
          onSubmit={handleCreateTicket}
          className="w-full grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 items-end"
        >
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1">عنوان تیکت</label>
            <input
              type="text"
              required
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="مثال: مشکل در همگام‌سازی دسترسی‌ها"
              className="input-field"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1">شرح درخواست</label>
            <input
              type="text"
              required
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="توضیح کامل مورد یا درخواست..."
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
              <option value="Urgent">فوری (Urgent)</option>
            </select>
          </div>

          <div>
            <button
              type="submit"
              disabled={loading}
              className="w-full btn-primary-nexus py-2 text-xs bg-rose-600 hover:bg-rose-700"
            >
              <Plus className="w-3.5 h-3.5" />
              <span>ثبت تیکت</span>
            </button>
          </div>
        </form>
      </div>

      {/* Ticket List and Details Split View */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Tickets List */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-4 space-y-3">
          <div className="font-bold text-sm text-slate-800 pb-2 border-b border-slate-100 flex items-center justify-between">
            <span>تیکت‌ها ({tickets.length})</span>
          </div>

          <div className="space-y-2 max-h-[500px] overflow-y-auto">
            {tickets.map((t) => {
              const isSelected = selectedTicket?.id === t.id;
              return (
                <div
                  key={t.id}
                  onClick={() => setSelectedTicket(t)}
                  className={`p-3.5 rounded-xl border cursor-pointer transition-all ${
                    isSelected
                      ? 'bg-rose-50/70 border-rose-300 shadow-xs'
                      : 'bg-slate-50/60 border-slate-200 hover:bg-slate-100'
                  }`}
                >
                  <div className="flex items-start justify-between gap-2 mb-1.5">
                    <div className="font-bold text-xs text-slate-900 line-clamp-1">{t.title}</div>
                    <span className="text-[10px] px-2 py-0.5 rounded-full font-bold bg-white text-slate-700 border border-slate-200">
                      {t.priority}
                    </span>
                  </div>
                  <p className="text-[11px] text-slate-600 line-clamp-2 leading-relaxed mb-2">
                    {t.description}
                  </p>
                  <div className="flex items-center justify-between text-[10px] text-slate-400">
                    <span className="flex items-center gap-1">
                      <MessageCircle className="w-3 h-3" />
                      {t.commentsCount} نظر
                    </span>
                    <span
                      className={`px-2 py-0.5 rounded font-semibold ${
                        t.status === 'Resolved'
                          ? 'bg-emerald-100 text-emerald-800'
                          : t.status === 'InProgress'
                          ? 'bg-amber-100 text-amber-800'
                          : 'bg-blue-100 text-blue-800'
                      }`}
                    >
                      {t.status}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Ticket Details & Comments Area */}
        <div className="lg:col-span-2 bg-white rounded-xl border border-slate-200 shadow-sm p-5 flex flex-col justify-between">
          {selectedTicket ? (
            <div className="space-y-5">
              {/* Ticket Top Info */}
              <div>
                <div className="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-slate-100">
                  <div>
                    <span className="text-[11px] font-mono text-slate-400 block mb-0.5">
                      شناسه تیکت: {selectedTicket.id}
                    </span>
                    <h2 className="text-base font-bold text-slate-900">{selectedTicket.title}</h2>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-slate-500 font-semibold">تغییر وضعیت:</span>
                    <select
                      value={selectedTicket.status}
                      onChange={(e) => handleStatusChange(e.target.value)}
                      className="input-field py-1 text-xs w-auto bg-slate-50"
                    >
                      <option value="Open">باز (Open)</option>
                      <option value="InProgress">در حال بررسی (InProgress)</option>
                      <option value="Resolved">حل شده (Resolved)</option>
                      <option value="Closed">بسته شده (Closed)</option>
                    </select>
                  </div>
                </div>
                <p className="text-xs text-slate-700 leading-relaxed mt-3 bg-slate-50 p-3 rounded-lg border border-slate-200">
                  {selectedTicket.description}
                </p>
              </div>

              {/* Comments Thread */}
              <div>
                <h3 className="font-bold text-xs text-slate-700 mb-3 flex items-center gap-1.5">
                  <MessageCircle className="w-3.5 h-3.5 text-rose-600" />
                  <span>گفتگو و نظرات کارشناسان</span>
                </h3>
                <div className="space-y-2.5 max-h-[220px] overflow-y-auto pr-1">
                  {comments.length > 0 ? (
                    comments.map((c) => (
                      <div
                        key={c.id}
                        className="p-3 rounded-lg bg-slate-50 border border-slate-200 text-xs"
                      >
                        <div className="flex items-center justify-between mb-1">
                          <span className="font-bold text-slate-800">{c.authorName}</span>
                          <span className="text-[10px] text-slate-400 font-mono">
                            {new Date(c.createdAtUtc).toLocaleTimeString('fa-IR')}
                          </span>
                        </div>
                        <p className="text-slate-600 leading-relaxed">{c.comment}</p>
                      </div>
                    ))
                  ) : (
                    <div className="text-center py-6 text-slate-400 text-xs">
                      هنوز نظری برای این تیکت ثبت نشده است.
                    </div>
                  )}
                </div>
              </div>

              {/* Comment Input */}
              <form onSubmit={handleSendComment} className="pt-3 border-t border-slate-100 flex gap-2">
                <input
                  type="text"
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  placeholder="پاسخ یا یادداشت فنی خود را وارد کنید..."
                  className="input-field flex-1"
                />
                <button type="submit" className="btn-primary-nexus text-xs bg-rose-600 hover:bg-rose-700">
                  <Send className="w-3.5 h-3.5" />
                  <span>ارسال نظر</span>
                </button>
              </form>
            </div>
          ) : (
            <div className="py-20 text-center text-slate-400 text-xs">
              یک تیکت را برای مشاهده جزئیات انتخاب نمایید.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
