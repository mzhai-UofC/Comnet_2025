import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./layout/header/header.component";
import { HttpClient } from '@angular/common/http';
import { Product } from './shared/models/product';
import { Pagination } from './shared/models/pagination';
import { ShopService } from './core/services/shop.service';
import { ShopComponent } from "./features/shop/shop.component";
import { HomeComponent } from './features/home/home.component';

@Component({
    selector: 'app-root',
    imports: [RouterOutlet, HeaderComponent, ShopComponent, HomeComponent],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss'
})
export class AppComponent  {
 title = 'FatBun Universe';
}