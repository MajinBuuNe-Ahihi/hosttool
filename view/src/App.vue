<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { schedulerService } from '../src/api/scheduler'
import type { Scheduler, SchedulerDay, CreateUpdateSchedulerDto } from '../src/api/scheduler'
import { v7 as uuidv7 } from 'uuid'
import NotificationCenter from './components/NotificationCenter.vue'
import DeviceManagement from './components/DeviceManagement.vue'
// state
const schedulers = ref<Scheduler[]>([])
const totalCount = ref(0)
const isModalOpen = ref(false)
const edit = ref(false)
// master
const selectedScheduler = reactive<Partial<Scheduler>>({})
const masterForm = reactive<Partial<Scheduler>>({
  schedulerName: '',
  schedulerDescription: '',
  schedulerPath: '',
  active: true,
  runAll: false,
})

// detail
const detailForm = ref<SchedulerDay[]>([])

// load list
async function loadData() {
  const result = await schedulerService.get()
  schedulers.value = result
  totalCount.value = result.length
}

function createScheduler() {
  edit.value = false
  Object.assign(selectedScheduler, { })
  Object.assign(masterForm, {
    schedulerId: uuidv7(),
    schedulerName: '',
    schedulerDescription: '',
    schedulerPath: '',
    active: true,
    runAll: false,
  })
  detailForm.value = []
  isModalOpen.value = true
}

async function editScheduler(obj: Scheduler) {
  edit.value = true
  const s = await schedulerService.getDetail(obj.schedulerId)
  Object.assign(masterForm, obj)
  detailForm.value = s ?? []
  isModalOpen.value = true
}

async function save() {
  const id = masterForm.schedulerId ?? uuidv7()
  const dto: CreateUpdateSchedulerDto = {
    master: {
      schedulerId: id,
      schedulerName: masterForm.schedulerName!,
      schedulerDescription: masterForm.schedulerDescription!,
      schedulerPath: masterForm.schedulerPath!,
      active: masterForm.active!,
      runAll: masterForm.runAll!,
    },
    detail: detailForm.value.map((x) => {
      return { ...x, schedulerId: id }
    }),
    editMode: edit.value ? 0 : 1, // 1=edit,0=create
  }

  if (edit.value) {
    await schedulerService.update(selectedScheduler.schedulerId, dto)
  } else {
    await schedulerService.create(dto)
  }

  isModalOpen.value = false
  await loadData()
}

async function removeScheduler(obj: Scheduler) {
  if (confirm('Are you sure to delete?')) {
    const s = await schedulerService.getDetail(obj.schedulerId)
    const id = obj.schedulerId ?? uuidv7()
    const dto: CreateUpdateSchedulerDto = {
      master: {
        schedulerId: id,
        schedulerName: obj.schedulerName!,
        schedulerDescription: obj.schedulerDescription!,
        schedulerPath: obj.schedulerPath!,
        active: obj.active!,
        runAll: obj.runAll!,
      },
      detail: s,
      editMode: 1, // 1=edit,0=create
    }

    await schedulerService.update(selectedScheduler.schedulerId, dto)
    await loadData()
  }
}

function addDetail() {
  detailForm.value.push({
    schedulerDayId: uuidv7(),
    schedulerId: selectedScheduler.schedulerId ?? uuidv7(),
    day: 1,
    from: '08:00',
    to: '17:00',
    active: true,
  })
}

function removeDetail(index: number) {
  detailForm.value.splice(index, 1)
}

onMounted(loadData)
</script>

<template>
  <div class="container-fluid">
    <!-- Notification Center -->
    <NotificationCenter />

    <!-- Device Management -->
    <DeviceManagement />

    <!-- Schedulers Section -->
    <div class="card shadow-sm">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">Schedulers</h5>
        <button class="btn btn-primary" @click="createScheduler">
          <i class="fa fa-plus me-1"></i> Thêm mới
        </button>
      </div>

      <div class="card-body">
        <table class="table table-striped table-hover align-middle">
          <thead class="table-light">
            <tr>
              <th>Actions</th>
              <th>Name</th>
              <th>Description</th>
              <th>Path</th>
              <th>Active</th>
              <th>RunAll</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in schedulers" :key="s.schedulerId">
              <td>
                <div class="btn-group">
                  <button class="btn btn-sm btn-outline-primary" @click="editScheduler(s)">
                    <i class="fa fa-edit"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="removeScheduler(s)">
                    <i class="fa fa-trash"></i>
                  </button>
                </div>
              </td>
              <td>{{ s.schedulerName }}</td>
              <td>{{ s.schedulerDescription }}</td>
              <td>{{ s.schedulerPath }}</td>
              <td>
                <span class="badge" :class="s.active ? 'bg-success' : 'bg-secondary'">
                  {{ s.active ? 'Yes' : 'No' }}
                </span>
              </td>
              <td>
                <span class="badge" :class="s.runAll ? 'bg-info' : 'bg-secondary'">
                  {{ s.runAll ? 'Yes' : 'No' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <!-- Modal -->
  <div class="modal fade show" tabindex="-1" style="display: block" v-if="isModalOpen">
    <div class="modal-dialog modal-lg modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header bg-light">
          <h5 class="modal-title">
            {{ selectedScheduler.schedulerId ? 'Edit Scheduler' : 'New Scheduler' }}
          </h5>
          <button type="button" class="btn-close" @click="isModalOpen = false"></button>
        </div>
        <div class="modal-body">
          <!-- Master form -->
          <div class="mb-3">
            <label class="form-label">Name</label>
            <input v-model="masterForm.schedulerName" class="form-control" required />
          </div>
          <div class="mb-3">
            <label class="form-label">Description</label>
            <input v-model="masterForm.schedulerDescription" class="form-control" />
          </div>
          <div class="mb-3">
            <label class="form-label">Path</label>
            <input v-model="masterForm.schedulerPath" class="form-control" />
          </div>
          <div class="form-check form-switch mb-2">
            <input
              v-model="masterForm.active"
              type="checkbox"
              class="form-check-input"
              id="active"
            />
            <label class="form-check-label" for="active">Active</label>
          </div>
          <div class="form-check form-switch mb-2">
            <input
              v-model="masterForm.runAll"
              type="checkbox"
              class="form-check-input"
              id="runAll"
            />
            <label class="form-check-label" for="runAll">Run All</label>
          </div>

          <hr />
          <!-- Detail grid -->
          <h6>Days</h6>
          <table class="table table-bordered">
            <thead class="table-light">
              <tr>
                <th>Day</th>
                <th>From</th>
                <th>To</th>
                <th>Active</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(d, i) in detailForm" :key="d.schedulerDayId">
                <td>
                  <select v-model.number="d.day" class="form-select">
                    <option :value="1">Thứ 2</option>
                    <option :value="2">Thứ 3</option>
                    <option :value="3">Thứ 4</option>
                    <option :value="4">Thứ 5</option>
                    <option :value="5">Thứ 6</option>
                    <option :value="6">Thứ 7</option>
                    <option :value="0">Chủ nhật</option>
                  </select>
                </td>
                <td><input v-model="d.from" type="time" class="form-control" /></td>
                <td><input v-model="d.to" type="time" class="form-control" /></td>
                <td class="text-center">
                  <input v-model="d.active" type="checkbox" class="form-check-input" />
                </td>
                <td class="text-center">
                  <button
                    style="height: 16px; width: 16px; color: black; font-size: 12pxl"
                    class="btn btn-sm btn-outline-danger"
                    @click="removeDetail(i)"
                  >
                    <i class="bi bi-trash"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <button class="btn btn-sm btn-outline-primary" @click="addDetail">
            <i class="fa fa-plus"></i> Add Day
          </button>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="isModalOpen = false">Close</button>
          <button class="btn btn-primary" @click="save">Save</button>
        </div>
      </div>
    </div>
  </div>
</template>
