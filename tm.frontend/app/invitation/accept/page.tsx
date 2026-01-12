'use client'

import { useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { apiService, InvitationResponse } from '@/lib/api'

export default function AcceptInvitationPage() {
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

  const handleAccept = async () => {
    if (!token) return

    const userToken = localStorage.getItem('token')
    if (!userToken) {
      // Redirect to login with return URL
      router.push(`/login?returnUrl=/invitation/accept?token=${encodeURIComponent(token)}`)
      return
    }

    setProcessing(true)
    setError('')

    try {
      await apiService.acceptInvitation(token)
      alert('Đã chấp nhận lời mời thành công!')
      router.push('/projects')
    } catch (err: any) {
      const errorMessage = err.message || 'Không thể chấp nhận lời mời'
      console.error('Accept invitation error:', err)
      
      if (errorMessage.includes('Email does not match')) {
        setError('Email của bạn không khớp với email được mời. Vui lòng đăng nhập bằng email đúng.')
      } else if (errorMessage.includes('subscription')) {
        setError('Dự án này yêu cầu đăng ký gói dịch vụ để thêm thành viên.')
      } else if (errorMessage.includes('already been processed')) {
        setError('Lời mời này đã được xử lý rồi.')
      } else if (errorMessage.includes('expired')) {
        setError('Lời mời này đã hết hạn.')
      } else if (errorMessage.includes('already a member')) {
        setError('Bạn đã là thành viên của dự án này rồi.')
      } else {
        setError(errorMessage)
      }
    } finally {
      setProcessing(false)
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
        <h1 className="text-2xl font-bold mb-6 text-center">Lời mời tham gia dự án</h1>

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
          <div>
            <label className="text-sm font-medium text-gray-700">Email được mời</label>
            <p className="text-lg">{invitation.invitedEmail}</p>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-700">Hết hạn</label>
            <p className="text-sm text-gray-600">
              {new Date(invitation.expiresAt).toLocaleString('vi-VN')}
            </p>
          </div>
        </div>

        {!isLoggedIn ? (
          <div className="space-y-4">
            <div className="p-4 bg-yellow-50 border border-yellow-200 rounded">
              <p className="text-sm text-yellow-800 mb-4">
                Bạn cần đăng nhập để chấp nhận lời mời. Nếu chưa có tài khoản, vui lòng đăng ký.
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => router.push(`/login?returnUrl=/invitation/accept?token=${encodeURIComponent(token || '')}`)}
                  className="flex-1 bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700"
                >
                  Đăng nhập
                </button>
                <button
                  onClick={() => router.push(`/register?returnUrl=/invitation/accept?token=${encodeURIComponent(token || '')}`)}
                  className="flex-1 bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700"
                >
                  Đăng ký
                </button>
              </div>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            <button
              onClick={handleAccept}
              disabled={processing}
              className="w-full bg-green-600 text-white py-2 px-4 rounded-md hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {processing ? 'Đang xử lý...' : 'Chấp nhận lời mời'}
            </button>
            <button
              onClick={handleDecline}
              disabled={processing}
              className="w-full bg-red-600 text-white py-2 px-4 rounded-md hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {processing ? 'Đang xử lý...' : 'Từ chối lời mời'}
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

