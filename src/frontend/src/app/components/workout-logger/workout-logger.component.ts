import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { WorkoutService } from '../../services/workout.service';
import {
  WorkoutSessionDto,
  WorkoutExerciseDto,
  WorkoutStatus,
  WorkoutStatusLabels,
  ProgressionEvaluationStatus,
  ProgressionEvaluationStatusLabels,
  RecordWorkoutSetRequestDto
} from '../../models/workout.models';

@Component({
  selector: 'app-workout-logger',
  templateUrl: './workout-logger.component.html',
  styleUrls: ['./workout-logger.component.css']
})
export class WorkoutLoggerComponent implements OnInit {
  workoutId: string | null = null;
  workout: WorkoutSessionDto | null = null;
  isLoading = true;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // New set inputs map by workoutExerciseId
  newSetReps: Record<string, number> = {};
  newSetLoad: Record<string, number> = {};
  newSetRir: Record<string, number | null> = {};

  readonly WorkoutStatus = WorkoutStatus;
  readonly WorkoutStatusLabels = WorkoutStatusLabels;
  readonly ProgressionEvaluationStatus = ProgressionEvaluationStatus;
  readonly ProgressionEvaluationStatusLabels = ProgressionEvaluationStatusLabels;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private workoutService: WorkoutService
  ) {}

  ngOnInit(): void {
    this.workoutId = this.route.snapshot.paramMap.get('id');
    if (this.workoutId) {
      this.loadWorkout(this.workoutId);
    } else {
      this.errorMessage = 'No workout ID specified.';
      this.isLoading = false;
    }
  }

  loadWorkout(id: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.workoutService.getWorkoutById(id).subscribe({
      next: (w) => {
        this.workout = w;
        this.initInputDefaults(w);
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to load workout session.';
        this.isLoading = false;
      }
    });
  }

  initInputDefaults(w: WorkoutSessionDto): void {
    for (const ex of w.exercises) {
      if (this.newSetReps[ex.id] === undefined) {
        this.newSetReps[ex.id] = 10;
      }
      if (this.newSetLoad[ex.id] === undefined) {
        this.newSetLoad[ex.id] = 50;
      }
      if (this.newSetRir[ex.id] === undefined) {
        this.newSetRir[ex.id] = 2;
      }
    }
  }

  addSet(exercise: WorkoutExerciseDto): void {
    if (!this.workout) return;

    const nextSetNumber = exercise.sets.length + 1;
    const reps = this.newSetReps[exercise.id] ?? 10;
    const load = this.newSetLoad[exercise.id] ?? 0;
    const rir = this.newSetRir[exercise.id] ?? null;

    const req: RecordWorkoutSetRequestDto = {
      setNumber: nextSetNumber,
      repetitions: reps,
      loadKg: load,
      rir: rir !== null ? rir : undefined,
      isCompleted: true
    };

    this.workoutService.recordSet(this.workout.id, exercise.id, req).subscribe({
      next: (updated) => {
        this.workout = updated;
        this.successMessage = `Set ${nextSetNumber} recorded for ${exercise.exerciseName}.`;
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to record set.';
      }
    });
  }

  completeWorkout(): void {
    if (!this.workout) return;

    this.workoutService.completeWorkout(this.workout.id, { notes: 'Completed in gym' }).subscribe({
      next: (updated) => {
        this.workout = updated;
        this.successMessage = 'Workout marked as Completed!';
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to complete workout.';
      }
    });
  }

  abandonWorkout(): void {
    if (!this.workout) return;

    this.workoutService.abandonWorkout(this.workout.id).subscribe({
      next: (updated) => {
        this.workout = updated;
        this.successMessage = 'Workout marked as Abandoned.';
        setTimeout(() => this.successMessage = null, 3000);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to abandon workout.';
      }
    });
  }
}
