import { Component, OnInit } from '@angular/core';
import { ClientService } from '../../services/client.service';
import { ClientStatus, ClientStatusLabels, ClientSummary } from '../../models/client.models';

@Component({
  selector: 'app-client-list',
  templateUrl: './client-list.component.html',
  styleUrls: ['./client-list.component.css']
})
export class ClientListComponent implements OnInit {
  clients: ClientSummary[] = [];
  filteredClients: ClientSummary[] = [];
  isLoading = true;
  errorMessage = '';

  searchQuery = '';
  selectedStatusFilter: 'all' | 'active' | 'archived' = 'all';

  ClientStatus = ClientStatus;
  ClientStatusLabels = ClientStatusLabels;

  constructor(private clientService: ClientService) {}

  ngOnInit(): void {
    this.loadClients();
  }

  loadClients(): void {
    this.isLoading = true;
    this.errorMessage = '';

    let statusArg: ClientStatus | undefined = undefined;
    if (this.selectedStatusFilter === 'active') {
      statusArg = ClientStatus.Active;
    } else if (this.selectedStatusFilter === 'archived') {
      statusArg = ClientStatus.Archived;
    }

    this.clientService.getClients(statusArg).subscribe({
      next: (data) => {
        this.clients = data;
        this.applyFilter();
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load clients. Please try again.';
      }
    });
  }

  onFilterChange(status: 'all' | 'active' | 'archived'): void {
    this.selectedStatusFilter = status;
    this.loadClients();
  }

  onSearchChange(): void {
    this.applyFilter();
  }

  applyFilter(): void {
    if (!this.searchQuery.trim()) {
      this.filteredClients = [...this.clients];
      return;
    }

    const q = this.searchQuery.toLowerCase().trim();
    this.filteredClients = this.clients.filter(c =>
      c.firstName.toLowerCase().includes(q) ||
      c.lastName.toLowerCase().includes(q) ||
      (c.email && c.email.toLowerCase().includes(q)) ||
      (c.primaryGoal && c.primaryGoal.toLowerCase().includes(q))
    );
  }

  getStatusBadgeClass(status: ClientStatus): string {
    return status === ClientStatus.Active ? 'badge-active' : 'badge-archived';
  }
}
