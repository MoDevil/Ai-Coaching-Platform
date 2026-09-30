import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client.models';
import { TriggerReasoningComponent } from '../trigger-reasoning/trigger-reasoning.component';
import { RecommendationHistoryComponent } from '../recommendation-history/recommendation-history.component';

@Component({
  selector: 'app-client-recommendations',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TriggerReasoningComponent,
    RecommendationHistoryComponent
  ],
  templateUrl: './client-recommendations.component.html',
  styleUrls: ['./client-recommendations.component.css']
})
export class ClientRecommendationsComponent implements OnInit {
  @ViewChild('historyComp') historyComp?: RecommendationHistoryComponent;

  clientId: string = '';
  client: Client | null = null;
  showTriggerForm: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private clientService: ClientService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (this.clientId) {
      this.loadClient();
    }
  }

  loadClient(): void {
    this.clientService.getClientById(this.clientId).subscribe({
      next: (client) => {
        this.client = client;
      },
      error: (err) => {
        console.error('Failed to load client profile', err);
      }
    });
  }

  onReasoningGenerated(result: any): void {
    if (this.historyComp) {
      this.historyComp.loadHistory();
    }
  }
}
