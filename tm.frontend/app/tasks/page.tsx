'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { apiService, TaskResponse } from '@/lib/api'

export default function TasksPage() {
  const router = useRouter()
  const [tasks, setTasks] = useState<TaskResponse[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (!token) {
      router.push('/login')
      return
    }
    loadTasks()
  }, [router])

  const loadTasks = async () => {
    try {
      const data = await apiService.getUserTasks()
      setTasks(data)
    } catch (err) {
      console.error('Failed to load tasks:', err)
      if (err instanceof Error && err.message.includes('401')) {
        localStorage.removeItem('token')
        router.push('/login')
      }
    } finally {
      setLoading(false)
    }
  }

  const handleSubmitTask = async (taskId: string) => {
    if (!confirm('Bạn có chắc muốn nộp task này?')) return

    try {
      await apiService.submitTask(taskId)
      loadTasks()
      alert('Nộp task thành công!')
    } catch (err: any) {
      alert(err.message || 'Nộp task thất bại')
    }
  }

  if (loading) {
    return <div className="p-8">Đang tải...</div>
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold mb-6">Task của tôi</h1>

        {tasks.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <p className="text-gray-500">Bạn chưa có task nào</p>
          </div>
        ) : (
          <div className="space-y-4">
            {tasks.map((task) => (
              <div
                key={task.id}
                className="bg-white rounded-lg shadow-md p-6"
              >
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h2 className="text-xl font-semibold">{task.title}</h2>
                    <p className="text-gray-600 mt-1">{task.description}</p>
                    <div className="text-sm text-gray-500 mt-2">
                      Dự án: {task.projectName} | Giao bởi: {task.assignedByUserName}
                    </div>
                    <div className="text-sm text-gray-500">
                      Deadline: {new Date(task.deadline).toLocaleString('vi-VN')}
                    </div>
                  </div>
                  <span
                    className={`px-3 py-1 rounded text-sm ${
                      task.status === 'Completed'
                        ? 'bg-green-100 text-green-800'
                        : task.status === 'Overdue'
                        ? 'bg-red-100 text-red-800'
                        : task.status === 'InProgress'
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-yellow-100 text-yellow-800'
                    }`}
                  >
                    {task.status}
                  </span>
                </div>
                {task.status !== 'Completed' && (
                  <button
                    onClick={() => handleSubmitTask(task.id)}
                    className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700"
                  >
                    Nộp task
                  </button>
                )}
                {task.completedAt && (
                  <div className="text-sm text-green-600 mt-2">
                    Đã hoàn thành: {new Date(task.completedAt).toLocaleString('vi-VN')}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

