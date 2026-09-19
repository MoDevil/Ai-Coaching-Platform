import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { ExerciseService } from '../../services/exercise.service';
import { TrainingProfileService } from '../../services/training-profile.service';
import { Client } from '../../models/client.models';
import { Equipment } from '../../models/exercise.models';
import {
  ClientTrainingPriority,
  DayOfWeekLabels,
  TrainingExperienceLevel,
  TrainingExperienceLevelLabels,
  TrainingProfile,
  UpdateTrainingProfileRequest
} from '../../models/training-profile.models';

@Component({
  selector: 'app-client-training-profile',
  templateUrl: './client-training-profile.component.html',
  styleUrls: ['./client-training-profile.component.css']
})
export class ClientTrainingProfileComponent implements OnInit {
  clientId = '';
  client: Client | null = null;
  profile: TrainingProfile | null = null;
  equipmentList: Equipment[] = [];

  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // Form Model
  experienceLevel: TrainingExperienceLevel = TrainingExperienceLevel.Intermediate;
  minDuration: number | null = 45;
  targetDuration: number | null = 60;
  maxDuration: number | null = 75;
  sessionsPerWeek = 3;
  availableDays: number[] = [1, 3, 5]; // Mon, Wed, Fri
  preferredDays: number[] = [1, 3, 5];
  selectedEquipmentIds: string[] = [];
  exercisePreferences = '';
  exerciseConstraints = '';
  priorities: ClientTrainingPriority[] = [];

  // Enums & Dictionaries
  TrainingExperienceLevel = TrainingExperienceLevel;
  TrainingExperienceLevelLabels = TrainingExperienceLevelLabels;
  DayOfWeekLabels = DayOfWeekLabels;

  experienceLevels = [
    { value: TrainingExperienceLevel.Beginner, label: TrainingExperienceLevelLabels[TrainingExperienceLevel.Beginner] },
    { value: TrainingExperienceLevel.Novice, label: TrainingExperienceLevelLabels[TrainingExperienceLevel.Novice] },
    { value: TrainingExperienceLevel.Intermediate, label: TrainingExperienceLevelLabels[TrainingExperienceLevel.Intermediate] },
    { value: TrainingExperienceLevel.Advanced, label: TrainingExperienceLevelLabels[TrainingExperienceLevel.Advanced] }
  ];

  daysOfWeek = [
    { day: 0, label: 'Sunday' },
    { day: 1, label: 'Monday' },
    { day: 2, label: 'Tuesday' },
    { day: 3, label: 'Wednesday' },
    { day: 4, label: 'Thursday' },
    { day: 5, label: 'Friday' },
    { day: 6, label: 'Saturday' }
  ];

  constructor(
    private route: ActivatedRoute,
    private clientService: ClientService,
    private trainingProfileService: TrainingProfileService,
    private exerciseService: ExerciseService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (!this.clientId) {
      this.errorMessage = 'Client ID missing.';
      this.isLoading = false;
      return;
    }

    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.clientService.getClientById(this.clientId).subscribe({
      next: (c) => (this.client = c),
      error: () => (this.errorMessage = 'Failed to load client.')
    });

    this.exerciseService.getEquipment().subscribe({
      next: (eq) => (this.equipmentList = eq)
    });

    this.trainingProfileService.getProfile(this.clientId).subscribe({
      next: (p) => {
        this.profile = p;
        this.populateForm(p);
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load client training profile.';
      }
    });
  }

  populateForm(p: TrainingProfile): void {
    this.experienceLevel = p.experienceLevel;
    this.minDuration = p.sessionDurationMinMinutes ?? null;
    this.targetDuration = p.sessionDurationTargetMinutes ?? null;
    this.maxDuration = p.sessionDurationMaxMinutes ?? null;
    this.sessionsPerWeek = p.weeklyAvailability.sessionsPerWeek;
    this.availableDays = [...p.weeklyAvailability.availableDays];
    this.preferredDays = [...p.weeklyAvailability.preferredDays];
    this.selectedEquipmentIds = [...p.availableEquipmentIds];
    this.exercisePreferences = p.exercisePreferences || '';
    this.exerciseConstraints = p.exerciseConstraints || '';
    this.priorities = (p.priorities || []).map(pr => ({ ...pr }));
  }

  toggleAvailableDay(day: number): void {
    const idx = this.availableDays.indexOf(day);
    if (idx >= 0) {
      this.availableDays.splice(idx, 1);
      // Remove from preferred too if no longer available
      const prefIdx = this.preferredDays.indexOf(day);
      if (prefIdx >= 0) {
        this.preferredDays.splice(prefIdx, 1);
      }
    } else {
      this.availableDays.push(day);
    }
  }

  togglePreferredDay(day: number): void {
    const idx = this.preferredDays.indexOf(day);
    if (idx >= 0) {
      this.preferredDays.splice(idx, 1);
    } else {
      this.preferredDays.push(day);
      // Ensure it is in available days
      if (!this.availableDays.includes(day)) {
        this.availableDays.push(day);
      }
    }
  }

  toggleEquipment(id: string): void {
    const idx = this.selectedEquipmentIds.indexOf(id);
    if (idx >= 0) {
      this.selectedEquipmentIds.splice(idx, 1);
    } else {
      this.selectedEquipmentIds.push(id);
    }
  }

  addPriority(): void {
    const nextOrder = this.priorities.length + 1;
    this.priorities.push({
      order: nextOrder,
      focusArea: '',
      notes: ''
    });
  }

  removePriority(index: number): void {
    this.priorities.splice(index, 1);
    this.reindexPriorities();
  }

  movePriorityUp(index: number): void {
    if (index === 0) return;
    const temp = this.priorities[index];
    this.priorities[index] = this.priorities[index - 1];
    this.priorities[index - 1] = temp;
    this.reindexPriorities();
  }

  movePriorityDown(index: number): void {
    if (index >= this.priorities.length - 1) return;
    const temp = this.priorities[index];
    this.priorities[index] = this.priorities[index + 1];
    this.priorities[index + 1] = temp;
    this.reindexPriorities();
  }

  private reindexPriorities(): void {
    this.priorities.forEach((p, idx) => (p.order = idx + 1));
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    // Validate duration invariants
    if (this.minDuration && this.targetDuration && this.minDuration > this.targetDuration) {
      this.errorMessage = 'Minimum duration cannot exceed target duration.';
      return;
    }
    if (this.targetDuration && this.maxDuration && this.targetDuration > this.maxDuration) {
      this.errorMessage = 'Target duration cannot exceed maximum duration.';
      return;
    }
    if (this.minDuration && this.maxDuration && this.minDuration > this.maxDuration) {
      this.errorMessage = 'Minimum duration cannot exceed maximum duration.';
      return;
    }

    // Filter valid priorities
    const validPriorities = this.priorities
      .filter(p => p.focusArea.trim().length > 0)
      .map((p, idx) => ({
        order: idx + 1,
        focusArea: p.focusArea.trim(),
        notes: p.notes?.trim() || undefined
      }));

    const request: UpdateTrainingProfileRequest = {
      experienceLevel: Number(this.experienceLevel),
      sessionDurationMinMinutes: this.minDuration ? Number(this.minDuration) : null,
      sessionDurationTargetMinutes: this.targetDuration ? Number(this.targetDuration) : null,
      sessionDurationMaxMinutes: this.maxDuration ? Number(this.maxDuration) : null,
      weeklyAvailability: {
        sessionsPerWeek: Number(this.sessionsPerWeek),
        availableDays: this.availableDays,
        preferredDays: this.preferredDays
      },
      availableEquipmentIds: this.selectedEquipmentIds,
      exercisePreferences: this.exercisePreferences.trim() || null,
      exerciseConstraints: this.exerciseConstraints.trim() || null,
      priorities: validPriorities
    };

    this.isSaving = true;

    this.trainingProfileService.updateProfile(this.clientId, request).subscribe({
      next: (updated) => {
        this.isSaving = false;
        this.profile = updated;
        this.populateForm(updated);
        this.successMessage = 'Training profile updated successfully.';
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || 'Failed to update training profile.';
      }
    });
  }
}
