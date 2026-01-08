# Fundo Loan Management - Frontend

Angular 19 standalone application for managing loans.

## Tech Stack
- **Angular**: 19.1.0 (standalone components)
- **Angular Material**: 19.2.4
- **TypeScript**: 5.7.2
- **RxJS**: 7.8.0

## Project Structure
```
src/app/
├── components/
│   └── loans/              # Loan listing component
├── models/
│   └── loan.model.ts       # Loan interface
├── services/
│   └── loan.service.ts     # HTTP service for loan operations
└── environments/           # Environment configurations
```

## Running the Frontend

### Prerequisites
- Node.js 18+ 
- npm 9+
- Backend API running at `https://localhost:56807`

Install dependencies:
```sh
npm install
```  

Start the development server:  
```sh
npm start
```

Open `http://localhost:4200/` in your browser.

### Build for Production
```bash
npm run build
```

## Features Implemented

### Loan Listing
- Displays all loans from API
- Shows: Applicant, Loan Amount, Current Balance, Status
- Real-time data fetching with loading states
- Error handling with retry capability
- Status badges (paid/active)
- Refresh button

## Architecture

### Angular 19 Best Practices
- **Standalone Components**: No NgModules, tree-shakable by default
- **Signals**: Reactive state management with `signal()`
- **New Control Flow**: `@if`, `@else` syntax instead of `*ngIf`
- **Inject Function**: Modern DI with `inject()`
- **HttpClient**: Configured with `provideHttpClient()`

### Component Structure
```typescript
LoansComponent
├── Signals: loans(), loading(), error()
├── Service: LoanService (injected)
└── Template: Angular Material table with control flow
```

### Environment Configuration
- `environment.ts` - Production config (API: `http://localhost:56808/api`)
- `environment.development.ts` - Development config (API: `http://localhost:56808/api`)
- Uses HTTP to avoid SSL certificate issues in Docker

## Material Design
Using Angular Material components:
- `MatTableModule` - Data table
- `MatButtonModule` - Action buttons
- `MatProgressSpinnerModule` - Loading indicator
