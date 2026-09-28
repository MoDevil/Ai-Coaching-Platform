import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
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
import { WorkoutLoggerComponent } from './components/workout-logger/workout-logger.component';
import { SubstanceReferenceComponent } from './components/substance-reference/substance-reference.component';
import { ClientMemoryComponent } from './components/client-memory/client-memory.component';
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    ClientListComponent,
    ClientCreateComponent,
    ClientDetailComponent,
    ClientEditComponent,
    ExerciseListComponent,
    ExerciseDetailComponent,
    ClientTrainingProfileComponent,
    ProgramDetailComponent,
    WorkoutLoggerComponent,
    SubstanceReferenceComponent,
    ClientMemoryComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
