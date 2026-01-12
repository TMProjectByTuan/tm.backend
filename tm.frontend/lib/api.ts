const API_BASE_URL = 'http://localhost:5290/api';

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  fullName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  fullName: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
}

export interface ProjectResponse {
  id: string;
  name: string;
  description: string;
  createdByUserId: string;
  createdAt: string;
  members: ProjectMemberDto[];
}

export interface ProjectMemberDto {
  userId: string;
  fullName: string;
  email: string;
  role: string;
}

export interface InviteMemberRequest {
  projectId: string;
  email: string;
}

export interface TransferLeadershipRequest {
  projectId: string;
  newLeaderUserId: string;
}

export interface CreateTaskRequest {
  projectId: string;
  assignedToUserId: string;
  title: string;
  description: string;
  deadline: string;
}

export interface TaskResponse {
  id: string;
  projectId: string;
  projectName: string;
  assignedToUserId: string;
  assignedToUserName: string;
  assignedByUserId: string;
  assignedByUserName: string;
  title: string;
  description: string;
  status: string;
  deadline: string;
  completedAt: string | null;
  createdAt: string;
}

export interface TaskActivityResponse {
  projectId: string;
  projectName: string;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
  inProgressTasks: number;
  overdueTasks: number;
  completionPercentage: number;
  tasks: TaskResponse[];
}

export interface CreateSubscriptionRequest {
  projectId: string;
  packageName: string;
  price: number;
  durationMonths: number;
}

export interface InvitationResponse {
  id: string;
  projectId: string;
  projectName: string;
  invitedByUserName: string;
  invitedEmail: string;
  status: string;
  expiresAt: string;
  createdAt: string;
}

class ApiService {
  private getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('token');
    }
    return null;
  }

  private getHeaders(): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };
    const token = this.getToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
  }

  // Auth
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Registration failed');
    return response.json();
  }

  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Login failed');
    return response.json();
  }

  // Projects
  async createProject(data: CreateProjectRequest): Promise<ProjectResponse> {
    const response = await fetch(`${API_BASE_URL}/projects`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create project');
    return response.json();
  }

  async getProject(projectId: string): Promise<ProjectResponse> {
    const response = await fetch(`${API_BASE_URL}/projects/${projectId}`);
    if (!response.ok) throw new Error('Failed to get project');
    return response.json();
  }

  async getUserProjects(): Promise<ProjectResponse[]> {
    const response = await fetch(`${API_BASE_URL}/projects/my-projects`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to get user projects');
    return response.json();
  }

  async inviteMember(data: InviteMemberRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/projects/invite`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to invite member');
  }

  async transferLeadership(data: TransferLeadershipRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/projects/transfer-leadership`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to transfer leadership');
  }

  // Tasks
  async createTask(data: CreateTaskRequest): Promise<TaskResponse> {
    const response = await fetch(`${API_BASE_URL}/tasks`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create task');
    return response.json();
  }

  async submitTask(taskId: string): Promise<TaskResponse> {
    const response = await fetch(`${API_BASE_URL}/tasks/${taskId}/submit`, {
      method: 'POST',
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to submit task');
    return response.json();
  }

  async getProjectTaskActivity(projectId: string): Promise<TaskActivityResponse> {
    const response = await fetch(`${API_BASE_URL}/tasks/project/${projectId}/activity`);
    if (!response.ok) throw new Error('Failed to get task activity');
    return response.json();
  }

  async getUserTasks(): Promise<TaskResponse[]> {
    const response = await fetch(`${API_BASE_URL}/tasks/my-tasks`, {
      headers: this.getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to get user tasks');
    return response.json();
  }

  // Subscriptions
  async createSubscription(data: CreateSubscriptionRequest): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/subscriptions`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create subscription');
  }

  // Invitations
  async getInvitation(token: string): Promise<InvitationResponse> {
    const response = await fetch(`${API_BASE_URL}/invitations/${encodeURIComponent(token)}`);
    if (!response.ok) throw new Error('Failed to get invitation');
    return response.json();
  }

  async acceptInvitation(token: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/invitations/${encodeURIComponent(token)}/accept`, {
      method: 'POST',
      headers: this.getHeaders(),
    });
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.error || 'Failed to accept invitation');
    }
  }

  async declineInvitation(token: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/invitations/${encodeURIComponent(token)}/decline`, {
      method: 'POST',
      headers: this.getHeaders(),
    });
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.error || 'Failed to decline invitation');
    }
  }
}

export const apiService = new ApiService();

