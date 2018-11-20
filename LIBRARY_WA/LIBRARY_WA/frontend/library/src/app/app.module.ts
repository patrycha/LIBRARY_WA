//styles
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { Routes, RouterModule } from '@angular/router';
//services
import { WorkoutService } from './_services/workout.service';
import { UserService } from './_services/user.service';
import { ResourceService } from './_services/resource.service';

import { HttpClientModule } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { DatePipe } from '@angular/common';

//components
import { AppComponent } from './app.component';
import { AddResourceComponent } from './add-resource/add-resource.component';
import { AddUserComponent } from './user/add-user/add-user.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { UserAccountComponent } from './user-account/user-account.component';
import { SearchUserComponent } from './user/search-user/search-user.component';
import { SearchResourceComponent } from './search-resource/search-resource.component';
import { GridJoggingComponentComponent } from './grid-jogging-component/grid-jogging-component.component';
import { UserComponent } from './user/user.component';
import { BookBookingModule } from './user-account/details/book-booking/book-booking.module';
import { BookBorrowModule } from './user-account/details/book-borrow/book-borrow.module';
import { CurrentBorrowModule } from './user-account/details/current-borrow/current-borrow.module';
import { UserPIModule } from './user-account/details/user-pi/user-pi.module';

export const appRoutes: Routes = [
  {
    path: 'app-login',
    component: LoginComponent
  },
  {
    path: 'app-user',
    component: UserComponent,
    children: [
      {
        
        path: 'app-add-user',
        component: AddUserComponent
     
      },
      {
        path: 'app-search-user',
        component: SearchUserComponent
      }]
  },
  
  {
    path: 'app-add-resource',
    component: AddResourceComponent
  },
  {
    path: 'app-search-resource',
    component: SearchResourceComponent
  },

  {
    path: 'app-home',
    component: HomeComponent
  },
  {
    path: 'app-user-account',
    component: UserAccountComponent,
    children: [
      {

        path: 'app-book-booking',
        component: BookBookingModule

      },
      {
        path: 'app-book-borrow',
        component: BookBorrowModule
      },
    {

      path: 'app-current-borrow',
      component: CurrentBorrowModule

      },
      {
        path: 'app-user-pi',
        component: UserPIModule
      },
    {
        path: '',
      component: CurrentBorrowModule
      }]
  },
  {
    path: '',
    component: HomeComponent
  },

];

@NgModule({
  declarations: [
    AppComponent,
    AddResourceComponent,
    AddUserComponent,
    AddResourceComponent,
    HomeComponent,
    LoginComponent,
    UserAccountComponent,
    SearchUserComponent,
    SearchResourceComponent,
    CurrentBorrowModule,
    GridJoggingComponentComponent,
    UserComponent,
    UserPIModule,
    BookBookingModule,
    BookBorrowModule
  ],
  imports: [
    RouterModule.forRoot(appRoutes),
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpModule,
    HttpClientModule
  ],
  providers: [WorkoutService,UserService, ResourceService],
  bootstrap: [AppComponent]
})
export class AppModule { }
