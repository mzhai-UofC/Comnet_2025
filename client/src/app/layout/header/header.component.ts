import { Component, inject } from '@angular/core';
import { MatBadge } from '@angular/material/badge';
import { MatButton } from '@angular/material/button';
import {MatIcon} from '@angular/material/icon';
import {MatProgressBar} from '@angular/material/progress-bar';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { BusyService } from '../../core/services/busy.service';
import { CartService } from '../../core/services/cart.service';
import { AccountService } from '../../core/services/account.service';
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';
import { MatDivider } from '@angular/material/divider';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-header',
    imports: [
        MatIcon,
        MatButton,
        MatBadge,
        RouterLink,
        RouterLinkActive,
        MatProgressBar,
        MatMenu,
        MatDivider,
        MatMenuTrigger,
        CommonModule, // 添加 CommonModule
    ],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {
  busyService = inject(BusyService);
  cartService = inject(CartService);
  accountService = inject(AccountService);
  private router = inject(Router);
  isUserActionsHidden = true; // 控制用户操作区域显示状态

  logout(){
    this.accountService.logout().subscribe({
      next: () => {
        this.accountService.currentUser.set(null);
        this.router.navigateByUrl('/');
      },
      error: (error) => console.log(error)
      
    })
  }

  toggleUserActions() {
    this.isUserActionsHidden = !this.isUserActionsHidden; // 切换用户操作区域显示状态
  }

}
