'use client'

import { useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { apiService, InvitationResponse } from '@/lib/api'

export default function DeclineInvitationPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const token = searchParams.get('token')

  const [invitation, setInvitation] = useState<InvitationResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [processing, setProcessing] = useState(false)
  const [error, setError] = useState('')
  const [isLoggedIn, setIsLoggedIn] = useState(false)

  useEffect(() => {
    const checkAuth = () => {
      const token = localStorage.getItem('token')
      setIsLoggedIn(!!token)
    }
    checkAuth()

    if (!token) {
      setError('Invalid invitation link')
      setLoading(false)
      return
    }

    loadInvitation(token)
  }, [token])

  const loadInvitation = async (invitationToken: string) => {
    try {
      const data = await apiService.getInvitation(invitationToken)
      setInvitation(data)
    } catch (err: any) {
      setError(err.message || 'Failed to load invitation')
    } finally {
      setLoading(false)
    }
  }

  const handleDecline = async () => {
    if (!token) return

    const userToken = localStorage.getItem('token')
    if (!userToken) {
      router.push(`/login?returnUrl=/invitation/decline?token=${encodeURIComponent(token)}`)
      return
    }

    setProcessing(true)
    setError('')

    try {
      await apiService.declineInvitation(token)
      alert('Đã từ chối lời mời')
      router.push('/projects')
    } catch (err: any) {
      setError(err.message || 'Không thể từ chối lời mời')
    } finally {
      setProcessing(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="text-xl">Đang tải...</div>
        </div>
      </div>
    )
  }

  if (error && !invitation) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="max-w-md w-full bg-white rounded-lg shadow-md p-8 text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-4">Lỗi</h1>
          <p className="text-gray-600 mb-6">{error}</p>
          <button
            onClick={() => router.push('/')}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
          >
            Về trang chủ
          </button>
        </div>
      </div>
    )
  }

  if (!invitation) {
    return null
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow-md p-8">
        <h1 className="text-2xl font-bold mb-6 text-center">Từ chối lời mời</h1>

        {error && (
          <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        <div className="space-y-4 mb-6">
          <div>
            <label className="text-sm font-medium text-gray-700">Dự án</label>
            <p className="text-lg font-semibold">{invitation.projectName}</p>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-700">Người mời</label>
            <p className="text-lg">{invitation.invitedByUserName}</p>
          </div>
        </div>

        {!isLoggedIn ? (
          <div className="space-y-4">
            <div className="p-4 bg-yellow-50 border border-yellow-200 rounded">
              <p className="text-sm text-yellow-800 mb-4">
                Bạn cần đăng nhập để từ chối lời mời.
              </p>
              <button
                onClick={() => router.push(`/login?returnUrl=/invitation/decline?token=${encodeURIComponent(token || '')}`)}
                className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700"
              >
                Đăng nhập
              </button>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            <p className="text-gray-600 mb-4">
              Bạn có chắc muốn từ chối lời mời tham gia dự án <strong>{invitation.projectName}</strong>?
            </p>
            <button
              onClick={handleDecline}
              disabled={processing}
              className="w-full bg-red-600 text-white py-2 px-4 rounded-md hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {processing ? 'Đang xử lý...' : 'Xác nhận từ chối'}
            </button>
            <button
              onClick={() => router.push('/projects')}
              className="w-full bg-gray-200 text-gray-800 py-2 px-4 rounded-md hover:bg-gray-300"
            >
              Hủy
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

