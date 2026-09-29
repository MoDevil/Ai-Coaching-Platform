import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { VideoService } from '../../services/video.service';
import {
  ClientVideoSummaryDto,
  VideoFrameDto,
  ExerciseTechniqueObservationResultDto,
  VideoAnalysisJobStatusDto
} from '../../models/video.models';

@Component({
  selector: 'app-client-videos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-videos.component.html',
  styleUrls: ['./client-videos.component.css']
})
export class ClientVideosComponent implements OnInit, OnDestroy {
  @Input() clientId!: string;

  videos: ClientVideoSummaryDto[] = [];
  selectedVideo: ClientVideoSummaryDto | null = null;
  selectedVideoUrl: string | null = null;
  selectedFrames: VideoFrameDto[] = [];
  selectedObservation: ExerciseTechniqueObservationResultDto | null = null;
  observationHistory: ExerciseTechniqueObservationResultDto[] = [];

  isLoading = false;
  isAnalyzing = false;
  activeJobId: string | null = null;
  jobProgress = 0;
  jobStatusText = '';
  errorMessage = '';
  successMessage = '';

  // Upload state
  showUploadModal = false;
  uploadExerciseName = '';
  uploadNotes = '';
  selectedFile: File | null = null;

  // Analysis focus
  focusAreasInput = '';

  private pollSubscription?: Subscription;

  constructor(
    private videoService: VideoService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    if (!this.clientId && this.route) {
      this.clientId = this.route.snapshot.paramMap.get('id') || '';
    }
    if (this.clientId) {
      this.loadVideos();
    }
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  loadVideos(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.videoService.getVideos(this.clientId).subscribe({
      next: (data) => {
        this.videos = data;
        this.isLoading = false;
        if (this.videos.length > 0 && !this.selectedVideo) {
          this.selectVideo(this.videos[0]);
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load client videos.';
        this.isLoading = false;
      }
    });
  }

  selectVideo(video: ClientVideoSummaryDto): void {
    this.selectedVideo = video;
    this.selectedVideoUrl = null;
    this.selectedFrames = [];
    this.selectedObservation = null;
    this.observationHistory = [];

    // Load signed URL for video
    this.videoService.getSignedUrl(this.clientId, video.id).subscribe({
      next: (res) => {
        this.selectedVideoUrl = res.signedUrl;
      },
      error: () => {
        this.selectedVideoUrl = null;
      }
    });

    // Load extracted frames
    this.videoService.getVideoFrames(this.clientId, video.id).subscribe({
      next: (frames) => {
        this.selectedFrames = frames;
      },
      error: () => {
        this.selectedFrames = [];
      }
    });

    // Load latest observation if available
    if (video.observationRecordId) {
      this.videoService.getObservation(this.clientId, video.id).subscribe({
        next: (obs) => {
          this.selectedObservation = obs;
        },
        error: () => {
          this.selectedObservation = null;
        }
      });

      // Load observation history
      this.videoService.getObservations(this.clientId, video.id).subscribe({
        next: (history) => {
          this.observationHistory = history;
        },
        error: () => {
          this.observationHistory = [];
        }
      });
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (file.size > 100 * 1024 * 1024) {
        this.errorMessage = 'Video exceeds 100 MB limit.';
        this.selectedFile = null;
        return;
      }
      this.selectedFile = file;
    }
  }

  submitUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a video file.';
      return;
    }
    if (!this.uploadExerciseName.trim()) {
      this.errorMessage = 'Exercise name is required.';
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('exerciseName', this.uploadExerciseName.trim());
    if (this.uploadNotes.trim()) {
      formData.append('notes', this.uploadNotes.trim());
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.videoService.uploadVideo(this.clientId, formData).subscribe({
      next: (video) => {
        this.isLoading = false;
        this.showUploadModal = false;
        this.selectedFile = null;
        this.uploadExerciseName = '';
        this.uploadNotes = '';
        this.successMessage = 'Video uploaded, normalized to H.264 (audio stripped), and frames extracted successfully.';
        this.loadVideos();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || 'Failed to upload video.';
      }
    });
  }

  triggerAnalysis(): void {
    if (!this.selectedVideo) return;

    this.isAnalyzing = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.jobProgress = 0;
    this.jobStatusText = 'Queued';

    const focusAreas = this.focusAreasInput
      ? this.focusAreasInput.split(',').map((s) => s.trim()).filter((s) => s.length > 0)
      : [];

    this.videoService.analyzeVideo(this.clientId, this.selectedVideo.id, focusAreas).subscribe({
      next: (resp) => {
        this.activeJobId = resp.jobId;
        this.startPollingStatus(this.selectedVideo!.id);
      },
      error: (err) => {
        this.isAnalyzing = false;
        this.errorMessage = err.error?.detail || 'Failed to enqueue video analysis.';
      }
    });
  }

  private startPollingStatus(videoId: string): void {
    this.stopPolling();
    this.pollSubscription = interval(1500).subscribe(() => {
      this.videoService.getAnalysisStatus(this.clientId, videoId).subscribe({
        next: (status: VideoAnalysisJobStatusDto) => {
          this.jobProgress = status.progressPercentage;
          this.jobStatusText = status.status;

          if (status.status === 'Completed') {
            this.stopPolling();
            this.isAnalyzing = false;
            this.successMessage = 'Video technique analysis completed successfully.';
            this.selectVideo(this.selectedVideo!);
          } else if (status.status === 'Failed') {
            this.stopPolling();
            this.isAnalyzing = false;
            this.errorMessage = status.errorMessage || 'Video analysis failed.';
          }
        },
        error: () => {
          this.stopPolling();
          this.isAnalyzing = false;
          this.errorMessage = 'Lost connection to analysis status.';
        }
      });
    });
  }

  private stopPolling(): void {
    if (this.pollSubscription) {
      this.pollSubscription.unsubscribe();
      this.pollSubscription = undefined;
    }
  }

  deleteSelectedVideo(): void {
    if (!this.selectedVideo || !confirm('Are you sure you want to anonymize/delete this video and all extracted frames?')) return;

    this.videoService.deleteVideo(this.clientId, this.selectedVideo.id).subscribe({
      next: () => {
        this.selectedVideo = null;
        this.selectedVideoUrl = null;
        this.selectedFrames = [];
        this.selectedObservation = null;
        this.observationHistory = [];
        this.loadVideos();
      },
      error: () => {
        this.errorMessage = 'Failed to delete video.';
      }
    });
  }
}
