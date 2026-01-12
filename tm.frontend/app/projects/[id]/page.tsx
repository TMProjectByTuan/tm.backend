'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { apiService, ProjectResponse, TaskActivityResponse } from '@/lib/api'

export default function ProjectDetailPage() {
  const params = useParams()
  const router = useRouter()
  const projectId = params.id as string
  const [userId, setUserId] = useState<string | null>(null)

  const [project, setProject] = useState<ProjectResponse | null>(null)
  const [activity, setActivity] = useState<TaskActivityResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [showInviteModal, setShowInviteModal] = useState(false)
  const [showTaskModal, setShowTaskModal] = useState(false)
  const [showSubscriptionModal, setShowSubscriptionModal] = useState(false)
  const [inviteEmail, setInviteEmail] = useState('')
  const [newTask, setNewTask] = useState({
    assignedToUserId: '',
    title: '',
    description: '',
    deadline: '',
  })

  useEffect(() => {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token')
      const storedUserId = localStorage.getItem('userId')
      setUserId(storedUserId)
      
      if (!token) {
        router.push('/login')
        return
      }
      loadData()
    }
  }, [projectId, router])

  const loadData = async () => {
    try {
      const [projectData, activityData] = await Promise.all([
        apiService.getProject(projectId),
        apiService.getProjectTaskActivity(projectId),
      ])
      setProject(projectData)
      setActivity(activityData)
    } catch (err) {
      console.error('Failed to load data:', err)
    } finally {
      setLoading(false)
    }
  }

  const isLeader = project?.members.some(
    (m) => m.userId === userId && m.role === 'Leader'
  ) ?? false

  const handleInvite = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await apiService.inviteMember({ projectId, email: inviteEmail })
      setShowInviteModal(false)
      setInviteEmail('')
      loadData()
      alert('Mời thành viên thành công!')
    } catch (err: any) {
      if (err.message.includes('subscription')) {
        setShowInviteModal(false)
        setShowSubscriptionModal(true)
      } else {
        alert(err.message || 'Mời thành viên thất bại')
      }
    }
  }

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await apiService.createTask({
        projectId,
        assignedToUserId: newTask.assignedToUserId,
        title: newTask.title,
        description: newTask.description,
        deadline: newTask.deadline,
      })
      setShowTaskModal(false)
      setNewTask({ assignedToUserId: '', title: '', description: '', deadline: '' })
      loadData()
      alert('Giao task thành công!')
    } catch (err: any) {
      alert(err.message || 'Giao task thất bại')
    }
  }

  const handleTransferLeadership = async (newLeaderUserId: string) => {
    if (!confirm('Bạn có chắc muốn chuyển quyền leader?')) return

    try {
      await apiService.transferLeadership({ projectId, newLeaderUserId })
      loadData()
      alert('Chuyển quyền leader thành công!')
    } catch (err: any) {
      alert(err.message || 'Chuyển quyền thất bại')
    }
  }

  const handleCreateSubscription = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await apiService.createSubscription({
        projectId,
        packageName: 'Premium Package',
        price: 100000,
        durationMonths: 1,
      })
      setShowSubscriptionModal(false)
      alert('Đăng ký gói dịch vụ thành công!')
    } catch (err: any) {
      alert(err.message || 'Đăng ký thất bại')
    }
  }

  if (loading) {
    return <div className="p-8">Đang tải...</div>
  }

  if (!project) {
    return <div className="p-8">Không tìm thấy dự án</div>
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-6xl mx-auto">
        <div className="mb-6">
          <button
            onClick={() => router.push('/projects')}
            className="text-blue-600 hover:underline mb-4"
          >
            ← Quay lại danh sách dự án
          </button>
          <h1 className="text-3xl font-bold">{project.name}</h1>
          <p className="text-gray-600 mt-2">{project.description}</p>
        </div>

        {/* Task Activity Bar */}
        {activity && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h2 className="text-xl font-semibold mb-4">Thanh hoạt động Task</h2>
            <div className="mb-4">
              <div className="flex justify-between mb-2">
                <span>Tiến độ hoàn thành</span>
                <span className="font-bold">{activity.completionPercentage.toFixed(1)}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-4">
                <div
                  className="bg-blue-600 h-4 rounded-full transition-all"
                  style={{ width: `${activity.completionPercentage}%` }}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-4 text-center">
              <div>
                <div className="text-2xl font-bold">{activity.totalTasks}</div>
                <div className="text-sm text-gray-600">Tổng task</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-green-600">
                  {activity.completedTasks}
                </div>
                <div className="text-sm text-gray-600">Hoàn thành</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-yellow-600">
                  {activity.pendingTasks}
                </div>
                <div className="text-sm text-gray-600">Chờ xử lý</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-blue-600">
                  {activity.inProgressTasks}
                </div>
                <div className="text-sm text-gray-600">Đang làm</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-red-600">
                  {activity.overdueTasks}
                </div>
                <div className="text-sm text-gray-600">Quá hạn</div>
              </div>
            </div>
          </div>
        )}

        {/* Actions */}
        {isLeader && (
          <div className="flex gap-2 mb-6">
            <button
              onClick={() => setShowInviteModal(true)}
              className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700"
            >
              + Mời thành viên
            </button>
            <button
              onClick={() => setShowTaskModal(true)}
              className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
            >
              + Giao task
            </button>
          </div>
        )}

        {/* Members */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Thành viên</h2>
          <div className="space-y-2">
            {project.members.map((member) => (
              <div
                key={member.userId}
                className="flex justify-between items-center p-3 bg-gray-50 rounded"
              >
                <div>
                  <span className="font-medium">{member.fullName}</span>
                  <span className="text-gray-600 ml-2">({member.email})</span>
                  {member.role === 'Leader' && (
                    <span className="ml-2 bg-yellow-100 text-yellow-800 text-xs px-2 py-1 rounded">
                      Leader
                    </span>
                  )}
                </div>
                {isLeader &&
                  member.userId !== userId &&
                  member.role !== 'Leader' && (
                    <button
                      onClick={() => handleTransferLeadership(member.userId)}
                      className="text-blue-600 hover:underline text-sm"
                    >
                      Chuyển quyền leader
                    </button>
                  )}
              </div>
            ))}
          </div>
        </div>

        {/* Tasks */}
        {activity && activity.tasks.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-semibold mb-4">Danh sách Task</h2>
            <div className="space-y-3">
              {activity.tasks.map((task) => (
                <div
                  key={task.id}
                  className="p-4 border rounded-lg"
                >
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="font-semibold">{task.title}</h3>
                      <p className="text-sm text-gray-600 mt-1">{task.description}</p>
                      <div className="text-xs text-gray-500 mt-2">
                        Giao cho: {task.assignedToUserName} | Deadline:{' '}
                        {new Date(task.deadline).toLocaleDateString('vi-VN')}
                      </div>
                    </div>
                    <span
                      className={`px-2 py-1 rounded text-xs ${
                        task.status === 'Completed'
                          ? 'bg-green-100 text-green-800'
                          : task.status === 'Overdue'
                          ? 'bg-red-100 text-red-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}
                    >
                      {task.status}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Invite Modal */}
        {showInviteModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full">
              <h2 className="text-xl font-bold mb-4">Mời thành viên</h2>
              <form onSubmit={handleInvite} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">Email</label>
                  <input
                    type="email"
                    required
                    value={inviteEmail}
                    onChange={(e) => setInviteEmail(e.target.value)}
                    className="w-full px-3 py-2 border rounded-md"
                  />
                </div>
                <div className="flex gap-2">
                  <button
                    type="submit"
                    className="flex-1 bg-green-600 text-white py-2 rounded-md hover:bg-green-700"
                  >
                    Mời
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowInviteModal(false)}
                    className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-md hover:bg-gray-300"
                  >
                    Hủy
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Task Modal */}
        {showTaskModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full">
              <h2 className="text-xl font-bold mb-4">Giao task</h2>
              <form onSubmit={handleCreateTask} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">
                    Giao cho thành viên
                  </label>
                  <select
                    required
                    value={newTask.assignedToUserId}
                    onChange={(e) =>
                      setNewTask({ ...newTask, assignedToUserId: e.target.value })
                    }
                    className="w-full px-3 py-2 border rounded-md"
                  >
                    <option value="">Chọn thành viên</option>
                    {project.members
                      .filter((m) => m.userId !== userId && m.role !== 'Leader')
                      .map((m) => (
                        <option key={m.userId} value={m.userId}>
                          {m.fullName}
                        </option>
                      ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Tiêu đề</label>
                  <input
                    type="text"
                    required
                    value={newTask.title}
                    onChange={(e) =>
                      setNewTask({ ...newTask, title: e.target.value })
                    }
                    className="w-full px-3 py-2 border rounded-md"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Mô tả</label>
                  <textarea
                    value={newTask.description}
                    onChange={(e) =>
                      setNewTask({ ...newTask, description: e.target.value })
                    }
                    className="w-full px-3 py-2 border rounded-md"
                    rows={3}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Deadline</label>
                  <input
                    type="datetime-local"
                    required
                    value={newTask.deadline}
                    onChange={(e) =>
                      setNewTask({ ...newTask, deadline: e.target.value })
                    }
                    className="w-full px-3 py-2 border rounded-md"
                  />
                </div>
                <div className="flex gap-2">
                  <button
                    type="submit"
                    className="flex-1 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700"
                  >
                    Giao task
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowTaskModal(false)}
                    className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-md hover:bg-gray-300"
                  >
                    Hủy
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Subscription Modal */}
        {showSubscriptionModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md w-full">
              <h2 className="text-xl font-bold mb-4">Đăng ký gói dịch vụ</h2>
              <p className="text-gray-600 mb-4">
                Dự án của bạn có hơn 4 thành viên. Vui lòng đăng ký gói dịch vụ để tiếp
                tục mời thành viên.
              </p>
              <form onSubmit={handleCreateSubscription} className="space-y-4">
                <div className="flex gap-2">
                  <button
                    type="submit"
                    className="flex-1 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700"
                  >
                    Đăng ký (100,000 VNĐ/tháng)
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowSubscriptionModal(false)}
                    className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-md hover:bg-gray-300"
                  >
                    Hủy
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

