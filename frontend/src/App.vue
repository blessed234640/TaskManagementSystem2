<script setup>
// Простой одностраничный UI:
// - сверху форма логина;
// - ниже список задач и действия в зависимости от роли.
import { ref, computed, onMounted } from 'vue'

// URL backend API; для Docker используем http://localhost:8080
const apiBaseUrl = 'http://localhost:8080'

const email = ref('manager@example.com')
const password = ref('manager123')

const token = ref(localStorage.getItem('token') || '')
const currentUserName = ref(localStorage.getItem('fullName') || '')
const currentRole = ref(localStorage.getItem('role') || '')

const tasks = ref([])
const users = ref([])
const isLoading = ref(false)
const error = ref('')

const canCreateTasks = computed(() => currentRole.value === 'MANAGER')
const canAssignTasks = computed(() => currentRole.value === 'MANAGER')
const canDeleteTasks = computed(() => currentRole.value === 'MANAGER')

const editingTask = ref(null)
const editForm = ref({
  title: '',
  description: ''
})

// Человекочитаемые подписи для статуса и приоритета
const statusLabels = {
  0: 'Новая',
  1: 'В работе',
  2: 'Завершена',
}

const priorityLabels = {
  0: 'Низкий',
  1: 'Средний',
  2: 'Высокий',
}

const formatStatus = (value) => statusLabels[value] ?? value
const formatPriority = (value) => priorityLabels[value] ?? value
const assignableUsers = computed(() => {
  return users.value.filter(u => u.role?.code !== 'VIEWER')
})

async function apiFetch(path, options = {}) {
  error.value = ''
  const headers = {
    'Content-Type': 'application/json',
    ...(token.value ? { Authorization: `Bearer ${token.value}` } : {}),
    ...(options.headers || {}),
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    const raw = await response.text()
    let message = ''
    try {
      const parsed = raw ? JSON.parse(raw) : null
      message = parsed?.message || parsed?.title || ''
    } catch {
      message = raw
    }

    // Больше человекочитаемые сообщения для ограничений по ролям
    if (response.status === 403) {
      message ||= 'Недостаточно прав для выполнения операции.'
    }

    throw new Error(message || `Ошибка запроса (HTTP ${response.status})`)
  }

  if (response.status === 204) return null
  return await response.json()
}

async function login() {
  try {
    isLoading.value = true
    const data = await apiFetch('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({
        email: email.value,
        password: password.value,
      }),
    })
    token.value = data.token
    currentUserName.value = data.fullName
    currentRole.value = data.roleCode

    localStorage.setItem('token', token.value)
    localStorage.setItem('fullName', currentUserName.value)
    localStorage.setItem('role', currentRole.value)

    await loadInitialData()
  } catch (e) {
    error.value = e.message || 'Ошибка входа'
  } finally {
    isLoading.value = false
  }
}

function logout() {
  token.value = ''
  currentUserName.value = ''
  currentRole.value = ''
  tasks.value = []
  localStorage.removeItem('token')
  localStorage.removeItem('fullName')
  localStorage.removeItem('role')
}

async function loadTasks() {
  const data = await apiFetch('/api/tasks')
  tasks.value = data.items || data
}

async function loadUsers() {
  const data = await apiFetch('/api/users')
  users.value = data
}

async function loadInitialData() {
  if (!token.value) return
  isLoading.value = true
  try {
    await Promise.all([loadTasks(), loadUsers()])
  } catch (e) {
    error.value = e.message || 'Ошибка загрузки данных'
  } finally {
    isLoading.value = false
  }
}

const newTask = ref({
  title: '',
  description: '',
  priority: 1,
  assignedToUserId: null,
})

async function createTask() {
  if (!newTask.value.title || !newTask.value.assignedToUserId) {
    error.value = 'Укажите заголовок и исполнителя'
    return
  }
  try {
    isLoading.value = true
    await apiFetch('/api/tasks', {
      method: 'POST',
      body: JSON.stringify(newTask.value),
    })
    newTask.value = { title: '', description: '', priority: 1, assignedToUserId: null }
    await loadTasks()
  } catch (e) {
    error.value = e.message || 'Ошибка создания задачи'
  } finally {
    isLoading.value = false
  }
}

async function changeStatus(task, status) {
  try {
    isLoading.value = true
    await apiFetch(`/api/tasks/${task.id}/status`, {
      method: 'POST',
      body: JSON.stringify(status),
    })
    await loadTasks()
  } catch (e) {
    error.value = e.message || 'Ошибка смены статуса'
  } finally {
    isLoading.value = false
  }
}

async function assignTask(task, userId) {
  try {
    isLoading.value = true
    await apiFetch(`/api/tasks/${task.id}/assign`, {
      method: 'POST',
      body: JSON.stringify(userId),
    })
    await loadTasks()
  } catch (e) {
    error.value = e.message || 'Ошибка назначения задачи'
  } finally {
    isLoading.value = false
  }
}

function startEdit(task) {
  editingTask.value = task
  editForm.value = {
    title: task.title,
    description: task.description || ''
  }
}

function cancelEdit() {
  editingTask.value = null
  editForm.value = { title: '', description: '' }
}

async function saveEdit() {
  if (!editingTask.value || !editForm.value.title) {
    error.value = 'Укажите заголовок задачи'
    return
  }
  try {
    isLoading.value = true
    await apiFetch(`/api/tasks/${editingTask.value.id}`, {
      method: 'PUT',
      body: JSON.stringify({
        title: editForm.value.title,
        description: editForm.value.description
      }),
    })
    await loadTasks()
    cancelEdit()
  } catch (e) {
    error.value = e.message || 'Ошибка редактирования задачи'
  } finally {
    isLoading.value = false
  }
}

async function deleteTask(task) {
  if (!confirm(`Удалить задачу "${task.title}"?`)) {
    return
  }
  try {
    isLoading.value = true
    await apiFetch(`/api/tasks/${task.id}`, {
      method: 'DELETE',
    })
    await loadTasks()
  } catch (e) {
    error.value = e.message || 'Ошибка удаления задачи'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  loadInitialData()
})
</script>

<template>
  <div class="page">
    <section class="login">
      <h1>Task Management</h1>
      <div v-if="!token" class="login-form">
        <label>
          Email
          <input v-model="email" type="email" />
        </label>
        <label>
          Пароль
          <input v-model="password" type="password" />
        </label>
        <button @click="login" :disabled="isLoading">Войти</button>
      </div>
      <div v-else class="user-info">
        <span>Вы вошли как: {{ currentUserName }} ({{ currentRole }})</span>
        <button @click="logout">Выйти</button>
      </div>
      <p v-if="error" class="error">{{ error }}</p>
    </section>

    <section v-if="token" class="content">
      <div class="actions" v-if="canCreateTasks">
        <h2>Создать задачу</h2>
        <label>
          Заголовок
          <input v-model="newTask.title" />
        </label>
        <label>
          Описание
          <textarea v-model="newTask.description" />
        </label>
        <label>
          Приоритет
          <select v-model.number="newTask.priority">
            <option :value="0">Low</option>
            <option :value="1">Medium</option>
            <option :value="2">High</option>
          </select>
        </label>
        <label>
          Исполнитель
          <select v-model.number="newTask.assignedToUserId">
            <option disabled :value="null">Выберите пользователя</option>
            <option v-for="u in assignableUsers" :key="u.id" :value="u.id">
              {{ u.fullName }} ({{ u.role?.code }})
            </option>
          </select>
        </label>
        <button @click="createTask" :disabled="isLoading">Создать</button>
      </div>

      <div class="tasks">
        <h2>Задачи</h2>
        <p v-if="isLoading">Загрузка...</p>
        <ul v-else>
          <li v-for="t in tasks" :key="t.id" class="task-item">
            <!-- Режим редактирования -->
            <div v-if="editingTask?.id === t.id" class="task-edit">
              <h3>Редактирование задачи</h3>
              <label>
                Заголовок
                <input v-model="editForm.title" />
              </label>
              <label>
                Описание
                <textarea v-model="editForm.description" />
              </label>
              <div class="edit-actions">
                <button @click="saveEdit" :disabled="isLoading">Сохранить</button>
                <button @click="cancelEdit">Отмена</button>
              </div>
            </div>
            
            <!-- Обычный режим отображения -->
            <div v-else>
              <div class="task-main">
                <strong>{{ t.title }}</strong>
                <span v-if="t.description" class="task-description">{{ t.description }}</span>
                <span>Статус: {{ formatStatus(t.status) }}</span>
                <span>Приоритет: {{ formatPriority(t.priority) }}</span>
                <span>Исполнитель: {{ t.assignedToUser?.fullName }}</span>
              </div>
              <div class="task-actions">
                <button @click="changeStatus(t, 1)">В работу</button>
                <button @click="changeStatus(t, 2)">Завершить</button>
                
                <button v-if="canDeleteTasks" @click="deleteTask(t)" class="delete-btn">Удалить</button>
                <button @click="startEdit(t)" class="edit-btn">Редактировать</button>

                <div v-if="canAssignTasks">
                  <select @change="assignTask(t, Number($event.target.value))">
                    <option :value="t.assignedToUserId">Переназначить...</option>
                    <option v-for="u in assignableUsers" :key="u.id" :value="u.id">
                      {{ u.fullName }}
                    </option>
                  </select>
                </div>
              </div>
            </div>
          </li>
        </ul>
      </div>
    </section>
  </div>
</template>

<style scoped>
.page {
  max-width: 900px;
  margin: 0 auto;
  padding: 1rem;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
}

.login {
  border-bottom: 1px solid #ddd;
  padding-bottom: 1rem;
  margin-bottom: 1rem;
}

.login-form,
.user-info {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

label {
  display: flex;
  flex-direction: column;
  font-size: 0.9rem;
}

input,
textarea,
select {
  padding: 0.25rem 0.4rem;
}

button {
  padding: 0.4rem 0.8rem;
  cursor: pointer;
}

.error {
  color: darkred;
  margin-top: 0.5rem;
}

.content {
  display: flex;
  gap: 2rem;
  align-items: flex-start;
}

.actions {
  flex: 1;
}

.tasks {
  flex: 1;
}

.task-item {
  border: 1px solid #ddd;
  padding: 0.5rem;
  margin-bottom: 0.5rem;
}

.task-main {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  margin-bottom: 0.4rem;
}

.task-description {
  color: #666;
  font-style: italic;
  margin: 0.2rem 0;
}

.task-edit {
  border: 2px solid #007bff;
  padding: 1rem;
  margin-bottom: 0.5rem;
  border-radius: 4px;
  background: #f8f9fa;
}

.task-edit h3 {
  margin: 0 0 0.5rem 0;
  color: #007bff;
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.edit-btn {
  background: #28a745;
  color: white;
}

.delete-btn {
  background: #dc3545;
  color: white;
}

.task-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}
</style>
