import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoanService } from '../../services/loan.service';
import { Loan } from '../../models/loan.model';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-loans',
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './loans.component.html',
  styleUrls: ['./loans.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoansComponent implements OnInit {
  private readonly loanService = inject(LoanService);

  loans = signal<Loan[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  displayedColumns: string[] = [
    'applicantName',
    'amount',
    'currentBalance',
    'status',
  ];

  ngOnInit(): void {
    this.loadLoans();
  }

  async loadLoans() {
    this.loading.set(true);
    this.error.set(null);

    try {
      let loans = await lastValueFrom(this.loanService.getLoans());
      this.loans.set(loans);
    } catch (err) {
      this.error.set('Failed to load loans. Please try again.');
      console.error('Error loading loans:', err);
    } finally {
      this.loading.set(false);
    }
  }
}
