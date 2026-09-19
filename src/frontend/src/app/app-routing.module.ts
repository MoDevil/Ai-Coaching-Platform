import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { ClientListComponent } from './components/client-list/client-list.component';
import { ClientCreateComponent } from './components/client-create/client-create.component';
import { ClientDetailComponent } from './components/client-detail/client-detail.component';
import { ClientEditComponent } from './components/client-edit/client-edit.component';
import { ExerciseListComponent } from './components/exercise-list/exercise-list.component';
import { ExerciseDetailComponent } from './components/exercise-detail/exercise-detail.component';
import { ClientTrainingProfileComponent } from './components/client-training-profile/client-training-profile.component';
import { ProgramDetailComponent } from './components/program-detail/program-detail.component';
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'clients', component: ClientListComponent, canActivate: [AuthGuard] },
  { path: 'clients/new', component: ClientCreateComponent, canActivate: [AuthGuard] },
  { path: 'clients/:id', component: ClientDetailComponent, canActivate: [AuthGuard] },
  { path: 'clients/:id/edit', component: ClientEditComponent, canActivate: [AuthGuard] },
  { path: 'clients/:id/training-profile', component: ClientTrainingProfileComponent, canActivate: [AuthGuard] },
  { path: 'programs/:id', component: ProgramDetailComponent, canActivate: [AuthGuard] },
  { path: 'programs', component: ProgramDetailComponent, canActivate: [AuthGuard] },
  { path: 'exercises', component: ExerciseListComponent, canActivate: [AuthGuard] },
  { path: 'exercises/:id', component: ExerciseDetailComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/clients', pathMatch: 'full' },
  { path: '**', redirectTo: '/clients' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
