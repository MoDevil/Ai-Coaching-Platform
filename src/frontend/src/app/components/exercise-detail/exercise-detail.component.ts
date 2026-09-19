import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ExerciseService } from '../../services/exercise.service';
import {
  ExerciseCategoryLabels,
  ExerciseDetail,
  QualitativeRating,
  QualitativeRatingLabels,
  ResistanceProfileLabels
} from '../../models/exercise.models';

@Component({
  selector: 'app-exercise-detail',
  templateUrl: './exercise-detail.component.html',
  styleUrls: ['./exercise-detail.component.css']
})
export class ExerciseDetailComponent implements OnInit {
  exerciseId = '';
  exercise: ExerciseDetail | null = null;
  isLoading = true;
  errorMessage = '';

  ExerciseCategoryLabels = ExerciseCategoryLabels;
  ResistanceProfileLabels = ResistanceProfileLabels;
  QualitativeRatingLabels = QualitativeRatingLabels;

  constructor(
    private route: ActivatedRoute,
    private exerciseService: ExerciseService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.exerciseId = params.get('id') || '';
      if (this.exerciseId) {
        this.loadExercise();
      }
    });
  }

  loadExercise(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.exerciseService.getExerciseById(this.exerciseId).subscribe({
      next: (data) => {
        this.exercise = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load exercise details.';
      }
    });
  }

  hasSecondaryMuscles(): boolean {
    return this.exercise?.muscles.some(m => !m.isPrimary) ?? false;
  }

  getRatingBadgeClass(rating: QualitativeRating): string {
    switch (rating) {
      case QualitativeRating.High:
        return 'rating-high';
      case QualitativeRating.Moderate:
        return 'rating-moderate';
      case QualitativeRating.Low:
        return 'rating-low';
      default:
        return '';
    }
  }
}
