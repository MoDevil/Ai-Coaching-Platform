import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PhotoService } from '../../services/photo.service';
import {
  ClientPhotoSummaryDto,
  PhotoSetType,
  PhysiqueObservationResultDto,
  UploadPhotoRequestDto
} from '../../models/photo.models';

@Component({
  selector: 'app-client-photos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-photos.component.html',
  styleUrls: ['./client-photos.component.css']
})
export class ClientPhotosComponent implements OnInit {
  @Input() clientId!: string;

  photos: ClientPhotoSummaryDto[] = [];
  selectedPhoto: ClientPhotoSummaryDto | null = null;
  selectedPhotoUrl: string | null = null;
  selectedObservation: PhysiqueObservationResultDto | null = null;

  isLoading = false;
  isAnalyzing = false;
  errorMessage = '';
  successMessage = '';

  // Upload state
  showUploadModal = false;
  uploadSetType: PhotoSetType = PhotoSetType.FrontRelaxed;
  uploadNotes = '';
  selectedFile: File | null = null;

  // Analysis state
  selectedBaselineId: string = '';
  coachPrompt: string = '';

  PhotoSetType = PhotoSetType;

  constructor(private photoService: PhotoService) {}

  ngOnInit(): void {
    if (this.clientId) {
      this.loadPhotos();
    }
  }

  loadPhotos(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.photoService.getPhotos(this.clientId).subscribe({
      next: (data) => {
        this.photos = data;
        this.isLoading = false;
        if (this.photos.length > 0 && !this.selectedPhoto) {
          this.selectPhoto(this.photos[0]);
        }
      },
      error: (err) => {
        this.errorMessage = 'Failed to load progress photos.';
        this.isLoading = false;
      }
    });
  }

  selectPhoto(photo: ClientPhotoSummaryDto): void {
    this.selectedPhoto = photo;
    this.selectedPhotoUrl = null;
    this.selectedObservation = null;

    // Load signed URL (15 min validity)
    this.photoService.getSignedUrl(this.clientId, photo.id).subscribe({
      next: (res) => {
        this.selectedPhotoUrl = res.url;
      },
      error: () => {
        this.selectedPhotoUrl = null;
      }
    });

    if (photo.hasObservation) {
      this.photoService.getObservation(this.clientId, photo.id).subscribe({
        next: (obs) => {
          this.selectedObservation = obs;
        },
        error: () => {
          this.selectedObservation = null;
        }
      });
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (file.size > 10 * 1024 * 1024) {
        this.errorMessage = 'Photo exceeds 10 MB limit.';
        this.selectedFile = null;
        return;
      }
      this.selectedFile = file;
    }
  }

  submitUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a photo file.';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const arrayBuffer = reader.result as ArrayBuffer;
      const bytes = Array.from(new Uint8Array(arrayBuffer));

      const request: UploadPhotoRequestDto = {
        fileBytes: bytes,
        mimeType: this.selectedFile!.type || 'image/jpeg',
        photoSetType: Number(this.uploadSetType),
        notes: this.uploadNotes || undefined
      };

      this.isLoading = true;
      this.errorMessage = '';
      this.photoService.uploadPhoto(this.clientId, request).subscribe({
        next: (photo) => {
          this.isLoading = false;
          this.showUploadModal = false;
          this.selectedFile = null;
          this.uploadNotes = '';
          this.successMessage = 'Photo uploaded successfully with EXIF metadata stripped.';
          this.loadPhotos();
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.detail || 'Failed to upload photo.';
        }
      });
    };
    reader.readAsArrayBuffer(this.selectedFile);
  }

  triggerAnalysis(): void {
    if (!this.selectedPhoto) return;

    this.isAnalyzing = true;
    this.errorMessage = '';
    this.successMessage = '';

    const req = {
      baselinePhotoId: this.selectedBaselineId || undefined,
      coachPrompt: this.coachPrompt || undefined
    };

    this.photoService.analyzePhoto(this.clientId, this.selectedPhoto.id, req).subscribe({
      next: (result) => {
        this.isAnalyzing = false;
        this.selectedObservation = result;
        this.selectedPhoto!.hasObservation = true;
        this.successMessage = 'Photo vision analysis completed and persisted to Client Memory.';
      },
      error: (err) => {
        this.isAnalyzing = false;
        this.errorMessage = err.error?.detail || 'Vision analysis failed.';
      }
    });
  }

  deleteSelectedPhoto(): void {
    if (!this.selectedPhoto || !confirm('Are you sure you want to delete this photo and its stored file?')) return;

    this.photoService.deletePhoto(this.clientId, this.selectedPhoto.id).subscribe({
      next: () => {
        this.selectedPhoto = null;
        this.selectedPhotoUrl = null;
        this.selectedObservation = null;
        this.loadPhotos();
      },
      error: () => {
        this.errorMessage = 'Failed to delete photo.';
      }
    });
  }

  getSetTypeLabel(type: PhotoSetType): string {
    switch (type) {
      case PhotoSetType.FrontRelaxed: return 'Front Relaxed';
      case PhotoSetType.SideRelaxed: return 'Side Relaxed';
      case PhotoSetType.BackRelaxed: return 'Back Relaxed';
      case PhotoSetType.FrontFlexed: return 'Front Flexed';
      case PhotoSetType.SideFlexed: return 'Side Flexed';
      case PhotoSetType.BackFlexed: return 'Back Flexed';
      default: return 'Standard';
    }
  }
}
