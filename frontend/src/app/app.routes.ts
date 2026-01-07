import { Routes } from '@angular/router';
import { LoansComponent } from './components/loans/loans.component';

export const routes: Routes = [
  { path: '', redirectTo: '/loans', pathMatch: 'full' },
  { path: 'loans', component: LoansComponent },
  { path: '**', redirectTo: '/loans' }
];
