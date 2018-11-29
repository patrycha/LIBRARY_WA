import { Component, OnInit, Output, EventEmitter, NgModule } from '@angular/core';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormGroup, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { UserService } from '../_services/user.service';
import { User } from '../_models/User';
import { appRoutes } from '../app.module';
//import { AlertService, AuthenticationService } from '../_services';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
@NgModule({
  imports: [RouterModule.forRoot(appRoutes)]
})

export class LoginComponent implements OnInit {
  @Output() logedUser = new EventEmitter<User>();


  loginForm: FormGroup;
  submitted = false;
  user: User;
  valid: boolean;
  loginData: User;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private userService: UserService,
    private router: Router) { }

  ngOnInit() {
    this.valid = true;
    this.user = null;
    this.loginForm = this.formBuilder.group({
      login: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  onClickForm() {
    this.loginForm.get('login').markAsPristine();
    this.loginForm.get('password').markAsPristine();
  }


  //f czyli fałszywy login/hasło
  login() {
      this.userService.IsLogged(this.logedUser).subscribe(response => {
      let token = (<any>response).token;
      let user_id = (<any>response).id;
      let fullname = (<any>response).fullname;
      let user_type = (<any>response).user_type;
      let expires = (<any>response).expires;
      localStorage.setItem("token", token);
      localStorage.setItem("user_id", user_id);
      localStorage.setItem("user_fullname", fullname);
      localStorage.setItem("expires", expires);
      localStorage.setItem("user_type", user_type);
      
      this.valid = true;
      window.location.reload();
      this.router.navigateByUrl('/')
        
    //   this.router.navigate["/"];
      }, err => {
      this.valid= false;
    });
    this.submitted = true;
  /*  if (this.user === null) {
      this.valid = false;
     
      return;
    }
    else {
      this.logedUser.emit(this.user);
      this.router.navigate(['app-home']);*/
     // this.logedUser.emit(this.user);
    //  this.router.navigate(['home']);
  }

 
  }





