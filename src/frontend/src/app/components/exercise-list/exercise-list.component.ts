import { Component, OnInit } from '@angular/core';
import { ExerciseService } from '../../services/exercise.service';
import {
  Equipment,
  ExerciseCategory,
  ExerciseCategoryLabels,
  ExerciseSummary,
  MovementPattern,
  QualitativeRating,
  QualitativeRatingLabels,
  ResistanceProfile,
  ResistanceProfileLabels
} from '../../models/exercise.models';

@Component({
  selector: 'app-exercise-list',
  templateUrl: './exercise-list.component.html',
  styleUrls: ['./exercise-list.component.css']
})
export class ExerciseListComponent implements OnInit {
  exercises: ExerciseSummary[] = [];
  patterns: MovementPattern[] = [];
  equipmentList: Equipment[] = [];

  searchQuery = '';
  selectedPatternId = '';
  selectedCategoryId: number | null = null;
  isLoading = true;
  errorMessage = '';

  ExerciseCategory = ExerciseCategory;
  ExerciseCategoryLabels = ExerciseCategoryLabels;
  QualitativeRating = QualitativeRating;
  QualitativeRatingLabels = QualitativeRatingLabels;
  ResistanceProfile = ResistanceProfile;
  ResistanceProfileLabels = ResistanceProfileLabels;

  categories = [
    { value: ExerciseCategory.Compound, label: 'Compound' },
    { value: ExerciseCategory.Isolation, label: 'Isolation' },
    { value: ExerciseCategory.Machine, label: 'Machine' },
    { value: ExerciseCategory.Bodyweight, label: 'Bodyweight' }
  ];

  constructor(private exerciseService: ExerciseService) {}

  ngOnInit(): void {
    this.loadMetadata();
    this.loadExercises();
  }

  loadMetadata(): void {
    this.exerciseService.getMovementPatterns().subscribe({
      next: (data) => (this.patterns = data)
    });
    this.exerciseService.getEquipment().subscribe({
      next: (data) => (this.equipmentList = data)
    });
  }

  loadExercises(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const filter = {
      search: this.searchQuery || undefined,
      movementPatternId: this.selectedPatternId || undefined,
      category: this.selectedCategoryId !== null ? this.selectedCategoryId : undefined
    };

    this.exerciseService.getExercises(filter).subscribe({
      next: (data) => {
        this.exercises = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load exercises. Please try again.';
      }
    });
  }

  onFilterChange(): void {
    this.loadExercises();
  }

  onResetFilters(): void {
    this.searchQuery = '';
    this.selectedPatternId = '';
    this.selectedCategoryId = null;
    this.loadExercises();
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
